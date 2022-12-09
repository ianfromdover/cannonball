// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Text;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.Editor;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities.Editor;
using Niantic.ARDK.Utilities.Extensions;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.VirtualStudio.Networking.Mock;

using UnityEditor;
using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  [Serializable]
  internal sealed class _MockPlayConfigurationEditor
  {
    private _MockModeLauncher Launcher
    {
      get
      {
        return (_MockModeLauncher)_VirtualStudioLauncher.GetOrCreateModeLauncher(RuntimeEnvironment.Mock);
      }
    }

    private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    private string[] _mockSceneGuids;
    private string[] _mockSceneNames;
    private int _selectedMockSceneIndex;

    public void OnSelectionChange(bool isSelected)
    {
      if (isSelected)
        LoadMockScenes();
    }

    private void LoadMockScenes()
    {
      var mockPrefabs =
        _AssetDatabaseUtilities.FindPrefabsWithComponent<MockSceneConfiguration>("Assets");

      _mockSceneGuids = new string[mockPrefabs.Length];

      _mockSceneNames = new string[mockPrefabs.Length + 1];
      _mockSceneNames[0] = "None";

      for (var i = 0; i < mockPrefabs.Length; i++)
      {
        var guid = mockPrefabs[i].Guid;
        _mockSceneGuids[i] = guid;
        _mockSceneNames[i + 1] = mockPrefabs[i].Name;

        if (Launcher.SceneGuid == guid)
          _selectedMockSceneIndex = i + 1;
      }
    }

    private static void ValidateAllMockSceneLayers()
    {
      var mockPrefabs =
        _AssetDatabaseUtilities.FindPrefabsWithComponent<MockSceneConfiguration>("Assets");

      foreach (var prefab in mockPrefabs)
      {
        var path = AssetDatabase.GUIDToAssetPath(prefab.Guid);
        var asset = AssetDatabase.LoadMainAssetAtPath(path);

        ((GameObject)asset).GetComponent<MockSceneConfiguration>()._SetLayersIfNeeded(true);
      }
    }

    public void DrawGUI()
    {
      EditorGUILayout.LabelField("AR", VirtualStudioConfigurationEditor._HeaderStyle);
      GUILayout.Space(10);

      DrawMockSceneGUI();
      GUILayout.Space(10);

      DrawCameraConfigurationGUI();
      GUILayout.Space(20);

      EditorGUILayout.LabelField("Multiplayer", VirtualStudioConfigurationEditor._HeaderStyle);
      GUILayout.Space(10);

      DrawPlayConfigurationSelector();
      DrawSessionMetadataGUI();
      GUILayout.Space(20);

      DrawPlayers();
    }



    private void DrawMockSceneGUI()
    {
      EditorGUILayout.BeginHorizontal();

      if (_mockSceneNames == null)
        LoadMockScenes();

      var newMockSceneIndex =
        EditorGUILayout.Popup("Mock Scene: ", _selectedMockSceneIndex, _mockSceneNames);

      if (newMockSceneIndex != _selectedMockSceneIndex)
      {
        _selectedMockSceneIndex = newMockSceneIndex;

        var guid = _selectedMockSceneIndex >= 1 ? _mockSceneGuids[_selectedMockSceneIndex - 1] : "";
        Launcher.SceneGuid = guid;
      }

      if (CommonStyles.RefreshButton())
        LoadMockScenes();

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      if (GUILayout.Button("Validate all Mock scenes", GUILayout.MaxWidth(200)))
        ValidateAllMockSceneLayers();

      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
    }

    private void DrawCameraConfigurationGUI()
    {
      EditorGUILayout.LabelField("Camera Controls", VirtualStudioConfigurationEditor._SubHeadingStyle);
      GUILayout.Space(10);

      EditorGUI.BeginDisabledGroup(Application.isPlaying);

      var newFps = EditorGUILayout.IntField("FPS", Launcher.FPS);
      if (newFps != Launcher.FPS)
        Launcher.FPS = newFps;

      EditorGUI.EndDisabledGroup();

      var newMovespeed = EditorGUILayout.Slider("Move Speed", Launcher.MoveSpeed, 0.1f, 20f);
      if (newMovespeed != Launcher.MoveSpeed)
        Launcher.MoveSpeed = newMovespeed;

      var newLookSpeed = EditorGUILayout.IntSlider("Look Speed", Launcher.LookSpeed, 1, 180);
      if (newLookSpeed != Launcher.LookSpeed)
        Launcher.LookSpeed = newLookSpeed;

      var newScrollDirection = EditorGUILayout.Toggle("Scroll Direction: Natural", Launcher.ScrollDirection);
      if (newScrollDirection != Launcher.ScrollDirection)
        Launcher.ScrollDirection = newScrollDirection;
    }

    private void DrawPlayConfigurationSelector()
    {
      var newPlayConfiguration =
        (MockPlayConfiguration) EditorGUILayout.ObjectField
        (
          "Play Configuration",
          Launcher.PlayConfiguration,
          typeof(MockPlayConfiguration),
          false
        );

      if (newPlayConfiguration != Launcher.PlayConfiguration)
        Launcher.PlayConfiguration = newPlayConfiguration;
    }

    private void DrawPlayers()
    {
      EditorGUILayout.LabelField("Players", VirtualStudioConfigurationEditor._SubHeadingStyle);
      GUILayout.Space(10);
      DrawLocalPlayer();

      var playConfiguration = Launcher.PlayConfiguration;
      EditorGUI.BeginDisabledGroup(playConfiguration == null);

      EditorGUI.BeginDisabledGroup(!Application.isPlaying);
      if (GUILayout.Button("Connect and Run All", GUILayout.Width(200)))
      {
        playConfiguration.ConnectAllPlayersNetworkings(GetSessionMetadata());
        playConfiguration.RunAllPlayersARSessions();
      }

      EditorGUI.EndDisabledGroup();

      if (playConfiguration != null)
      {
        foreach (var profile in playConfiguration.Profiles)
        {
          DrawPlayerProfile(profile);
          GUILayout.Space(10);
        }
      }

      EditorGUI.EndDisabledGroup();
    }

    private void DrawLocalPlayer()
    {
      var localName = _VirtualStudioSessionsManager.LOCAL_PLAYER_NAME;
      if (!_foldoutStates.ContainsKey(localName))
        _foldoutStates.Add(localName, true);

      var showFoldout  = EditorGUILayout.Foldout(_foldoutStates[localName], localName);
      _foldoutStates[localName] = showFoldout;

      if (Application.isPlaying)
      {
        var localPlayer = _VirtualStudioSessionsManager.Instance.LocalPlayer;
        if (localPlayer == null)
          return;

        var arNetworking = localPlayer.ARNetworking;
        if (arNetworking != null && arNetworking.Networking.IsConnected)
        {
          var currState = arNetworking.LocalPeerState;
          var newState = (PeerState)EditorGUILayout.EnumPopup(currState);
          if (newState != currState)
            localPlayer.SetPeerState(newState);
        }
      }
    }

    private void DrawPlayerProfile(MockArdkPlayerProfile profile)
    {
      var playerName = profile.PlayerName;
      if (!_foldoutStates.ContainsKey(playerName))
        _foldoutStates.Add(playerName, true);

      var showFoldout = EditorGUILayout.Foldout(_foldoutStates[playerName], playerName);
      _foldoutStates[playerName] = showFoldout;

      if (!showFoldout)
        return;

      using (var horizontal = new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(300)))
      {
        GUILayout.Space(20);

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        using (var col1 = new GUILayout.VerticalScope())
        {
          var style = new GUILayoutOption[]
          {
            GUILayout.Width(150)
          };

          if (EditorGUILayout.ToggleLeft("Active", profile.IsActive, style) != profile.IsActive)
            profile.IsActive = !profile.IsActive;

          if (EditorGUILayout.ToggleLeft("Create AR", profile.UsingAR, style) != profile.UsingAR)
            profile.UsingAR = !profile.UsingAR;

          var newUsingNetworking =
            EditorGUILayout.ToggleLeft("Create Network", profile.UsingNetwork, style);

          if (newUsingNetworking != profile.UsingNetwork)
            profile.UsingNetwork = newUsingNetworking;

          var newUsingARNetworking =
            EditorGUILayout.ToggleLeft("Create ARNetworking", profile.UsingARNetworking, style);

          if (newUsingARNetworking != profile.UsingARNetworking)
            profile.UsingARNetworking = newUsingARNetworking;
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(20);

        if (!Application.isPlaying || !Launcher.PlayConfiguration._Initialized || !profile.IsActive)
          return;

        using (var col2 = new EditorGUILayout.VerticalScope())
        {
          var style = new[] { GUILayout.Width(100) };

          GUILayout.Space(18);
          EditorGUI.BeginDisabledGroup(!profile.UsingAR);

          var player = profile.GetPlayer();

          var arSession = player.ARSession;
          if (arSession != null && arSession.State == ARSessionState.Running)
          {
            if (GUILayout.Button("Dispose", style))
              arSession.Dispose();
          }
          else
          {
            if (GUILayout.Button("Run", style))
              arSession.Run(Launcher.PlayConfiguration.GetARConfiguration(profile));
          }

          EditorGUI.EndDisabledGroup();

          GUILayout.Space(2);
          EditorGUI.BeginDisabledGroup(!profile.UsingNetwork);
          var networking = player.Networking;
          if (networking != null && networking.IsConnected)
          {
            if (GUILayout.Button("Dispose", style))
              networking.Dispose();
          }
          else
          {
            if (GUILayout.Button("Join", style))
              networking.Join(GetSessionMetadata());
          }

          GUILayout.Space(2);
          var arNetworking = player.ARNetworking;
          if (arNetworking != null && arNetworking.Networking.IsConnected)
          {
            var currState = arNetworking.LocalPeerState;
            var newState = (PeerState)EditorGUILayout.EnumPopup(currState);
            if (newState != currState)
              player.SetPeerState(newState);
          }

          EditorGUI.EndDisabledGroup();
        }
      }
    }

    private void DrawSessionMetadataGUI()
    {
      if (Launcher.HasDetectedSessionMetadata)
      {
        EditorGUILayout.LabelField("Session Identifier", "Detected");
        return;
      }

      var newInputSessionIdentifier =
        EditorGUILayout.TextField("Session Identifier", Launcher.InputSessionIdentifier);

      if (newInputSessionIdentifier != Launcher.InputSessionIdentifier)
        Launcher.InputSessionIdentifier = newInputSessionIdentifier;
    }

    private byte[] GetSessionMetadata()
    {
      if (Launcher.HasDetectedSessionMetadata)
        return Launcher.DetectedSessionMetadata;

      if (string.IsNullOrWhiteSpace(Launcher.InputSessionIdentifier))
      {
        ARLog._Error
        (
          "Must enter a non-empty session identifier in the Virtual Studio window " +
          "in order to join a networking session."
        );

        return null;
      }

      return Encoding.UTF8.GetBytes(Launcher.InputSessionIdentifier);
    }
  }
}
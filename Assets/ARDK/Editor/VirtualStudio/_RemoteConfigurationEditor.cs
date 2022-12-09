// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Text.RegularExpressions;

using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.Remote;

using UnityEditor;
using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  // UI for connecting to the ARDK Remote Feed app
  [Serializable]
  internal sealed class _RemoteConfigurationEditor
  {
    private _RemoteModeLauncher Launcher
    {
      get
      {
        return (_RemoteModeLauncher)_VirtualStudioLauncher.GetOrCreateModeLauncher(RuntimeEnvironment.Remote);
      }
    }

    public void OnSelectionChange(bool isSelected)
    {
      _RemoteConnection.IsEnabled = isSelected;
    }

    private GUIStyle _statusStyle;
    private GUIStyle StatusStyle
    {
      get
      {
        if (_statusStyle == null)
        {
          _statusStyle = new GUIStyle(EditorStyles.largeLabel);
          _statusStyle.fontSize = 20;
          _statusStyle.fixedHeight = 30;
        }

        return _statusStyle;
      }
    }

    public void DrawGUI()
    {
      EditorGUI.BeginDisabledGroup(Application.isPlaying);

      var newImageCompression = EditorGUILayout.IntField("Image Compression:", Launcher.ImageCompression);
      if (newImageCompression != Launcher.ImageCompression)
        Launcher.ImageCompression = newImageCompression;

      var newImageFramerate = EditorGUILayout.IntField("Image Framerate:", Launcher.ImageFramerate);
      if (newImageFramerate != Launcher.ImageFramerate)
        Launcher.ImageFramerate = newImageFramerate;

      var newAwarenessFramerate = EditorGUILayout.IntField("Awareness Framerate:", Launcher.AwarenessFramerate);
      if (newAwarenessFramerate != Launcher.AwarenessFramerate)
        Launcher.AwarenessFramerate = newAwarenessFramerate;

      var newFeaturePointFramerate = EditorGUILayout.IntField("Feature Point Framerate:", Launcher.FeaturePointFramerate);
      if (newFeaturePointFramerate != Launcher.FeaturePointFramerate)
        Launcher.FeaturePointFramerate = newFeaturePointFramerate;

      EditorGUI.EndDisabledGroup();

      GUILayout.Space(20);

      if (Application.isPlaying)
      {
        if (!_RemoteConnection.IsReady)
        {
          if (_RemoteConnection.IsEnabled)
          {
            StatusStyle.normal.textColor = Color.magenta;
            EditorGUILayout.LabelField("Waiting for Remote Connection to be ready", StatusStyle);
          }
          else
          {
            StatusStyle.normal.textColor = Color.gray;
            EditorGUILayout.LabelField("Not active...", StatusStyle);
          }
        }
        else if (!_RemoteConnection.IsConnected)
        {
          StatusStyle.normal.textColor = Color.cyan;
          EditorGUILayout.LabelField("Waiting for remote device to connect", StatusStyle);
        }
        else
        {
          StatusStyle.normal.textColor = Color.green;
          EditorGUILayout.LabelField("Connected", StatusStyle);
        }
      }
      else
      {
        StatusStyle.normal.textColor = Color.grey;
        EditorGUILayout.LabelField("Waiting for play mode", StatusStyle);
      }
    }
  }
}

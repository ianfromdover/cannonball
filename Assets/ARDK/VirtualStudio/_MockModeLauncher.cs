using System;

using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.VirtualStudio.Networking.Mock;

using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio
{
#if UNITY_EDITOR
  internal class _MockModeLauncher:
    _IVirtualStudioModeLauncher
  {
    private const string PLAY_CONFIGURATION_KEY = "ARDK_PlayConfiguration";
    private const string INPUT_SESSION_ID_KEY = "ARDK_Input_Session_Identifier";
    private const string MOCK_SCENE_KEY = "ARDK_Mock_Scene_Guid";

    private string _sceneGuid;

    // Camera configuration
    private int _fps;
    private float _moveSpeed;
    private int _lookSpeed;
    private bool _scrollDirection;

    // Shared AR configuration
    private string _inputSessionIdentifier;
    private MockPlayConfiguration _playConfiguration;

    private bool _listeningForJoin;

    public int FPS
    {
      get
      {
        return _fps;
      }
      set
      {
        _fps = value;
        _MockCameraConfiguration.FPS = value;
      }
    }

    public float MoveSpeed
    {
      get
      {
        return _moveSpeed;
      }
      set
      {
        _moveSpeed = value;
        _MockCameraConfiguration.MoveSpeed = value;
      }
    }

    public int LookSpeed
    {
      get
      {
        return _lookSpeed;
      }
      set
      {
        _lookSpeed = value;
        _MockCameraConfiguration.LookSpeed = value;
      }
    }

    public bool ScrollDirection
    {
      get
      {
        return _scrollDirection;
      }
      set
      {
        _scrollDirection = value;
        _MockCameraConfiguration.ScrollDirection = value ? -1 : 1;
      }
    }

    public string SceneGuid
    {
      get
      {
        return _sceneGuid;
      }
      set
      {
        PlayerPrefs.SetString(MOCK_SCENE_KEY, value);
        _sceneGuid = value;
      }
    }

    public string InputSessionIdentifier
    {
      get
      {
        return _inputSessionIdentifier;
      }
      set
      {
        PlayerPrefs.SetString(INPUT_SESSION_ID_KEY, value);
        _inputSessionIdentifier = value;
      }
    }

    public byte[] DetectedSessionMetadata { get; private set; }

    public bool HasDetectedSessionMetadata
    {
      get
      {
        return DetectedSessionMetadata != null && DetectedSessionMetadata.Length > 0;
      }
    }

    public MockPlayConfiguration PlayConfiguration
    {
      get
      {
        return _playConfiguration;
      }
      set
      {
        PlayerPrefs.SetString
        (
          PLAY_CONFIGURATION_KEY,
          value == null ? null : value.name
        );

        _playConfiguration = value;
      }
    }

    public _MockModeLauncher()
    {
      if (_playConfiguration != null)
        _playConfiguration._Initialize();

      var playConfigurationName = PlayerPrefs.GetString(PLAY_CONFIGURATION_KEY, null);

      if (!string.IsNullOrEmpty(playConfigurationName))
        _playConfiguration = GetPlayConfiguration(playConfigurationName);

      _fps = _MockCameraConfiguration.FPS;
      _moveSpeed = _MockCameraConfiguration.MoveSpeed;
      _lookSpeed = _MockCameraConfiguration.LookSpeed;
      _scrollDirection = _MockCameraConfiguration.ScrollDirection == -1;

      _inputSessionIdentifier = PlayerPrefs.GetString(INPUT_SESSION_ID_KEY, "ABC");
      _sceneGuid = PlayerPrefs.GetString(MOCK_SCENE_KEY, "");
    }

    public void ExitEditMode()
    {
      // Must do this before entering Play Mode, because saving the prefab asset
      // while in Play Mode causes unexplained errors during mock awareness buffer generation.
      var scenePrefab = GetMockScenePrefab();
      if (scenePrefab != null)
        scenePrefab.GetComponent<MockSceneConfiguration>()._SetLayersIfNeeded(true);
    }

    public void EnterPlayMode()
    {
      DetectedSessionMetadata = null;

      if (!_listeningForJoin)
      {
        MultipeerNetworkingFactory.NetworkingInitialized += OnNetworkingInitialized;
        _listeningForJoin = true;
      }

      var existingConfig = _VirtualStudioSessionsManager.Instance.PlayConfiguration;
      if (existingConfig == null)
      {
        if (_playConfiguration != null)
          _playConfiguration._Initialize();
      }
      else
      {
        _playConfiguration = existingConfig;
      }

      // Instantiate Mock Scene
      var scenePrefab = GetMockScenePrefab();
      if (scenePrefab != null)
        PrefabUtility.InstantiatePrefab(scenePrefab);
    }

    public void ExitPlayMode()
    {
      DetectedSessionMetadata = null;

      MultipeerNetworkingFactory.NetworkingInitialized -= OnNetworkingInitialized;
      _listeningForJoin = false;
    }

    private GameObject GetMockScenePrefab()
    {
      if (string.IsNullOrEmpty(SceneGuid))
        return null;

      var path = AssetDatabase.GUIDToAssetPath(SceneGuid);
      var asset = AssetDatabase.LoadMainAssetAtPath(path);
      if (asset == null)
      {
        ARLog._Error("Could not load selected mock scene asset.");
        return null;
      }

      var go = asset as GameObject;
      if (go == null || go.GetComponent<MockSceneConfiguration>() == null)
      {
        ARLog._Error("Loaded asset is not a valid Mock Scene prefab.");
        return null;
      }

      return go;
    }

    private static MockPlayConfiguration GetPlayConfiguration(string name)
    {
      var filter = string.Format("{0} t:MockPlayConfiguration", name);
      var guids = AssetDatabase.FindAssets(filter);

      if (guids.Length == 0)
      {
        ARLog._WarnFormat("Could not load MockPlayConfiguration named: {0}", objs: name);
        return null;
      }

      var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
      var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MockPlayConfiguration));

      ARLog._DebugFormat("Loaded MockPlayConfiguration named: {0}", objs: name);
      return asset as MockPlayConfiguration;
    }

    private void OnNetworkingInitialized(AnyMultipeerNetworkingInitializedArgs args)
    {
      args.Networking.Connected +=
        connectedArgs =>
        {
          if (args.Networking is _MockMultipeerNetworking mockNetworking)
            DetectedSessionMetadata = mockNetworking.JoinedSessionMetadata;
        };
    }
  }
#endif
}

using System;

using System.Collections.Generic;

using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;
using UnityEditor;

namespace Niantic.ARDK.VirtualStudio
{
#if UNITY_EDITOR
  [InitializeOnLoad]
#endif
  internal static class _VirtualStudioLauncher
  {
    private const string VS_MODE_KEY = "ARDK_VirtualStudio_Mode";

    private static readonly Dictionary<RuntimeEnvironment, _IVirtualStudioModeLauncher> _modeLaunchers;

    // Default is invalid value
    private static RuntimeEnvironment _selectedMode = RuntimeEnvironment.Default;

    public static RuntimeEnvironment SelectedMode
    {
      get
      {
        if (_selectedMode == RuntimeEnvironment.Default)
        {
          _selectedMode =
            (RuntimeEnvironment)PlayerPrefs.GetInt
            (
              VS_MODE_KEY,
#if UNITY_EDITOR
              (int)RuntimeEnvironment.Mock
#else
              (int)RuntimeEnvironment.LiveDevice
#endif

            );
        }

        return _selectedMode;
      }

      set
      {
        if (value == RuntimeEnvironment.Default)
          throw new InvalidOperationException("Cannot set Virtual Studio to run in `Default` mode.");

        if (!_ArdkPlatformUtility.AreNativeBinariesAvailable && value == RuntimeEnvironment.Playback)
          throw new InvalidOperationException("Playback Mode is not supported on this platform.");

        _selectedMode = value;
        PlayerPrefs.SetInt(VS_MODE_KEY, (int)_selectedMode);
      }
    }

    public static _IVirtualStudioModeLauncher GetOrCreateModeLauncher(RuntimeEnvironment env)
    {
      if (!_modeLaunchers.ContainsKey(env))
      {
        switch (env)
        {
          case RuntimeEnvironment.Playback:
            _modeLaunchers[env] = new _PlaybackModeLauncher();
            break;

          case RuntimeEnvironment.Mock:
#if UNITY_EDITOR
            _modeLaunchers[env] = new _MockModeLauncher();
#else
            ARLog._Warn("Cannot run Virtual Studio in Mock Mode outside of the Unity Editor");
            _modeLaunchers[env] = null;
#endif
            break;

          case RuntimeEnvironment.Remote:
            _modeLaunchers[env] = new _RemoteModeLauncher();
            break;

          case RuntimeEnvironment.LiveDevice:
            _modeLaunchers[env] = null;
            break;

          // default to Mock mode
          case RuntimeEnvironment.Default:
            ARLog._Warn("Running _VirtualStudioLauncher with environment Default, which should not happen");
            _modeLaunchers[env] = null;
            break;

          default:
            ARLog._Warn("Not a valid runtime environment for _VirtualStudioLauncher");
            _modeLaunchers[env] = null;
            break;
        }
      }

      return _modeLaunchers[env];
    }

    static _VirtualStudioLauncher()
    {
      _modeLaunchers = new Dictionary<RuntimeEnvironment, _IVirtualStudioModeLauncher>();

#if UNITY_EDITOR && !ARDK_TEST
      EditorApplication.playModeStateChanged += OnStateChanged;
#endif
    }

#if UNITY_EDITOR
    private static void OnStateChanged(PlayModeStateChange state)
    {
      var launcher = GetOrCreateModeLauncher(SelectedMode);
      if (launcher == null)
        return;

      switch (state)
      {
        case PlayModeStateChange.ExitingEditMode:
          launcher.ExitEditMode();
          break;

        case PlayModeStateChange.EnteredPlayMode:
          launcher.EnterPlayMode();
          break;

        case PlayModeStateChange.ExitingPlayMode:
          launcher.ExitPlayMode();
          break;
      }
    }
#endif
  }
}


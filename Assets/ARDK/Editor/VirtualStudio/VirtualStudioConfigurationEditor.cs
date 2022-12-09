// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  public sealed class VirtualStudioConfigurationEditor : EditorWindow
  {
    // "None" tab correlates to RuntimeEnvironment.Native (as in nothing running in Virtual Studio),
    // so _vsModeTabSelection == (int)currRuntimeEnvironment - 1
    private static readonly string[] _modeSelectionGridStrings = { "None", "Remote", "Mock", "Playback" };
    private int _vsModeTabSelection = -1;

    [SerializeField]
    private _RemoteConfigurationEditor _remoteConfigEditor;

    [SerializeField]
    private _MockPlayConfigurationEditor _mockPlayConfigEditor;

    [SerializeField]
    private _PlaybackConfigurationEditor _playbackConfigEditor;

    private static GUIStyle _headerStyle;

    internal static GUIStyle _HeaderStyle
    {
      get
      {
        if (_headerStyle == null)
        {
          _headerStyle = new GUIStyle(EditorStyles.boldLabel);
          _headerStyle.fontSize = 18;
          _headerStyle.fixedHeight = 36;
        }

        return _headerStyle;
      }
    }

    private static GUIStyle _subHeadingStyle;

    internal static GUIStyle _SubHeadingStyle
    {
      get
      {
        if (_subHeadingStyle == null)
        {
          _subHeadingStyle = new GUIStyle(EditorStyles.boldLabel);
          _subHeadingStyle.fontSize = 14;
          _subHeadingStyle.fixedHeight = 28;
        }

        return _subHeadingStyle;
      }
    }

    private static GUIStyle _lineBreakStyle;
    internal static GUIStyle _LineBreakStyle
    {
      get
      {
        if (_lineBreakStyle == null)
        {
          _lineBreakStyle = new GUIStyle(EditorStyles.label);
          _lineBreakStyle.wordWrap = false;
        }

        return _lineBreakStyle;
      }
    }

    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Lightship/ARDK/Virtual Studio")]
    public static void Init()
    {
      var window = GetWindow<VirtualStudioConfigurationEditor>(false, "Virtual Studio");
      window.Show();

      window._mockPlayConfigEditor = new _MockPlayConfigurationEditor();
      window._remoteConfigEditor = new _RemoteConfigurationEditor();
      window._playbackConfigEditor = new _PlaybackConfigurationEditor();

      window.ApplyModeChange();
    }

    private void ApplyModeChange()
    {
      var currentRuntime = _VirtualStudioLauncher.SelectedMode;

      // Valid RuntimeEnvironment values start at 1
      _vsModeTabSelection = (int)currentRuntime - 1;

      _remoteConfigEditor.OnSelectionChange(currentRuntime == RuntimeEnvironment.Remote);
      _mockPlayConfigEditor.OnSelectionChange(currentRuntime == RuntimeEnvironment.Mock);
      _playbackConfigEditor.OnSelectionChange(currentRuntime == RuntimeEnvironment.Playback);
    }

    private void OnGUI()
    {
      using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
      {
        scrollPos = scrollView.scrollPosition;

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        DrawEnabledGUI();
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(50);

        switch (_VirtualStudioLauncher.SelectedMode)
        {
          case RuntimeEnvironment.Remote:
            EditorGUILayout.LabelField("Remote Mode - USB", _HeaderStyle);
            GUILayout.Space(10);
            _remoteConfigEditor.DrawGUI();
            break;

          case RuntimeEnvironment.Mock:
            EditorGUILayout.LabelField("Mock Mode", _HeaderStyle);
            GUILayout.Space(10);
            _mockPlayConfigEditor.DrawGUI();
            break;

          case RuntimeEnvironment.Playback:
            EditorGUILayout.LabelField("Playback Mode", _HeaderStyle);
            GUILayout.Space(10);
            _playbackConfigEditor.DrawGUI();
            break;
        }
      }
    }

    private void DrawEnabledGUI()
    {
      if (_vsModeTabSelection < 0)
        _vsModeTabSelection = (int)_VirtualStudioLauncher.SelectedMode - 1;

      var newModeSelection =
        GUI.SelectionGrid
        (
          new Rect(10, 20, 300, 20),
          _vsModeTabSelection,
          _modeSelectionGridStrings,
          4
        );

      if (newModeSelection != _vsModeTabSelection)
      {
        _VirtualStudioLauncher.SelectedMode = (RuntimeEnvironment)(newModeSelection + 1);

        // The _VirtualStudioLauncher.SelectedMode value is the source of truth, since an invalid
        // mode might have been selected through the UI
        _vsModeTabSelection = (int)_VirtualStudioLauncher.SelectedMode - 1;;
        ApplyModeChange();
      }
    }
  }
}

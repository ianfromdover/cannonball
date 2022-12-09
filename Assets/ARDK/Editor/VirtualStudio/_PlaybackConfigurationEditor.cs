using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Niantic.ARDK.Editor;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR;

using UnityEditor;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.Editor
{
  [Serializable]
  internal sealed class _PlaybackConfigurationEditor
  {
    private _PlaybackModeLauncher Launcher
    {
      get
      {
        return (_PlaybackModeLauncher)_VirtualStudioLauncher.GetOrCreateModeLauncher(RuntimeEnvironment.Playback);
      }
    }

    public void OnSelectionChange(bool isSelected)
    {
    }

    public void DrawGUI()
    {
      EditorGUILayout.LabelField("Dataset", VirtualStudioConfigurationEditor._HeaderStyle);
      GUILayout.Space(10);

      DrawDatsetGUI();
    }

    private void DrawDatsetGUI()
    {
      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.TextField("Dataset", Launcher.DatasetPath);

      if (GUILayout.Button(CommonStyles.FindIcon.ToString(), GUILayout.Width(50)))
      {
        var path = EditorUtility.OpenFolderPanel("Select Dataset Directory", Launcher.DatasetPath, "");
        if (path.Length > 0)
        {
          Launcher.DatasetPath = path;
        }
      }

      EditorGUILayout.EndHorizontal();
    }
  }
}

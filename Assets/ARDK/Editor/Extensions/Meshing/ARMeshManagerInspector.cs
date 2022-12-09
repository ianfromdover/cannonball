using System;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.Extensions.Meshing;

using UnityEditor;

namespace ARDK.Editor.Extensions.Meshing
{
  [CustomEditor(typeof(ARMeshManager))]
  public class ARMeshManagerInspector
    : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      
      ARMeshManager meshManager = (ARMeshManager)target;

      if (meshManager.SelectedMeshingMode == ARMeshManager.MeshingMode.Custom)
      {
          EditorGUILayout.HelpBox("Custom meshing range max and voxel size are untested and can have " +
            "heavy impact on performance. \nThis is an experimental feature. Experimental features " +
            "should not be used in production products as they are subject to breaking changes, " +
            "not officially supported, and may be deprecated without notice.", MessageType.Warning);
      }
    }
  }
}

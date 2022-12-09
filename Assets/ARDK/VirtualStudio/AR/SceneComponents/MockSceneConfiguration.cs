// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.Remote;

using UnityEngine;

#if UNITY_EDITOR
using Niantic.ARDK.Utilities.Extensions;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

#endif

namespace Niantic.ARDK.VirtualStudio.AR.Mock
{
  public sealed class MockSceneConfiguration:
    MonoBehaviour
  {
#if UNITY_EDITOR
    private void OnEnable()
    {
      const string layerName = _MockFrameBufferProvider.MOCK_LAYER_NAME;
      var mockLayer = LayerMask.NameToLayer(layerName);
      if (mockLayer < 0 && !_MockFrameBufferProvider.CreateLayer(layerName, out mockLayer))
        return;

      _SetLayersIfNeeded(true);
    }

    private void Reset()
    {
      _SetLayersIfNeeded(true);
    }

    [MenuItem("GameObject/3D Object/ARDK/MockScene", false, 0)]
    private static void CreateRoot(MenuCommand menuCommand)
    {
      var mockSceneRoot = new GameObject("MockSceneRoot");
      var mockScene = mockSceneRoot.AddComponent<MockSceneConfiguration>();
      mockScene._SetLayersIfNeeded();

      // Ensure it gets re-parented if this was a context click (otherwise does nothing)
      GameObjectUtility.SetParentAndAlign(mockSceneRoot, menuCommand.context as GameObject);

      // Register the creation in the undo system
      Undo.RegisterCreatedObjectUndo(mockSceneRoot, "Create " + mockSceneRoot.name);

      Selection.activeObject = mockSceneRoot;
    }

    // Sets the layer of this component's GameObject and all its descendants
    // to _MockFrameBufferProvider.MOCK_LAYER_NAME, if the layer is currently something different.
    // The method will add the mock layer to the TagManager.asset if it does not already exist.
    internal void _SetLayersIfNeeded(bool verbose = false)
    {
      const string layerName = _MockFrameBufferProvider.MOCK_LAYER_NAME;
      var layerIndex = LayerMask.NameToLayer(layerName);
      if (layerIndex < 0)
      {
        if (!_MockFrameBufferProvider.CreateLayer(layerName, out layerIndex))
          return;
      }

      var changesCount = 0;
      foreach (var descendant in gameObject.GetComponentsInChildren<Transform>())
      {
        if (descendant.gameObject.layer != layerIndex)
        {
          changesCount++;
          descendant.gameObject.layer = layerIndex;
        }
      }

      if (gameObject.layer != layerIndex)
      {
        changesCount++;
        gameObject.layer = layerIndex;
      }

      if (changesCount > 0)
      {
        var isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);

        if (verbose)
        {
          const string prefabInstMessage =
            "\nUse the Virtual Studio Window > Mock > Validate all Mock scenes button to apply fixes to the prefab.";

          ARLog._ReleaseFormat
          (
            "Changed the layers of {0} GameObject and descendents in the {1} object to Layer: {2}, " +
            "as it is required for use of Virtual Studio Mock mode. {3}",
            false,
            changesCount,
            gameObject.name,
            layerName,
            isPrefabInstance ? prefabInstMessage : ""
          );
        }

        var hasAssetPath = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(gameObject));
        if (!isPrefabInstance && hasAssetPath)
          PrefabUtility.SavePrefabAsset(gameObject.transform.root.gameObject);
      }
    }
#endif
  }
}


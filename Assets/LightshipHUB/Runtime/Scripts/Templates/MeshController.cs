// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using UnityEngine;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Mesh;
using Niantic.ARDK.Extensions.Meshing;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Logging;
using Niantic.LightshipHub.Tools;

namespace Niantic.LightshipHub.Templates
{
  public class MeshController : MonoBehaviour
  {
    [HideInInspector]
    public ARSessionManager ARSessionManager;
    [HideInInspector]
    public ARMeshManager ARMeshManager;
    [HideInInspector]
    public Material InvisibleMaterial;
    public bool ShowWorldMesh = false;
    public Material WorldMeshMaterial;
    private bool _contextAwarenessLoadComplete = false;
    private GameObject _originalMeshPrefab;
    private Material _originalMaterial;

    IEnumerator ChangeMeshMaterial()
    {
      yield return new WaitForSeconds(0.2f);

      if (WorldMeshMaterial != null) ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial = WorldMeshMaterial;
      ARMeshManager.SetUseInvisibleMaterial(!ShowWorldMesh);

      MeshMaterial meshMat = (MeshMaterial)GameObject.FindObjectOfType(typeof(MeshMaterial));
      if (meshMat != null)
      {
        if (!ShowWorldMesh)
        {
          meshMat.SetMaterialToMesh(InvisibleMaterial);
        }
        else
        {
          meshMat.SetMaterialToMesh(WorldMeshMaterial);
        }
      }
    }

    void Start()
    {
      //Following lines were on Awake function. Moved to start so as to not generate conflict with preloading. Now the coroutine starts when preloading has finished.
      var logFeatures = new string[] { "Niantic.ARDK.Extensions.Meshing", "UnityEngine.Events.UnityAction" };
      ARLog.EnableLogFeatures(logFeatures);

      _originalMeshPrefab = ARMeshManager.MeshPrefab;
      _originalMaterial = ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial;

      StartCoroutine(ChangeMeshMaterial());

      ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnDestroy()
    {
      // Set original prefab material again. If we don't do that the prefab stays with new material.
      _originalMeshPrefab.GetComponent<MeshRenderer>().sharedMaterial = _originalMaterial;
      ARSessionFactory.SessionInitialized -= OnSessionInitialized;

      if (ARSessionManager.ARSession != null) ARSessionManager.ARSession.Mesh.MeshBlocksUpdated -= OnMeshUpdated;
    }

    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      args.Session.Mesh.MeshBlocksUpdated += OnMeshUpdated;

      _contextAwarenessLoadComplete = false;
    }

    private void OnMeshUpdated(MeshBlocksUpdatedArgs args)
    {
      if (!_contextAwarenessLoadComplete)
      {
        _contextAwarenessLoadComplete = true;
      }
    }

    private void Update()
    {
      if (ARSessionManager.ARSession != null && !_contextAwarenessLoadComplete)
      {
        var status = ARSessionManager.ARSession.GetAwarenessInitializationStatus(
            out AwarenessInitializationError error,
            out string errorMessage
        );

        if (status == AwarenessInitializationStatus.Ready)
        {
          _contextAwarenessLoadComplete = true;
        }
        else if (status == AwarenessInitializationStatus.Failed)
        {
          _contextAwarenessLoadComplete = true;
          Debug.LogErrorFormat(
              "Failed to initialize Context Awareness processes. Error: {0} ({1})",
              error,
              errorMessage
          );
        }
      }
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
  public class MeshingShadersController : MonoBehaviour
  {
    [HideInInspector]
    public ARSessionManager ARSessionManager;
    [HideInInspector]
    public ARMeshManager ARMeshManager;
    public enum MeshShaders 
    { 
      Grid, 
      Waves, 
      Radar,
      Gradient
    };
    public MeshShaders MeshShader;
    public bool UseCustomMaterial;
    public Material CustomMaterial;
    public bool ShowShaderDropdownInRuntime;
    public Dropdown ShadersDropdown;
    [HideInInspector]
    public List<Material> materials;
    private List<string> allMaterials = new List<string>();
    private bool _contextAwarenessLoadComplete = false;
    private GameObject _originalMeshPrefab;
    private Material _originalMaterial;    

    void Awake()
    {
      ShadersDropdown.gameObject.SetActive(ShowShaderDropdownInRuntime);
      if (ShowShaderDropdownInRuntime)
      {
        ShadersDropdown.ClearOptions();
        allMaterials = new List<string>();
        if (UseCustomMaterial && CustomMaterial != null) allMaterials.Add(CustomMaterial.name);
        foreach (var material in materials) {
          allMaterials.Add(material.name);
        }
        ShadersDropdown.AddOptions(allMaterials);
      }
    }

    void Start()
    {
      //Following lines were on Awake function. Moved to start so as to not generate conflict with preloading. Now the coroutine starts when preloading has finished.
      var logFeatures = new string[] { "Niantic.ARDK.Extensions.Meshing", "UnityEngine.Events.UnityAction" };
      ARLog.EnableLogFeatures(logFeatures);

      _originalMeshPrefab = ARMeshManager.MeshPrefab;
      _originalMaterial = ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial;

      if (UseCustomMaterial && CustomMaterial == null) Debug.LogWarning($"If you want to use a custom material, you need to add one in the field Custom Material of the MeshingShaderController inside the ARController game object. On the contrary, uncheck the property Use Custom Material.");
      
      if (!ShowShaderDropdownInRuntime) 
        ChangeMeshMaterial();
      else
        ChangeMeshMaterialToAll();

      ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    void ChangeMeshMaterial(Material mat = null)
    {
      Material shaderMat = null;

      if (mat != null)
      {
        shaderMat = mat;
      }
      else if (UseCustomMaterial && CustomMaterial != null) 
      {
        shaderMat = CustomMaterial;
      }
      else
      {
        shaderMat = materials[(int)MeshShader];
      }

      // This changes the material of the Mesh prefab
      ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial = shaderMat;

      // This changes the material of the mock scene
      MeshMaterial meshMat = (MeshMaterial)GameObject.FindObjectOfType(typeof(MeshMaterial));
      if (meshMat != null)
      {
        meshMat.SetMaterialToMesh(shaderMat);
      }
    }

    // This changes the material on runtime
    public void ChangeMeshMaterialToAll()
    {
      Material shaderMat = null;

      if (UseCustomMaterial && CustomMaterial != null && allMaterials[ShadersDropdown.value].Equals(CustomMaterial.name)) 
      {
        shaderMat = CustomMaterial;
      }
      else
      {
        foreach (var material in materials) {
          if (allMaterials[ShadersDropdown.value].Equals(material.name)) 
          {
            shaderMat = material;
            break;
          }
        }
      }

      ChangeMeshMaterial(shaderMat);

      // This changes the material of old instantiated meshes
      foreach (var child in ARMeshManager.GetComponentsInChildren<MeshRenderer>()) 
      {
        child.sharedMaterial = shaderMat;
      }
    }

    private void OnDestroy()
    {
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

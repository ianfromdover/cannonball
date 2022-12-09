// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.LightshipHub.Tools
{
  public class MeshMaterial : MonoBehaviour
  {
    [HideInInspector]
    public GameObject MeshScene;

    void Awake()
    {
      // MeshScene.SetActive(false);
    }

    public void SetMaterialToMesh(Material material)
    {
      // MeshScene.SetActive(true);
      SetMaterialToGameObject(MeshScene, material);
    }

    void SetMaterialToGameObject(GameObject obj, Material material)
    {
      if (obj == null)
      {
#if UNITY_EDITOR
                Debug.LogWarning("Your MockScene does not have a Mesh Object Helper, to properly test this template select [MeshInteriorScene] as a mock environment in Virtual Studio.");
#endif
        return;
      }

      MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
      if (mesh != null) mesh.sharedMaterial = material;

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetMaterialToGameObject(child.gameObject, material);
      }
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;
using Niantic.ARDK.Extensions;

namespace Niantic.LightshipHub.Templates
{
  public class DepthTextureController : MonoBehaviour
  {
    [HideInInspector]
    public ARDepthManager DepthManager;
    public bool ShowDepthTexture;

    void Update()
    {
      DepthManager.ToggleDebugVisualization(ShowDepthTexture);
    }
  }
}

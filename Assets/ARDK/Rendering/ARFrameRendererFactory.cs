// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.Remote;

using UnityEngine;

namespace Niantic.ARDK.Rendering
{
  public static class ARFrameRendererFactory
  {
    // Recommended near and far clipping settings for AR rendering
    private const float DefaultNearClipping = 0.1f;
    private const float DefaultFarClipping = 100f;

    /// Create an ARFrameRenderer with the specified RuntimeEnvironment.
    /// @param env The runtime environment to create the renderer for.
    /// @returns The created renderer, or null if it was not possible to create a renderer.
    public static ARFrameRenderer Create
    (
      RenderTarget target,
      RuntimeEnvironment env,
      float nearClipping = DefaultNearClipping,
      float farClipping = DefaultFarClipping
    )
    {
      if (env == RuntimeEnvironment.Default)
        return Create(target, _VirtualStudioLauncher.SelectedMode, nearClipping, farClipping);

      ARFrameRenderer result;
      switch (env)
      {
        case RuntimeEnvironment.LiveDevice:
#if UNITY_IOS
          result = new _ARKitFrameRenderer(target, nearClipping, farClipping);
          break;
#elif UNITY_ANDROID
          result = new _ARCoreFrameRenderer(target, nearClipping, farClipping);
          break;
#else
          return null;
#endif

        case RuntimeEnvironment.Remote:
          result = new _RemoteFrameRenderer(target, nearClipping, farClipping);
          break;

        case RuntimeEnvironment.Mock:
          result = new _MockFrameRenderer(target, nearClipping, farClipping);
          break;

        case RuntimeEnvironment.Playback:
          result = new _ARPlaybackFrameRenderer(target,nearClipping, farClipping);
          break;

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, env.GetType());
      }

      return result;
    }
  }
}

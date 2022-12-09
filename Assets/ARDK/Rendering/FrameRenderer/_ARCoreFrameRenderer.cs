// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;
using UnityEngine.Rendering;

using System.Runtime.InteropServices;
using AOT;

using Object = UnityEngine.Object;

namespace Niantic.ARDK.Rendering
{
  internal sealed class _ARCoreFrameRenderer: 
    ARFrameRenderer
  {
    // Rendering resources
    private CommandBuffer _commandBuffer;
    private Texture2D _nativeTexture;
    protected override Shader Shader { get; }

    public _ARCoreFrameRenderer(RenderTarget target)
      : base(target)
    {
      // The main shader used for rendering the background
      Shader = Resources.Load<Shader>("ARCoreFrame");
    }

    public _ARCoreFrameRenderer
    (
      RenderTarget target,
      float near,
      float far,
      Shader customShader = null
    ) : base(target, near, far)
    {
      // The main shader used for rendering the background
      Shader = customShader ? customShader : Resources.Load<Shader>("ARCoreFrame");
      ARLog._Debug("Loaded: " + (Shader != null ? Shader.name : null));
    }

    protected override GraphicsFence? OnConfigurePipeline
    (
      RenderTarget target,
      Material renderMaterial
    )
    {
      _commandBuffer = new CommandBuffer
      {
        name = "ARCoreFrameRenderer"
      };
      
      AddResetOpenGLState(_commandBuffer);
      _commandBuffer.ClearRenderTarget(true, true, Color.clear);
      _commandBuffer.Blit(null, Target.Identifier, renderMaterial);

#if UNITY_2019_1_OR_NEWER
      return _commandBuffer.CreateAsyncGraphicsFence();
#else
      return _commandBuffer.CreateGPUFence();
#endif
    }

    protected override void OnAddToCamera(Camera camera)
    {
      // Take over fetching ARCore updates
      if (Session is _NativeARSession nativeARSession)
        nativeARSession.SetUpdatingCamera(camera);
      
      ARSessionBuffersHelper.AddBackgroundBuffer(camera, _commandBuffer);
    }

    protected override void OnRemoveFromCamera(Camera camera)
    {
      // Delegate fetching updates back to the session
      if (Session is _NativeARSession nativeARSession)
        nativeARSession.SetUpdatingCamera(null);
      
      ARSessionBuffersHelper.RemoveBackgroundBuffer(camera, _commandBuffer);
    }

    protected override bool OnUpdateState
    (
      IARFrame frame,
      Matrix4x4 projectionTransform,
      Matrix4x4 displayTransform,
      Material material
    )
    {
      // We require a single plane image as source
      if (frame.CapturedImageTextures.Length < 1 || frame.CapturedImageTextures[0] == IntPtr.Zero)
        return false;

      // Update the native texture
      CreateOrUpdateExternalTexture
      (
        ref _nativeTexture,
        frame.Camera.ImageResolution,
        TextureFormat.ARGB32,
        frame.CapturedImageTextures[0]
      );

      // Bind texture and the display transform
      material.SetTexture(PropertyBindings.FullImage, _nativeTexture);
      material.SetMatrix(PropertyBindings.DisplayTransform, displayTransform);

      return true;
    }

    protected override void OnIssueCommands()
    {
      Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    protected override void OnRelease()
    {
      _commandBuffer?.Dispose();

      if (_nativeTexture != null)
        Object.Destroy(_nativeTexture);
    }
    
    // Does nothing but returning from an IssuePluginEvent has the effect of Unity resetting all 
    // the OpenGL states to known values
    [MonoPInvokeCallback(typeof(Action<int>))]
    static void ResetGlState(int eventId) {}
    static Action<int> s_ResetGlStateDelegate = ResetGlState;
    static readonly IntPtr s_ResetGlStateFuncPtr = Marshal.GetFunctionPointerForDelegate(s_ResetGlStateDelegate);
    private static void AddResetOpenGLState(CommandBuffer commandBuffer)
    {
      commandBuffer.IssuePluginEvent(s_ResetGlStateFuncPtr, 0);
    } 
  }
}
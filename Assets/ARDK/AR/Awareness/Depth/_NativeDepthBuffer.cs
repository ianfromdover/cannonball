// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.Internals;

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Camera;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness.Depth
{
  internal sealed class _NativeDepthBuffer:
    _NativeAwarenessBufferBase<float>,
    IDepthBuffer
  {
    static _NativeDepthBuffer()
    {
      _Platform.Init();
    }

    internal _NativeDepthBuffer(IntPtr nativeHandle, float worldScale, CameraIntrinsics intrinsics)
      : base
      (
        nativeHandle,
        worldScale,
        GetNativeWidth(nativeHandle),
        GetNativeHeight(nativeHandle),
        IsNativeKeyframe(nativeHandle),
        intrinsics
      )
    {
    }

    public float NearDistance
    {
      get
      {
        return _DepthBuffer_GetNearDistance(_nativeHandle);
      }
    }

    public float FarDistance
    {
      get
      {
        return _DepthBuffer_GetFarDistance(_nativeHandle);
      }
    }

    public override IAwarenessBuffer GetCopy()
    {
      var newHandle = _DepthBuffer_GetCopy(_nativeHandle);
      return new _NativeDepthBuffer(newHandle, _worldScale, Intrinsics);
    }

    public IDepthBuffer RotateToScreenOrientation()
    {
      var newHandle = _DepthBuffer_RotateToScreenOrientation(_nativeHandle);
      return new _NativeDepthBuffer(newHandle, _worldScale, Intrinsics);
    }

    public IDepthBuffer Interpolate
    (
      IARCamera arCamera,
      int viewportWidth,
      int viewportHeight,
      float backProjectionDistance = AwarenessParameters.DefaultBackProjectionDistance
    )
    {
      var projectionMatrix =
        arCamera.CalculateProjectionMatrix
        (
          Screen.orientation,
          viewportWidth,
          viewportHeight,
          NearDistance,
          FarDistance
        );

      var frameViewMatrix = arCamera.GetViewMatrix(Screen.orientation);
      var nativeProjectionMatrix = _UnityMatrixToNarArray(projectionMatrix);
      var nativeFrameViewMatrix = _UnityMatrixToNarArray(frameViewMatrix);

      var newHandle =
        _DepthBuffer_Interpolate
        (
          _nativeHandle,
          nativeProjectionMatrix,
          nativeFrameViewMatrix,
          backProjectionDistance
        );

      return new _NativeDepthBuffer(newHandle, _worldScale, Intrinsics);
    }

    public IDepthBuffer FitToViewport
    (
      int viewportWidth,
      int viewportHeight
    )
    {
      var newHandle =
        _DepthBuffer_FitToViewport
        (
          _nativeHandle,
          viewportWidth,
          viewportHeight
        );

      return new _NativeDepthBuffer(newHandle, _worldScale, Intrinsics);
    }

    public bool CreateOrUpdateTextureARGB32
    (
      ref Texture2D texture,
      FilterMode filterMode = FilterMode.Point,
      Func<float, float> valueConverter = null
    )
    {
      return _AwarenessBufferHelper._CreateOrUpdateTextureARGB32
      (
        Data,
        (int)Width,
        (int)Height,
        ref texture,
        filterMode,
        valueConverter
      );
    }

    public bool CreateOrUpdateTextureRFloat
    (
      ref Texture2D texture,
      FilterMode filterMode = FilterMode.Point
    )
    {
      return _AwarenessBufferHelper._CreateOrUpdateTextureRFloat
      (
        Data,
        (int)Width,
        (int)Height,
        ref texture,
        filterMode
      );
    }

    protected override void _GetViewMatrix(float[] outViewMatrix)
    {
      _DepthBuffer_GetView(_nativeHandle, outViewMatrix);
    }

    protected override void _GetIntrinsics(float[] outVector)
    {
      _DepthBuffer_GetIntrinsics(_nativeHandle, outVector);
    }

    protected override void _OnRelease()
    {
      _DepthBuffer_Release(_nativeHandle);
    }

    protected override IntPtr _GetDataAddress()
    {
      return _DepthBuffer_GetDataAddress(_nativeHandle);
    }

    private static uint GetNativeWidth(IntPtr nativeHandle)
    {
      return _DepthBuffer_GetWidth(nativeHandle);
    }

    private static uint GetNativeHeight(IntPtr nativeHandle)
    {
      return _DepthBuffer_GetHeight(nativeHandle);
    }


    private static bool IsNativeKeyframe(IntPtr nativeHandle)
    {
      return _DepthBuffer_IsKeyframe(nativeHandle);
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _DepthBuffer_GetWidth(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _DepthBuffer_GetHeight(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _DepthBuffer_IsKeyframe(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _DepthBuffer_GetView(IntPtr nativeHandle, float[] outViewMatrix);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _DepthBuffer_GetIntrinsics(IntPtr nativeHandle, float[] outVector);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_GetDataAddress(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _DepthBuffer_GetNearDistance(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _DepthBuffer_GetFarDistance(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_GetCopy(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_RotateToScreenOrientation(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_Interpolate
    (
      IntPtr nativeHandle,
      float[] nativeProjectionMatrix,
      float[] nativeFrameViewMatrix,
      float backProjectionDistance
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _DepthBuffer_FitToViewport
    (
      IntPtr nativeHandle,
      int viewportWidth,
      int viewportHeight
    );
  }
}

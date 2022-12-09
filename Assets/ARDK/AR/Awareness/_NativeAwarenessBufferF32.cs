using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Camera;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  internal sealed class _NativeAwarenessBufferF32
    : _NativeAwarenessBufferBase<float>,
      IDataBufferFloat32
  {
    static _NativeAwarenessBufferF32()
    {
      _Platform.Init();
    }

    internal _NativeAwarenessBufferF32
      (IntPtr nativeHandle, float worldScale, CameraIntrinsics intrinsics)
      : base
      (
        nativeHandle,
        worldScale,
        GetNativeWidth(nativeHandle),
        GetNativeHeight(nativeHandle),
        isKeyframe: true,
        intrinsics
      )
    {
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

    private static uint GetNativeWidth(IntPtr nativeHandle)
    {
      return _AwarenessBufferF32_GetWidth(nativeHandle);
    }

    private static uint GetNativeHeight(IntPtr nativeHandle)
    {
      return _AwarenessBufferF32_GetHeight(nativeHandle);
    }

    public override IAwarenessBuffer GetCopy()
    {
      var newHandle = _AwarenessBufferF32_GetCopy(_nativeHandle);
      return new _NativeAwarenessBufferF32(newHandle, _worldScale, Intrinsics);
    }

    protected override void _GetViewMatrix(float[] outViewMatrix)
    {
      _AwarenessBufferF32_GetView(_nativeHandle, outViewMatrix);
    }

    protected override void _GetIntrinsics(float[] outVector)
    {
      _AwarenessBufferF32_GetIntrinsics(_nativeHandle, outVector);
    }

    protected override IntPtr _GetDataAddress()
    {
      return _AwarenessBufferF32_GetDataAddress(_nativeHandle);
    }

    protected override void _OnRelease()
    {
      _AwarenessBufferF32_Release(_nativeHandle);
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _AwarenessBufferF32_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _AwarenessBufferF32_GetWidth(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _AwarenessBufferF32_GetHeight(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _AwarenessBufferF32_GetView
      (IntPtr nativeHandle, float[] outViewMatrix);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _AwarenessBufferF32_GetIntrinsics
      (IntPtr nativeHandle, float[] outVector);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _AwarenessBufferF32_GetDataAddress(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _AwarenessBufferF32_GetCopy(IntPtr nativeHandle);
  }
}

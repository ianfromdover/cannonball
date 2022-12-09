// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


namespace Niantic.ARDK.AR.Image
{
  internal sealed class _NativeImagePlane:
    IImagePlane
  {
    private readonly IntPtr _nativeHandle;
    internal readonly UInt64 _planeIndex;

    private NativeArray<byte> _data;
#if UNITY_EDITOR
    private AtomicSafetyHandle? _safetyHandle;
#endif

    internal _NativeImagePlane(IntPtr nativeHandle, int planeIndex)
    {
      _NativeAccess.AssertNativeAccessValid();

      _nativeHandle = nativeHandle;
      _planeIndex = (UInt64)planeIndex;
    }

    public int PixelWidth
    {
      get
      {
        return _NARImage_GetWidthOfPlane(_nativeHandle, _planeIndex);
      }
    }

    public int PixelHeight
    {
      get
      {
        return _NARImage_GetHeightOfPlane(_nativeHandle, _planeIndex);
      }
    }

    public int BytesPerRow
    {
      get
      {
        return checked((int)_NARImage_GetBytesPerRowOfPlane(_nativeHandle, _planeIndex));
      }
    }

    public int BytesPerPixel
    {
      get
      {
        return checked((int)_NARImage_GetBytesPerPixelOfPlane(_nativeHandle, _planeIndex));
      }
    }


    public NativeArray<byte> Data
    {
      get
      {
        unsafe
        {
          if (!_data.IsCreated)
          {
            var dataAddress = _GetBaseDataAddress().ToPointer();
            var length = BytesPerRow * PixelHeight;

            _data =
              NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>
              (
                dataAddress,
                length,
                Allocator.None
              );

#if UNITY_EDITOR
            _safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref _data, _safetyHandle.Value);
#endif
          }

          return _data;
        }
      }
    }

    public void Dispose()
    {
      if (_data.IsCreated)
      {
        _data.Dispose();

#if UNITY_EDITOR
        if (_safetyHandle.HasValue)
          AtomicSafetyHandle.Release(_safetyHandle.Value);

        _safetyHandle = null;
#endif
      }
    }

    private IntPtr _GetBaseDataAddress()
    {
      return _NARImage_GetBaseAddressOfPlane(_nativeHandle, _planeIndex);
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARImage_GetBaseAddressOfPlane
    (
      IntPtr nativeHandle,
      UInt64 planeIndex
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NARImage_GetWidthOfPlane(IntPtr nativeHandle, UInt64 planeIndex);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NARImage_GetHeightOfPlane(IntPtr nativeHandle, UInt64 planeIndex);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt64 _NARImage_GetBytesPerRowOfPlane
    (
      IntPtr nativeHandle,
      UInt64 planeIndex
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt64 _NARImage_GetBytesPerPixelOfPlane
    (
      IntPtr nativeHandle,
      UInt64 planeIndex
    );
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Camera;
using Niantic.ARDK.Utilities;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  internal abstract class _NativeAwarenessBufferBase<T>:
    _AwarenessBufferBase,
    IDataBuffer<T>
  where T: struct
  {
    protected readonly float _worldScale = 0;
    protected IntPtr _nativeHandle;

    private long _consumedUnmanagedMemory;
    private Matrix4x4? _cacheViewMatrix = null;
    private CameraIntrinsics? _cachedIntrinsics = null;

    private NativeArray<T> _data;
#if UNITY_EDITOR
    private AtomicSafetyHandle? _safetyHandle;
#endif

    protected _NativeAwarenessBufferBase
    (
      IntPtr handle,
      float worldScale,
      UInt32 width,
      UInt32 height,
      bool isKeyframe,
      CameraIntrinsics intrinsics
    )
      : base(width, height, isKeyframe, intrinsics)
    {
      _NativeAccess.AssertNativeAccessValid();

      _worldScale = worldScale;
      _nativeHandle = handle;

      _consumedUnmanagedMemory = _CalculateConsumedMemory();
      GC.AddMemoryPressure(_consumedUnmanagedMemory);
    }

    ~_NativeAwarenessBufferBase()
    {
      Dispose();
    }

    public NativeArray<T> Data
    {
      get
      {
        unsafe
        {
          if (!_data.IsCreated)
          {
            UInt32 size = Width * Height;
            _data =
              NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>
              (
                _GetDataAddress().ToPointer(),
                (int)size,
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

    public T Sample(Vector2 uv)
    {
      var w = (int)Width;
      var h = (int)Height;

      var x = Mathf.Clamp(Mathf.RoundToInt(uv.x * w - 0.5f), 0, w - 1);
      var y = Mathf.Clamp(Mathf.RoundToInt(uv.y * h - 0.5f), 0, h - 1);

      return Data[x + w * y];
    }

    public T Sample(Vector2 uv, Matrix4x4 transform)
    {
      var w = (int)Width;
      var h = (int)Height;

      var st = transform * new Vector4(uv.x, uv.y, 1.0f, 1.0f);
      var sx = st.x / st.z;
      var sy = st.y / st.z;

      var x = Mathf.Clamp(Mathf.RoundToInt(sx * w - 0.5f), 0, w - 1);
      var y = Mathf.Clamp(Mathf.RoundToInt(sy * h - 0.5f), 0, h - 1);

      return Data[x + w * y];
    }

    public override Matrix4x4 ViewMatrix
    {
      get
      {
        if (_cacheViewMatrix == null)
        {
          var nativeMatrix = new float[16];
          _GetViewMatrix(nativeMatrix);

          var viewMatrix = NARConversions.FromNARToUnity
            (_Convert.InternalToMatrix4x4(nativeMatrix));

          _Convert.ApplyScale(ref viewMatrix, _worldScale);
          _cacheViewMatrix = viewMatrix;
        }

        return _cacheViewMatrix ?? Matrix4x4.identity;
      }
    }

    public override CameraIntrinsics Intrinsics
    {
      get
      {
        if (!_cachedIntrinsics.HasValue)
        {
          var nativeVector = new float[4];
          _GetIntrinsics(nativeVector);

          _cachedIntrinsics =
            new CameraIntrinsics
            (
              nativeVector[0],
              nativeVector[1],
              nativeVector[2],
              nativeVector[3]
            );
        }

        return _cachedIntrinsics.Value;
      }
    }

    protected abstract void _GetViewMatrix(float[] outViewMatrix);

    protected abstract void _GetIntrinsics(float[] outVector);

    protected abstract IntPtr _GetDataAddress();

    protected abstract void _OnRelease();

    protected float[] _UnityMatrixToNarArray(Matrix4x4 matrix)
    {
      return _Convert.Matrix4x4ToInternalArray(NARConversions.FromUnityToNAR(matrix));
    }

    public void Dispose()
    {
      if (_nativeHandle != IntPtr.Zero)
      {
        _OnRelease();
        GC.SuppressFinalize(this);
        GC.RemoveMemoryPressure(_consumedUnmanagedMemory);
        _nativeHandle = IntPtr.Zero;

#if UNITY_EDITOR
        if (_safetyHandle.HasValue)
          AtomicSafetyHandle.Release(_safetyHandle.Value);

        _safetyHandle = null;
#endif
      }
    }

    private long _CalculateConsumedMemory()
    {
      return Width * Height * Marshal.SizeOf(typeof(T));
    }
  }
}

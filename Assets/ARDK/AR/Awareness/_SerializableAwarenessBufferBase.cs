// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Camera;

using Unity.Collections;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  // Can't use [Serializable]. Need to provide a serializer.
  internal abstract class _SerializableAwarenessBufferBase<T>
    : _AwarenessBufferBase,
      IDataBuffer<T>
  where T: struct
  {
    private readonly Matrix4x4 _viewMatrix;

    private bool _disposed;
    private readonly long _consumedUnmanagedMemory;

    private readonly CameraIntrinsics _intrinsics;

    internal _SerializableAwarenessBufferBase
    (
      uint width,
      uint height,
      bool isKeyframe,
      Matrix4x4 viewMatrix,
      NativeArray<T> data,
      CameraIntrinsics intrinsics
    )
      : base(width, height, isKeyframe, intrinsics)
    {
      _viewMatrix = viewMatrix;
      _intrinsics = intrinsics;
      Data = data;

      _consumedUnmanagedMemory = _CalculateConsumedMemory();
      GC.AddMemoryPressure(_consumedUnmanagedMemory);
    }

    ~_SerializableAwarenessBufferBase()
    {
      Dispose();
    }

    public override Matrix4x4 ViewMatrix
    {
      get
      {
        return _viewMatrix;
      }
    }

    public override CameraIntrinsics Intrinsics
    {
      get
      {
        return _intrinsics;
      }
    }

    public NativeArray<T> Data { get; }

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

    public bool IsRotatedToScreenOrientation { get; set; }

    public void Dispose()
    {
      if (_disposed)
        return;

      if (Data.IsCreated)
        Data.Dispose();

      GC.SuppressFinalize(this);
      GC.RemoveMemoryPressure(_consumedUnmanagedMemory);
      _disposed = true;
    }

    private long _CalculateConsumedMemory()
    {
      return Width * Height * Marshal.SizeOf(typeof(T));
    }
  }
}

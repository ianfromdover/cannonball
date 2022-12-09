using System;

using Niantic.ARDK.AR.Camera;

using Unity.Collections;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  internal sealed class _SerializeableAwarenessBufferF32
    : _SerializableAwarenessBufferBase<float>,
      IDataBufferFloat32
  {
    public _SerializeableAwarenessBufferF32
    (
      uint width,
      uint height,
      bool isKeyframe,
      Matrix4x4 viewMatrix,
      NativeArray<float> data,
      CameraIntrinsics intrinsics
    )
      : base(width, height, isKeyframe, viewMatrix, data, intrinsics)
    {
    }

    public override IAwarenessBuffer GetCopy()
    {
      return new _SerializeableAwarenessBufferF32
      (
        Width,
        Height,
        false,
        ViewMatrix,
        new NativeArray<float>(Data, Allocator.Persistent),
        Intrinsics
      );
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
      (ref Texture2D texture, FilterMode filterMode = FilterMode.Point)
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
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Unity.Collections;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  public interface IDataBuffer<T> : IAwarenessBuffer, IDisposable
  where T: struct
  {
    /// Raw data of this buffer.
    NativeArray<T> Data { get; }

    /// Returns the nearest value to the specified normalized coordinates in the buffer.
    /// @param uv
    ///   Normalized coordinates.
    /// @returns
    ///   The value in the semantic buffer at the nearest location to the coordinates.
    T Sample(Vector2 uv);

    /// Returns the nearest value to the specified normalized coordinates in the buffer.
    /// @param uv
    ///   Normalized coordinates.
    /// @param transform
    ///   2D transformation applied to normalized coordinates before sampling.
    ///   This transformation should convert to the depth buffer's coordinate frame.
    /// @returns
    ///   The value in the semantic buffer at the nearest location to the
    ///   transformed coordinates.
    T Sample(Vector2 uv, Matrix4x4 transform);
  }
}

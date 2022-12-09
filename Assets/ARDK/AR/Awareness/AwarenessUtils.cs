// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  /// A collection of utility methods for working with contextual awareness buffers.
  public static class AwarenessUtils
  {
    /// Converts pixel coordinates from the raw awareness buffer's
    /// coordinate frame to viewport pixel coordinates.
    /// @param processor Reference to the context awareness processor.
    /// @param x Awareness buffer pixel position on the x axis.
    /// @param y Awareness buffer pixel position on the y axis.
    /// @returns Pixel coordinates on the viewport.
    public static Vector2Int FromBufferToScreenPosition<TBuffer>
    (
      AwarenessBufferProcessor<TBuffer> processor,
      int x,
      int y
    ) where TBuffer : class, IDisposable, IAwarenessBuffer
    {
      // Acquire the buffer resolution
      var buffer = processor.AwarenessBuffer;
      var bufferWidth = buffer.Width;
      var bufferHeight = buffer.Height;

      // Acquire the viewport resolution
      var viewport = processor.CurrentViewportResolution;
      var viewWidth = viewport.x;
      var viewHeight = viewport.y;

      // The sampler transform takes from viewport to buffer,
      // so we need to invert it to go the other way around
      var transform = processor.SamplerTransform.inverse;

      // Get normalized buffer coordinates
      var uv = new Vector4
      (
        Mathf.Clamp((float)x / bufferWidth, 0.0f, 1.0f),
        Mathf.Clamp((float)y / bufferHeight, 0.0f, 1.0f),
        1.0f,
        1.0f
      );

      // Apply transform
      var st = transform * uv;
      var sx = st.x / st.z;
      var sy = st.y / st.z;

      // Scale result to viewport
      return new Vector2Int
      (
        x: Mathf.Clamp(Mathf.RoundToInt(sx * viewWidth - 0.5f), 0, viewWidth - 1),
        y: Mathf.Clamp(Mathf.RoundToInt(sy * viewHeight - 0.5f), 0, viewHeight - 1)
      );
    }
    
    /// Extracts raw semantic confidences from the frame and uploads it to
    /// GPU memory in the form of a texture. If the frame does not hold
    /// semantics data, this API does nothing.
    /// @param frame The AR frame to get semantics data from.
    /// @param channelName The name of the semantic class to create a texture of.
    /// @param viewportWidth The width of the viewport the texture needs to be mapped to.
    /// @param viewportHeight The height of the viewport the texture needs to be mapped to.
    /// @param texture The texture to copy the data to. If this reference is null, the
    ///   texture will be allocated. Deallocating the texture is the responsibility of
    ///   the caller.
    /// @param samplerTransform The transformation matrix to be used to sample the texture.
    /// @returns Whether the texture has been successfully created/updated.
    public static bool CopySemanticConfidencesARGB32
    (
      this IARFrame frame,
      string channelName,
      int viewportWidth,
      int viewportHeight,
      ref Texture2D texture,
      out Matrix4x4 samplerTransform
    )
    {
      return CopySemanticConfidences
      (
        frame,
        channelName,
        viewportWidth,
        viewportHeight,
        ref texture,
        out samplerTransform,
        true
      );
    }
    
    /// Extracts raw semantic confidences from the frame and uploads it to
    /// GPU memory in the form of a texture. If the frame does not hold
    /// semantics data, this API does nothing.
    /// @param frame The AR frame to get semantics data from.
    /// @param channelName The name of the semantic class to create a texture of.
    /// @param viewportWidth The width of the viewport the texture needs to be mapped to.
    /// @param viewportHeight The height of the viewport the texture needs to be mapped to.
    /// @param texture The texture to copy the data to. If this reference is null, the
    ///   texture will be allocated. Deallocating the texture is the responsibility of
    ///   the caller.
    /// @param samplerTransform The transformation matrix to be used to sample the texture.
    /// @returns Whether the texture has been successfully created/updated.
    public static bool CopySemanticConfidencesRFloat
    (
      this IARFrame frame,
      string channelName,
      int viewportWidth,
      int viewportHeight,
      ref Texture2D texture,
      out Matrix4x4 samplerTransform
    )
    {
      return CopySemanticConfidences
      (
        frame,
        channelName,
        viewportWidth,
        viewportHeight,
        ref texture,
        out samplerTransform,
        false
      );
    }

    private static bool CopySemanticConfidences
    (
      IARFrame frame,
      string channelName,
      int viewportWidth,
      int viewportHeight,
      ref Texture2D texture,
      out Matrix4x4 samplerTransform,
      bool colorFormat
    )
    {
      // Defaults
      samplerTransform = Matrix4x4.identity;

      var buffer = frame.CopySemanticConfidences(channelName);
      if (buffer == null)
        return false;

      // Get the screen orientation
      var orientation = MathUtils.CalculateScreenOrientation();
      
      // Calculate display transform
      var displayTransform = buffer.CalculateDisplayTransform
      (
        viewportWidth,
        viewportHeight,
        orientation,
        invertVertically: true
      );

      // Calculate the interpolation transform,
      var interpolationTransform = buffer.CalculateInterpolationTransform
      (
        frame.Camera,
        orientation
      );

      // Calculate the final sampler transform
      samplerTransform = interpolationTransform * displayTransform;
      
      // Push to GPU
      var success = colorFormat
        ? buffer.CreateOrUpdateTextureARGB32(ref texture)
        : buffer.CreateOrUpdateTextureRFloat(ref texture);

      // Release CPU buffer
      buffer.Dispose();
      return success;
    }
  }
}

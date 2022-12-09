using System;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  public interface IDataBufferFloat32: IDataBuffer<float>
  {
    /// Update (or create, if needed) a texture with this buffer's data.
    /// @param texture
    ///   Reference to the texture to copy to. This method will create a texture if the reference
    ///   is null.
    /// @param valueConverter
    ///   Defines a function to perform additional processing on the values before pushing
    ///   to the GPU. This is usually used to normalize values for ARGB32 textures.
    /// @returns True if the buffer was successfully copied to the given texture.
    bool CreateOrUpdateTextureARGB32
    (
      ref Texture2D texture,
      FilterMode filterMode = FilterMode.Point,
      Func<float, float> valueConverter = null
    );

    /// Update (or create, if needed) a texture with this buffer's data.
    /// @param texture
    ///   Reference to the texture to copy to. This method will create a texture if the reference
    ///   is null.
    /// @returns True if the buffer was successfully copied to the given texture.
    bool CreateOrUpdateTextureRFloat
    (
      ref Texture2D texture,
      FilterMode filterMode = FilterMode.Point
    );
  }
}

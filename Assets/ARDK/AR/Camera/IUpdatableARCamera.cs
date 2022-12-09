using UnityEngine;

namespace Niantic.ARDK.AR.Camera
{
  /// API to provide tooling for updating the native plugin's understanding of Unity's
  /// current screen orientation and resolution.
  /// @note This will be moved to the IARCamera API in a future release.
  public interface IUpdatableARCamera : 
    IARCamera
  {
    /// @brief Update the display geometry when orientation has changed. This function needs to be called
    /// before retrieving view/displayTransform/projection matrices on Android.
    /// @param orientation The current orientation of the interface.
    /// @param viewportWidth Viewport width, in pixels.
    /// @param viewportHeight Viewport height, in pixels.
    void UpdateDisplayGeometry
    (
      ScreenOrientation orientation,
      int viewportWidth,
      int viewportHeight
    );
  }
}

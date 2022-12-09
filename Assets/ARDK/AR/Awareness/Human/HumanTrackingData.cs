using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class HumanTrackingData
  {
    // Transform that converts normalized coordinates from AR image to viewport
    private readonly Matrix4x4 _viewportTransform;

    // Holds current cache of aligned detections
    private IReadOnlyList<Detection> _alignedDetections;

    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HumanTrackingData(IReadOnlyList<Detection> detections, Matrix4x4 viewportTransform)
    {
      Detections = detections?.Count > 0 ? detections : new List<Detection>();
      _viewportTransform = viewportTransform;
    }

    /// The original raw detections in the AR image's coordinate frame
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public readonly IReadOnlyList<Detection> Detections;

    /// Detections aligned with display orientation and viewport
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public IReadOnlyList<Detection> AlignedDetections
    {
      get
      {
        if (_alignedDetections != null)
          return _alignedDetections;

        _alignedDetections = Detections.Select
          (
            entry => new Detection
            {
              Confidence = entry.Confidence,
              Rect = FromImageToViewport(entry.Rect, _viewportTransform)
            }
          )
          .ToList();

        return _alignedDetections;
      }
    }

    // Converts rectangle in camera oriented coordinated space to device viewport coordinate space
    // Corrects the rect from the camera space that usually has the image 90deg and not scaled to
    // the screen space
    private static Rect FromImageToViewport(Rect imageRect, Matrix4x4 transform)
    {
      var topLeft = new Vector2(imageRect.xMin, imageRect.yMin);
      var topLeftTransformed = FromImageToViewport(topLeft, transform);

      var bottomRight = new Vector2(imageRect.xMax, imageRect.yMax);
      var bottomRightTransformed = FromImageToViewport(bottomRight, transform);

      var xMin = Mathf.Min(topLeftTransformed.x, bottomRightTransformed.x);
      var xMax = Mathf.Max(topLeftTransformed.x, bottomRightTransformed.x);

      var yMin = Mathf.Min(topLeftTransformed.y, bottomRightTransformed.y);
      var yMax = Mathf.Max(topLeftTransformed.y, bottomRightTransformed.y);

      return new Rect
      (
        position: new Vector2(xMin, yMin),
        size: new Vector2(xMax - xMin, yMax - yMin)
      );
    }

    private static Vector2 FromImageToViewport
    (
      Vector2 imageCoordinates,
      Matrix4x4 transform
    )
    {
      // Normalize image coordinates
      var uv = new Vector4
        (imageCoordinates.x, imageCoordinates.y, 1.0f, 1.0f);

      // Apply transform
      var st = transform * uv;
      var sx = st.x / st.z;
      var sy = st.y / st.z;
      return new Vector2(sx, sy);
    }
  }
}

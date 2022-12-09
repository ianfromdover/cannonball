using System.Runtime.InteropServices;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness
{
  /// Detection rectangle and confidence
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  [StructLayout(LayoutKind.Sequential)]
  public struct Detection
  {
    /// Position x of detection rectangle
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public float X;

    /// Position y of detection rectangle
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public float Y;

    /// Width of detection rectangle
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public float Width;

    /// Height of detection rectangle
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public float Height;

    /// Confidence level between 0 to 1 where 0 is uncertain and 1 is 100% confident it is found
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public float Confidence;

    /// Rectangle representing the position of the detection.
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public Rect Rect
    {
      get
      {
        return new Rect(X, Y, Width, Height);
      }

      set
      {
        X = value.x;
        Y = value.y;
        Width = value.width;
        Height = value.height;
      }
    }
  }
}

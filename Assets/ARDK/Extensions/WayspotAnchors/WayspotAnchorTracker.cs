using Niantic.ARDK.AR.WayspotAnchors;

using UnityEngine;

namespace Niantic.ARDK.Extensions
{
  /// Add this component to a GameObject and call the AttachAnchor(anchor) method
  /// to have the GameObject's transform automatically moved to match
  /// the transform of its WayspotAnchor.
  ///
  /// @note
  ///   Extend this class and override its' virtual methods to implement
  ///   custom behavior. For an example, see the ColorChangingTracker class in the
  ///   ARDK-Examples project (part of the WayspotAnchors scene).
  public class WayspotAnchorTracker: MonoBehaviour
  {
    public IWayspotAnchor WayspotAnchor { get; private set; }

    /// Link an anchor to automatically surface the anchor's
    /// status code and transform updates through this component.
    public void AttachAnchor(IWayspotAnchor anchor)
    {
      WayspotAnchor = anchor;
      WayspotAnchor.TransformUpdated += OnTransformUpdated;
      WayspotAnchor.StatusCodeUpdated += OnStatusCodeUpdated;

      OnAnchorAttached();
    }

    /// Invoked when a WayspotAnchor is attached through thee AttachAnchor method.
    protected virtual void OnAnchorAttached()
    {

    }

    /// Invoked when the Monobehaviour is destroyed. When overriding this method,
    /// make sure to call base.OnDestroy to ensure proper cleanup of this class.
    protected virtual void OnDestroy()
    {
      if (WayspotAnchor != null)
      {
        WayspotAnchor.TransformUpdated -= OnTransformUpdated;
        WayspotAnchor.StatusCodeUpdated -= OnStatusCodeUpdated;
        WayspotAnchor = null;
      }
    }

    /// If a WayspotAnchor has been attached through the AttachAnchor method,
    /// this method is invoked when the WayspotAnchor's status code is updated.
    protected virtual void OnStatusCodeUpdated(WayspotAnchorStatusUpdate args)
    {

    }

    /// If a WayspotAnchor has been attached through the AttachAnchor method,
    /// this method is invoked when the WayspotAnchor's transform is updated.
    protected virtual void OnTransformUpdated(WayspotAnchorResolvedArgs args)
    {
      transform.SetPositionAndRotation(args.Position, args.Rotation);
    }
  }
}

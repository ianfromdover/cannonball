// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal interface _IInternalTrackable
  {
    /// Sets whether or not the anchor should be tracked
    void SetTrackingEnabled(bool tracking);

    void SetTransform(Vector3 position, Quaternion rotation);

    void SetStatusCode(WayspotAnchorStatusCode statusCode);
  }
}

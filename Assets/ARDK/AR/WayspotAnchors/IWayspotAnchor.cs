// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  public interface IWayspotAnchor:
    IDisposable
  {
    /// Called when the status, position, or rotation of the wayspot anchor has been updated
    /// @note
    ///   This is only surfaced automatically when using the WayspotAnchorService.
    ///   It needs to be invoked in your application code when using the WayspotAnchorController.
    [Obsolete("Subscribe to TransformUpdated event instead")]
    ArdkEventHandler<WayspotAnchorResolvedArgs> TrackingStateUpdated { get; set; }

    event ArdkEventHandler<WayspotAnchorResolvedArgs> TransformUpdated;
    event ArdkEventHandler<WayspotAnchorStatusUpdate> StatusCodeUpdated;

    /// Gets the ID of the wayspot anchor
    Guid ID { get; }

    /// Gets the payload for the wayspot anchor. Only valid if the anchor is created (i.e. the
    /// value of Status is Success or Limited).
    WayspotAnchorPayload Payload { get; }

    /// Whether or not the wayspot anchor is currently being tracked
    bool Tracking { get; }

    WayspotAnchorStatusCode Status { get; }

    /// Current position of the anchor. Only valid if the anchor has resolved (i.e. the
    /// value of Status is Success or Limited).
    Vector3 LastKnownPosition { get; }

    /// Current rotation of the anchor. Only valid if the anchor has resolved (i.e. the
    /// value of Status is Success or Limited).
    Quaternion LastKnownRotation { get; }
  }
}

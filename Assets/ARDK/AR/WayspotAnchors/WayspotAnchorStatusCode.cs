// Copyright 2022 Niantic, Inc. All Rights Reserved.
namespace Niantic.ARDK.AR.WayspotAnchors
{
  /// The enum for the wayspot anchor status codes
  public enum WayspotAnchorStatusCode
  {
    /// Anchor has not yet been created or restored using VPS
    Pending = 0,

    /// Anchor creation or restoration was successful using VPS
    Success = 1,

    /// Anchor creation or restoration failed because:
    /// (1) Creation/resolution was attempted before starting VPS
    /// (2) the Wayspot the anchor was created at is too far away
    /// (3) the payload is corrupted
    Failed = 2,

    /// Anchor cannot be restored because it has expired.
    Invalid = 3,

    /// Anchor creation or restoration was successful but using GPS instead of VPS
    Limited = 4
  }
}

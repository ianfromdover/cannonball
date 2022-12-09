// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.Utilities;

namespace Niantic.ARDK.AR.Awareness
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct HumanTrackingArgs:
    IArdkEventArgs
  {
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HumanTrackingArgs(HumanTrackingData trackingData):
      this()
    {
      TrackingData = trackingData;
    }

    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HumanTrackingData TrackingData { get; }
  }
}

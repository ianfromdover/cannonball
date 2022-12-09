// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  /// Configuration information for a VPS localization attempt.
  public interface IWayspotAnchorsConfiguration:
    IDisposable
  {
    /// The timeout in seconds for the entire localization attempt. An attempt will send
    /// localization requests until the localization succeeds, times out, or is canceled.
    /// A value of -1 indicates no timeout.
    /// @note The default value is 30
    float LocalizationTimeout { get; set; }

    /// The timeout in seconds for an individual request made during the overall localization attempt.
    /// A value of -1 represents no timeout.
    /// @note The default value is 10
    float RequestTimeLimit { get; set; }

    /// The max number of network requests per second
    /// @note The default and maximum value is 1.0
    float RequestsPerSecond { get; set; }

    /// The max number of wayspot anchor resolutions per second.
    /// A value of -1 indicates no maximum (resolve as frequent as possible).
    /// @note The default value is 1
    float MaxResolutionsPerSecond { get; set; }

    /// The number of seconds that the system is required to wait after
    /// entering a good tracking state to start running.
    /// @note The default value is 3.0
    float GoodTrackingWait { get; set; }

    /// When enabled, VPS will continue to localize to the user’s current surroundings,
    /// after the initial localization. Continuous localization can help avoid drift in
    /// longer (10+ minutes) VPS sessions.
    /// @note The default value is false
    bool ContinuousLocalizationEnabled { get; set; }

    // The following configurations are for internal testing and debugging.
    // Do not change the default values.

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    bool CloudProcessingForced { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    bool ClientProcessingForced { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string ConfigURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string HealthURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string LocalizationURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string GraphSyncURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string WayspotAnchorCreateURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string WayspotAnchorResolveURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string RegisterNodeURL { get; set; }

    /// Method deprecated. Will be removed in a future release.
    [Obsolete("Method deprecated. Will be removed in a future release.")]
    string LookUpNodeURL { get; set; }
  }
}
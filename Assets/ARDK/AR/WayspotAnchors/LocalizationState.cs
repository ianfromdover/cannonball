// Copyright 2022 Niantic, Inc. All Rights Reserved.
namespace Niantic.ARDK.AR.WayspotAnchors
{
  /// Possible states for the localization process. Once the `Localized` state has been reached,
  /// you can proceed to create or restore WayspotAnchors.
  /// @see [Working with the Visual Positioning System (VPS)](@ref working_with_vps)
  public enum LocalizationState
  {
    /// System has not been started yet
    Uninitialized = 0,

    /// System is using device and GPS information to determine if localization is possible.
    Initializing = 1,

    /// Localization in process. Continue to scan the localization target.
    Localizing = 2,

    /// Localization succeeded. Anchors can now be created.
    Localized = 3,

    /// Localization failed, a failure reason will be provided.
    Failed = 4,

    /// Localization stopped by user
    Stopped = 5

  }
}

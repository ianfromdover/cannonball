// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.VirtualStudio;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  /// Class factory for [WayspotAnchorsConfiguration]
  /// @see [Working with the Visual Positioning System (VPS)](@ref working_with_vps)
  public static class WayspotAnchorsConfigurationFactory
  {
    /// Initializes a new instance of the WayspotAnchorsConfiguration class.
    public static IWayspotAnchorsConfiguration Create()
    {
      return Create(_VirtualStudioLauncher.SelectedMode);
    }

    /// Create an WayspotAnchorsConfiguration for the specified RuntimeEnvironment.
    ///
    /// @param env
    ///
    /// @returns The created configuration, or null if it was not possible to create a configuration.
    public static IWayspotAnchorsConfiguration Create(RuntimeEnvironment environment)
    {
      switch (environment)
      {
        case RuntimeEnvironment.LiveDevice:
          return new _NativeWayspotAnchorsConfiguration();

        case RuntimeEnvironment.Playback:
          return new _NativeWayspotAnchorsConfiguration();

        case RuntimeEnvironment.Mock:
          return new _SerializableWayspotAnchorsConfiguration();
      }

      return null;
    }
  }
}

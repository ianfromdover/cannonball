// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.Utilities;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.AR.Configuration;

namespace Niantic.ARDK.AR.Configuration
{
  public static class ARWorldTrackingConfigurationFactory
  {
    /// Perform an asynchronous check as to whether the hardware and software are capable of and
    /// support the ARWorldTrackingConfiguration.
    /// @note
    ///   Returns ARHardwareCapability.Capable and ARSoftwareSupport.Supported when run
    ///   in the Unity Editor.
    public static void CheckCapabilityAndSupport
    (
      Action<ARHardwareCapability, ARSoftwareSupport> callback
    )
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        _NativeARWorldTrackingConfiguration._CheckCapabilityAndSupport(callback);
      #pragma warning disable 0162
      else
        callback(ARHardwareCapability.Capable, ARSoftwareSupport.Supported);
      #pragma warning restore 0162
    }

    /// Check whether the device supports lidar depth.
    /// @note Returns false when run in the Unity Editor.
    public static bool CheckLidarDepthSupport()
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        return _NativeARWorldTrackingConfiguration._CheckLidarDepthSupport();

      #pragma warning disable 0162
      return false;
      #pragma warning restore 0162
    }

    /// Check whether the device supports depth estimation.
    /// @note Returns true when run in the Unity Editor.
    public static bool CheckDepthEstimationSupport()
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        return _NativeARWorldTrackingConfiguration._CheckDepthEstimationSupport();

      #pragma warning disable 0162
      return true;
      #pragma warning restore 0162
    }

    /// Check whether the device supports depth
    /// @note Returns true when run in the Unity Editor.
    public static bool CheckDepthSupport()
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        return _NativeARWorldTrackingConfiguration._CheckDepthSupport();

      #pragma warning disable 0162
      return true;
      #pragma warning restore 0162
    }

    /// Check whether the device supports semantic segmentation.
    /// @note Returns true when run in the Unity Editor.
    public static bool CheckSemanticSegmentationSupport()
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        return _NativeARWorldTrackingConfiguration._CheckSemanticSegmentationSupport();

      #pragma warning disable 0162
      return true;
      #pragma warning restore 0162
    }

    /// Check whether the device supports meshing.
    /// @note Returns true when run in the Unity Editor.
    public static bool CheckMeshingSupport()
    {
      if (_NativeAccess.Mode == _NativeAccess.ModeType.Native)
        return _NativeARWorldTrackingConfiguration._CheckMeshingSupport();

      #pragma warning disable 0162
      return true;
      #pragma warning restore 0162
    }

    /// Initializes a new instance of the ARWorldTrackingConfiguration class.
    public static IARWorldTrackingConfiguration Create()
    {
      return Create(_VirtualStudioLauncher.SelectedMode);
    }

    /// Create an ARWorldTrackingConfiguration for the specified RuntimeEnvironment.
    ///
    /// @param env
    ///
    /// @returns The created configuration, or null if it was not possible to create a configuration.
    public static IARWorldTrackingConfiguration Create(RuntimeEnvironment environment)
    {
      switch (environment)
      {
        case RuntimeEnvironment.LiveDevice:
          return new _NativeARWorldTrackingConfiguration();

        case RuntimeEnvironment.Playback:
          return new _PlaybackARWorldTrackingConfiguration();

        case RuntimeEnvironment.Mock:
          return new _SerializableARWorldTrackingConfiguration();
        
        case RuntimeEnvironment.Remote:
          return new _SerializableARWorldTrackingConfiguration();
      }

      return null;
    }
  }
}

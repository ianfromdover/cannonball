// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.VpsCoverage;

namespace Niantic.ARDK.VPSCoverage
{
  /// Factory to create CoverageClient instances.
  public static class CoverageClientFactory
  {
    /// Create an ICoverageLoader implementation appropriate for the current device.
    ///
    /// On a mobile device, the attempted order will be LiveDevice, Remote, and finally Mock.
    /// In the Unity Editor, the attempted order will be Remote, then Mock.
    ///
    /// @returns The created loader, or throws if it was not possible to create a loader.
    public static ICoverageClient Create()
    {
      return Create(_VirtualStudioLauncher.SelectedMode);
    }

    /// Create an ICoverageLoader with the specified RuntimeEnvironment.
    ///
    /// @param env
    ///   The env used to create the loader for.
    /// @param mockResponses
    ///   A ScriptableObject containing the data that a Mock implementation of the ICoverageClient
    ///   will return. This is a required argument for using the mock client on a mobile
    ///   device. It is optional in the Unity Editor; the mock client will simply use the data
    ///   provided in the ARDK/VirtualStudio/VpsCoverage/VPS Coverage Responses.asset file.
    ///
    /// @returns The created loader, or null if it was not possible to create a loader.
    public static ICoverageClient Create(RuntimeEnvironment env, VpsCoverageResponses mockResponses = null)
    {
      if (env == RuntimeEnvironment.Default)
        return Create(_VirtualStudioLauncher.SelectedMode, mockResponses);

      ICoverageClient result;
      switch (env)
      {
        case RuntimeEnvironment.Default:
          return Create();

        case RuntimeEnvironment.LiveDevice:
          result = new _NativeCoverageClient();
          break;

        case RuntimeEnvironment.Remote:
          throw new NotSupportedException();

        case RuntimeEnvironment.Mock:
          result = new _MockCoverageClient(mockResponses);
          break;

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, env.GetType());
      }

      return result;
    }
  }
}

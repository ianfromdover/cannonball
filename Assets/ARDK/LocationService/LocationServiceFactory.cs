// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.AR;
using Niantic.ARDK.VirtualStudio.LocationService;
using Niantic.ARDK.VirtualStudio.Remote;

namespace Niantic.ARDK.LocationService
{
  public class LocationServiceFactory
  {
    public static ILocationService Create()
    {
      return Create(_VirtualStudioLauncher.SelectedMode);
    }

    public static ILocationService Create(RuntimeEnvironment env)
    {
      switch (env)
      {
        case RuntimeEnvironment.Default:
          return Create();

        case RuntimeEnvironment.LiveDevice:
#if !UNITY_EDITOR
          return new _UnityLocationService();
#else
          throw new InvalidOperationException();
#endif

        case RuntimeEnvironment.Remote:
          return null;

        case RuntimeEnvironment.Mock:
          return new SpoofLocationService();

        case RuntimeEnvironment.Playback:
          return new _PlaybackLocationService();

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, env.GetType());
      }
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.Remote;

namespace Niantic.ARDK.Utilities.Preloading
{
  public static class FeaturePreloaderFactory
  {
    public static IFeaturePreloader Create()
    {
      return Create(_VirtualStudioLauncher.SelectedMode);
    }

    public static IFeaturePreloader Create(RuntimeEnvironment env)
    {
      switch (env)
      {
        case RuntimeEnvironment.Mock:
          return new _MockFeaturePreloader();

        case RuntimeEnvironment.Playback:
          return new _MockFeaturePreloader();

        case RuntimeEnvironment.Remote:
          ARLog._Warn
          (
            "Preloading is not yet supported over Remote. Required features will be downloaded " +
            "to the ARSession when it is run on device."
          );

          return new _MockFeaturePreloader();

        case RuntimeEnvironment.LiveDevice:
#pragma warning disable CS0162
          if (_NativeAccess.Mode != _NativeAccess.ModeType.Native &&
            _NativeAccess.Mode != _NativeAccess.ModeType.Testing)
            return null;
#pragma warning restore CS0162

          return new _NativeFeaturePreloader();
      }

      return null;
    }
  }
}

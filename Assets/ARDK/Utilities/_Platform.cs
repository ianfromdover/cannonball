// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR;

using UnityEngine;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.Utilities
{
  /// Internal helper class to initialize the platform.
  /// @remark Each ARDK class has a static initializer that will ensure _Platform.Init() is called
  /// before any property or method is accessed/called.
  internal static class _Platform
  {
    private static bool _initAttempted;

    /// Initialize platform.
    internal static void Init()
    {
      if (_initAttempted)
      {
        return;
      }

      if (_NativeAccess.IsNativeAccessValid())
      {
        ARLog._Debug("Initializing native ARDK systems");
        try
        {
          _ARDK_Init_Platform();
        }
        catch (DllNotFoundException e)
        {
          ARLog._DebugFormat("Failed to initialize native ARDK systems: {0}", false, e);
        }
      }

      _initAttempted = true;
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _ARDK_Init_Platform();
  }
}

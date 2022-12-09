using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.Utilities
{
  internal static class _ArdkPlatformUtility
  {
    // only caching this value because remote needs it on a per frame basis
    public static readonly bool AreNativeBinariesAvailable;

    static _ArdkPlatformUtility()
    {
      AreNativeBinariesAvailable = _IsNativeSupportEnabled();
    }

    private static bool _IsNativeSupportEnabled()
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
      // For IOS and Android, we always use the native implementation.
      ARLog._Debug("Native support is enabled for this platform");
      return true;

#else
      // Macbooks which are M1 processors or are Catalina and below do not have native support
      bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
      bool isMacCompatibleWithNative = true;
      if (isMacOS)
      {
        if (_IsM1Processor() || !_IsOperatingSystemBigSurAndAbove())
        {
          isMacCompatibleWithNative = false;
        }
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
          !isMacCompatibleWithNative)
      {
        // return 0 as native handler for windows, m1 macbooks and macbooks below BigSur
        ARLog._Debug("Native support is disabled for this platform.");
        return false;
      }

      ARLog._Debug("Native support is enabled for this platform");
      return true;
#endif
    }

    private static bool _IsOperatingSystemBigSurAndAbove()
    {
      // https://en.wikipedia.org/wiki/Darwin_%28operating_system%29#Release_history
      // 20.0.0 Darwin is the first version of BigSur
      return Environment.OSVersion.Version >= new Version(20, 0, 0);
    }

    public static bool IsUsingRosetta()
    {
      return
        _IsM1Processor() &&
        RuntimeInformation.ProcessArchitecture == Architecture.X64;
    }

    private static bool _IsM1Processor()
    {
      /*
       * https://developer.apple.com/documentation/apple-silicon/about-the-rosetta-translation-environment
       * From sysctl.proc_translated,
       * Intel/iPhone => -1
       * M1 => 0
       */
      int _;
      var size = (IntPtr)4;
      var param = "sysctl.proc_translated";
      var result = sysctlbyname(param, out _, ref size, IntPtr.Zero, (IntPtr)0);

      return result >= 0;
    }

    [DllImport("libSystem.dylib")]
    private static extern int sysctlbyname ([MarshalAs(UnmanagedType.LPStr)]string name, out int int_val, ref IntPtr length, IntPtr newp, IntPtr newlen);
  }
}

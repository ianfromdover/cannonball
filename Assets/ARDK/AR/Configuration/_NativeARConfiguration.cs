// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.AR.VideoFormat;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;

namespace Niantic.ARDK.AR.Configuration
{
  internal abstract class _NativeARConfiguration:
    IARConfiguration
  {
    static _NativeARConfiguration()
    {
      _Platform.Init();
    }

    internal _NativeARConfiguration(IntPtr nativeHandle)
    {
      _NativeAccess.AssertNativeAccessValid();

      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException("nativeHandle can't be Zero.", nameof(nativeHandle));

      NativeHandle = nativeHandle;
      GC.AddMemoryPressure(_MemoryPressure);
    }

    private static void _ReleaseImmediate(IntPtr nativeHandle)
    {
      _NARConfiguration_Release(nativeHandle);
    }

    ~_NativeARConfiguration()
    {
      _ReleaseImmediate(NativeHandle);
      GC.RemoveMemoryPressure(_MemoryPressure);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);

      var nativeHandle = NativeHandle;
      if (nativeHandle != IntPtr.Zero)
      {
        NativeHandle = IntPtr.Zero;

        _ReleaseImmediate(nativeHandle);
        GC.RemoveMemoryPressure(_MemoryPressure);
      }
    }

    public IntPtr NativeHandle { get; private set; }

    // Used to inform the C# GC that there is managed memory held by this object.
    protected virtual long _MemoryPressure
    {
      get { return (1L * 1L) + (1L * 8L); }
    }

    public abstract IReadOnlyCollection<IARVideoFormat> SupportedVideoFormats { get; }

    public virtual bool IsLightEstimationEnabled
    {
      get
      {
        return _NARConfiguration_IsLightEstimationEnabled(NativeHandle) != 0;
      }
      set
      {
        _NARConfiguration_SetLightEstimationEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public virtual WorldAlignment WorldAlignment
    {
      get
      {
        return (WorldAlignment)_NARConfiguration_GetWorldAlignment(NativeHandle);
      }
      set
      {
        _NARConfiguration_SetWorldAlignment(NativeHandle, (UInt64)value);
      }
    }

    // TODO: Maybe we should review this. It seems we create a new instance every time we call
    // get. Are users disposing it? Are they allowed to dispose it?
    public IARVideoFormat VideoFormat
    {
      get
      {
        var videoFormatHandle = _NARConfiguration_GetVideoFormat(NativeHandle);

        if (videoFormatHandle == IntPtr.Zero)
          return null;

        return _NativeARVideoFormat._FromNativeHandle(videoFormatHandle);
      }
      set
      {
        if (!(value is _NativeARVideoFormat nativeFormat))
            return;

        _NARConfiguration_SetVideoFormat(NativeHandle, nativeFormat._NativeHandle);
      }
    }

    public virtual void CopyTo(IARConfiguration target)
    {
      target.IsLightEstimationEnabled = IsLightEstimationEnabled;
      target.WorldAlignment = WorldAlignment;

      var videoFormat = VideoFormat;
      if (videoFormat != null)
        target.VideoFormat = videoFormat;
    }

    [MonoPInvokeCallback(typeof(_ARConfiguration_CheckCapabilityAndSupport_Callback))]
    protected static void ConfigurationCheckCapabilityAndSupportCallback
    (
      IntPtr context,
      UInt64 hardwareCapability,
      UInt64 softwareSupport
    )
    {
      var safeHandle =
        SafeGCHandle<Action<ARHardwareCapability, ARSoftwareSupport>>.FromIntPtr(context);

      var callback = safeHandle.TryGetInstance();
      safeHandle.Free();

      if (callback == null)
      {
        // callback was deallocated
        return;
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          callback((ARHardwareCapability)hardwareCapability, (ARSoftwareSupport)softwareSupport);
        }
      );
    }

    [DllImport(_ARDKLibrary.libraryName)]
    protected static extern void _NARConfiguration_CheckCapabilityAndSupport
    (
      UInt64 type,
      IntPtr applicationContext,
      _ARConfiguration_CheckCapabilityAndSupport_Callback callback
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_IsLightEstimationEnabled(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetLightEstimationEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt64 _NARConfiguration_GetWorldAlignment(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetWorldAlignment
    (
      IntPtr nativeHandle,
      UInt64 worldAlignment
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARConfiguration_GetVideoFormat(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetVideoFormat
    (
      IntPtr nativeHandle,
      IntPtr nativeVideoFormat
    );

    protected delegate void _ARConfiguration_CheckCapabilityAndSupport_Callback
    (
      IntPtr context,
      UInt64 hardwareCapability,
      UInt64 softwareSupport
    );
  }
}

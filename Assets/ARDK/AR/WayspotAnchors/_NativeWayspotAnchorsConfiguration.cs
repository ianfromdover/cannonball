// Copyright 2022 Niantic, Inc. All Rights Reserved.
using Niantic.ARDK.Internals;

using System;
using System.Text;
using System.Runtime.InteropServices;

using Niantic.ARDK.Utilities;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal sealed class _NativeWayspotAnchorsConfiguration:
    IWayspotAnchorsConfiguration
  {
    internal _NativeWayspotAnchorsConfiguration()
    {
      _NativeAccess.AssertNativeAccessValid();

      var nativeHandle = _NARVPSConfiguration_Init();
      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException("nativeHandle can't be Zero.", nameof(nativeHandle));

      _nativeHandle = nativeHandle;
      GC.AddMemoryPressure(_memoryPressure);
    }

    private static void _ReleaseImmediate(IntPtr nativeHandle)
    {
      _NARVPSConfiguration_Release(nativeHandle);
    }

    ~_NativeWayspotAnchorsConfiguration()
    {
      if (_nativeHandle != IntPtr.Zero)
        _ReleaseImmediate(_nativeHandle);

      GC.RemoveMemoryPressure(_memoryPressure);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);

      var nativeHandle = _nativeHandle;
      if (nativeHandle != IntPtr.Zero)
      {
        _nativeHandle = IntPtr.Zero;

        _ReleaseImmediate(nativeHandle);
        GC.RemoveMemoryPressure(_memoryPressure);
      }
    }

    private IntPtr _nativeHandle;

    internal IntPtr _NativeHandle
    {
      get => _nativeHandle;
    }

    // (string/uuid)coordinateSpace + (float)localizationTimeOut + (float)requestTimeLimit +
    // (float)requestPerSecond + (float)gpsWait + (uint32_t)maxRequestInTransit + (uint32_t)meshType
    // (string)localizationEndpoint + (string)meshDownloadEndpoint
    // 1 string/uuid + 4 floats + 2 uint32_t + 2(120-byte strings)
    private long _memoryPressure
    {
      get => (1L * 8L) + (4L * 4L) + (2L * 4L) + (2L * 120L);
    }

    public float LocalizationTimeout
    {
      get
      {
        return _NARVPSConfiguration_GetLocalizationTimeout(_NativeHandle);
      }
      set
      {
        _NARVPSConfiguration_SetLocalizationTimeout(_NativeHandle, value);
      }
    }

    public float RequestTimeLimit
    {
      get
      {
        return _NARVPSConfiguration_GetRequestTimeLimit(_NativeHandle);
      }
      set
      {
        _NARVPSConfiguration_SetRequestTimeLimit(_NativeHandle, value);
      }
    }

    public float RequestsPerSecond
    {
      get
      {
        return _NARVPSConfiguration_GetRequestsPerSecond(_NativeHandle);
      }
      set
      {
        _NARVPSConfiguration_SetRequestsPerSecond(_NativeHandle, value);
      }
    }

    public float MaxResolutionsPerSecond
    {
      get
      {
        return _NARVPSConfiguration_GetResolutionsPerSecond(_NativeHandle);
      }
      set
      {
        _NARVPSConfiguration_SetResolutionsPerSecond(_NativeHandle, value);
      }
    }

    public float GoodTrackingWait
    {
      get
      {
        return _NARVPSConfiguration_GetGoodTrackingWaitSeconds(_NativeHandle);
      }
      set
      {
        _NARVPSConfiguration_SetGoodTrackingWaitSeconds(_NativeHandle, value);
      }
    }


    public bool ContinuousLocalizationEnabled
    {
      get
      {
        return _NARVPSConfiguration_GetContinuousLocalizationEnabled(_NativeHandle) != 0;
      }
      set
      {
        _NARVPSConfiguration_SetContinuousLocalizationEnabled(_NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public bool CloudProcessingForced
    {
      get
      {
        return _NARVPSConfiguration_GetCloudProcessingForced(_NativeHandle) != 0;
      }
      set
      {
        _NARVPSConfiguration_SetCloudProcessingForced(_NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public bool ClientProcessingForced
    {
      get
      {
        return _NARVPSConfiguration_GetClientProcessingForced(_NativeHandle) != 0;
      }
      set
      {
        _NARVPSConfiguration_SetClientProcessingForced(_NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public string ConfigURL
    {
      get
      {
        const int MaxUrlLength = 2083;
        var stringBuilder = new StringBuilder(MaxUrlLength);

        _NARVPSConfiguration_GetConfigEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetConfigEndpoint(_NativeHandle, value);
      }
    }

    public string HealthURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetHealthEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetHealthEndpoint(_NativeHandle, value);
      }
    }

    public string LocalizationURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetLocalizationEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetLocalizationEndpoint(_NativeHandle, value);
      }
    }

    public string GraphSyncURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetGraphSyncEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetGraphSyncEndpoint(_NativeHandle, value);
      }
    }

    public string WayspotAnchorCreateURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetManagedPoseCreateEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetManagedPoseCreateEndpoint(_NativeHandle, value);
      }
    }

    public string WayspotAnchorResolveURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetManagedPoseResolveEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetManagedPoseResolveEndpoint(_NativeHandle, value);
      }
    }

    public string RegisterNodeURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetRegisterNodeEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetRegisterNodeEndpoint(_NativeHandle, value);
      }
    }

    public string LookUpNodeURL
    {
      get
      {
        var stringBuilder = new StringBuilder(512);

        _NARVPSConfiguration_GetLookUpNodeEndpoint
        (
          _NativeHandle,
          stringBuilder,
          (ulong)stringBuilder.Capacity
        );

        var result = stringBuilder.ToString();
        return result;
      }
      set
      {
        _NARVPSConfiguration_SetLookUpNodeEndpoint(_NativeHandle, value);
      }
    }


    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARVPSConfiguration_Init();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARVPSConfiguration_GetLocalizationTimeout(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetLocalizationTimeout
    (
      IntPtr nativeHandle,
      float value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARVPSConfiguration_GetRequestTimeLimit(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetRequestTimeLimit
    (
      IntPtr nativeHandle,
      float value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARVPSConfiguration_GetRequestsPerSecond(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetRequestsPerSecond
    (
      IntPtr nativeHandle,
      float value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARVPSConfiguration_GetResolutionsPerSecond(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetResolutionsPerSecond
    (
      IntPtr nativeHandle,
      float value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARVPSConfiguration_GetGoodTrackingWaitSeconds(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetGoodTrackingWaitSeconds
    (
      IntPtr nativeHandle,
      float value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARVPSConfiguration_GetContinuousLocalizationEnabled
      (IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetContinuousLocalizationEnabled
    (
      IntPtr nativeHandle,
      UInt32 value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARVPSConfiguration_GetCloudProcessingForced(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetCloudProcessingForced
    (
      IntPtr nativeHandle,
      UInt32 value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARVPSConfiguration_GetClientProcessingForced
      (IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetClientProcessingForced
    (
      IntPtr nativeHandle,
      UInt32 value
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetConfigEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetConfigEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetHealthEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetHealthEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );

    // Set VPS Endpoint
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetLocalizationEndpoint
      (IntPtr nativeHandle, string url);

    // Get VPS Endpoint
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetLocalizationEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );


    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetGraphSyncEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetGraphSyncEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );


    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetManagedPoseCreateEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetManagedPoseCreateEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetManagedPoseResolveEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetManagedPoseResolveEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );


    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetRegisterNodeEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetRegisterNodeEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_SetLookUpNodeEndpoint
      (IntPtr nativeHandle, string url);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVPSConfiguration_GetLookUpNodeEndpoint
    (
      IntPtr nativeHandle,
      StringBuilder outUrl,
      ulong maxKeySize
    );
  }
}

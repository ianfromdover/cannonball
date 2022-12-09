using System;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.Internals;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities;

using UnityEngine;

using LocationInfo = Niantic.ARDK.LocationService.LocationInfo;
using LocationServiceStatus = Niantic.ARDK.LocationService.LocationServiceStatus;

namespace Niantic.ARDK.VirtualStudio.LocationService
{
  internal class _PlaybackLocationService:
    _ThreadCheckedObject,
    ILocationService
  {
    public LocationServiceStatus Status { get; private set; }
    public LocationInfo LastData { get; private set; }

    public event ArdkEventHandler<LocationStatusUpdatedArgs> StatusUpdated;
    public event ArdkEventHandler<LocationUpdatedArgs> LocationUpdated;

    public _PlaybackLocationService()
    {
      _NativeAccess.AssertNativeAccessValid();
    }

    ~_PlaybackLocationService()
    {
      _cachedHandle.Free();
      _cachedHandleIntPtr = IntPtr.Zero;
    }

    public void Start()
    {
      Start(_UnityLocationService._DefaultAccuracyMeters, _UnityLocationService._DefaultDistanceMeters);
    }

    // Specified accuracy and update distance will be overriden by the dataset configuration.
    public void Start(float desiredAccuracyInMeters, float updateDistanceInMeters)
    {
      SetStatus(LocationServiceStatus.Initializing);
      SubscribeToLocationUpdated();
    }

    public void Stop()
    {
      SetStatus(LocationServiceStatus.Stopped);
    }

    private void SetStatus(LocationServiceStatus status)
    {
      if (Status != status)
      {
        Status = status;
        StatusUpdated?.Invoke(new LocationStatusUpdatedArgs(Status));
      }
    }

    private void SetData(LocationInfo info)
    {
      LastData = info;
      LocationUpdated?.Invoke(new LocationUpdatedArgs(info));
    }

    private bool _locationUpdatedInitialized;
    private void SubscribeToLocationUpdated()
    {
      _CheckThread();

      if (_locationUpdatedInitialized)
        return;

      _NARPlaybackSession_Set_didUpdateLocationCallback(_handle, OnNativeLocationUpdated);
      _locationUpdatedInitialized = true;
    }


    [MonoPInvokeCallback(typeof(_locationUpdated_Callback))]
    private static void OnNativeLocationUpdated(IntPtr context, IntPtr dataHandle)
    {
      var elemSize = sizeof(Int64);

      var locationInfo = new LocationInfo
      (
        timestamp: Marshal.ReadInt64(dataHandle, 0),
        latitude:  Marshal.ReadInt64(dataHandle, elemSize),
        longitude:  Marshal.ReadInt64(dataHandle, 2 * elemSize)
      );

      var service = SafeGCHandle.TryGetInstance<_PlaybackLocationService>(context);
      if (service == null)
        return;

      _CallbackQueue.QueueCallback
      (
        () =>
        {

          if (service.Status == LocationServiceStatus.Initializing)
            service.SetStatus(LocationServiceStatus.Running);
          else if (service.Status == LocationServiceStatus.Stopped)
            return;

          service.SetData(locationInfo);
        }
      );
    }

    #pragma warning disable CS0067
    public event ArdkEventHandler<CompassUpdatedArgs> CompassUpdated;
    #pragma warning restore CS0067

    // Caching `this` for native device callbacks
    private IntPtr _cachedHandleIntPtr = IntPtr.Zero;
    private SafeGCHandle<_PlaybackLocationService> _cachedHandle;

    private IntPtr _handle
    {
      get
      {
        _CheckThread();

        var cachedHandleIntPtr = _cachedHandleIntPtr;
        if (cachedHandleIntPtr != IntPtr.Zero)
          return cachedHandleIntPtr;

        _cachedHandle = SafeGCHandle.Alloc(this);
        cachedHandleIntPtr = _cachedHandle.ToIntPtr();
        _cachedHandleIntPtr = cachedHandleIntPtr;

        return cachedHandleIntPtr;
      }
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARPlaybackSession_Set_didUpdateLocationCallback
    (
      IntPtr applicationSession,
      _locationUpdated_Callback callback
    );

    private delegate void _locationUpdated_Callback(IntPtr context, IntPtr locationHandle);
  }
}

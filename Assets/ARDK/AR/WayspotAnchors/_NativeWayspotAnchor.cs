// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Runtime.InteropServices;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;
using UnityEngine.Assertions;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal class _NativeWayspotAnchor:
    _ThreadCheckedObject,
    IWayspotAnchor,
    _IInternalTrackable
  {
    internal IntPtr _NativeHandle { get; private set; } = IntPtr.Zero;

    public ArdkEventHandler<WayspotAnchorResolvedArgs> TrackingStateUpdated { get; set; }

    public event ArdkEventHandler<WayspotAnchorResolvedArgs> TransformUpdated
    {
      add
      {
        _CheckThread();

        _transformUpdated += value;

        if (Status == WayspotAnchorStatusCode.Success)
          value.Invoke(new WayspotAnchorResolvedArgs(ID, LastKnownPosition, LastKnownRotation));
      }
      remove
      {
        _transformUpdated -= value;
      }
    }

    public event ArdkEventHandler<WayspotAnchorStatusUpdate> StatusCodeUpdated
    {
      add
      {
        _CheckThread();

        _statusCodeUpdated += value;
        value.Invoke(new WayspotAnchorStatusUpdate(ID, Status));
      }
      remove
      {
        _statusCodeUpdated -= value;
      }
    }

    /// Checks whether or not the wayspot anchor's position/rotation is being tracked
    public bool Tracking { get; private set; }

    public WayspotAnchorStatusCode Status { get; private set; }

    public Vector3 LastKnownPosition { get; private set; } = new Vector3(float.NaN, float.NaN, float.NaN);

    public Quaternion LastKnownRotation { get; private set; } = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

    // There is a bug(?) where the native layer sometimes invokes a status code callback before it
    // surfaces the creation callback. Queuing the status update if the creation callback has not
    // yet been invoked (i.e. NativeHandle is not set) and only invoking it once the creation callback
    // is invoked prevents issues where a native anchor in a "Success" state is not actually ready for use.
    private WayspotAnchorStatusCode? _queuedStatus;

    // Part of _IInternalTrackable interface
    public void SetTrackingEnabled(bool tracking)
    {
      Tracking = tracking;
    }

    // Part of _IInternalTrackable interface
    public void SetTransform(Vector3 position, Quaternion rotation)
    {
      if (position == LastKnownPosition && rotation == LastKnownRotation)
        return;

      ARLog._Debug($"_NativeWayspotAnchor.SetTransform({position}, {rotation.eulerAngles})");
      LastKnownPosition = position;
      LastKnownRotation = rotation;

      TryUnqueueSuccessCode();
      _transformUpdated?.Invoke(new WayspotAnchorResolvedArgs(ID, LastKnownPosition, LastKnownRotation));
    }

    // Part of _IInternalTrackable interface
    public void SetStatusCode(WayspotAnchorStatusCode statusCode)
    {
      if (statusCode == Status)
        return;

      var isQueueableStatus = statusCode == WayspotAnchorStatusCode.Limited || statusCode == WayspotAnchorStatusCode.Success;
      if (isQueueableStatus && (_NativeHandle == IntPtr.Zero || float.IsNaN(LastKnownPosition.x)))
      {
        ARLog._Debug($"Status update queued because no identifier ({_NativeHandle == IntPtr.Zero}) or no transform ({float.IsNaN(LastKnownPosition.x)})");
        _queuedStatus = statusCode;
      }
      else
      {
        ARLog._Debug($"_NativeWayspotAnchor.SetStatusCode({statusCode})");
        Status = statusCode;
        _statusCodeUpdated?.Invoke(new WayspotAnchorStatusUpdate(ID, Status));
      }
    }

    private void TryUnqueueSuccessCode()
    {
      if (_queuedStatus.HasValue && (_NativeHandle != IntPtr.Zero || !float.IsNaN(LastKnownPosition.x)))
      {
        SetStatusCode(_queuedStatus.Value);
        _queuedStatus = null;
      }
    }

    public _NativeWayspotAnchor(Guid identifier, Matrix4x4 localPose)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));
      _NativeAccess.AssertNativeAccessValid();

      _identifier = identifier;
      LastKnownPosition = localPose.ToPosition();
      LastKnownRotation = localPose.ToRotation();
      Status = WayspotAnchorStatusCode.Pending;
    }

    public _NativeWayspotAnchor(IntPtr nativeHandle)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));
      _NativeAccess.AssertNativeAccessValid();

      SetNativeHandle(nativeHandle);
    }

    /// Creates a new native wayspot anchor
    /// @param data The blob of data used to create the wayspot anchor
    public _NativeWayspotAnchor(byte[] data)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));
      _NativeAccess.AssertNativeAccessValid();

      var nativeHandle = _NAR_ManagedPose_InitFromBlob(data, data.Length);
      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException("Failed to create _NativeWayspotAnchor from payload", nameof(data));

      SetNativeHandle(nativeHandle);
    }

    /// @param nativeHandle The pointer to the native handle
    public void SetNativeHandle(IntPtr nativeHandle)
    {
      if (nativeHandle == IntPtr.Zero)
        throw new ArgumentException($"{nameof(nativeHandle)} value cannot be IntPtr.Zero");

      _NativeHandle = nativeHandle;
      GC.AddMemoryPressure(_MemoryPressure);

      TryUnqueueSuccessCode();
    }

    private static void _ReleaseImmediate(IntPtr nativeHandle)
    {
      if (nativeHandle != IntPtr.Zero)
        _NAR_ManagedPose_Release(nativeHandle);
    }

    /// Disposes of the native wayspot anchor
    public void Dispose()
    {
      GC.SuppressFinalize(this);

      var nativeHandle = _NativeHandle;
      if (nativeHandle == IntPtr.Zero)
        return;

      _NativeHandle = IntPtr.Zero;

      _ReleaseImmediate(nativeHandle);
      GC.RemoveMemoryPressure(_MemoryPressure);
    }

    private Guid _identifier = Guid.Empty;

    /// Gets the ID of the native wayspot anchor
    public Guid ID
    {
      get
      {
        if (_identifier == Guid.Empty)
        {
          if (_NativeHandle == IntPtr.Zero)
            throw new InvalidOperationException("No identifier or native handle set.");

          _NAR_ManagedPose_GetIdentifier(_NativeHandle, out _identifier);
        }

        return _identifier;
      }
    }

    /// Gets the payload for the native wayspot anchor
    public WayspotAnchorPayload Payload
    {
      get
      {
        if (_NativeHandle == IntPtr.Zero)
        {
          throw new InvalidOperationException
          (
            "Cannot get payload of WayspotAnchor until its StatusCode is either Success or Limited."
          );
        }

        var dataSize = _NAR_ManagedPose_GetDataSize(_NativeHandle);
        byte[] dataArray = new byte[dataSize];
        unsafe
        {
          fixed (byte* ptr = dataArray)
          {
            IntPtr identifierPtr = (IntPtr)ptr;
            _NAR_ManagedPose_GetData(_NativeHandle, identifierPtr);
          }
        }

        var payload = new WayspotAnchorPayload(dataArray);
        return payload;
      }
    }

    private long _MemoryPressure
    {
      get => (255L);
    }

    private event ArdkEventHandler<WayspotAnchorResolvedArgs> _transformUpdated = args => {};
    private event ArdkEventHandler<WayspotAnchorStatusUpdate> _statusCodeUpdated = args => {};

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NAR_ManagedPose_InitFromBlob(byte[] blob, int dataSize);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ManagedPose_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    internal static extern bool _NAR_ManagedPose_GetIdentifier
    (
      IntPtr nativeHandle,
      out Guid wayspotAnchorId
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPose_GetData
    (
      IntPtr nativeHandle,
      IntPtr wayspotAnchorData
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern int _NAR_ManagedPose_GetDataSize(IntPtr nativeHandle);
  }
}
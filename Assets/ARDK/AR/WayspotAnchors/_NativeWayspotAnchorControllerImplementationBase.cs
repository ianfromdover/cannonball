using System;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal class _NativeWayspotAnchorControllerImplementationBase: _ThreadCheckedObject,
    _IWayspotAnchorControllerImplementation
  {
    protected IntPtr _nativeHandle;
    private IntPtr _cachedHandleIntPtr = IntPtr.Zero;
    private SafeGCHandle<_NativeWayspotAnchorControllerImplementationBase> _cachedHandle;
    private ArdkEventHandler<LocalizationStateUpdatedArgs> _localizationStateUpdated;
    private ArdkEventHandler<WayspotAnchorStatusUpdatedArgs> _wayspotAnchorStatusesUpdated;
    private ArdkEventHandler<WayspotAnchorsCreatedArgs> _wayspotAnchorsCreated;
    private ArdkEventHandler<WayspotAnchorsResolvedArgs> _wayspotAnchorsResolved;
    private bool _onDidUpdateLocalizationStateInitialized;
    private bool _onDidUpdateWayspotAnchorStatusesInitialized;
    private bool _onDidCreateWayspotAnchorsInitialized;
    private bool _onDidResolveWayspotAnchorsInitialized;

    private IntPtr _handle
    {
      get
      {
        _CheckThread();

        var cachedHandleIntPtr = _cachedHandleIntPtr;
        if (cachedHandleIntPtr != IntPtr.Zero)
          return cachedHandleIntPtr;

        _cachedHandle = SafeGCHandle.Alloc<_NativeWayspotAnchorControllerImplementationBase>(this);
        cachedHandleIntPtr = _cachedHandle.ToIntPtr();
        _cachedHandleIntPtr = cachedHandleIntPtr;

        return cachedHandleIntPtr;
      }
    }

    /// Whether or not the native wayspot anchor controller has been destroyed
    public bool IsDestroyed
    {
      get => _nativeHandle == IntPtr.Zero;
    }

    /// Called when the localization state has changed
    public event ArdkEventHandler<LocalizationStateUpdatedArgs> LocalizationStateUpdated
    {
      add
      {
        _CheckThread();

        _SubscribeToDidUpdateLocalizationState();

        _localizationStateUpdated += value;
      }
      remove
      {
        _localizationStateUpdated -= value;
      }
    }

    /// Called when the status of wayspot anchors has changed
    public event ArdkEventHandler<WayspotAnchorStatusUpdatedArgs> WayspotAnchorStatusUpdated
    {
      add
      {
        _CheckThread();

        _SubscribeToDidUpdateWayspotAnchorStatuses();

        _wayspotAnchorStatusesUpdated += value;
      }
      remove
      {
        _wayspotAnchorStatusesUpdated -= value;
      }
    }

    /// Called when new wayspot anchors have been created
    public event ArdkEventHandler<WayspotAnchorsCreatedArgs> WayspotAnchorsCreated
    {
      add
      {
        _CheckThread();

        _SubscribeToDidCreateWayspotAnchors();

        _wayspotAnchorsCreated += value;
      }
      remove
      {
        _wayspotAnchorsCreated -= value;
      }
    }

    /// Called when wayspot anchors have been resolved
    public event ArdkEventHandler<WayspotAnchorsResolvedArgs> WayspotAnchorsResolved
    {
      add
      {
        _CheckThread();

        _SubscribeToDidResoveWayspotAnchors();

        _wayspotAnchorsResolved += value;
      }
      remove
      {
        _wayspotAnchorsResolved -= value;
      }
    }

    /// Creates new wayspot anchors
    /// @param localPoses The local poses (position and rotation only) used to create the wayspot anchors
    /// @return The IDs of the newly created wayspot anchors
    public Guid[] SendWayspotAnchorsCreateRequest(params Matrix4x4[] localPoses)
    {
      _CheckThread();

      var numPoses = localPoses.Length;
      float[] nativeTransforms = new float[16 * numPoses];
      for (int i = 0; i < numPoses; i++)
      {
        var poseArray = _Convert.Matrix4x4ToInternalArray(NARConversions.FromUnityToNAR(localPoses[i]));

        for (int x = 0; x < 16; x++)
          nativeTransforms[16 * i + x] = poseArray[x];
      }

      var identifierArray = new Guid[numPoses];

      unsafe
      {
        fixed (Guid* identifiersPtr = identifierArray)
        {
          _NAR_ManagedPoseController_CreateManagedPoses
          (
            _nativeHandle,
            (UInt64)numPoses,
            nativeTransforms,
            (IntPtr)identifiersPtr,
            1,
            0
          );
        }
      }

      foreach (var identifier in identifierArray)
      {
        // Noticed this happening in the QA test scene in Playback mode but cannot reproduce in release
        // or on device.
        // Putting this log here to make the bug easier to triage if it appears.
        if (identifier == Guid.Empty)
        {
          ARLog._Error("`_NAR_ManagedPoseController_CreateManagedPoses` returned empty anchor identifier.");
          return identifierArray;
        }
      }

      return identifierArray;
    }

    /// Starts resolving wayspot anchors.  Resolving anchors will have their position and rotation updates reported via the _wayspotAnchorsResolved
    /// event
    /// @param wayspotAnchors The wayspot anchors to update
    public virtual void StartResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
      _CheckThread();

      var numPoses = wayspotAnchors.Length;

      IntPtr[] wayspotAnchorsHandles = new IntPtr[numPoses];
      for (int i = 0; i < numPoses; i++)
      {
        var wayspotAnchor = wayspotAnchors[i];
        if (wayspotAnchor is _NativeWayspotAnchor nativeWayspotAnchor)
        {
          wayspotAnchorsHandles[i] = nativeWayspotAnchor._NativeHandle;
        }
        else
        {
          ARLog._Error
          (
            $"Must use a {nameof(_NativeWayspotAnchor)} with {nameof(_NativeWayspotAnchorControllerImplementationBase)}"
          );

          return;
        }
      }

      _NAR_ManagedPoseController_StartResolvingManagedPoses(_nativeHandle, (UInt64)numPoses, wayspotAnchorsHandles);
    }

    /// Stops resolving the wayspot anchors
    /// @param wayspotAnchors The wayspot anchors to stop resolving
    public virtual void StopResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
      _CheckThread();

      var numPoses = wayspotAnchors.Length;

      IntPtr[] wayspotAnchorHandles = new IntPtr[numPoses];
      for (int i = 0; i < numPoses; i++)
      {
        var wayspotAnchor = wayspotAnchors[i];
        if (wayspotAnchor is _NativeWayspotAnchor nativeWayspotAnchor)
        {
          ARLog._Debug($"Stopping resolve of anchor {wayspotAnchor.ID}");
          wayspotAnchorHandles[i] = nativeWayspotAnchor._NativeHandle;
        }
        else
        {
          var error = "Must use a _NativeManagedPose with _NativeManagedPoseController";
          ARLog._Error(error);
          return;
        }
      }

      _NAR_ManagedPoseController_StopResolvingManagedPoses
      (
        _nativeHandle,
        (UInt64)numPoses,
        wayspotAnchorHandles
      );
    }

    /// Disposes of the native wayspot anchor controller
    public void Dispose()
    {
      _CheckThread();

      ARLog._Debug("Disposing _NativeWayspotAnchorImplementation");
      GC.SuppressFinalize(this);

      var nativeHandle = _nativeHandle;
      if (nativeHandle != IntPtr.Zero)
      {
        _nativeHandle = IntPtr.Zero;

        StopVPS();
        _ReleaseImmediate(nativeHandle);
      }

      _cachedHandle.Free();
      _cachedHandleIntPtr = IntPtr.Zero;
    }

    /// <inheritdoc />
    public virtual void StartVPS(IWayspotAnchorsConfiguration wayspotAnchorsConfiguration)
    {
      _CheckThread();

      if (wayspotAnchorsConfiguration == null)
      {
        throw new ArgumentNullException(nameof(wayspotAnchorsConfiguration));
      }

      if (wayspotAnchorsConfiguration is _NativeWayspotAnchorsConfiguration nativeConfig)
      {
        _NAR_ManagedPoseController_StartVPS
        (
          _nativeHandle,
          nativeConfig._NativeHandle
        );
      }
      else
      {
        ARLog._Error
        (
          $"Must use a {nameof(_NativeWayspotAnchorsConfiguration)} with {nameof(_NativeWayspotAnchorControllerImplementationBase)}"
        );
      }
    }

    public virtual void StopVPS()
    {
      _CheckThread();

      _NAR_ManagedPoseController_StopVPS(_nativeHandle);
    }

    private static void _ReleaseImmediate(IntPtr nativeHandle)
    {
      _NAR_ManagedPoseController_Release(nativeHandle);
    }

    private void _SubscribeToDidUpdateLocalizationState()
    {
      _CheckThread();

      if (_onDidUpdateLocalizationStateInitialized)
        return;

      _NAR_ManagedPoseController_Set_didUpdateLocalizationStateCallback
      (
        _handle,
        _nativeHandle,
        _onDidUpdateLocalizationStateNative
      );

      ARLog._Debug("Subscribed to native didUpdateLocalizationStateCallback updated");

      _onDidUpdateLocalizationStateInitialized = true;
    }

    private void _SubscribeToDidUpdateWayspotAnchorStatuses()
    {
      _CheckThread();

      if (_onDidUpdateWayspotAnchorStatusesInitialized)
        return;

      _NAR_ManagedPoseController_Set_didUpdateManagedPoseStatusCodesCallback
      (
        _handle,
        _nativeHandle,
        _onDidUpdateManagedPoseStatusesNative
      );

      ARLog._Debug("Subscribed to native didUpdateManagedPoseStatusCodesCallback updated");

      _onDidUpdateWayspotAnchorStatusesInitialized = true;
    }

    private void _SubscribeToDidCreateWayspotAnchors()
    {
      _CheckThread();

      if (_onDidCreateWayspotAnchorsInitialized)
        return;

      _NAR_ManagedPoseController_Set_didCreateManagedPosesCallback
      (
        _handle,
        _nativeHandle,
        _onDidCreateManagedPosesNative
      );

      ARLog._Debug("Subscribed to native didCreateManagedPosesCallback updated");

      _onDidCreateWayspotAnchorsInitialized = true;
    }

    private void _SubscribeToDidResoveWayspotAnchors()
    {
      _CheckThread();

      if (_onDidResolveWayspotAnchorsInitialized)
        return;

      _NAR_ManagedPoseController_Set_didResolveManagedPosesCallback
      (
        _handle,
        _nativeHandle,
        _onDidResolveManagedPosesNative
      );

      ARLog._Debug("Subscribed to native didResolveManagedPosesCallback updated");

      _onDidResolveWayspotAnchorsInitialized = true;
    }

    [MonoPInvokeCallback(typeof(_onDidUpdateLocalizationState_Definition))]
    private static void _onDidUpdateLocalizationStateNative
    (
      IntPtr context,
      UInt32 state,
      UInt32 failureReason
    )
    {
      var controller = SafeGCHandle.TryGetInstance<_NativeWayspotAnchorControllerImplementationBase>(context);
      if (controller == null || controller.IsDestroyed)
      {
        // controller was deallocated
        ARLog._Debug("controller is null in _onDidUpdateLocalizationStateNative()");
        return;
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (controller.IsDestroyed)
          {
            // controller was deallocated
            return;
          }

          if (controller._localizationStateUpdated != null)
          {
            var args =
              new LocalizationStateUpdatedArgs
              (
                (LocalizationState)state,
                (LocalizationFailureReason)failureReason
              );

            controller._localizationStateUpdated(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(_onDidUpdateManagedPoseStatusesDefinition))]
    private static void _onDidUpdateManagedPoseStatusesNative
    (
      IntPtr context,
      IntPtr identifiersPtr,
      IntPtr statusCodesPtr,
      UInt64 anchorsCount
    )
    {
      var controller = SafeGCHandle.TryGetInstance<_NativeWayspotAnchorControllerImplementationBase>(context);
      if (controller == null || controller.IsDestroyed)
      {
        // controller was deallocated
        return;
      }

      var count = (int)anchorsCount;

      const int kBytesPerIdentifier = 16;
      var statusUpdates = new WayspotAnchorStatusUpdate[anchorsCount];

      var allIdentifierBytes = new byte[kBytesPerIdentifier * count];
      Marshal.Copy(identifiersPtr, allIdentifierBytes, 0, kBytesPerIdentifier * count);

      var statusCodes = new int[count];
      Marshal.Copy(statusCodesPtr, statusCodes, 0, count);

      var identifierStaging = new byte[kBytesPerIdentifier];
      for (int i = 0; i < count; i++)
      {
        Buffer.BlockCopy
        (
          allIdentifierBytes,
          i * kBytesPerIdentifier,
          identifierStaging,
          0,
          kBytesPerIdentifier
        );

        statusUpdates[i] =
          new WayspotAnchorStatusUpdate
          (
            new Guid(identifierStaging),
            (WayspotAnchorStatusCode)statusCodes[i]
          );
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (controller.IsDestroyed)
          {
            // controller was deallocated
            return;
          }

          if (controller._wayspotAnchorStatusesUpdated != null)
          {
            var args = new WayspotAnchorStatusUpdatedArgs(statusUpdates);
            controller._wayspotAnchorStatusesUpdated(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(_onDidCreateManagedPoses_Definition))]
    private static void _onDidCreateManagedPosesNative
    (
      IntPtr context,
      IntPtr wayspotAnchorPtrs,
      UInt64 anchorsCount
    )
    {
      var controller = SafeGCHandle.TryGetInstance<_NativeWayspotAnchorControllerImplementationBase>(context);
      if (controller == null || controller.IsDestroyed)
      {
        // controller was deallocated
        return;
      }

      var anchorHandles = new IntPtr[anchorsCount];
      for (var i = 0; i < (int)anchorsCount; i++)
        anchorHandles[i] = Marshal.ReadIntPtr(wayspotAnchorPtrs, i * IntPtr.Size);

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (controller.IsDestroyed)
          {
            return;
          }

          var anchors = new IWayspotAnchor[anchorsCount];
          for (var i = 0; i < (int)anchorsCount; i++)
          {
            var anchor = _WayspotAnchorFactory.GetOrCreateFromNativeHandle(anchorHandles[i]);
            anchors[i] = anchor;
          }

          if (controller._wayspotAnchorsCreated != null)
          {
            var args = new WayspotAnchorsCreatedArgs(anchors);
            controller._wayspotAnchorsCreated(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(_onDidResolveManagedPoses_Definition))]
    private static void _onDidResolveManagedPosesNative
    (
      IntPtr context,
      IntPtr identifiers, //guid[]
      IntPtr localPoses,  //float[]
      IntPtr accuracies,  //float[]
      UInt64 anchorsCount
    )
    {
      var controller = SafeGCHandle.TryGetInstance<_NativeWayspotAnchorControllerImplementationBase>(context);
      if (controller == null || controller.IsDestroyed)
        return;

      int count = (int)anchorsCount;
      if (count <= 0)
      {
        ARLog._Error($"Invalid number of managed poses resolved: ({count})");
        return;
      }

      var byteArray = new byte[count * 16];
      var guids = new Guid[count];
      Marshal.Copy(identifiers, byteArray, 0, count * 16);
      for (int i = 0; i < count; i++)
      {
        var identifierBytes = new byte[16];
        for (int b = 0; b < 16; b++)
          identifierBytes[b] = byteArray[16 * i + b];

        guids[i] = new Guid(identifierBytes);
      }

      var lpSize = count * 16;
      var lp = new float[lpSize];
      Marshal.Copy(localPoses, lp, 0, lpSize);

      var accSize = count * 6;
      var acc = new float[accSize];
      Marshal.Copy(accuracies, acc, 0, accSize);

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (controller.IsDestroyed)
          {
            // controller was deallocated
            return;
          }

          if (controller._wayspotAnchorsResolved == null)
            return;

          var resolutions = new WayspotAnchorResolvedArgs[anchorsCount];
          for (int i = 0; i < (int)anchorsCount; i++)
          {
            var localPoseArray = new float[16];
            for (int x = 0; x < 16; x++)
              localPoseArray[x] = lp[i * 16 + x];

            var localPoseTransform =
              NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(localPoseArray));

            var anchor = _WayspotAnchorFactory.GetOrCreateFromIdentifier(guids[i]);

            resolutions[i] =
              new WayspotAnchorResolvedArgs
              (
                anchor.ID,
                localPoseTransform.ToPosition(),
                localPoseTransform.ToRotation()
              );
          }

          var args = new WayspotAnchorsResolvedArgs(resolutions);
          controller._wayspotAnchorsResolved(args);
        }
      );
    }

    [DllImport(_ARDKLibrary.libraryName)]
    protected static extern IntPtr _NAR_ManagedPoseController_Init(byte[] stageIdentifier);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ManagedPoseController_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPoseController_StartVPS
    (
      IntPtr nativeHandle,
      IntPtr nativeConfigHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPoseController_StopVPS(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPoseController_CreateManagedPoses
    (
      [In] IntPtr nativeHandle,
      [In] UInt64 numOfWayspotAnchors,
      [In] float[] localPosesIn,
      [In, Out] IntPtr wayspotAnchorIdsOut,
      [In] UInt32 requireNodeAssociations,
      [In] UInt32 requireGeoAssociations
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPoseController_StartResolvingManagedPoses
    (
      IntPtr nativeHandle,
      UInt64 numOfWayspotAnchors,
      IntPtr[] wayspotAnchorHandlesArray
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ManagedPoseController_StopResolvingManagedPoses
    (
      IntPtr nativeHandle,
      UInt64 numOfWayspotAnchors,
      IntPtr[] wayspotAnchorHandlesArray
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ManagedPoseController_Set_didUpdateLocalizationStateCallback
    (
      IntPtr applicationSession,
      IntPtr platformSession,
      _onDidUpdateLocalizationState_Definition callback
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void
      _NAR_ManagedPoseController_Set_didUpdateManagedPoseStatusCodesCallback
      (
        IntPtr applicationSession,
        IntPtr platformSession,
        _onDidUpdateManagedPoseStatusesDefinition callback
      );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ManagedPoseController_Set_didCreateManagedPosesCallback
    (
      IntPtr applicationSession,
      IntPtr platformSession,
      _onDidCreateManagedPoses_Definition callback
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ManagedPoseController_Set_didResolveManagedPosesCallback
    (
      IntPtr applicationSession,
      IntPtr platformSession,
      _onDidResolveManagedPoses_Definition callback
    );

    private delegate void _onDidUpdateLocalizationState_Definition
    (
      IntPtr handle,
      UInt32 state,
      UInt32 error
    );

    private delegate void _onDidUpdateManagedPoseStatusesDefinition
    (
      IntPtr handle,
      IntPtr identifierArrays,
      IntPtr statusCodes,
      UInt64 numOfWayspotAnchors
    );

    private delegate void _onDidCreateManagedPoses_Definition
    (
      IntPtr handle,
      IntPtr wayspotAnchorHandles,
      UInt64 numOfWayspotAnchors
    );

    private delegate void _onDidResolveManagedPoses_Definition
    (
      IntPtr handle,
      IntPtr identifierArrays,
      IntPtr localPoses,
      IntPtr accuracies,
      UInt64 numOfWayspotAnchors
    );
  }
}

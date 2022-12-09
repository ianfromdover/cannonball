// Copyright 2022 Niantic, Inc. All Rights Reserved.

#pragma warning disable 0067
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class _NativeVPSColocalization :
    IColocalization
  {
    private bool _isDestroyed;

    internal readonly INetworking Networking;
    
    public _NativeVPSColocalization(INetworking networking, IARSession arSession, IWayspotAnchor node = null)
    {
      Networking = networking;
      
      if (Networking is _NativeNetworking nativeNeworking)
      {
        _nativeHandle = (node != null)
          ? _NARColocalization_InitWithNode
          (
            _applicationHandle, 
            arSession.StageIdentifier.ToByteArray(), 
            nativeNeworking.GetNativeHandle(), 
            node.Payload._Blob, 
            (ulong)node.Payload._Blob.Length
          )
          : _NARColocalization_Init
          (
            _applicationHandle,
            arSession.StageIdentifier.ToByteArray(),
            nativeNeworking.GetNativeHandle()
          );

        GC.AddMemoryPressure(GCPressure);
        SubscribeToNativeCallbacks();
      }
      else
      {
        ARLog._Error("Can only use NativeVPSColocalization with NativeNetworking for now");
      }
    }

    public void Start()
    {
      _NARColocalization_Start(_nativeHandle);
    }

    public void Stop()
    {
      _NARColocalization_Pause(_nativeHandle);
    }

    private readonly Dictionary<IPeerID, ColocalizationState> _colocalizationStates;
    public ReadOnlyDictionary<IPeerID, ColocalizationState> ColocalizationStates { get; }
    public ColocalizationFailureReason FailureReason { get; }

    public Matrix4x4 AlignedSpaceOrigin
    {
      get
      {
        var floats = new float[16];

        _NARColocalization_GetAlignedSpaceOrigin(_nativeHandle, floats);

        return NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(floats));
      }
    }

    public event ArdkEventHandler<ColocalizationStateUpdatedArgs> ColocalizationStateUpdated;

    public void LocalPoseToAligned(Matrix4x4 poseInLocalSpace, out Matrix4x4 poseInAlignedSpace)
    {
      var poseArray = _Convert.Matrix4x4ToInternalArray
        (NARConversions.FromUnityToNAR(poseInLocalSpace));

      var outArray = new float[16];

      _NARColocalization_LocalPoseToAligned(_nativeHandle, poseArray, outArray);

      poseInAlignedSpace = NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(outArray));
    }

    public ColocalizationAlignmentResult AlignedPoseToLocal(IPeerID id, Matrix4x4 poseInAlignedSpace, out Matrix4x4 poseInLocalSpace)
    {
      var poseArray = _Convert.Matrix4x4ToInternalArray
        (NARConversions.FromUnityToNAR(poseInAlignedSpace));

      var outArray = new float[16];
      var peerGuid = id.Identifier.ToByteArray();
      byte capiResult = _NARColocalization_AlignedPoseToLocal(_nativeHandle, peerGuid, poseArray, outArray);
      ColocalizationAlignmentResult result = (ColocalizationAlignmentResult)capiResult;

      poseInLocalSpace = (result == ColocalizationAlignmentResult.Success) ? 
        NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(outArray)) : 
        Matrix4x4.identity;

      return result;
    }

    public void Dispose()
    {
      if (_nativeHandle != IntPtr.Zero)
      {
        _NARColocalization_Release(_nativeHandle);
        GC.RemoveMemoryPressure(GCPressure);
        _nativeHandle = IntPtr.Zero;
        _isDestroyed = true;
      }
    }

    #region Handles
    // Below here are private fields and methods to handle native code and callbacks

    // The pointer to the C++ object handling functionality at the native level
    private IntPtr _nativeHandle;

    private IntPtr _cachedHandleIntPtr = IntPtr.Zero;
    private SafeGCHandle<_NativeVPSColocalization> _cachedHandle;

    // Approx memory size of native object
    // Magic number for 64KB
    private const long GCPressure = 64L * 1024L;

    // Used to round-trip a pointer through c++,
    // so that we can keep our this pointer even in c# functions
    // marshaled and called by native code
    private IntPtr _applicationHandle
    {
      get
      {
        if (_cachedHandleIntPtr != IntPtr.Zero)
          return _cachedHandleIntPtr;

        lock (this)
        {
          if (_cachedHandleIntPtr != IntPtr.Zero)
            return _cachedHandleIntPtr;

          // https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.gchandle.tointptr.aspx
          _cachedHandle = SafeGCHandle.Alloc(this);
          _cachedHandleIntPtr = _cachedHandle.ToIntPtr();
        }

        return _cachedHandleIntPtr;
      }
    }
#endregion
    private bool _didSubscribeToNativeEvents;

    private void SubscribeToNativeCallbacks()
    {
      if (_didSubscribeToNativeEvents)
        return;

      lock (this)
      {
        if (_didSubscribeToNativeEvents)
          return;

        _NARColocalization_Set_colocalizationStateCallback
          (_applicationHandle, _nativeHandle, _colocalizationStateCallbackNative);

        _didSubscribeToNativeEvents = true;
      }
    }

    // PInvoke
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARColocalization_Init
      (IntPtr applicationHandle, byte[] stageId, IntPtr networkingHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARColocalization_InitWithNode
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle,
      byte[] data,
      ulong dataSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_Start(IntPtr nativeHandle);


    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_Pause(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_GetAlignedSpaceOrigin
      (IntPtr nativeHandle, float[] outPose);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARColocalization_GetFailureReason(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARColocalization_AlignedPoseToLocal
    (
      IntPtr nativeHandle,
      byte[] peerId,
      float[] alignedPose,
      float[] outPose
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_LocalPoseToAligned
    (
      IntPtr nativeHandle,
      float[] localPose,
      float[] outPose
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_Set_peerPoseCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      _NARColocalization_peerPoseCallback callback
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARColocalization_Set_colocalizationStateCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      _NARColocalization_colocalizationStateCallback callback
    );

    // C++ -> C# callbacks
    private delegate void _NARColocalization_colocalizationStateCallback
    (
      IntPtr context,
      UInt32 state,
      byte[] peerId
    );

    private delegate void _NARColocalization_peerPoseCallback
    (
      IntPtr context,
      float[] pose,
      byte[] peerId
    );

    [MonoPInvokeCallback(typeof(_NARColocalization_colocalizationStateCallback))]
    private static void _colocalizationStateCallbackNative(IntPtr context, UInt32 state, byte[] peerId)
    {
      var instance = SafeGCHandle.TryGetInstance<_NativeVPSColocalization>(context);

      if (instance == null || instance._isDestroyed)
        return;

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance == null || instance._isDestroyed)
          {
            ARLog._Warn
            (
              "Queued _colocalizationStateCallbackNative invoked after C# instance was destroyed."
            );

            return;
          }

          var handler = instance.ColocalizationStateUpdated;
          if (handler != null)
          {
            ARLog._DebugFormat("Surfacing ColocalizationState event: {0}", false, state);
            var args = new ColocalizationStateUpdatedArgs(null, (ColocalizationState)state);
            handler(args);
          }
        }
      );
    }
  }
}
#pragma warning restore 0067

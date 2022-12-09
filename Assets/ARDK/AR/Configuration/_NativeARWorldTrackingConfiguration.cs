// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.AR.Awareness.Depth.Generators;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.AR.SLAM;
using Niantic.ARDK.AR.VideoFormat;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Collections;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.Configuration
{
  internal class _NativeARWorldTrackingConfiguration:
    _NativeARConfiguration,
    IARWorldTrackingConfiguration
  {
    /// Perform an asynchronous check as to whether the hardware and software are capable of and
    /// support the ARWorldTrackingConfiguration.
    internal static void _CheckCapabilityAndSupport
    (
      Action<ARHardwareCapability, ARSoftwareSupport> callback
    )
    {
      try
      {
        _NARConfiguration_CheckCapabilityAndSupport
        (
          1,
          SafeGCHandle.AllocAsIntPtr(callback),
          ConfigurationCheckCapabilityAndSupportCallback
        );
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        callback(ARHardwareCapability.NotCapable, ARSoftwareSupport.NotSupported);
      }
    }

    internal static bool _CheckLidarDepthSupport()
    {
      try
      {
        return _NARConfiguration_CheckLidarDepthSupport();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        return false;
      }
    }

    internal static bool _CheckDepthEstimationSupport()
    {
      try
      {
        return _NARConfiguration_CheckDepthEstimationSupport();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        return false;
      }
    }

    internal static bool _CheckDepthSupport()
    {
      try
      {
        return _NARConfiguration_CheckDepthSupport();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        return false;
      }
    }

    internal static bool _CheckSemanticSegmentationSupport()
    {
      try
      {
        return _NARConfiguration_CheckSemanticSegmentationSupport();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        return false;
      }
    }

    internal static bool _CheckMeshingSupport()
    {
      try
      {
        return _NARConfiguration_CheckMeshingSupport();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to check ARDK system compatibility: {0}", false, e);
        return false;
      }
    }

    internal _NativeARWorldTrackingConfiguration(): this(_NARWorldTrackingConfiguration_Init())
    {
    }

    internal _NativeARWorldTrackingConfiguration(IntPtr nativeHandle): base(nativeHandle)
    {
    }

    protected override long _MemoryPressure
    {
      get { return base._MemoryPressure + 8; }
    }

    public override IReadOnlyCollection<IARVideoFormat> SupportedVideoFormats
    {
      get
      {
        var rawFormats = EmptyArray<IntPtr>.Instance;

        while (true)
        {
          var obtained =
            _NARWorldTrackingConfiguration_GetSupportedVideoFormats
            (
              NativeHandle,
              rawFormats.Length,
              rawFormats
            );

          if (obtained == rawFormats.Length)
            break;

          rawFormats = new IntPtr[Math.Abs(obtained)];
        }

        var count = rawFormats.Length;

        if (count == 0)
          return EmptyReadOnlyCollection<IARVideoFormat>.Instance;

        var formats = new IARVideoFormat[count];

        for (var i = 0; i < count; i++)
          formats[i] = _NativeARVideoFormat._FromNativeHandle(rawFormats[i]);

        return new ReadOnlyCollection<IARVideoFormat>(formats);
      }
    }

    public virtual PlaneDetection PlaneDetection
    {
      get
      {
        return (PlaneDetection)_NARWorldTrackingConfiguration_GetPlaneDetection(NativeHandle);
      }
      set
      {
        _NARWorldTrackingConfiguration_SetPlaneDetection(NativeHandle, (UInt64)value);
      }
    }

    public virtual IReadOnlyCollection<IARReferenceImage> DetectionImages
    {
      get
      {
        var rawImages = EmptyArray<IntPtr>.Instance;

        while (true)
        {
          var obtained =
            _NARWorldTrackingConfiguration_GetDetectionImages
            (
              NativeHandle,
              rawImages.Length,
              rawImages
            );

          if (obtained == rawImages.Length)
            break;

          rawImages = new IntPtr[Math.Abs(obtained)];
        }

        var images = new HashSet<IARReferenceImage>();
        var count = rawImages.Length;

        for (var i = 0; i < count; i++)
          images.Add(_NativeARReferenceImage._FromNativeHandle(rawImages[i]));

        return images.AsArdkReadOnly();
      }
      set
      {
        var rawImages =
          (from image in value select ((_NativeARReferenceImage)image)._NativeHandle).ToArray();

        _NARWorldTrackingConfiguration_SetDetectionImages
        (
          NativeHandle,
          rawImages,
          (UInt64)rawImages.Length
        );
      }
    }

    public virtual bool IsAutoFocusEnabled
    {
      get
      {
        return _NARWorldTrackingConfiguration_IsAutoFocusEnabled(NativeHandle) != 0;
      }
      set
      {
        _NARWorldTrackingConfiguration_SetAutoFocusEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public virtual bool IsSharedExperienceEnabled
    {
      get
      {
        return _NARWorldTrackingConfiguration_IsCollaborationEnabled(NativeHandle) != 0;
      }
      set
      {
        UInt32 uintValue = value ? 1 : (UInt32)0;
        _NARWorldTrackingConfiguration_SetCollaborationEnabled(NativeHandle, uintValue);
      }
    }

    public uint DepthTargetFrameRate
    {
      get
      {
        return _NARConfiguration_GetDepthTargetFrameRate(NativeHandle);
      }
      set
      {
        _NARConfiguration_SetDepthTargetFrameRate(NativeHandle, value);
      }
    }

    public uint SemanticTargetFrameRate
    {
      get
      {
        return _NARConfiguration_GetSemanticTargetFrameRate(NativeHandle);
      }
      set
      {
        _NARConfiguration_SetSemanticTargetFrameRate(NativeHandle, value);
      }
    }

    public bool IsDepthEnabled
    {
      get
      {
        return _NARConfiguration_IsDepthEnabled(NativeHandle) != 0;
      }
      set
      {
        _NARConfiguration_SetDepthEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public bool IsDepthPointCloudEnabled { get; set; }

    public bool IsSemanticSegmentationEnabled
    {
      get
      {
        return _NARConfiguration_IsSemanticSegmentationEnabled(NativeHandle) != 0;
      }
      set
      {
        _NARConfiguration_SetSemanticSegmentationEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public bool IsMeshingEnabled
    {
      get
      {
        return _NARConfiguration_IsMeshingEnabled(NativeHandle) != 0;
      }
      set
      {
        _NARConfiguration_SetMeshingEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public uint MeshingTargetFrameRate
    {
      get
      {
        return _NARConfiguration_GetMeshingTargetFrameRate(NativeHandle);
      }
      set
      {
        _NARConfiguration_SetMeshingTargetFrameRate(NativeHandle, value);
      }
    }

    [Obsolete("This property is obsolete. Use MeshDecimationThreshold instead.", false)]
    public float MeshingRadius { 
      get => MeshDecimationThreshold;
      set { MeshDecimationThreshold = value; }
    }

    public float MeshDecimationThreshold
    {
      get
      {
        return _NARConfiguration_GetMeshingRadius(NativeHandle);
      }
      set
      {
        if (value > 0 && value < 5)
        {
          ARLog._Error
          (
            "The smallest meshing radius possible is 5 meters. " +
            "Set the value to 0 for an infinite radius."
          );

          return;
        }

        _NARConfiguration_SetMeshingRadius(NativeHandle, value);
      }
    }

    public float MeshingRangeMax
    {
      get
      {
        return _NARConfiguration_GetViewDepthMax(NativeHandle);
      }
      set
      {
        if (value <= 0)
        {
          ARLog._Error
          (
            "The maximum meshing range must be larger then zero."
          );

          return;
        }

        if (_NativeAccess.IsNativeAccessValid())
          _NARConfiguration_SetViewDepthMax(NativeHandle, value);
      }
    }

    public float VoxelSize
    {
      get
      {
        return _NARConfiguration_GetVoxelSize(NativeHandle);
      }
      set
      {
        if (value <= 0)
        {
          ARLog._Error
          (
            "The voxel size must be larger than 0."
          );

          return;
        }

        if (_NativeAccess.IsNativeAccessValid())
          _NARConfiguration_SetVoxelSize(NativeHandle, value);
      }
    }

    public float MeshingTargetBlockSize
    {
      get
      {
        return _NARConfiguration_GetMeshingTargetBlockSize(NativeHandle);
      }
      set
      {
        _NARConfiguration_SetMeshingTargetBlockSize(NativeHandle, value);
      }
    }

    public bool IsPalmDetectionEnabled
    {
      get
      {
        return _NARConfiguration_IsPalmDetectionEnabled(NativeHandle) != 0;
      }
      set
      {
        if (_NativeAccess.IsNativeAccessValid())
          _NARConfiguration_SetPalmDetectionEnabled(NativeHandle, value ? 1 : (UInt32)0);
      }
    }

    public virtual void SetDetectionImagesAsync
    (
      IReadOnlyCollection<IARReferenceImage> detectionImages,
      Action completionHandler
    )
    {
      var rawImages =
        (from image in detectionImages select ((_NativeARReferenceImage)image)._NativeHandle).ToArray();

      _NARWorldTrackingConfiguration_SetDetectionImagesAsync
      (
        NativeHandle,
        rawImages,
        (UInt64)rawImages.Length,
        SafeGCHandle.AllocAsIntPtr(completionHandler),
        SetDetectionImagesAsyncCallback
      );
    }

    public override void CopyTo(IARConfiguration target)
    {
      if (!(target is IARWorldTrackingConfiguration worldTarget))
      {
        var msg =
          "ARWorldTrackingConfiguration cannot be copied into a non-ARWorldTrackingConfiguration.";

        throw new ArgumentException(msg);
      }

      base.CopyTo(target);

      worldTarget.PlaneDetection = PlaneDetection;
      worldTarget.IsAutoFocusEnabled = IsAutoFocusEnabled;
      worldTarget.IsSharedExperienceEnabled = IsSharedExperienceEnabled;
      worldTarget.DetectionImages = DetectionImages;

      worldTarget.IsDepthEnabled = IsDepthEnabled;
      worldTarget.DepthTargetFrameRate = DepthTargetFrameRate;
      worldTarget.IsDepthPointCloudEnabled = IsDepthPointCloudEnabled;

      worldTarget.IsSemanticSegmentationEnabled = IsSemanticSegmentationEnabled;
      worldTarget.SemanticTargetFrameRate = SemanticTargetFrameRate;

      worldTarget.IsMeshingEnabled = IsMeshingEnabled;
      worldTarget.MeshingTargetFrameRate = MeshingTargetFrameRate;
      worldTarget.MeshingTargetBlockSize = MeshingTargetBlockSize;
      worldTarget.MeshDecimationThreshold = MeshDecimationThreshold;
      worldTarget.MeshingRangeMax = MeshingRangeMax;
      worldTarget.VoxelSize = VoxelSize;
      worldTarget.IsPalmDetectionEnabled = IsPalmDetectionEnabled;
    }

    [MonoPInvokeCallback(typeof(_ARWorldTrackingConfiguration_SetDetectionImagesAsync_Handler))]
    private static void SetDetectionImagesAsyncCallback(IntPtr context)
    {
      var actionHandle = SafeGCHandle<Action>.FromIntPtr(context);
      var callback = actionHandle.TryGetInstance();
      actionHandle.Free();

      if (callback != null)
        _CallbackQueue.QueueCallback(() => { callback(); });
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARWorldTrackingConfiguration_Init();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARPlaybackWorldTrackingConfiguration_Init();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NARWorldTrackingConfiguration_GetSupportedVideoFormats
    (
      IntPtr nativeHandle,
      Int32 lengthOfOutSupportedVideoFormats,
      IntPtr[] outSupportedVideoFormats
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt64 _NARWorldTrackingConfiguration_GetPlaneDetection
    (
      IntPtr nativeHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARWorldTrackingConfiguration_SetPlaneDetection
    (
      IntPtr nativeHandle,
      UInt64 planeDetection
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NARWorldTrackingConfiguration_GetDetectionImages
    (
      IntPtr nativeHandle,
      Int32 lengthOfOutDetectionImages,
      IntPtr[] outDetectionImages
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARWorldTrackingConfiguration_SetDetectionImages
    (
      IntPtr nativeHandle,
      IntPtr[] detectionImages,
      UInt64 detectionImageCount
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARWorldTrackingConfiguration_SetDetectionImagesAsync
    (
      IntPtr nativeHandle,
      IntPtr[] detectionImages,
      UInt64 detectionImageCount,
      IntPtr applicationContext,
      _ARWorldTrackingConfiguration_SetDetectionImagesAsync_Handler completionHandler
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARWorldTrackingConfiguration_IsAutoFocusEnabled
    (
      IntPtr nativeHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARWorldTrackingConfiguration_SetAutoFocusEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARWorldTrackingConfiguration_IsCollaborationEnabled
    (
      IntPtr nativeHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARWorldTrackingConfiguration_SetCollaborationEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_IsDepthEnabled(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetDepthEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_GetDepthTargetFrameRate(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetDepthTargetFrameRate
    (
      IntPtr nativeHandle,
      UInt32 targetFrameRate
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_IsSemanticSegmentationEnabled(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetSemanticSegmentationEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_GetSemanticTargetFrameRate(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetSemanticTargetFrameRate
    (
      IntPtr nativeHandle,
      UInt32 targetFrameRate
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetSemanticThreshold
    (
      IntPtr nativeHandle,
      string channelName,
      float threshold
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_IsMeshingEnabled(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetMeshingEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_GetMeshingTargetFrameRate(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetMeshingTargetFrameRate
    (
      IntPtr nativeHandle,
      UInt32 targetFrameRate
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARConfiguration_GetMeshingRadius(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetMeshingRadius
    (
      IntPtr nativeHandle,
      float meshDecimationThreshold
    );
    
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARConfiguration_GetViewDepthMax(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetViewDepthMax
    (
      IntPtr nativeHandle,
      float viewDepthMax
    );
    
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARConfiguration_GetVoxelSize(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetVoxelSize
    (
      IntPtr nativeHandle,
      float voxelSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern float _NARConfiguration_GetMeshingTargetBlockSize(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetMeshingTargetBlockSize
    (
      IntPtr nativeHandle,
      float targetFrameRate
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NARConfiguration_CheckLidarDepthSupport();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NARConfiguration_CheckDepthEstimationSupport();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NARConfiguration_CheckDepthSupport();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NARConfiguration_CheckSemanticSegmentationSupport();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NARConfiguration_CheckMeshingSupport();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARConfiguration_IsPalmDetectionEnabled(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARConfiguration_SetPalmDetectionEnabled
    (
      IntPtr nativeHandle,
      UInt32 enabled
    );

    private delegate void _ARWorldTrackingConfiguration_SetDetectionImagesAsync_Handler
    (
      IntPtr context
    );
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.Utilities.Collections;
using Niantic.ARDK.Utilities.Logging;


namespace Niantic.ARDK.AR.Configuration
{
  internal sealed class _SerializableARWorldTrackingConfiguration:
    _SerializableARConfiguration,
    IARWorldTrackingConfiguration
  {
    public PlaneDetection PlaneDetection { get; set; }

    public bool IsAutoFocusEnabled { get; set; }

    public bool IsSharedExperienceEnabled { get; set; }

    public bool IsDepthEnabled { get; set; }

    public bool IsDepthPointCloudEnabled { get; set; }

    public uint DepthTargetFrameRate { get; set; }

    public bool IsSemanticSegmentationEnabled { get; set; }

    public uint SemanticTargetFrameRate { get; set; }

    public bool IsMeshingEnabled { get; set; }

    public uint MeshingTargetFrameRate { get; set; }
    
    [Obsolete("This property is obsolete. Use MeshDecimationThreshold instead.", false)]
    public float MeshingRadius { 
      get => MeshDecimationThreshold;
      set { MeshDecimationThreshold = value; }
    }

    public float MeshingTargetBlockSize { get; set; }
    
    public bool IsPalmDetectionEnabled { get; set; }

    public float MeshDecimationThreshold
    {
      get => _meshDecimationThreshold;
      set
      {
        if (value > 0 && value < MeshingRangeMax)
        {
          ARLog._Error
          (
            "The smallest mesh decimation threshold possible is the maximum meshing range " +
            "distance. Set the value to 0 for an infinite distance."
          );

          return;
        }

        _meshDecimationThreshold = value;
      }
    }

    private float _meshDecimationThreshold;
    
    public float MeshingRangeMax
    {
      get => _meshingRangeMax;
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
        
        _meshingRangeMax = value;
      }
    }

    private float _meshingRangeMax = 5f;
    
    public float VoxelSize
    {
      get => _voxelSize;
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
        
        _voxelSize = value;
      }
    }

    private float _voxelSize = 0.025f;

    private IReadOnlyCollection<IARReferenceImage> _detectionImages =
      EmptyArdkReadOnlyCollection<IARReferenceImage>.Instance;
    public IReadOnlyCollection<IARReferenceImage> DetectionImages
    {
      get => _detectionImages;
      set => _detectionImages = value;
    }

    public void SetDetectionImagesAsync
    (
      IReadOnlyCollection<IARReferenceImage> detectionImages,
      Action completionHandler
    )
    {
      _detectionImages = detectionImages;
      completionHandler();
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

      // Not copying DetectionImages because ARReferenceImage is not supported in Editor.
    }
  }
}

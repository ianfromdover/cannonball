// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

using Niantic.ARDK.AR.Awareness.Depth.Generators;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.AR.SLAM;
using Niantic.ARDK.Utilities.Collections;

namespace Niantic.ARDK.AR.Configuration
{
  public interface IARWorldTrackingConfiguration:
    IARConfiguration
  {
    /// A value specifying how and whether the session will detect real-world surfaces.
    /// @note Defaults to PlaneDetection.None.
    PlaneDetection PlaneDetection { get; set; }

    /// Used to get or set the reference images to detect when running this configuration.
    /// @note Not supported in Editor.
    IReadOnlyCollection<IARReferenceImage> DetectionImages { get; set; }

    /// A value specifying whether the camera should use autofocus or not when running.
    bool IsAutoFocusEnabled { get; set; }

    /// A boolean specifying whether the session will generate the necessary data to enable peer-to-peer AR experiences.
    /// Defaults to false
    bool IsSharedExperienceEnabled { get; set; }

    /// A boolean specifying whether or not depths are enabled.
    bool IsDepthEnabled { get; set; }

    /// A boolean specifying whether or not depth point cloud generation are enabled.
    bool IsDepthPointCloudEnabled { get; set; }

    /// A value specifying how many times the depth generation routine
    /// should target running per second.
    UInt32 DepthTargetFrameRate { get; set; }

    /// A boolean specifying whether or not semantic segmentation is enabled.
    bool IsSemanticSegmentationEnabled { get; set; }

    /// A value specifying how many times the semantic segmentation routine
    /// should target running per second.
    UInt32 SemanticTargetFrameRate { get; set; }

    /// A boolean specifying whether or not meshing is enabled.
    bool IsMeshingEnabled { get; set; }

    /// A value specifying how many times the meshing routine
    /// should target running per second.
    UInt32 MeshingTargetFrameRate { get; set; }
    
    /// The value specifying the distance, in meters, of the meshed surface around the player. Existing mesh blocks are
    /// decimated when distance to device is bigger than this threshold. Minimum distance is maximum meshing range.
    /// @note A value of 0 represents 'Infinity'
    [Obsolete("This property is obsolete. Use MeshDecimationThreshold instead.", false)]
    float MeshingRadius { get; set; }

    /// The value specifying the distance, in meters, of the meshed surface around the player. Existing mesh blocks are
    /// decimated when distance to device is bigger than this threshold. Minimum distance is maximum meshing range.
    /// @note A value of 0 represents 'Infinity'
    float MeshDecimationThreshold { get; set; }
    
    /// The value specifying the maximum range in meters of a depth measurement / estimation used
    /// for meshing.
    float MeshingRangeMax { get; set; }

    /// The value specifying the edge length of the meshing voxels in meters.
    float VoxelSize { get; set; }
    
    /// A value specifying the target size of a mesh block in meters.
    float MeshingTargetBlockSize { get; set; }

    /// A boolean specifying whether or not palms are detected.
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    bool IsPalmDetectionEnabled { get; set; }

    /// <summary>
    /// Set the detection images for this configuration asynchronously. The provided callback will
    ///   be called upon completion.
    /// </summary>
    void SetDetectionImagesAsync
    (
      IReadOnlyCollection<IARReferenceImage> detectionImages,
      Action completionHandler
    );
  }
}

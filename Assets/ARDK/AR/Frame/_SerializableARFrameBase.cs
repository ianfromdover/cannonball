// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Depth;
using Niantic.ARDK.AR.Awareness.Semantics;
using Niantic.ARDK.AR.Camera;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.Image;
using Niantic.ARDK.AR.LightEstimate;
using Niantic.ARDK.AR.PointCloud;
using Niantic.ARDK.AR.SLAM;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Collections;
using Niantic.ARDK.VirtualStudio.AR.Mock;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace Niantic.ARDK.AR.Frame
{
  [Serializable]
  internal abstract class _SerializableARFrameBase:
    _IARFrame
  {
    protected _SerializableARFrameBase()
    {
    }

    protected _SerializableARFrameBase
    (
      _SerializableImageBuffer capturedImageBuffer,
      _SerializableDepthBuffer depthBuffer,
      _SerializableSemanticBuffer buffer,
      _SerializableARCamera camera,
      _SerializableARLightEstimate lightEstimate,
      ReadOnlyCollection<IARAnchor> anchors, // Even native ARAnchors are directly serializable.
      _SerializableARMap[] maps,
      float worldScale,
      _SerializableARPointCloud featurePoints
    )
    {
      CapturedImageBuffer = capturedImageBuffer;
      DepthBuffer = depthBuffer;
      SemanticBuffer = buffer;
      Camera = camera;
      LightEstimate = lightEstimate;
      WorldScale = worldScale;
      RawFeaturePoints = featurePoints;
      Anchors = anchors;
      Maps = maps.AsNonNullReadOnly<IARMap>();
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      ReleaseImageAndTextures();
    }

    public ARFrameDisposalPolicy? DisposalPolicy { get; set; }
    public _SerializableImageBuffer CapturedImageBuffer { get; set; }
    public _SerializableDepthBuffer DepthBuffer { get; set; }
    public _SerializableSemanticBuffer SemanticBuffer { get; set; }

    public IDataBufferFloat32 CopySemanticConfidences(string channelName)
    {
      // Require keyframe semantics
      if (!(SemanticBuffer is {IsKeyframe: true}))
        return null;

      if (!SemanticBuffer.DoesChannelExist(channelName))
        return null;

      var data = new NativeArray<float>
      (
        (int)SemanticBuffer.Width * (int)SemanticBuffer.Height,
        Allocator.Persistent,
        NativeArrayOptions.UninitializedMemory
      );

#if ENABLE_UNITY_COLLECTIONS_CHECKS
      NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref data, AtomicSafetyHandle.Create());
#endif

      var flag = SemanticBuffer.GetChannelTextureMask(channelName);
      var source = SemanticBuffer.Data;
      for (int i = 0; i < source.Length; i++)
        data[i] = (source[i] & flag) != 0 ? 1.0f : 0.0f;

      return new _SerializeableAwarenessBufferF32
      (
        SemanticBuffer.Width,
        SemanticBuffer.Height,
        true,
        SemanticBuffer.ViewMatrix,
        data,
        SemanticBuffer.Intrinsics
      );
    }

    public IReadOnlyList<Detection> PalmDetections { get; set; }

    public _SerializableARCamera Camera { get; set; }
    public _SerializableARLightEstimate LightEstimate { get; set; }
    public ReadOnlyCollection<IARAnchor> Anchors { get; set; }
    public IDepthPointCloud DepthPointCloud { get; set; }
    public ReadOnlyCollection<IARMap> Maps { get; set; }
    public float WorldScale { get; set; }
    public _SerializableARPointCloud RawFeaturePoints { get; set; }

    public IntPtr[] CapturedImageTextures
    {
      get => EmptyArray<IntPtr>.Instance;
    }

    public abstract ReadOnlyCollection<IARHitTestResult> HitTest
    (
      int viewportWidth,
      int viewportHeight,
      Vector2 screenPoint,
      ARHitTestResultType types
    );

    public Matrix4x4 CalculateDisplayTransform
    (
      ScreenOrientation orientation,
      int viewportWidth,
      int viewportHeight
    )
    {
       // Cannot use Screen properties in Editor due to this bug: Unity Issue-598763
 #if UNITY_EDITOR
       return
         MathUtils.CalculateDisplayTransform
         (
           _MockFrameBufferProvider._ARImageWidth,
           _MockFrameBufferProvider._ARImageHeight,
           viewportWidth,
           viewportHeight,
           orientation,
           invertVertically: true
         );
#else
      throw new InvalidOperationException();
#endif
    }

    public void ReleaseImageAndTextures()
    {
      var capturedBuffer = CapturedImageBuffer;
      if (capturedBuffer != null)
      {
        CapturedImageBuffer = null;
        capturedBuffer.Dispose();
      }

      var depthBuffer = DepthBuffer;
      if (depthBuffer != null)
      {
        DepthBuffer = null;
        depthBuffer.Dispose();
      }

      var semanticBuffer = SemanticBuffer;
      if (semanticBuffer != null)
      {
        SemanticBuffer = null;
        semanticBuffer.Dispose();
      }
    }

    IARPointCloud IARFrame.RawFeaturePoints
    {
      get => RawFeaturePoints;
    }

    IImageBuffer IARFrame.CapturedImageBuffer
    {
      get => CapturedImageBuffer;
    }
    IDepthBuffer IARFrame.Depth
    {
      get => DepthBuffer;
    }

    ISemanticBuffer IARFrame.Semantics
    {
      get => SemanticBuffer;
    }

    IARCamera IARFrame.Camera
    {
      get => Camera;
    }

    IARLightEstimate IARFrame.LightEstimate
    {
      get => LightEstimate;
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.VirtualStudio.AR.Mock;

namespace Niantic.LightshipHub.Templates
{
  public class SegmentationController : MonoBehaviour
  {
    [HideInInspector]
    public Canvas Canvas;
    [HideInInspector]
    public ARSessionManager ARSessionManager = null;
    [HideInInspector]
    public ARSemanticSegmentationManager SemanticSegmentationManager;
    [HideInInspector]
    public Shader CustomShader;

    [Serializable]
    public struct Segmentation
    {
      [SerializeField]
      public MockSemanticLabel.ChannelName ChannelType;
      [SerializeField]
      public Texture2D Texture;
    }

    public Segmentation[] Segmentations;

    private Texture2D[] _mask = new Texture2D[8];
    private RawImage[] _rawImages = new RawImage[8];
    ARVideoFeed _videoFeed;

    void Start()
    {
      Application.targetFrameRate = 60;

      ARSessionManager.EnableFeatures();
      SemanticSegmentationManager.SemanticBufferUpdated += OnSemanticBufferUpdated;
      ARSessionFactory.SessionInitialized += OnSessionInitialized;

      int index = 0;
      foreach (Segmentation segm in Segmentations)
      {
        string channelName = segm.ChannelType.ToString().ToLower();

        RawImage segmentationOverlay = new GameObject(channelName + "Segmentation").AddComponent<RawImage>();
        segmentationOverlay.transform.SetParent(Canvas.transform);
        segmentationOverlay.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        segmentationOverlay.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        segmentationOverlay.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        segmentationOverlay.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        segmentationOverlay.rectTransform.anchorMin = new Vector2(0, 0);
        segmentationOverlay.rectTransform.anchorMax = new Vector2(1, 1);
        segmentationOverlay.transform.localScale = new Vector3(1, 1, 1);

        Material mat = new Material(CustomShader);
        segmentationOverlay.material = mat;

        _rawImages[index] = segmentationOverlay;
        index++;
      }

    }

    void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      Resolution resolution = new Resolution();
      resolution.width = Screen.width;
      resolution.height = Screen.height;
      ARSessionFactory.SessionInitialized -= OnSessionInitialized;

      _videoFeed = new ARVideoFeed(args.Session, resolution);
    }

    void OnSemanticBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
      ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;

      int index = 0;
      foreach (Segmentation segm in Segmentations)
      {
        string channelName = segm.ChannelType.ToString().ToLower();

        int channel = semanticBuffer.GetChannelIndex(channelName);
        SemanticSegmentationManager.SemanticBufferProcessor.CopyToAlignedTextureARGB32(
            texture: ref _mask[index],
            channel: channel,
            orientation: Screen.orientation
        );
        _rawImages[index].material.SetTexture("_Mask", _mask[index]);
        _rawImages[index].material.SetTexture("_Tex", segm.Texture);
        index++;
      }
    }
  }
}
// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Linq;
using System.Collections.Generic;
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
  public class ObjectMaskingController : MonoBehaviour
  {
    [HideInInspector]
    public Camera Camera;
    [HideInInspector]
    public Canvas Canvas;
    [HideInInspector]
    public ARSessionManager ARSessionManager = null;
    [HideInInspector]
    public ARSemanticSegmentationManager SemanticSegmentationManager;
    [HideInInspector]
    public Shader CustomShader;
    [HideInInspector]
    public GameObject SegmentationCamerasGO;
    [HideInInspector]
    public List<MockSemanticLabel.ChannelName> AllChannels;

    private Texture2D[] _mask = new Texture2D[7];
    private Image[] _image = new Image[7];
    private Camera[] _segmentationCamera = new Camera[7];
    private MockSemanticLabel.ChannelName[] _camerasToRender;
    private ARVideoFeed _videoFeed;

    void Awake()
    {
      int indx = 0;
      foreach (Camera camera in SegmentationCamerasGO.transform.GetComponentsInChildren<Camera>())
      {
        camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _segmentationCamera[indx] = camera;
        indx++;
      }
    }

    void Start()
    {
      Application.targetFrameRate = 60;

      ARSessionManager.EnableFeatures();
      SemanticSegmentationManager.SemanticBufferUpdated += OnSemanticBufferUpdated;
      ARSessionFactory.SessionInitialized += OnSessionInitialized;

      _camerasToRender = AllChannels.Distinct().ToArray();

      int index = -1;
      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        index++;
        if (!_camerasToRender.Contains(channel)) continue;

        string channelName = channel.ToString().ToLower();

        Image segmentationOverlay = new GameObject(channelName + "Segmentation").AddComponent<Image>();

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
        _image[index] = segmentationOverlay;
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

      int index = -1;
      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        index++;
        if (!_camerasToRender.Contains(channel)) continue;

        string channelName = channel.ToString().ToLower();

        SemanticSegmentationManager.SemanticBufferProcessor.CopyToAlignedTextureARGB32(
            texture: ref _mask[index],
            channel: semanticBuffer.GetChannelIndex(channelName),
            orientation: Screen.orientation
        );

        _image[index].material.SetTexture("_Mask", _mask[index]);
        _image[index].material.SetTexture("_CameraTex", _videoFeed.GPUTexture);
        _image[index].material.SetTexture("_SegmentationCameraTex", _segmentationCamera[index].activeTexture);
      }
    }
  }
}
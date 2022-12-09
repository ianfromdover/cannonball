// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;
using Niantic.ARDK.Configuration;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Rendering;

using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDKExamples
{
  /// This example showcases how to create textures from semantic segmentation data.
  /// ARDK supports two types of semantics data:
  /// 
  ///   Thresholded: This is accessed through IARFrame.Semantics or
  ///   _semanticSegmentationManager.SemanticBufferProcessor?.AwarenessBuffer when
  ///   using the manager. This buffer contains all semantic classifications encoded
  ///   in a 32-bit value per pixel, where each bit represent whether the network is
  ///   confident in the particular classification.
  ///
  ///   Confidences: This is accessed through IARFrame.CopySemanticConfidences(channel)
  ///   or _semanticSegmentationManager.GetConfidences(channel) when using the manager.
  ///   This floating point buffer contains precisely how confident was the network for
  ///   the specified classification for each pixel. The values range from 0.0f to 1.0f.
  public class SemanticSegmentationExampleManager:
    MonoBehaviour
  {
    [SerializeField]
    private ARSemanticSegmentationManager _semanticSegmentationManager;

    [SerializeField]
    private Material _overlayMaterial;

    [Header("UI")]
    [SerializeField]
    private GameObject _togglesParent;

    [SerializeField]
    private Text _toggleFeaturesButtonText;

    [SerializeField]
    private Text _toggleInterpolationText;

    [SerializeField]
    private Text _channelNameText;

    [SerializeField]
    private Text _selectedModeText;
    
    private Texture2D _semanticTexture;
    private bool _useThresholdedSemantics = true;
    private bool _semanticsUpdated = false;

    // All channels available in the model will be stored here. 
    private string[] _channels;
    
    // The index of the currently displayed semantics channel in the array
    private int _featureChannel;
    
    private void Start()
    {
      // Disable the UI while contextual awareness is initializing
      if (_togglesParent != null)
        _togglesParent.SetActive(false);
      
      // Enable the UI when the semantic segmentation stream starts
      _semanticSegmentationManager.SemanticBufferInitialized += EnableUserInterface;
      _semanticSegmentationManager.SemanticBufferUpdated += OnSemanticBufferUpdated;
    }

    private void OnSemanticBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
      if (!args.IsKeyFrame)
      {
        return;
      }

      // Get the name of the observed channel
      var currentChannelName = _channels[_featureChannel];
      
      if (_useThresholdedSemantics)
      {
        // Update the texture using thresholded semantics
        _semanticsUpdated = _semanticSegmentationManager.GetThresholdedARGB32
        (
          ref _semanticTexture,
          currentChannelName
        );
      }
      else
      {
        // Update the texture using confidence values
        _semanticsUpdated = _semanticSegmentationManager.GetConfidencesARGB32
        (
          ref _semanticTexture,
          currentChannelName
        );  
      }
    }

    // We use this callback to overlay semantics on the rendered background
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
      if (!_semanticSegmentationManager.enabled || !_semanticsUpdated)
      {
        Graphics.Blit(src, dest);
        return;
      }

      // Get the transformation to correctly map the texture to the viewport
      var sampler = _semanticSegmentationManager.SemanticBufferProcessor.SamplerTransform;

      // Update the transform
      _overlayMaterial.SetMatrix(PropertyBindings.SemanticsTransform, sampler);

      // Update the texture
      _overlayMaterial.SetTexture(PropertyBindings.SemanticChannel, _semanticTexture);

      // Display semantics
      Graphics.Blit(src, dest, _overlayMaterial);
    }

    private void OnDestroy()
    {
      // Release semantic overlay texture
      if (_semanticTexture != null)
        Destroy(_semanticTexture);
      
      _semanticSegmentationManager.SemanticBufferUpdated -= OnSemanticBufferUpdated;
    }

    private void EnableUserInterface(ContextAwarenessArgs<ISemanticBuffer> args)
    {
      _channels = _semanticSegmentationManager.SemanticBufferProcessor.Channels;
      
      _semanticSegmentationManager.SemanticBufferInitialized -= EnableUserInterface;
      
      if (_togglesParent != null)
        _togglesParent.SetActive(true);

      _channelNameText.text = _channels[_featureChannel];
      
      // Tell the manager for which channels the semantic confidence is needed.
      _semanticSegmentationManager.SetConfidenceChannels(_channels[_featureChannel]);
    }

    public void ChangeFeatureChannel()
    {
      // Increment the channel count with wraparound.
      _featureChannel += 1;
      if (_featureChannel == _channels.Length)
        _featureChannel = 0;
      
      // Tell the manager for which channels the semantic confidence is needed.
      _semanticSegmentationManager.SetConfidenceChannels(_channels[_featureChannel]);
      _channelNameText.text = _channels[_featureChannel];
    }

    public void ToggleSessionSemanticFeatures()
    {
      var newEnabledState = !_semanticSegmentationManager.enabled;
      _toggleFeaturesButtonText.text = newEnabledState ? "Disable Features" : "Enable Features";
      _semanticSegmentationManager.enabled = newEnabledState;
    }

    public void ToggleInterpolation()
    {
      var provider = _semanticSegmentationManager.SemanticBufferProcessor;
      var current = provider.InterpolationMode;
      provider.InterpolationMode = current == InterpolationMode.None
        ? InterpolationMode.Smooth
        : InterpolationMode.None;

      _toggleInterpolationText.text =
        provider.InterpolationMode != InterpolationMode.None
          ? "Disable Interpolation"
          : "Enable Interpolation";
    }

    public void ToggleBinaryAndConfidence()
    {
      _useThresholdedSemantics = !_useThresholdedSemantics;

      _selectedModeText.text = _useThresholdedSemantics
        ? "Mode:\nThresholded"
        : "Mode:\nConfidence";
    }
  }
}

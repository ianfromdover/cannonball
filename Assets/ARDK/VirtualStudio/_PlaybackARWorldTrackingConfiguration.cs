using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.ReferenceImage;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.VirtualStudio.AR.Configuration
{
  internal class _PlaybackARWorldTrackingConfiguration:
    _NativeARWorldTrackingConfiguration
  {
    internal _PlaybackARWorldTrackingConfiguration():
      base(_NARPlaybackWorldTrackingConfiguration_Init())
    {
      _isLightEstimationEnabledOverride = base.IsLightEstimationEnabled;
      _planeDetectionOverride = base.PlaneDetection;
    }

    private bool _isLightEstimationEnabledOverride;
    private PlaneDetection _planeDetectionOverride;

    public override bool IsLightEstimationEnabled
    {
      get { return _isLightEstimationEnabledOverride; }
      set
      {
        if (value)
        {
          ARLog._WarnRelease($"LightEstimation is not currently supported in Playback mode.");
          return;
        }

        _isLightEstimationEnabledOverride = value;
      }
    }

    public override WorldAlignment WorldAlignment
    {
      get { return base.WorldAlignment; }
      set
      {
        if (value != base.WorldAlignment)
        {
          ARLog._WarnRelease
          (
            "Changing WorldAlignment is not currently supported in Playback mode."
          );
        }
      }
    }

    public override PlaneDetection PlaneDetection
    {
      get { return _planeDetectionOverride; }
      set
      {
        if (value != PlaneDetection.None)
        {
          ARLog._WarnRelease("PlaneDetection is not currently supported in Playback mode.");
        }

        // TODO (AR-11051): Compare against actual value from dataset, for now it's always none
        // Warning for when PlaneAnchors are supported
        // if (!base.PlaneDetection.HasFlag(value))
        // {

          // ARLog._WarnRelease
          // (
          //   "Detected PlaneDetection flags that are not a subset of the PlaneDetection flags " +
          //   "enabled in the dataset. Only planes of types enabled in the " +
          //   "recording can be surfaced during playback."
          // );
        // }

        // _planeDetectionOverride = base.PlaneDetection & value;
      }
    }

    public override bool IsAutoFocusEnabled
    {
      get
      {
        // TODO AR-11051 Return actual value from dataset, for now, assume it's always true
        // return base.IsAutoFocusEnabled;

        return true;
      }
      set
      {
        ARLog._WarnRelease
        (
          "Changing IsAutoFocusEnabled is not currently supported in Playback mode."
        );
      }
    }

    public override bool IsSharedExperienceEnabled
    {
      get { return base.IsSharedExperienceEnabled; }
      set
      {
        if (value)
        {
          ARLog._WarnRelease
          (
            "Shared AR Experiences are not currently supported in Playback mode."
          );
        }
      }
    }

    private IReadOnlyCollection<IARReferenceImage> _detectionImagesOverride;
    public override IReadOnlyCollection<IARReferenceImage> DetectionImages
    {
      get { return _detectionImagesOverride; }
      set
      {
        if (value != null && value.Count > 0)
        {
          ARLog._WarnRelease("ImageDetection is not currently supported in Playback mode.");
        }

        // Warning for when ImageAnchors are supported
        // ARLog._WarnRelease
        // (
        //   "Image detection in PlaybackMode will surface image anchors with a matching name " +
        //   "in this collection, regardless of what the image actually is."
        // );
        //
        // _detectionImagesOverride = value;
      }
    }

    public override void SetDetectionImagesAsync
    (
      IReadOnlyCollection<IARReferenceImage> detectionImages,
      Action completionHandler
    )
    {
      DetectionImages = detectionImages;
      completionHandler?.Invoke();
    }

    public override void CopyTo(IARConfiguration target)
    {
      if (!(target is _PlaybackARWorldTrackingConfiguration playbackTarget))
      {
        var msg =
          "_PlaybackARWorldTrackingConfiguration can only be copied to another _PlaybackARWorldTrackingConfiguration.";

        throw new ArgumentException(msg);
      }

      base.CopyTo(target);

      playbackTarget._planeDetectionOverride = PlaneDetection;
      playbackTarget._isLightEstimationEnabledOverride = IsLightEstimationEnabled;
      playbackTarget._detectionImagesOverride = DetectionImages;
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARPlaybackWorldTrackingConfiguration_Init();
  }
}

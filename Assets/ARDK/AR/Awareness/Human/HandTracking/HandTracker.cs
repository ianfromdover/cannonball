// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Rendering;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.Awareness.Human
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class HandTracker
  {
    /// Alerts subscribers when hand tracking has changed either positions of detections appear
    /// or disappear or positions are different
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public event ArdkEventHandler<HumanTrackingArgs> HandTrackingStreamUpdated;

    // The currently active AR session
    private IARSession _session;

    // The render target descriptor used to determine the viewport resolution
    private RenderTarget _viewport;

    private Resolution _imageResolution;

    private ScreenOrientation _lastOrientation;
    private int _lastTargetWidth;
    private int _lastTargetHeight;

    private bool _didReceiveFirstUpdate;

    // Transform that converts normalized coordinates from AR image to viewport
    private Matrix4x4 _viewportTransform = Matrix4x4.identity;

    #region Public API

    /// Assigns a new render target descriptor for the hand tracking module.
    /// The render target defines the viewport attributes to correctly
    /// fit detections.
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public void AssignViewport(RenderTarget target)
    {
      _viewport = target;
    }

    /// Get the most recent hand tracking data
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HumanTrackingData TrackingData { get; private set; }

    /// Allocates a new hand tracking module. By default, the
    /// awareness buffer will be fit to the main camera's viewport.
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HandTracker(IARSession session)
    {
      _viewport = UnityEngine.Camera.main;
      _session = session;
      session.FrameUpdated += OnFrameUpdated;
      session.Deinitialized += HandleSessionDeinitialized;
    }

    /// Allocates a new hand tracking module.
    /// @param viewport Determines the target viewport to fit the awareness buffer to.
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public HandTracker(IARSession session, RenderTarget viewport)
    {
      _viewport = viewport;
      _session = session;
      session.FrameUpdated += OnFrameUpdated;
      session.Deinitialized += HandleSessionDeinitialized;
    }

    #endregion

    #region Implementation
    private void HandleSessionDeinitialized(ARSessionDeinitializedArgs arSessionDeinitializedArgs)
    {
      _session.FrameUpdated -= OnFrameUpdated;
      _session.Deinitialized -= HandleSessionDeinitialized;
    }

    private void OnFrameUpdated(FrameUpdatedArgs args)
    {
      var frame = args.Frame;
      if (frame == null)
        return;

      if (frame.PalmDetections == null)
      {
        if (TrackingData == null)
          return;

        TrackingData = null;
      }
      else
      {
        var targetOrientation = MathUtils.CalculateScreenOrientation();
        var targetResolution = _viewport.GetResolution(forOrientation: targetOrientation);

        // Check whether the viewport has been rotated and update the viewport transform
        var isViewportTransformDirty = !_didReceiveFirstUpdate ||
          _lastOrientation != targetOrientation ||
          _lastTargetWidth != targetResolution.width ||
          _lastTargetHeight != targetResolution.height;

        if (isViewportTransformDirty)
        {

          if (!_didReceiveFirstUpdate)
          {
            _didReceiveFirstUpdate = true;

            // Cache the image resolution
            _imageResolution = frame.Camera.CPUImageResolution;
          }

          _lastOrientation = targetOrientation;
          _lastTargetWidth = targetResolution.width;
          _lastTargetHeight = targetResolution.height;

          // Calculate the display transform of the AR frame for the current orientation
          var displayTransform = MathUtils.CalculateDisplayTransform
          (
            _imageResolution.width,
            _imageResolution.height,
            _lastTargetWidth,
            _lastTargetHeight,
            targetOrientation
          );

          // To go from camera image to viewport, we have to inverse the display transform and another
          // Y-axis invert because unity reads bottom up
          _viewportTransform = MathUtils.AffineInvertVertical() * Matrix4x4.Inverse(displayTransform);
        }

        TrackingData = new HumanTrackingData(frame.PalmDetections, _viewportTransform);
      }

      HandTrackingStreamUpdated?.Invoke(new HumanTrackingArgs(TrackingData));
    }
    #endregion
  }
}

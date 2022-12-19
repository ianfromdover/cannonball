// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace C_Scripts
{
  /// <summary>
  /// Helper script that finds planes and returns the result.
  /// On each updated frame, applies a hit test from the middle of the screen.
  /// Returns the result of the test.
  /// </summary>
  public class ARHitTestCenter: MonoBehaviour
  {
    /// The camera used to render the scene. Used to get the center of the screen.
    public Camera Camera;
    public IARHitTestResult Result { get; private set; }
    public bool IsPlaneDetected { get; private set; }
    private IARSession _session;

    private void Start()
    {
      ARSessionFactory.SessionInitialized += _SessionInitialized;
    }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= _SessionInitialized;

      var session = _session; 
      if (session != null) session.FrameUpdated -= _FrameUpdated;
    }

    private void _SessionInitialized(AnyARSessionInitializedArgs args)
    {
      var oldSession = _session;
      if (oldSession != null) oldSession.FrameUpdated -= _FrameUpdated;

      var newSession = args.Session;
      _session = newSession;
      newSession.FrameUpdated += _FrameUpdated;
    }

    /// <summary>
    /// Checks if the middle of the screen has a surface to
    /// place an object on
    /// </summary>
    /// <param name="args"></param>
    private void _FrameUpdated(FrameUpdatedArgs args)
    {
      var camera = Camera;
      if (camera == null)
      {
        IsPlaneDetected = false;
        return;
      }

      var viewportWidth = camera.pixelWidth;
      var viewportHeight = camera.pixelHeight;

      // Hit testing for cursor in the middle of the screen
      var middle = new Vector2(viewportWidth / 2f, viewportHeight / 2f);

      var frame = args.Frame;
      // Perform a hit test and either estimate a vertical plane
      // or an existing plane and its extents.
      var hitTestResults =
        frame.HitTest
        (
          viewportWidth,
          viewportHeight,
          middle,
          ARHitTestResultType.ExistingPlaneUsingExtent |
          ARHitTestResultType.EstimatedVerticalPlane
        );

      if (hitTestResults.Count == 0)
      {
        IsPlaneDetected = false;
        return;
      }

      IsPlaneDetected = true;
      Result = hitTestResults[0];
    }
  }
}

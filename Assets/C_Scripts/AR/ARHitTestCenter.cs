// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace C_Scripts
{
  //! Helper script that spawns a cursor on a plane if it finds one
  /// <summary>
  /// A sample class that can be added to a scene to demonstrate basic plane finding and hit
  ///   testing usage. On each updated frame, a hit test will be applied from the middle of the
  ///   screen and spawn a cursor if it finds a plane.
  /// </summary>
  public class ARHitTestCenter: MonoBehaviour
  {
    /// The camera used to render the scene. Used to get the center of the screen.
    public Camera Camera;
    public IARHitTestResult Result { get; private set; }
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
      if (camera == null) return;

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

      if (hitTestResults.Count == 0) return;
      Result = hitTestResults[0];
    }
  }
}

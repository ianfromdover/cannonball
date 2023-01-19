// Copyright 2022 Niantic, Inc. All Rights Reserved.

using C_Scripts.Event_Channels;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using UnityEngine;

namespace C_Scripts.AR
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
    [SerializeField] private EventChannel anchorsFound;
    
    // The result of the hit test. Used by ARObjectPlacer to place objects.
    public IARHitTestResult Result { get; private set; }
    public bool IsPlaneDetected { get; private set; }
    public bool DoPlanesExist { get; private set; }
    private IARSession _session;

    private void Start()
    {
      ARSessionFactory.SessionInitialized += _SessionInitialized;
      DoPlanesExist = false;
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
    /// place an object on. Sets the result if it does.
    /// </summary>
    private void _FrameUpdated(FrameUpdatedArgs args)
    {
      var camera = Camera;
      if (camera == null)
      {
        IsPlaneDetected = false;
        return;
      }

      // Hit test for cursor in the middle of the screen
      var viewportWidth = camera.pixelWidth;
      var viewportHeight = camera.pixelHeight;
      var middle = new Vector2(viewportWidth / 2f, viewportHeight / 2f);

      // Perform a hit test and either estimate a vertical plane
      // or an existing plane and its extents.
      var frame = args.Frame;
      var hitTestResults =
        frame.HitTest
        (
          viewportWidth,
          viewportHeight,
          middle,
          ARHitTestResultType.ExistingPlaneUsingExtent
        );

      if (hitTestResults.Count == 0)
      {
        IsPlaneDetected = false;
        return;
      }

      // the phone detected its first plane
      if (!DoPlanesExist)
      {
        DoPlanesExist = true;
        anchorsFound.Publish(); // allow the user to continue with the tutorial
      }
      
      IsPlaneDetected = true;
      Result = hitTestResults[0];
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using C_Scripts.Event_Channels;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;
using UnityEngine;

namespace C_Scripts.AR
{
  /// <summary>
  /// Spawns a preview object on a plane if it finds one,
  /// and then spawns the actual object in the preview location.
  ///
  /// In this project, the objects used are the moving target boards.
  /// </summary>
  public class ARObjectPlacer : MonoBehaviour
  {
    [SerializeField] private float reminderDist = 0.5f;
    [SerializeField] private GameObject previewObject;
    [SerializeField] private GameObject placementObject; 
    [SerializeField] private EventChannel tooCloseToTarget;
    [SerializeField] private ARHitTestCenter hitTester;

    /// The spawned preview object in the center of the screen.
    private GameObject _spawnedPreviewObject;
    
    /// The placed GameObject, to be destroyed on OnDestroy.
    private GameObject _placedObject;
    
    // flag for disabling preview object
    private bool _isBeingDisabled = false; 
    
    private IARSession _session;

    private void Start()
    {
      ARSessionFactory.SessionInitialized += _SessionInitialized;
    }

    private void OnDisable()
    {
      DestroySpawnedPreviewObject();
    }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= _SessionInitialized;

      var session = _session;
      if (session != null) session.FrameUpdated -= _FrameUpdated;

      DestroySpawnedPreviewObject();
      ClearObject();
    }

    /// <summary>
    /// Destroy the ghost preview object.
    ///
    /// TODO: This method is being called by OnDisable but in the build,
    /// the previewObject appears to not be destroyed. It is probably being
    /// respawned. The problem could lie in the AR session frame synchronisation
    /// or the anchors generated.
    /// </summary>
    private void DestroySpawnedPreviewObject()
    {
      if (_spawnedPreviewObject == null) return;

      _isBeingDisabled = true;
      Destroy(_spawnedPreviewObject);
      _spawnedPreviewObject = null;
    }

    private void _SessionInitialized(AnyARSessionInitializedArgs args)
    {
      var oldSession = _session;
      if (oldSession != null) oldSession.FrameUpdated -= _FrameUpdated;

      var newSession = args.Session;
      _session = newSession;
      newSession.FrameUpdated += _FrameUpdated;
      newSession.Deinitialized += _OnSessionDeinitialized;
    }

    private void _OnSessionDeinitialized(ARSessionDeinitializedArgs args)
    {
      DestroySpawnedPreviewObject();
      ClearObject();
    }

    private void _FrameUpdated(FrameUpdatedArgs args)
    {
      if (!hitTester.IsPlaneDetected) return;
      
      // prevent this method from re-enabling the preview object in the same frame.
      if (_isBeingDisabled)
      {
        _isBeingDisabled = false;
        return; // this will be the last frame updated because this game obj
                // will be disabled in the next frame
                // and that will disable this script component
      }

      // create preview object
      if (_spawnedPreviewObject == null)
      {
        _spawnedPreviewObject = Instantiate
        (
          previewObject, 
          Vector2.one, 
          Quaternion.identity
        );
      }

      // Set the preview object to
      // the hit test result's position and its anchor's rotation
      var result = hitTester.Result;
      _spawnedPreviewObject.transform.position = result.WorldTransform.ToPosition();
      _spawnedPreviewObject.transform.rotation = result.Anchor != null
        ? result.Anchor.Transform.ToRotation()
        : Quaternion.identity;
      
      // remind player if they are too close to the target
      if (result.Distance < reminderDist) tooCloseToTarget.Publish();
    }

    /// <summary>
    /// Places a new object where the preview was.
    /// If the object has already been placed, move it instead.
    /// </summary>
    public void PlaceObject()
    {
      if (_placedObject == null)
      {
        _placedObject = Instantiate
        (
          placementObject, 
          _spawnedPreviewObject.transform.position, 
          _spawnedPreviewObject.transform.rotation
        );
      }
      else
      {
        _placedObject.transform.SetPositionAndRotation
        (
          _spawnedPreviewObject.transform.position,
          _spawnedPreviewObject.transform.rotation
        ); 
      }
    }

    private void ClearObject()
    {
      Destroy(_placedObject);
    }
  }
}

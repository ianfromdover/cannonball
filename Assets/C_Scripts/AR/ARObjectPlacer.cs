// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace C_Scripts
{
  /// <summary>
  /// Spawns a preview object on a plane if it finds one,
  /// and then spawns the object in the same location when the player decides to.
  /// </summary>
  public class ARObjectPlacer : MonoBehaviour
  {
    public GameObject previewObject;
    public GameObject placementObject; 
    [SerializeField] private EventChannel tooCloseToTarget;
    [SerializeField] private ARHitTestCenter hitTester;
    [SerializeField] private float reminderDist = 0.5f;
    

    /// A reference to the spawned cursor in the center of the screen.
    private GameObject _spawnedPreviewObject;
    /// A reference to the placed gameobject to be destroyed on OnDestroy.
    private GameObject _placedObject; // singleton
    private IARSession _session;
    private bool _isBeingDisabled = false; // flag for disabling preview object
    
    private void Start() { ARSessionFactory.SessionInitialized += _SessionInitialized; }

    private void OnDisable() { DestroySpawnedCursor(); }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= _SessionInitialized;

      var session = _session;
      if (session != null) session.FrameUpdated -= _FrameUpdated;

      DestroySpawnedCursor();
      ClearObject();
    }

    private void DestroySpawnedCursor()
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
      DestroySpawnedCursor();
      ClearObject();
    }

    private void _FrameUpdated(FrameUpdatedArgs args)
    {
      if (!hitTester.IsPlaneDetected) return;
      if (_isBeingDisabled)
      {
        _isBeingDisabled = false;
        return;
      }

      if (_spawnedPreviewObject == null)
      {
        _spawnedPreviewObject = Instantiate
        (
          previewObject, 
          Vector2.one, 
          Quaternion.identity
        );
      }

      // Set the cursor object to the hit test result's position and its anchor's rotation
      var result = hitTester.Result;
      _spawnedPreviewObject.transform.position = result.WorldTransform.ToPosition();
      _spawnedPreviewObject.transform.rotation = result.Anchor != null
        ? result.Anchor.Transform.ToRotation()
        : Quaternion.identity;
      
      // remind player if the target is too close
      if (result.Distance < reminderDist)
      {
        tooCloseToTarget.Publish();
      }
    }

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
    
    private void ClearObject() { Destroy(_placedObject); }
  }
}

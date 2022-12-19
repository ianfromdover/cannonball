// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace C_Scripts
{
  /// <summary>
  /// Automatically positions the scene rendering AR content and transforms its output
  /// Shakes the camera when a shot is fired
  /// </summary>
  public class ARCameraController: MonoBehaviour
  {
    [SerializeField]
    [Tooltip("The scene camera used to render AR content.")]
    private Camera _camera;

    /// Returns a reference to the scene camera used to render AR content, if present.
    public Camera Camera
    {
      get => _camera;
      set => _camera = value;
    }

    [SerializeField] private float shakeAmount = 0.03f;
    private IARSession _session;
    private Vector3 _cameraNextPos;
    private float _currShakeAmount = 0;

    private void Start()
    {
      ARSessionFactory.SessionInitialized += _OnSessionInitialized;
    }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= _OnSessionInitialized;

      var session = _session;
      if (session != null)
        session.FrameUpdated -= _FrameUpdated;
    }

    private void _OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      var oldSession = _session;
      if (oldSession != null)
        oldSession.FrameUpdated -= _FrameUpdated;

      var newSession = args.Session;
      _session = newSession;
      newSession.FrameUpdated += _FrameUpdated;
    }

    private void _FrameUpdated(FrameUpdatedArgs args)
    {
      var localCamera = Camera;
      if (localCamera == null)
        return;

      var session = _session;
      if (session == null)
        return;

      // Set the camera's position.
      var worldTransform = args.Frame.Camera.GetViewMatrix(Screen.orientation).inverse;
      _cameraNextPos = worldTransform.ToPosition();
      localCamera.transform.rotation = worldTransform.ToRotation();
      
      _currShakeAmount = Mathf.Lerp(_currShakeAmount, 0, 0.02f);
      localCamera.transform.position = _cameraNextPos + Random.onUnitSphere * _currShakeAmount;
      // todo: shake not working
    }
    
    public void Shake()
    {
        _currShakeAmount = shakeAmount;
    }
  }
}

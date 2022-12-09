// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class PlaneTrackerController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    [HideInInspector]
    public ARPlaneManager PlaneManager;
    public enum PlaneOption { Horizontal, Vertical };

    public bool Transition = true;
    public bool ShowPlaneHelper = true;
    public PlaneOption PlaneToTrack = PlaneOption.Horizontal;

    private bool _animationRunning = false;
    private Vector3 _targetPosition;
    private Vector3 planeNormal;
    private Quaternion currentRotation;

    void Awake()
    {
      if (!ShowPlaneHelper) PlaneManager.PlanePrefab = null;
    }

    void Start()
    {
      switch (PlaneToTrack)
      {
        case PlaneOption.Horizontal:
          PlaneManager.DetectedPlaneTypes = PlaneDetection.Horizontal;
          break;
        case PlaneOption.Vertical:
          PlaneManager.DetectedPlaneTypes = PlaneDetection.Vertical;
          break;
        default:
          Debug.Log("No option picked");
          break;
      }
    }

    void Update()
    {
      if (_animationRunning) TranslateToPosition();

      if (PlatformAgnosticInput.touchCount <= 0) return;

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.phase == TouchPhase.Began)
      {
        TouchBegan(touch);
      }
    }

    void TranslateToPosition()
    {
      if (!_animationRunning) return;

      float speed = 1.0f;
      float step = speed * Time.deltaTime;
      Vector3 direction = (_targetPosition - OHcontroller.ObjectHolder.transform.position).normalized;
      OHcontroller.ObjectHolder.transform.Translate(Vector3.forward * step, Space.Self);
      float angle = Vector3.SignedAngle(OHcontroller.ObjectHolder.transform.forward, direction, planeNormal.normalized);
      OHcontroller.ObjectHolder.transform.rotation = Quaternion.RotateTowards(OHcontroller.ObjectHolder.transform.rotation, Quaternion.LookRotation(direction, planeNormal), 8f);
      if (Vector3.Distance(OHcontroller.ObjectHolder.transform.position, _targetPosition) < 0.1f)
      {
        _animationRunning = false;
      }
    }

    private void TouchBegan(Touch touch)
    {
      var currentFrame = OHcontroller.Session.CurrentFrame;
      if (currentFrame == null) return;

      if (OHcontroller.Camera == null) return;

      var hitTestResults = currentFrame.HitTest(
          OHcontroller.Camera.pixelWidth,
          OHcontroller.Camera.pixelHeight,
          touch.position,
          ARHitTestResultType.EstimatedHorizontalPlane
      );

      var hitTestResultsLeft = currentFrame.HitTest(
          OHcontroller.Camera.pixelWidth,
          OHcontroller.Camera.pixelHeight,
          touch.position + new Vector2(5.0f, .0f),
          ARHitTestResultType.EstimatedHorizontalPlane
      );

      var hitTestResultsBottom = currentFrame.HitTest(
          OHcontroller.Camera.pixelWidth,
          OHcontroller.Camera.pixelHeight,
          touch.position + new Vector2(.0f, 5.0f),
          ARHitTestResultType.EstimatedHorizontalPlane
      );

      if (hitTestResults.Count <= 0) return;
      if (hitTestResultsLeft.Count <= 0) return;
      if (hitTestResultsBottom.Count <= 0) return;

      Vector3 posO = hitTestResults[0].WorldTransform.ToPosition();
      Vector3 posA = hitTestResultsLeft[0].WorldTransform.ToPosition();
      Vector3 posB = hitTestResultsBottom[0].WorldTransform.ToPosition();
      Vector3 normal = Vector3.Cross(posB - posO, posA - posO).normalized;

      Vector3 plane = Vector3.ProjectOnPlane(Vector3.forward + Vector3.right, normal);
      Quaternion rotation = Quaternion.LookRotation(plane, normal);
      currentRotation = rotation;
      planeNormal = normal;

      var position = hitTestResults[0].WorldTransform.ToPosition();
      OHcontroller.ObjectHolder.SetActive(true);

      if (Transition)
      {
        if (_targetPosition != position &&
            ((Math.Abs(_targetPosition.x - position.x) < 0.01f) ||
            (Math.Abs(_targetPosition.y - position.y) < 0.01f) ||
            (Math.Abs(_targetPosition.z - position.z) < 0.01f)))
        {
          _targetPosition = position;
          _animationRunning = true;
        }
        else
        {
          _targetPosition = position;
          OHcontroller.ObjectHolder.transform.position = position;
          OHcontroller.ObjectHolder.transform.rotation = rotation;
        }
      }
      else
      {
        OHcontroller.ObjectHolder.transform.position = position;
        OHcontroller.ObjectHolder.transform.rotation = rotation;
      }
    }
  }
}

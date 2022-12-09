// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

using Niantic.ARDK.Utilities;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class PlacementController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    public bool MultipleInstances;

    void Update()
    {
      if (PlatformAgnosticInput.touchCount <= 0) return;

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.phase == TouchPhase.Began)
      {
        TouchBegan(touch);
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

      if (hitTestResults.Count <= 0) return;

      var position = hitTestResults[0].WorldTransform.ToPosition();

      GameObject obj;
      if (MultipleInstances)
      {
        obj = Instantiate(OHcontroller.ObjectHolder, this.transform);
        obj.SetActive(true);
      }
      else
      {
        obj = OHcontroller.ObjectHolder;
      }

      obj.SetActive(true);
      obj.transform.position = position;
      obj.transform.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }
  }
}
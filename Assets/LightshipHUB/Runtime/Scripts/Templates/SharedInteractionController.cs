// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.LightshipHub.Tools;

namespace Niantic.LightshipHub.Templates
{
  public class SharedInteractionController : MonoBehaviour
  {
    [HideInInspector]
    public SharedSession SharedSession;
    public float TriggerDistance = 1.5f;

    private void Update()
    {
      float distance = Vector3.Distance(SharedSession.SharedObjectHolder.transform.position, Camera.main.gameObject.transform.position);

      if (distance <= TriggerDistance)
      {
        if (SharedSession._isHost) SharedSession.SharedObjectHolder.ObjectInteraction.AnimateObjectDistance();
        else SharedSession._messagingManager.AskHostToAnimateObjectDistance(SharedSession._host);
      }

      if (PlatformAgnosticInput.touchCount <= 0) return;

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.phase == TouchPhase.Began)
      {
        TouchBegan(touch);
      }
    }

    private void TouchBegan(Touch touch)
    {
      var currentFrame = SharedSession._arNetworking.ARSession.CurrentFrame;
      if (currentFrame == null) return;
      if (SharedSession._camera == null) return;

      var worldRay = SharedSession._camera.ScreenPointToRay(touch.position);
      RaycastHit hit;

      if (Physics.Raycast(worldRay, out hit, 1000f))
      {
        if (hit.transform.IsChildOf(SharedSession.SharedObjectHolder.gameObject.transform))
        {
          if (SharedSession._isHost) SharedSession.SharedObjectHolder.ObjectInteraction.AnimateObjectTap();
          else SharedSession._messagingManager.AskHostToAnimateObjectTap(SharedSession._host);
        }
      }
      else
      {
        var hitTestResults = currentFrame.HitTest(
            SharedSession._camera.pixelWidth,
            SharedSession._camera.pixelHeight,
            touch.position,
            ARHitTestResultType.EstimatedHorizontalPlane
        );

        if (hitTestResults.Count <= 0) return;

        var position = hitTestResults[0].WorldTransform.ToPosition();

        if (SharedSession._isHost)
        {
          if (!SharedSession.SharedObjectHolder.gameObject.activeSelf && SharedSession._isStable)
          {
            SharedSession.SharedObjectHolder.gameObject.SetActive(true);
          }
          SharedSession.SharedObjectHolder.MoveObject(position);
        }
        else
        {
          SharedSession._messagingManager.AskHostToMoveObject(SharedSession._host, position);
        }
      }
    }
  }
}

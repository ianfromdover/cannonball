// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class ObjectInteraction : MonoBehaviour
  {
    [HideInInspector]
    public InteractionController InteractionController;
    [Serializable]
    public class AREvent : UnityEvent { }
    public AREvent OnClick = new AREvent();
    public AREvent OnDistance = new AREvent();

    private ObjectAnimation objectAnimation;

    void Awake()
    {
      objectAnimation = GetComponent<ObjectAnimation>();
    }

    void Update()
    {
      if (InteractionController != null)
      {
        float distance = Vector3.Distance(this.transform.position, Camera.main.gameObject.transform.position);

        if (distance <= InteractionController.TriggerDistance)
        {
          OnDistance.Invoke();
        }
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
      var currentFrame = InteractionController.OHcontroller.Session.CurrentFrame;
      if (currentFrame == null) return;

      if (InteractionController.OHcontroller.Camera == null) return;

      CheckHit(touch);
    }

    private void CheckHit(Touch touch)
    {
      var worldRay = InteractionController.OHcontroller.Camera.ScreenPointToRay(touch.position);
      RaycastHit hit;

      if (Physics.Raycast(worldRay, out hit, 1000f))
      {
        ObjectAnimation objAnimation = hit.transform.GetComponentInParent(typeof(ObjectAnimation)) as ObjectAnimation;
        if (objAnimation != null && objAnimation == objectAnimation)
        {
          OnClick.Invoke();
        }
      }
    }
  }
}

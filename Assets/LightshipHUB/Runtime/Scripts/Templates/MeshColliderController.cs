// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class MeshColliderController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;

    void Update()
    {
      if (PlatformAgnosticInput.touchCount <= 0) { return; }

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.phase == TouchPhase.Began)
      {
        GameObject obj = Instantiate(OHcontroller.ObjectHolder, this.transform);
        obj.SetActive(true);


        Vector3 entrancePoint = OHcontroller.Camera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, OHcontroller.Camera.nearClipPlane));
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0f, 0f, 0f);
        rb.angularVelocity = new Vector3(0f, 0f, 0f);

        obj.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        obj.transform.position = entrancePoint;

        float force = 200.0f;
        rb.AddForce(OHcontroller.Camera.transform.forward * force);
      }
    }
  }
}

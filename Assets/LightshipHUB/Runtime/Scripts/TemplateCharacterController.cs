// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class TemplateCharacterController: MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    private GameObject character;
    [SerializeField]
    private bool interactWithPhysics = false;
    private bool objectSpawned = false;
    private float speed = .5f;
    private Vector3 touchPoint;
    private Animator characterAnim;

    void Update()
    {
      if (PlatformAgnosticInput.touchCount <= 0) return;

      var touch = PlatformAgnosticInput.GetTouch(0);

      if (touch.phase == TouchPhase.Began) touchPoint = TouchBegan(touch);

      if (objectSpawned) Move(character, touchPoint);

      else SpawnObject();
       
      if (touch.phase == TouchPhase.Ended && characterAnim != null) characterAnim.SetFloat("walkspeed", 0f);
    }

    private Vector3 TouchBegan(Touch touch)
    {
      var currentFrame = OHcontroller.Session.CurrentFrame;
      if (currentFrame == null) return OHcontroller.ObjectHolder.transform.position;

      if (OHcontroller.Camera == null) return OHcontroller.ObjectHolder.transform.position;

      var worldRay = Camera.main.ScreenPointToRay(touch.position);
      RaycastHit hit;

      if (Physics.Raycast(worldRay, out hit, 1000f))
      {
        if (hit.transform.gameObject.name.Contains("MeshCollider") ||
          hit.transform.gameObject.name.Contains("Interior_"))
        {
          return hit.point;
        }
      }
      return OHcontroller.ObjectHolder.transform.position;
    }

    private void Move(GameObject obj, Vector3 destination)
    {
      obj.transform.position = Vector3.MoveTowards(obj.transform.position, destination, speed * Time.deltaTime);

      float distance = Vector3.Distance(destination, obj.transform.position);

      if (characterAnim != null && distance >= 0.02f) characterAnim.SetFloat("walkspeed", 0.2f);
      else if (characterAnim != null && distance < 0.02f) characterAnim.SetFloat("walkspeed", 0);

      Vector3 targetDirection = (destination - obj.transform.position).normalized;
      if (targetDirection != Vector3.zero && distance > 0.02f) obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, Quaternion.LookRotation(targetDirection, Vector3.up), 20f);
    }

    private void SpawnObject()
    {
      GameObject objHolder = OHcontroller.ObjectHolder;
      var cursor = objHolder.transform.Find("cursor");
      if (cursor != null) Destroy(cursor.gameObject);

      objHolder.SetActive(true);
      objHolder.transform.position = touchPoint;
      character = objHolder.transform.gameObject;
      characterAnim = character.GetComponentInChildren<Animator>();
      if (character.GetComponent<Rigidbody>() == null)
      {
        Rigidbody charRb = character.AddComponent<Rigidbody>();
        charRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        charRb.isKinematic = !interactWithPhysics;
      }
      objectSpawned = true;
    }
  }
}

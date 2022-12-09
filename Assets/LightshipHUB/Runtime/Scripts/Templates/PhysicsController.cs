// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

namespace Niantic.LightshipHub.Templates
{
  public class PhysicsController : MonoBehaviour
  {
    //User preferences
    [Header("Object physics in runtime")]
    [Tooltip("Size of the object")]
    [SerializeField]
    [Range(0.01f, 5)]
    private float scale = 1;

    [Tooltip("Initial speed when spawning")]
    [SerializeField]
    [Range(0.5f, 5)]
    private float launchForce = 1;

    [SerializeField]
    [Range(0, 1)]
    private float bounciness;

    [Tooltip("Having zero friction should seem like standing on ice")]
    [SerializeField]
    [Range(0, 50)]
    private float friction = 0.6f;

    [Tooltip("Spin the object")]
    [SerializeField]
    [Range(0, 50)]
    private byte spiningForce;

    [Tooltip("Adds drag force")]
    [SerializeField]
    [Range(0, 5)]
    private float slowliness;

    [SerializeField]
    private bool rockSolid, zeroGravity;

    public void Start()
    {
      Rigidbody rb = this.gameObject.GetComponent<Rigidbody>();
      float defaultForce = 300.0f;
      rb.AddForce(Camera.main.transform.forward * defaultForce);
      rb.AddRelativeForce(Camera.main.transform.forward * launchForce, ForceMode.Impulse);

      //User preferences
      ApplyUserPreferencesToTransform(this.gameObject);
      ApplyUserPreferencesToRigidbody(rb);
      ApplyUserPreferencesToCollider(this.gameObject);
    }

    private void ApplyUserPreferencesToTransform(GameObject obj)
    {
      Vector3 newScale = new Vector3(scale, scale, scale);
      obj.transform.localScale = newScale;
    }

    private void ApplyUserPreferencesToRigidbody(Rigidbody rig)
    {
      if (rockSolid)
      {
        rig.constraints = RigidbodyConstraints.FreezeRotation;
        rig.mass = 50000f;
      }
      if (zeroGravity) rig.useGravity = false;
      rig.drag = slowliness;
      if (slowliness <= 1) rig.angularDrag = slowliness;
      rig.AddTorque(transform.up * spiningForce / 2, ForceMode.Impulse);
    }

    private void ApplyUserPreferencesToCollider(GameObject obj)
    {
      if (obj == null) return;

      Collider col = obj.GetComponent<MeshCollider>();

      if (col != null) {
        col.material = new PhysicMaterial();
        col.material.bounciness = bounciness;
        AdjustBouncinessCombine(col);
        col.material.dynamicFriction = friction;
        col.material.staticFriction = friction;
      }

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        ApplyUserPreferencesToCollider(child.gameObject);
      }
    }

    private void AdjustBouncinessCombine(Collider coll)
    {
      if (bounciness >= .8) coll.material.bounceCombine = PhysicMaterialCombine.Maximum;
      if (bounciness >= .5 && bounciness < .8) coll.material.bounceCombine = PhysicMaterialCombine.Multiply;
      if (bounciness < .5) coll.material.bounceCombine = PhysicMaterialCombine.Average;
    }
  }
}
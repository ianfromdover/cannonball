// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class PhysicsSceneController : MonoBehaviour
  {
    private List<GameObject> objectHolders = new List<GameObject>();
    private int objCount, maxObjects = 50;
  
    private void Start()
    {
      foreach (Transform child in this.gameObject.transform)
      {
        if (child.gameObject.name == "PreloadManager") continue;
        var cursor = child.gameObject.transform.Find("cursor");
        if (cursor != null) Destroy(cursor.gameObject);
        child.gameObject.SetActive(false);
        objectHolders.Add(child.gameObject);
      }
    }

    void Update()
    {
      if (PlatformAgnosticInput.touchCount <= 0) return;
      var touch = PlatformAgnosticInput.GetTouch(0);
      if (objCount <= maxObjects)
      {
        if (touch.phase == TouchPhase.Began) TouchBegan(touch);
      }
    }
    private void TouchBegan(Touch touch)
    {
      int rd = Random.Range(0, objectHolders.Count);
      GameObject objectHolder = Instantiate(objectHolders[rd]);

      if (!objectHolder.activeSelf)
      {
        objectHolder.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        objectHolder.transform.rotation = Quaternion.identity;
        objectHolder.transform.SetParent(this.gameObject.transform);
        objectHolder.SetActive(true);
        objCount++;
        
        StartCoroutine(DeactivateObject(objectHolder));
      }
    }
    private IEnumerator DeactivateObject(GameObject obj)
    {
      int seconds = Random.Range(10, 25);
      yield return new WaitForSeconds(seconds);
      obj.SetActive(false);
      objCount--;
    }
  }
}

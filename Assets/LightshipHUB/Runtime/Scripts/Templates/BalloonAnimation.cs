// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace Niantic.LightshipHub.Templates
{
  public class BalloonAnimation : MonoBehaviour
  {
    private bool _animationRunning = false;
    private float _speed = 0.2f;

    void Start()
    {
      MoveToNext();
    }

    private void MoveToNext()
    {
      if (_animationRunning) return;

      float x = Random.Range(-4.0f, -8.0f);
      float y = Random.Range(0.0f, 2.5f);
      float z = Random.Range(4.0f, 12.0f);
      var nextTargetPosition = new Vector3(x, y, z);

      StartCoroutine(MoveToTargetSmooth(nextTargetPosition));
    }

    private IEnumerator MoveToTargetSmooth(Vector3 targetPos)
    {
      if (_animationRunning) yield break;

      _animationRunning = true;

      var startPos = transform.position;
      var distance = Vector3.Distance(startPos, targetPos);
      var duration = distance / _speed;

      var timePassed = 0f;

      while (timePassed < duration)
      {
        var factor = timePassed / duration;
        factor = Mathf.SmoothStep(0, 1, factor);
        transform.position = Vector3.Lerp(startPos, targetPos, factor);

        yield return null;

        timePassed += Time.deltaTime;
      }

      transform.position = targetPos;
      _animationRunning = false;
      MoveToNext();
    }
  }
}

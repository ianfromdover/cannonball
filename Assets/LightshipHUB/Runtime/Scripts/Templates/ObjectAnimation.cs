// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

namespace Niantic.LightshipHub.Templates
{
  public class ObjectAnimation : MonoBehaviour
  {
    private bool _animationRuning, _scaleAnimationRunning, _scaleInOutRunning, _rotateAnimationRunning;
    private float _currentTargetScale, _currentRotation;

    void Awake()
    {
      foreach (Transform child in transform)
      {
        if (!child.name.Equals("cursor"))
        {
          AddMeshColliderGameObject(child.gameObject);
        }
      }
    }

    public static void AddMeshColliderGameObject(GameObject obj)
    {
      if (obj == null) return;

      MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
      MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
      SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();

      if (meshCollider == null && (meshFilter != null || skinnedMesh != null))
      {
        var collider = obj.AddComponent<MeshCollider>();
        collider.convex = true;
        if (skinnedMesh != null) collider.sharedMesh = skinnedMesh.sharedMesh;
      }

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        AddMeshColliderGameObject(child.gameObject);
      }
    }

    void Update()
    {
      if (_rotateAnimationRunning) _Rotate(true);
      if (_scaleAnimationRunning) _Scale(true);
      if (_scaleInOutRunning) _ScaleInOut(true);
    }

    public void Rotate()
    {
      _Rotate(false);
    }

    public void Scale()
    {
      _Scale(false);
    }

    public void ScaleInOut()
    {
      _ScaleInOut(false);
    }

    public void _Rotate(bool updating = false)
    {
      if (!_rotateAnimationRunning)
      {
        _rotateAnimationRunning = true;
        _currentRotation = 0;
      }

      if (!updating) return;

      var rotationStep = 450 * Time.deltaTime;
      _currentRotation += rotationStep;

      transform.Rotate(0, rotationStep, 0);

      if (_currentRotation >= 360)
      {
        _rotateAnimationRunning = false;
      }
    }

    public void _Scale(bool updating = false)
    {
      if (!_scaleAnimationRunning)
      {
        _scaleAnimationRunning = true;
        this.transform.localScale = Vector3.zero;
        _currentTargetScale = UnityEngine.Random.Range(0.3f, 1.0f);
      }

      if (!updating) return;

      float speed = 3.0f;
      float step = speed * Time.deltaTime;
      this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, new Vector3(_currentTargetScale, _currentTargetScale, _currentTargetScale), step);

      if (this.transform.localScale == new Vector3(_currentTargetScale, _currentTargetScale, _currentTargetScale))
      {
        _scaleAnimationRunning = false;
      }
    }

    public void _ScaleInOut(bool updating = false)
    {
      if (!_scaleInOutRunning)
      {
        _scaleInOutRunning = true;
        _currentTargetScale = this.transform.localScale.x == 1.0f ? 0.5f : 1.0f;
      }

      if (!updating) return;

      float speed = 3.0f;
      float step = speed * Time.deltaTime;
      this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, new Vector3(_currentTargetScale, _currentTargetScale, _currentTargetScale), step);

      if (this.transform.localScale.x == _currentTargetScale)
      {
        _scaleInOutRunning = false;
      }
    }
  }
}
// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;

namespace Niantic.LightshipHub.Templates
{
  public class MushroomAnimation : MonoBehaviour
  {
    SkinnedMeshRenderer skinnedMeshRenderer;
    float bendOffset;
    float bendRandomSpeed;

    void Awake()
    {
      skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    void Start()
    {
      bendOffset = Random.Range(0.0f, 6.2f);
      bendRandomSpeed = Random.Range(0.2f, 4.0f);
    }

    void FixedUpdate()
    {
      AnimationInOut(2, bendRandomSpeed, bendOffset);
    }

    public void AnimationInOut(int blendShape, float speed, float offset)
    {
      var animationTime = Mathf.Sin(Time.time * speed + offset) * 0.5f + 0.5f;
      skinnedMeshRenderer.SetBlendShapeWeight(blendShape, animationTime * 100);
    }
  }
}

using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Camera-shaking script, placed on a normal camera.
/// Used only in the cannon testing scene.
/// For the shaking on the AR camera, see ARCameraController.cs
/// </summary>
public class ScreenShakeAnim : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private EventChannel onShot;
    private float _currShakeAmount = 0;
    Vector3 _cameraStartPos;

    void Start()
    {
        _cameraStartPos = transform.position;
        onShot.OnChange += Shake;
    }

    void Update()
    {
        _currShakeAmount = Mathf.Lerp(_currShakeAmount, 0, 0.02f);
        transform.position = _cameraStartPos + Random.onUnitSphere * _currShakeAmount;
    }

    private void OnDestroy()
    {
        onShot.OnChange -= Shake;
    }

    public void Shake()
    {
        _currShakeAmount = shakeAmount;
    }
}
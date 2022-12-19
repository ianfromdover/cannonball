using UnityEngine;

public class ScreenShakeAnim : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 0.1f;
    private float _currShakeAmount = 0;
    Vector3 _cameraStartPos;

    void Start()
    {
        _cameraStartPos = transform.position;
    }

    void Update()
    {
        _currShakeAmount = Mathf.Lerp(_currShakeAmount, 0, 0.02f);
        transform.position = _cameraStartPos + Random.onUnitSphere * _currShakeAmount;
    }

    public void Shake()
    {
        _currShakeAmount = shakeAmount;
    }
}
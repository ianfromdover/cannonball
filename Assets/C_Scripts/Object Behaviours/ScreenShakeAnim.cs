using C_Scripts.Event_Channels;
using UnityEngine;
using Random = UnityEngine.Random;

namespace C_Scripts.Object_Behaviours
{
    /// <summary>
    /// Shakes the camera upon firing it. Used only in the start scene on a normal camera.
    /// For the shaking on the AR camera, see ARCameraController.cs in the AR folder.
    /// </summary>
    public class ScreenShakeAnim : MonoBehaviour
    {
        [SerializeField] private float shakeAmount = 0.003f;
        [SerializeField] private EventChannel onShot;
        
        private float _currShakeAmount = 0;
        private Vector3 _cameraStartPos;

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
}
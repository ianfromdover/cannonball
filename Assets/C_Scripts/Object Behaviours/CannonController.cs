using C_Scripts.AR;
using C_Scripts.Event_Channels;
using UnityEngine;

namespace C_Scripts.Object_Behaviours
{
    /// <summary>
    /// Controls the movement and shooting of the cannon.
    /// </summary>
    public class CannonController : MonoBehaviour
    {
    
        [SerializeField] private float shotPowerMultiplier = 5;
        public float ShotPower { get; private set; }
    
        [SerializeField] private float maxVertAngle = 70;
        [SerializeField] private float maxHorizAngle = 60;
        
        [SerializeField] private Transform shotSource;
        [SerializeField] private Transform cannonBarrel;

        [SerializeField] private GameObject explosion;
        [SerializeField] private GameObject cannonball;
        [SerializeField] private EventChannel onShot;
    
        [SerializeField] private AudioSource shotSound;
        [SerializeField] private Joystick joystick; // axes from -1..1 for both horiz and vert
        [SerializeField] private ARCameraController cameraController;
        
        private Vector3 _cannonOrigRotation;

        private void Start()
        {
            onShot.OnChange += Shoot;
            _cannonOrigRotation = cannonBarrel.localRotation.eulerAngles;
            ShotPower = shotPowerMultiplier;
        }

        /// <summary>
        /// Rotates the cannon and cannon barrel based on the joystick input.
        /// </summary>
        void Update()
        {
            // maps -1..1 to maxHorzAngle..-maxHorzAngle for the cannon
            float nextHorizAngle = -joystick.Horizontal * maxHorizAngle;
            
            // maps -1..1 to maxVertAngle..0 for the cannon barrel
            float nextVertAngle = -((joystick.Vertical - 1) / 2) * maxVertAngle; 
            
            // apply the transformations
            transform.localRotation = Quaternion.Euler(new Vector3(0, nextHorizAngle, 0));
            cannonBarrel.localRotation = Quaternion.Euler(new Vector3
            (
                _cannonOrigRotation.x, 
                _cannonOrigRotation.y, 
                _cannonOrigRotation.z + nextVertAngle
            ));
        }

        private void OnDestroy()
        {
            onShot.OnChange -= Shoot;
        }

        /// <summary>
        /// Returns whether the cannon is shooting or not.
        /// </summary>
        /// <returns>Whether the player is interacting with the joystick</returns>
        public bool IsShooting()
        {
            return joystick.Vertical != 0 || joystick.Horizontal != 0;
        }
    
        /// <summary>
        /// Shoots a cannonball from the cannon with visual effects
        /// </summary>
        private void Shoot()
        {
            Vector3 startPos = shotSource.position;
            Quaternion startRot = shotSource.rotation;
            
            GameObject newBall = Instantiate(cannonball, startPos, startRot);
            newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * ShotPower;
            Destroy(newBall, 10);
            
            // add explosion and screen shake
            Destroy(Instantiate(explosion, startPos, startRot), 2);
            if (cameraController != null) cameraController.Shake();
            shotSound.Play();
        }

        /// <summary>
        /// Angle clamp function by DerDicke
        /// Referenced from http://answers.unity.com/answers/1455566/view.html
        /// </summary>
        /// <returns>The clamped angle</returns>
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + min);
            return Mathf.Min(angle, max);
        }
    }
}

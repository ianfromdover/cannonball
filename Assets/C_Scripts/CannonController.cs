using System;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public Transform shotSource;
    public float shotPower = 5;
    public Joystick joystickAim; // 2 deg of freedom, both from -1..1
    public EventChannel onShot;
    
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float maxAimVertAngle = 70;
    [SerializeField] private float minAimVertAngle = 0;
    [SerializeField] private float maxAimHorizAngle = 60;

    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject cannonball;
    [SerializeField] private Transform cannonBarrel;
    
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private ScreenShakeAnim _screenShakeAnim;

    private void Start()
    {
        onShot.OnChange += Shoot;
    }

    void Update()
    {
        // aim the cannon using arrow keys
        float rotHoriz = joystickAim.Horizontal;
        float rotVert = -joystickAim.Vertical; // negative for those used to pilot controls
        
        // rotate cannon base
        float horizAimAngle = Mathf.Clamp(transform.localRotation.eulerAngles.y 
                                     + rotHoriz * rotationSpeed, 
                                     -maxAimHorizAngle, maxAimHorizAngle);
        transform.localRotation = Quaternion.Euler(new Vector3(0, horizAimAngle, 0));
        
        // rotate cannon barrel
        float vertAimAngle = Mathf.Clamp(cannonBarrel.localRotation.eulerAngles.z 
                                     + rotVert * rotationSpeed, 
                                     minAimVertAngle, maxAimVertAngle);
        cannonBarrel.localRotation = Quaternion.Euler(new Vector3(0, 0, vertAimAngle));
    }

    private void OnDestroy()
    {
        onShot.OnChange -= Shoot;
    }

    private void Shoot(float power)
    {
        float adjustedPower = -(power - 1) / 2; // TODO: map -1..1 to 1..0
        Vector3 startPos = shotSource.position;
        Quaternion startRot = shotSource.rotation;
            
        // TODO: use objectPooling
        GameObject newBall = Instantiate(cannonball, startPos, startRot);
        newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * (shotPower * adjustedPower);
        Destroy(newBall, 10);
            
        // add explosion and screen shake
        Destroy(Instantiate(explosion, startPos, startRot), 2);
        _screenShakeAnim.Shake();
        shotSound.Play();
    }
}

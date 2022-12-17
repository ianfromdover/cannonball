using System;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public Transform shotSource;
    public Joystick joystickAim; // 2 deg of freedom, both from -1..1
    public Joystick joystickPower;
    public EventChannelShot onShot; // TODO: don't need param anymore
    
    public float shotPowerMultiplier = 5;
    public float currShotPower = 0; // TODO: proper info hiding
    
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
        float rotVert = -joystickAim.Vertical; // made negative for those used to pilot controls
        
        // update shot power
        float remappedPowerInput = -(joystickPower.Vertical - 1) / 2; // map -1..1 to 1..0
        currShotPower = remappedPowerInput * shotPowerMultiplier;
        
        // rotate cannon base
        // TODO: clamp not working properly
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
        Vector3 startPos = shotSource.position;
        Quaternion startRot = shotSource.rotation;
            
        // TODO: use objectPooling
        GameObject newBall = Instantiate(cannonball, startPos, startRot);
        newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * currShotPower;
        Destroy(newBall, 10);
            
        // add explosion and screen shake
        Destroy(Instantiate(explosion, startPos, startRot), 2);
        _screenShakeAnim.Shake();
        shotSound.Play();
    }
}

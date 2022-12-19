using UnityEngine;

/// <summary>
/// Controls the movement and shooting of the cannon
/// </summary>
public class CannonController : MonoBehaviour
{
    public Transform shotSource;
    public Joystick joystickAim; // 2 deg of freedom, both from -1..1
    public Joystick joystickPower;
    public EventChannel onShot;
    
    public float shotPowerMultiplier = 5;
    public float currShotPower = 0; // TODO: proper info hiding
    
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float maxAimVertAngle = 70;
    [SerializeField] private float minAimVertAngle = 0;
    [SerializeField] private float maxAimHorizAngle = 60;

    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject cannonball;
    [SerializeField] private Transform cannonBarrel;
    private Vector3 cannonOrigRotation;
    
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private ScreenShakeAnim screenShakeAnim;

    private void Start()
    {
        onShot.OnChange += Shoot;
        cannonOrigRotation = cannonBarrel.localRotation.eulerAngles;
    }

    void Update()
    {
        // update shot power from joystick
        float remappedPowerInput = -(joystickPower.Vertical - 1) / 2; // map -1..1 to 1..0
        currShotPower = remappedPowerInput * shotPowerMultiplier;
        
        // rotate cannon base horizontally
        float rotHoriz = joystickAim.Horizontal;
        float rotVert = joystickAim.Vertical;
        
        float newHorizAngle = transform.localRotation.eulerAngles.y + rotHoriz * rotationSpeed;
        float horizAimAngle = ClampAngle(newHorizAngle, -maxAimHorizAngle, maxAimHorizAngle);
        transform.localRotation = Quaternion.Euler(new Vector3(0, horizAimAngle, 0));
        
        // rotate cannon barrel vertically
        float vertAimAngle = Mathf.Clamp(cannonBarrel.localRotation.eulerAngles.z + rotVert * rotationSpeed, 
            minAimVertAngle, maxAimVertAngle);
        cannonBarrel.localRotation = Quaternion.Euler(new Vector3(cannonOrigRotation.x, 
            cannonOrigRotation.y, cannonOrigRotation.z + vertAimAngle));
    }

    private void OnDestroy()
    {
        onShot.OnChange -= Shoot;
    }

    /// <summary>
    /// Returns whether the cannon is shooting or not.
    /// </summary>
    /// <returns>Whether the player is interacting with the power joystick</returns>
    public bool IsShooting()
    {
        return joystickPower.Vertical != 0;
    }
    
    /// <summary>
    /// Shoots a cannonball from the cannon with visual effects
    /// </summary>
    private void Shoot()
    {
        Vector3 startPos = shotSource.position;
        Quaternion startRot = shotSource.rotation;
            
        // TODO: use objectPooling
        GameObject newBall = Instantiate(cannonball, startPos, startRot);
        newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * currShotPower;
        Destroy(newBall, 10);
            
        // add explosion and screen shake
        Destroy(Instantiate(explosion, startPos, startRot), 2);
        screenShakeAnim.Shake();
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

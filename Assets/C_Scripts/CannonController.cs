using UnityEngine;

public class CannonController : MonoBehaviour
{
    [SerializeField] public Transform shotSource;
    [SerializeField] public float shotSpeed = 5;
    
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float maxAimAngle = 70;
    [SerializeField] private float minAimAngle = 0;

    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject cannonball;
    [SerializeField] private Transform cannonBarrel;
    
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private ScreenShakeAnim _screenShakeAnim;
    
    void Update()
    {
        // aim the cannon using arrow keys
        float rotHoriz = Input.GetAxis("Horizontal");
        float rotVert = Input.GetAxis("Vertical");
        
        // rotate cannon base
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles
            + new Vector3(0, rotHoriz * rotationSpeed, 0));
        
        // rotate cannon barrel
        float aimAngle = Mathf.Clamp(cannonBarrel.localRotation.eulerAngles.z 
                                     + rotVert * rotationSpeed, 
                                     minAimAngle, maxAimAngle);
        cannonBarrel.localRotation = Quaternion.Euler(new Vector3(0, 0, aimAngle));
        // TODO: perhaps the shotSpeed should be varied instead. this is a UX issue

        // shoot cannonball
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 startPos = shotSource.position;
            Quaternion startRot = shotSource.rotation;
            
            // TODO: use objectPooling
            GameObject newBall = Instantiate(cannonball, startPos, startRot);
            newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * shotSpeed;
            Destroy(newBall, 10);
            
            // add explosion and screen shake
            Destroy(Instantiate(explosion, startPos, startRot), 2);
            _screenShakeAnim.Shake();
            shotSound.Play();
        }
    }
}

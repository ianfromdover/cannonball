using UnityEngine;

public class CannonController : MonoBehaviour
{
    [SerializeField] public Transform shotSource;
    [SerializeField] public float shotSpeed = 5;
    
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float screenShakeAmount = 5;

    [SerializeField] private GameObject cannonball;
    [SerializeField] private GameObject explosion;
    
    void Update()
    {
        // get aim location
        float rotHoriz = Input.GetAxis("Horizontal");
        float rotVert = Input.GetAxis("Vertical");
        
        // rotate cannon
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles
            + new Vector3(0, rotHoriz * rotationSpeed, rotVert * rotationSpeed));

        // shoot cannonball
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Vector3 startPos = shotSource.position;
            Quaternion startRot = shotSource.rotation;
            
            // TODO: use objectPooling
            GameObject newBall = Instantiate(cannonball, startPos, startRot);
            newBall.GetComponent<Rigidbody>().velocity = shotSource.transform.up * shotSpeed;
            
            // add explosion and screen shake
            Destroy(Instantiate(explosion, startPos, startRot));
            // Screenshake.ShakeAmount = screenShakeAmount; // see tutorial video
        }
    }
}

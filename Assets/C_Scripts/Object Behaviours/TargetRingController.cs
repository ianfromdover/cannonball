using UnityEngine;

namespace C_Scripts
{
    /// <summary>
    /// Requires the target ring to have a collider
    /// </summary>
    public class TargetRingController : MonoBehaviour
    {
        [SerializeField] private int pointsWorth = 10;
        [SerializeField] private EventChannelScore addPoints;
        [SerializeField] private GameObject scoreParticle; // shows the number of points
        // [SerializeField] private AudioSource scoreSound;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.GetComponent<Cannonball>() != null) // the other object is a cannonball
            {
                addPoints.Publish(pointsWorth);
                var scoreParticleInst = Instantiate(scoreParticle);
                scoreParticleInst.transform.position = other.transform.position;
                // scoreSound.Play();
                Destroy(other.gameObject);
                Destroy(scoreParticleInst, 2); // todo: object pooling
            }
        }
    }
}

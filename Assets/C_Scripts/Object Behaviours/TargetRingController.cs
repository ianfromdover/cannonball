using C_Scripts.Event_Channels;
using UnityEngine;

namespace C_Scripts.Object_Behaviours
{
    /// <summary>
    /// Adds points, creates score particle, and
    /// destroys the cannonball when hit by it.
    /// </summary>
    public class TargetRingController : MonoBehaviour
    {
        [SerializeField] private int pointsWorth = 10;
        [SerializeField] private EventChannelScore addPoints;
        [SerializeField] private GameObject scoreParticle; // shows the number of points
        // [SerializeField] private AudioSource scoreSound;

        private void OnCollisionEnter(Collision other)
        {
            // the other object is not a cannonball
            if (other.gameObject.GetComponent<Cannonball>() == null) return;
                
            // give the player points
            addPoints.Publish(pointsWorth);
            
            // create particle that shows how many points the player earned
            var scoreParticleInst = Instantiate(scoreParticle);
            scoreParticleInst.transform.position = other.transform.position;
            
            // scoreSound.Play();
            
            Destroy(other.gameObject);
            Destroy(scoreParticleInst, 2);
        }
    }
}

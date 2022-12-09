using UnityEngine;

namespace Niantic.LightshipHub.SampleProjects
{
  public class PartycleSystemsController : MonoBehaviour
  {
    private ParticleSystem goalParticles;
    private void Start()
    {
      goalParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.GetComponent<BallBehaviour>() != null)
        goalParticles.Play();
    }
  }
}

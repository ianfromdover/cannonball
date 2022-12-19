using System;
using System.Collections;
using System.Collections.Generic;
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
        // [SerializeField] private GameObject scoreParticle; // shows the number of points
        // [SerializeField] private AudioSource scoreSound;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.GetComponent<Cannonball>() != null) // the other object is a cannonball
            {
                addPoints.Publish(pointsWorth);
                // Instantiate(scoreParticle, other.transform);
                // scoreSound.Play();
                Destroy(other.gameObject);
            }
        }
    }
}

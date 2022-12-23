using System;
using System.Collections.Generic;
using UnityEngine;

namespace C_Scripts.Object_Behaviours
{
    /// <summary>
    /// Draws the path that the cannonball will take when shot
    /// </summary>
    public class DrawProjection : MonoBehaviour
    {
        [SerializeField] private int maxPointsOnLine = 40;
        [SerializeField] private float timeBetweenPoints = 0.1f;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform shotSource;
        [SerializeField] private CannonController cannonController;
        [SerializeField] private LayerMask collidableLayers; // layers that stop the line from being drawn

        void Update()
        {
            if (!cannonController.IsShooting())
            {
                // don't draw the line
                lineRenderer.positionCount = 0;
                lineRenderer.SetPositions(Array.Empty<Vector3>());
                return;
            }
        
            // prepare to estimate projectile motion
            lineRenderer.positionCount = maxPointsOnLine;
            List<Vector3> linePositions = new List<Vector3>();
            Vector3 startPos = shotSource.position;
            Vector3 startVel = shotSource.up * cannonController.ShotPower;
        
            // estimate the projectile motion of the cannonball
            for (float t = 0; t < maxPointsOnLine; t += timeBetweenPoints)
            {
                Vector3 nextPos = startPos + t * startVel;

                // stop rendering line if it collides with something
                if (Physics.OverlapSphere(nextPos, 0.2f, collidableLayers).Length > 0)
                {
                    lineRenderer.positionCount = linePositions.Count;
                    break;
                }

                nextPos.y = startPos.y + startVel.y * t + 0.5f * Physics.gravity.y * t * t;
                linePositions.Add(nextPos);
            }
        
            lineRenderer.SetPositions(linePositions.ToArray());
        }
    }
}

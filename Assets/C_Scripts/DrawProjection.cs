using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws the path that the cannonball will take when shot
/// </summary>
public class DrawProjection : MonoBehaviour
{
    [SerializeField] private int maxPointsOnLine = 40;
    [SerializeField] private float timeBetweenPoints = 0.1f;
    [SerializeField] private LayerMask collidableLayers; // layers that stop the line from being drawn

    private CannonController _cannonController;
    private LineRenderer _lineRenderer;
    
    void Start()
    {
        _cannonController = GetComponent<CannonController>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!_cannonController.IsShooting())
        {
            // don't draw the line
            _lineRenderer.positionCount = 0;
            _lineRenderer.SetPositions(Array.Empty<Vector3>());
            return;
        }
        
        _lineRenderer.positionCount = maxPointsOnLine;
        List<Vector3> linePositions = new List<Vector3>();
        Vector3 startPos = _cannonController.shotSource.position;
        Vector3 startVel = _cannonController.shotSource.up * _cannonController.currShotPower;
        
        // estimate the projectile motion of the cannonball
        for (float t = 0; t < maxPointsOnLine; t += timeBetweenPoints)
        {
            Vector3 nextPos = startPos + t * startVel;

            // check if this point collides and stop rendering line if is does
            if (Physics.OverlapSphere(nextPos, 0.2f, collidableLayers).Length > 0)
            {
                _lineRenderer.positionCount = linePositions.Count;
                break;
            }

            nextPos.y = startPos.y + startVel.y * t + 0.5f * Physics.gravity.y * t * t;
            linePositions.Add(nextPos);
        }
        
        _lineRenderer.SetPositions(linePositions.ToArray());
    }
}

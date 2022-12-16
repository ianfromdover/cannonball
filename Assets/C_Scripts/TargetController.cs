using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private List<GameObject> scoreRegionsAscendingPoints = new List<GameObject>();
    // requires the regions to have a collider
    
    [SerializeField] private int lowestPoints = 10;
    [SerializeField] private int increment = 10;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

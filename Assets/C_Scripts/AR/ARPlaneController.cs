using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions;
using UnityEngine;

public class ARPlaneController : MonoBehaviour
{
    [SerializeField] private ARPlaneManager _arPlaneManager;
    void OnDisable() { _arPlaneManager.ClearAllPlanes(); }
}

using Niantic.ARDK.Extensions;
using UnityEngine;

namespace C_Scripts.AR
{
    /// <summary>
    /// Allows Unity's OnDisable lifecycle event to clear ARPlaneManager's planes.
    /// This is because ARPlaneManager does not inherit from MonoBehaviour.
    /// </summary>
    public class ARPlaneController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager arPlaneManager;
        void OnDisable()
        {
            arPlaneManager.ClearAllPlanes();
        }
    }
}

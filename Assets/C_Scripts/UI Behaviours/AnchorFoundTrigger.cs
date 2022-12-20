using C_Scripts.Managers;
using UnityEngine;

namespace C_Scripts.UI_Behaviours
{
    /// <summary>
    /// Triggers a UI change using the anchors found event channel.
    /// </summary>
    public class AnchorFoundTrigger : MonoBehaviour
    {
        public EventChannel anchorsFound;
        [SerializeField] private GameObject continueButton;

        private void Start() { anchorsFound.OnChange += Run; }
        private void OnDestroy() { anchorsFound.OnChange -= Run; }
        private void Run() { continueButton.SetActive(true); }
    }
}

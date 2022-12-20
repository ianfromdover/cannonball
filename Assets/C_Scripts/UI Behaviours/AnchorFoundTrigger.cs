using C_Scripts.Event_Channels;
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

        private void Start() { anchorsFound.OnChange += Enable; }
        private void OnDestroy() { anchorsFound.OnChange -= Enable; }
        private void Enable() { continueButton.SetActive(true); }
    }
}

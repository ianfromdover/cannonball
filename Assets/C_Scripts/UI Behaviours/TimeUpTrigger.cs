using C_Scripts.Managers;
using UnityEngine;

namespace C_Scripts.UI_Behaviours
{
    /// <summary>
    /// Triggers a UI change using the time up event channel.
    /// </summary>
    public class TimeUpTrigger : MonoBehaviour
    {
        public EventChannel timerStopped;
        [SerializeField] private UiScreenManager _uiScreenManager;

        private void Start() { timerStopped.OnChange += Run; }
        private void OnDestroy() { timerStopped.OnChange -= Run; }

        private void Run() { _uiScreenManager.EnableEndModal(); }
    }
}

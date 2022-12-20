using System.Collections;
using UnityEngine;

namespace C_Scripts.UI_Behaviours
{
    public class TooCloseReminder : MonoBehaviour
    {
        [SerializeField] private GameObject instructionText;
        [SerializeField] private GameObject reminderText;
        [SerializeField] private EventChannel tooCloseToTarget;
        [SerializeField] private float readingTime = 3.0f;
    
        void Start() { tooCloseToTarget.OnChange += ShowReminder; }
        void OnDestroy() { tooCloseToTarget.OnChange -= ShowReminder; }
        private void ShowReminder() { StartCoroutine(ShowReminderRoutine()); }
        private IEnumerator ShowReminderRoutine()
        {
            instructionText.SetActive(false);
            reminderText.SetActive(true);
            yield return new WaitForSeconds(readingTime);
            HideReminder();
        }
        private void HideReminder()
        {
            instructionText.SetActive(true);
            reminderText.SetActive(false);
        }

    }
}

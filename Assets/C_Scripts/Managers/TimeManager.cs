using C_Scripts.Event_Channels;
using TMPro;
using UnityEngine;

namespace C_Scripts.Managers
{
    /// <summary>
    /// Implements a timer which displays the time remaining in mins and secs.
    /// Triggers the highscore updates or resets the score in ScoreManager.
    ///
    /// @author Ian
    /// @author referenced https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/j:w
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private float durationInMinutes = 0.5f;
        [SerializeField] private TextMeshProUGUI display;
    
        [SerializeField] private EventChannel startTimer;
        [SerializeField] private EventChannel resetTimer;
        [SerializeField] private EventChannel timerStopped;
        private float _secsRemaining;
        private bool _timerIsRunning = false;
        void Start()
        {
            // Set up text object on UI
            _secsRemaining = durationInMinutes * 60;
            DisplayAsMinSec(_secsRemaining);
        
            startTimer.OnChange += StartTimer;
            timerStopped.OnChange += StopTimer;
            resetTimer.OnChange += ResetTimer;
        }

        private void OnDestroy()
        {
            startTimer.OnChange -= StartTimer;
            timerStopped.OnChange -= StopTimer;
            resetTimer.OnChange -= ResetTimer;
        }
        void Update()
        {
            if (!_timerIsRunning) return;
            
            if (_secsRemaining > 0) // timer counting down
            {
                // subtract the time taken for prev frame
                _secsRemaining -= Time.deltaTime; 
                DisplayAsMinSec(_secsRemaining);
            }
            else // timer just ended naturally
            {
                timerStopped.Publish();
            }
        }

        /// <summary>
        /// Displays a float of seconds as minutes and seconds, floored.
        /// </summary>
        /// <param name="time"></param>
        private void DisplayAsMinSec(float time)
        {
            float m = Mathf.FloorToInt(time / 60);
            float s = Mathf.FloorToInt(time % 60);
            display.text = $"{m:00}:{s:00}";
        }
    
        /// <summary>
        /// Starts and resets timer.
        /// </summary>
        public void ControlTime()
        {
            resetTimer.Publish();
            startTimer.Publish();
        }
    
        private void StartTimer()
        {
            _timerIsRunning = true;
        }
        private void StopTimer()
        {
            _timerIsRunning = false;
            _secsRemaining = 0;
            display.text = "00:00";
        }
    
        private void ResetTimer()
        {
            _timerIsRunning = false;
            _secsRemaining = durationInMinutes * 60;
            DisplayAsMinSec(_secsRemaining);
        }
    }
}

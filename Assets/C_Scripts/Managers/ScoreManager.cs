using C_Scripts.Event_Channels;
using TMPro;
using UnityEngine;

namespace C_Scripts.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int score = 0;
        [SerializeField] private TextMeshProUGUI highscoreDisplay;
        [SerializeField] private TextMeshProUGUI finalScoreDisplay;
        [SerializeField] private TextMeshProUGUI scoreDisplay;
        [SerializeField] private EventChannelScore pointScored;
        [SerializeField] private EventChannel startTimer;
        [SerializeField] private EventChannel stopTimer;
        [SerializeField] private EventChannel resetTimer;
        private int _highscore = 0;
        private bool _isCountingScore = false;
        private const string Zero = "0";
        void Start()
        {
            // Listen to events
            pointScored.OnChange += AddPoint;
            startTimer.OnChange += StartCountingScore;
            stopTimer.OnChange += StopCountingScore;
            stopTimer.OnChange += SaveHighscore;
            resetTimer.OnChange += ResetScore;
        
            // Clear display texts
            scoreDisplay.text = Zero;
            highscoreDisplay.text = Zero;
            finalScoreDisplay.text = Zero;
        }

        private void OnDestroy()
        {
            pointScored.OnChange -= AddPoint;
            startTimer.OnChange -= StartCountingScore;
            stopTimer.OnChange -= StopCountingScore;
            stopTimer.OnChange -= SaveHighscore;
            resetTimer.OnChange -= ResetScore;
        }

        /// <summary>
        /// Enables counting score only when the timer is running.
        /// </summary>
        public void StartCountingScore()
        {
            _isCountingScore = true;
        }
    
        /// <summary>
        /// Disables the counting of score.
        /// Does not count score when timer is not running.
        /// </summary>
        public void StopCountingScore()
        {
            _isCountingScore = false;
        }
    
        public void AddPoint(int points)
        {
            if (_isCountingScore)
            {
                score += points;
                scoreDisplay.text = score + "";
            }
        }
        
        /// <summary>
        /// Updates the final score and highscore after the timer is done.
        /// </summary>
        public void SaveHighscore()
        {
            finalScoreDisplay.text = "" + score;
            if (score > _highscore)
            {
                _highscore = score;
                highscoreDisplay.text = "" + _highscore;
            }
        }
    
        /// <summary>
        /// Resets score when timer is reset.
        /// </summary>
        public void ResetScore()
        {
            score = 0;
            scoreDisplay.text = score + "";
        }
    }
}

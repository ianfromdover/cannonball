using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ScoreManager : MonoBehaviour
{
    public EventChannelScore pointScored;
    public EventChannel startTimer;
    public EventChannel stopTimer;
    public EventChannel resetTimer;
    public TextMeshProUGUI highscoreDisplay;
    public TextMeshProUGUI display;
    public int score = 0;
    private int _highscore = 0;
    private bool _isCountingScore = false;
    private const string Zero = "0";
    private const string HighscoreString = "Highscore: ";
    void Start()
    {
        pointScored.OnChange += AddPoint;
        startTimer.OnChange += StartCountingScore;
        stopTimer.OnChange += StopCountingScore;
        stopTimer.OnChange += SaveHighscore;
        resetTimer.OnChange += ResetScore;
        
        display.text = Zero;
        highscoreDisplay.text = HighscoreString + Zero;
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
    /// Enables collisions with targets to count score.
    /// Only count score when the timer is running.
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
            display.text = score + "";
        }
    }
    /// <summary>
    /// Adds score to leaderboard after timer is done.
    /// </summary>
    public void SaveHighscore()
    {
        if (score > _highscore)
        {
            _highscore = score;
            highscoreDisplay.text = HighscoreString + _highscore;
        }
    }
    
    /// <summary>
    /// Resets score when timer is reset.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        display.text = score + "";
    }
}

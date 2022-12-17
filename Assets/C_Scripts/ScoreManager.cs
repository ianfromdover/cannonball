using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreManager : MonoBehaviour
{
    public EventChannelScore pointScored;
    public EventChannel startTimer;
    public EventChannel stopTimer;
    public EventChannel resetTimer;
    private List<Text> leaderboardValues = new List<Text>(); // todo: just deprecate
    public Text scoreCounter; // todo: old handheld score indicator
    public int score = 0;
    private int _highscore = 0;
    private bool _isCountingScore;
    private static readonly string ZERO = "0";
    void Start()
    {
        scoreCounter.text = ZERO;
        pointScored.OnChange += AddPoint;
        startTimer.OnChange += StartCountingScore;
        stopTimer.OnChange += StopCountingScore;
        stopTimer.OnChange += SaveHighscore;
        resetTimer.OnChange += ResetScore;
        
        // updates the score of all the leaderboards in the scene
        // TODO: deprecate this. only 1 highscore.
        List<GameObject> tempLeaderboard = new List<GameObject>(GameObject.FindGameObjectsWithTag("Highscore"));
        tempLeaderboard.ForEach(t => leaderboardValues.Add(t.GetComponent<Text>()));
        leaderboardValues.ForEach(t => t.text = ZERO);
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
    /// TODO: Or when the pigeon has not entered the hoop.
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
            scoreCounter.text = score + "";
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
            leaderboardValues.ForEach(t => t.text = _highscore + "");
        }
    }
    
    /// <summary>
    /// Resets score when timer is reset.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        scoreCounter.text = score + "";
    }
}

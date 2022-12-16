using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreCounter : MonoBehaviour
{
    public EventChannel pointScored;
    public EventChannel stopTimer;
    public EventChannel resetTimer;
    public EventChannel timeSlowed; // pigeon enters hoop
    public EventChannel gunUnequipped; // pigeon hits ground
    private List<Text> leaderboardValues = new List<Text>();
    public Text scoreCounter;
    public int score = 0;
    private int _highscore = 0;
    private bool _isCountingScore;
    private static readonly string ZERO = "0";
    void Start()
    {
        scoreCounter.text = ZERO;
        pointScored.OnChange += AddPoint;
        timeSlowed.OnChange += StartCountingScore; // when pigeon enters ring
        stopTimer.OnChange += SaveScore;
        resetTimer.OnChange += ResetScore;
        gunUnequipped.OnChange += StopCountingScore;
        
        List<GameObject> tempLeaderboard = new List<GameObject>(GameObject.FindGameObjectsWithTag("Highscore"));
        tempLeaderboard.ForEach(t => leaderboardValues.Add(t.GetComponent<Text>()));
        leaderboardValues.ForEach(t => t.text = ZERO);
    }

    private void OnDestroy()
    {
        pointScored.OnChange -= AddPoint;
        timeSlowed.OnChange -= StartCountingScore;
        stopTimer.OnChange -= SaveScore;
        resetTimer.OnChange -= ResetScore;
        gunUnequipped.OnChange -= StopCountingScore;
    }

    public void StartCountingScore()
    {
        _isCountingScore = true;
    }
    
    /// <summary>
    /// Disables the counting of score.
    /// Does not count score when timer is not running.
    /// Or when the pigeon has not entered the hoop.
    /// </summary>
    public void StopCountingScore()
    {
        _isCountingScore = false;
    }
    
    public void AddPoint()
    {
        if (_isCountingScore)
        {
            score++;
            scoreCounter.text = score + "";
            gunUnequipped.Publish();
        }
    }
    /// <summary>
    /// Adds score to leaderboard after timer is done.
    /// </summary>
    public void SaveScore()
    {
        if (score > _highscore)
        {
            _highscore = score;
            leaderboardValues.ForEach(t => t.text = _highscore + "");
        }
    }
    
    /// <summary>
    /// Resets score on hand when timer is reset.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        scoreCounter.text = score + "";
    }
}

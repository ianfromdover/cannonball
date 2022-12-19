using System;
using UnityEngine;


/// <summary>
/// A blueprint for a 'score event'
/// </summary>
[CreateAssetMenu(fileName = "New Score Event Channel", menuName = "Score Event Channel")]
public class EventChannelScore : EventChannel
{
    public new Action<int> OnChange;
    private int _scoreVal;

    public void Publish(int val)
    {
        _scoreVal = val;
        OnChange.Invoke(_scoreVal);
    }
}

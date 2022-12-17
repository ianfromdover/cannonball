using System;
using UnityEngine;


/// <summary>
/// A blueprint for a 'score event'
/// </summary>
[CreateAssetMenu(fileName = "New Score Event Channel", menuName = "Score Event Channel")]
public class EventChannelScore : EventChannel
{
    public Action<int> OnChange;

    public void Publish(int val)
    {
        OnChange.Invoke(val);
    }
}

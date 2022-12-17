using System;
using UnityEngine;

/// <summary>
/// A blueprint for an 'event'
/// Makes it much easier to subscribe many methods across classes to the event and call them.
/// Reduces coupling because publishing classes don't need to know
/// what methods the subscribed classes have.
/// </summary>
[CreateAssetMenu(fileName = "New Event Channel", menuName = "Event Channel")]
public class EventChannel : ScriptableObject
{
    public Action OnChange;

    public void Publish()
    {
        OnChange.Invoke();
    }
}

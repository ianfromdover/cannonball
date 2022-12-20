using System;
using UnityEngine;

namespace C_Scripts.Event_Channels
{
    /// <summary>
    /// A blueprint for an 'event' built on a ScriptableObject
    /// Allows dev team to name and enumerate the events that are being published.
    /// Makes it easier to subscribe many methods across classes to the event and call them.
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
}

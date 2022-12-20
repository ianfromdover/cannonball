using System;
using UnityEngine;

namespace C_Scripts.Event_Channels
{
    /// <summary>
    /// A blueprint for a 'score event' that takes in an int.
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
}

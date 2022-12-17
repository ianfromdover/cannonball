using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shot Event Channel", menuName = "Shot Event Channel")]
public class EventChannelShot : EventChannel
{
    public Action<float> OnChange;

    public void Publish(float arg)
    {
        OnChange.Invoke(arg);
    }
}

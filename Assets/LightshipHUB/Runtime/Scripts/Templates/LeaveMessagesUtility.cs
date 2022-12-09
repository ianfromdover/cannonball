// Copyright 2022 Niantic, Inc. All Rights Reserved.

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Niantic.ARDK.AR.WayspotAnchors;

namespace Niantic.LightshipHub.Templates
{
  public static class LeaveMessagesUtility
  {
    private const string DataKey = "wayspot_anchor_messages";

    public static void SaveLocalMessages(MessagesData messagesData)
    {
      string messagesJson = JsonUtility.ToJson(messagesData);
      PlayerPrefs.SetString(DataKey, messagesJson);
    }

    public static MessagesData LoadLocalMessages()
    {
      if (PlayerPrefs.HasKey(DataKey))
      {
        var payloads = new List<WayspotAnchorPayload>();
        var json = PlayerPrefs.GetString(DataKey);
        var messagesData = JsonUtility.FromJson<MessagesData>(json);

        foreach (var wayspotAnchorPayload in messagesData.Payloads)
        {
          var payload = WayspotAnchorPayload.Deserialize(wayspotAnchorPayload);
          payloads.Add(payload);
        }

        return messagesData;
      }
      else
      {
        return new MessagesData();
      }
    }

    [Serializable]
    public class MessagesData
    {
      public string[] Payloads = Array.Empty<string>();
      public string[] Messages = Array.Empty<string>();
    }
  }
}

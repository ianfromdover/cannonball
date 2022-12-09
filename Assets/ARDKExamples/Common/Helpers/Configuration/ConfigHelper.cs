// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.Configuration;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDKExamples.Configuration
{
  /// <summary>
  /// Helper class for setting up a Global Configuration as soon as the app starts up.
  /// If URLs are not set using this component, default URLs will be used.
  /// This class is primarily meant for prototypes and demos, where you are never needing to update
  /// values in the global configuration remotely.
  /// </summary>
  [DefaultExecutionOrder(-1)]
  public class ConfigHelper:
    MonoBehaviour
  {
    /// Empty URL will trigger default URL
    [SerializeField]
    private string _contextAwarenessUrl = "";

    void Awake()
    {
      if (!string.IsNullOrEmpty(_contextAwarenessUrl))
      {
        if (ArdkGlobalConfig.SetContextAwarenessUrl(_contextAwarenessUrl))
          Debug.Log("Set the Context Awareness URL to: " + _contextAwarenessUrl);
      }
    }
  }
}

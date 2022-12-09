// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.LightshipHub.Tools
{
  // Simple scrolling window that prints to the application screen whatever is printed through
  // calls to the UnityEngine.Debug.Log method.
  [DefaultExecutionOrder(Int32.MinValue)]
  public class ScrollingLog:
    MonoBehaviour
  {
    /// Font size for log text entries. Spacing between log entries is also set to half this value.
    [SerializeField]
    private int LogEntryFontSize = 32;

    /// The maximum number of log entries to keep history of. Setting to -1 will save all entries
    [SerializeField]
    private int MaxLogCount = -1;

    /// Layout box containing the log entries
    [SerializeField]
    private VerticalLayoutGroup LogHistory = null;

    /// Log entry prefab used to generate new entries when requested
    [SerializeField]
    private Text LogEntryPrefab = null;

    private readonly List<Text> _logEntries = new List<Text>();

    private static ScrollingLog _instance;

    private RectTransform rectTransform;

    private bool moveIn, moveOut;
    private float animationStep = 1500.0f;

    private Vector2 offsetMinLogOpen = new Vector2(0.0f, 0.0f);
    private Vector2 offsetMinLogClosed = new Vector2(0.0f, -500f);
    private Vector2 offsetMaxLogOpen = new Vector2(0.0f, 500f);
    private Vector2 offsetMaxLogClosed = new Vector2(0.0f, 0.0f);

    private void Awake()
    {
      _instance = this;
      LogEntryPrefab.gameObject.SetActive(false);

      // Using logMessageReceived (instead of logMessageReceivedThreaded) to ensure that
      // HandleDebugLog is only called from one thread (the main thread).
      Application.logMessageReceived += AddLogEntry;
      rectTransform = GetComponent<RectTransform>();
    }

    protected void Start()
    {
      LogHistory.spacing = LogEntryFontSize / 2f;
    }

    private void OnDestroy()
    {
      _instance = null;

      Application.logMessageReceived -= AddLogEntry;
    }

    public void ShowOrHideLogScreen()
    {
      if (rectTransform.offsetMin.y <= offsetMinLogClosed.y)
      {
        moveIn = true;
      } 
      else if (rectTransform.offsetMin.y >= offsetMinLogOpen.y)
      {
        moveOut = true;
      }

    }

    void Update()
    {
      if (moveIn)
      {
        rectTransform.offsetMin = Vector2.MoveTowards(rectTransform.offsetMin, offsetMinLogOpen, animationStep * Time.deltaTime);
        rectTransform.offsetMax = Vector2.MoveTowards(rectTransform.offsetMax, offsetMaxLogOpen, animationStep * Time.deltaTime);
        if (rectTransform.offsetMin.y >= offsetMinLogOpen.y) 
          moveIn = false;
      }
      else if (moveOut)
      {
        rectTransform.offsetMin = Vector2.MoveTowards(rectTransform.offsetMin, offsetMinLogClosed, animationStep * Time.deltaTime);
        rectTransform.offsetMax = Vector2.MoveTowards(rectTransform.offsetMax, offsetMaxLogClosed, animationStep * Time.deltaTime);
        if (rectTransform.offsetMin.y <= offsetMinLogClosed.y) 
          moveOut = false;
      }
    }

    // Creates a new log entry using the provided string.
    private void AddLogEntry(string str, string stackTrace, LogType type)
    {
      var newLogEntry = Instantiate(LogEntryPrefab, Vector3.zero, Quaternion.identity);
      newLogEntry.text = str;
      newLogEntry.fontSize = LogEntryFontSize;
      newLogEntry.gameObject.SetActive(true);

      var transform = newLogEntry.transform;
      transform.SetParent(LogHistory.transform);
      transform.localScale = Vector3.one;

      _logEntries.Add(newLogEntry);

      if (MaxLogCount > 0 && _logEntries.Count > MaxLogCount)
      {
        var textObj = _logEntries.First();
        _logEntries.RemoveAt(0);
        Destroy(textObj.gameObject);
      }
    }

    public static void Clear()
    {
      if (_instance == null)
        return;

      foreach (var entry in _instance._logEntries)
        Destroy(entry.gameObject);

      _instance._logEntries.Clear();
    }
  }
}

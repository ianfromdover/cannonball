// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

using ARDK.Extensions;

using Niantic.ARDK.AR.Awareness;

using UnityEngine;

namespace Niantic.ARDKExamples
{
  /// Example of using hand tracking with rectangles over palms.
  /// A sample class that can be added to a scene to demonstrate basic palm detection
  ///   testing usage. On each updated frame, as long as there are palms in screen, it will draw a
  ///   rectangle around each palm up to 15.
  public class HandTrackingExampleManager:
    MonoBehaviour
  {
    [SerializeField]
    private ARHandTrackingManager _handTrackingManager = null;

    private IReadOnlyList<Detection> _detections;

    private Texture2D _lineTexture;
    private GUIStyle _fontStyle;

    private const int LineThickness = 10;
    private static readonly Color LineColor = Color.red;
    private const int TextSize = 50;
    private static readonly Color TextColor = Color.white;

    private void Start()
    {
      _handTrackingManager.HandTrackingUpdated += OnHandTrackingUpdated;

      // Setup for drawing rectangles and confidence
      if (_lineTexture == null)
      {
        _lineTexture = new Texture2D(1, 1);
        _lineTexture.SetPixel(0, 0, LineColor);
        _lineTexture.Apply();
      }

      if (_fontStyle == null)
      {
        _fontStyle = new GUIStyle();
        _fontStyle.fontSize = TextSize;
        _fontStyle.normal.textColor = TextColor;
      }
    }

    private void OnDestroy()
    {
      _handTrackingManager.HandTrackingUpdated -= OnHandTrackingUpdated;
    }

    private void OnHandTrackingUpdated(HumanTrackingArgs args)
    {
      _detections = args.TrackingData?.AlignedDetections;
    }

    void OnGUI()
    {
      // Draw bounding boxes of hands
      if (_detections != null)
      {
        foreach (var detection in _detections)
        {
          // Float rectangle to screen position
          var detectionPos = new Vector3(detection.Rect.x, detection.Rect.y, 0);
          var origin = Camera.main.ViewportToScreenPoint(detectionPos);

          var detectionSize = new Vector3(detection.Rect.width, detection.Rect.height, 0);
          var extent = Camera.main.ViewportToScreenPoint(detectionSize);

          var rect = new Rect(origin.x, origin.y, extent.x, extent.y);

          // Draw the lines
          Rect r = rect;
          r.height = LineThickness;
          GUI.DrawTexture(r, _lineTexture);
          r.y += rect.height - LineThickness;
          GUI.DrawTexture(r, _lineTexture);

          r = rect;
          r.width = LineThickness;
          GUI.DrawTexture(r, _lineTexture);
          r.x += rect.width - LineThickness;
          GUI.DrawTexture(r, _lineTexture);

          var score = Math.Round(detection.Confidence * 1000) / 10f;
          GUI.Label
          (
            new Rect(rect.x + 20, rect.y + 20, rect.width, rect.height),
            "Score: " + score + "%",
            _fontStyle
          );
        }
      }
    }
  }
}

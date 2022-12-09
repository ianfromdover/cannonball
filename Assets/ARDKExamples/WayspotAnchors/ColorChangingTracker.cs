using System;

using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Extensions;

using UnityEngine;

namespace Niantic.ARDKExamples.WayspotAnchors
{
  public class ColorChangingTracker: WayspotAnchorTracker
  {
    private readonly Color _pendingColor = Color.yellow;
    private readonly Color _limitedColor = Color.green;
    private readonly Color _successColor = Color.green;
    private readonly Color _failedColor = Color.red;
    private readonly Color _invalidColor = Color.red;

    private Renderer _renderer;

    private void Awake()
    {
      _renderer = GetComponent<Renderer>();

      if (_renderer == null)
        Debug.LogError("Missing Renderer component.");

      _renderer.material.color = _pendingColor;
      name = "Anchor (Pending)";
    }

    protected override void OnAnchorAttached()
    {
      base.OnAnchorAttached();

      name = $"Anchor {WayspotAnchor.ID}";
      ChangeColor(WayspotAnchor.Status);
    }

    protected override void OnStatusCodeUpdated(WayspotAnchorStatusUpdate args)
    {
      Debug.Log($"Anchor {WayspotAnchor.ID.ToString().Substring(0, 8)} status updated to {args.Code}");
      ChangeColor(args.Code);

      if (args.Code == WayspotAnchorStatusCode.Success || args.Code == WayspotAnchorStatusCode.Limited)
      {
        gameObject.SetActive(true);
      }
    }

    private void ChangeColor(WayspotAnchorStatusCode code)
    {
      switch (code)
      {
        case WayspotAnchorStatusCode.Pending:
          _renderer.material.color = _pendingColor;
          break;

        case WayspotAnchorStatusCode.Success:
          _renderer.material.color = _successColor;
          break;

        case WayspotAnchorStatusCode.Failed:
          _renderer.material.color = _failedColor;
          break;

        case WayspotAnchorStatusCode.Invalid:
          _renderer.material.color = _invalidColor;
          break;

        case WayspotAnchorStatusCode.Limited:
          _renderer.material.color = _limitedColor;
          break;
      }
    }
  }
}

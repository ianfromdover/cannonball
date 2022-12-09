// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Extensions;

namespace Niantic.LightshipHub.Templates
{
  public class ShowTracker: WayspotAnchorTracker
  {
    protected override void OnStatusCodeUpdated(WayspotAnchorStatusUpdate args)
    {
      if (args.Code == WayspotAnchorStatusCode.Success || args.Code == WayspotAnchorStatusCode.Limited)
      {
        gameObject.SetActive(true);
      }
    }

  }
}


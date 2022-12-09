// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal sealed class _MockWayspotAnchorControllerImplementation: _MockWayspotAnchorControllerImplementationBase
  {

    private IARSession _arSession;

    /// Creates a Mock Wayspot Anchor Controller
    /// @param arSession The AR Session to create the mock wayspot anchor controller with
    internal _MockWayspotAnchorControllerImplementation(IARSession arSession)
    {
      _arSession = arSession;
      if (arSession != null)
      {
        _arSession.Paused += HandleARSessionPaused;
      }
      _isDisposed = false;
      ResolveWayspotAnchorsAsync();
    }

    private void HandleARSessionPaused(ARSessionPausedArgs arSessionPausedArgs)
    {
      SetLocalizationState(LocalizationState.Localizing, LocalizationFailureReason.None);
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  public class WayspotAnchorController : WayspotAnchorControllerBase
  {
    private IARSession _arSession;

    /// Creates a new wayspot anchor API to consume
    /// @param arSession The AR session required by the WayspotAnchorController to run VPS.
    /// @param locationService The location service required by the WayspotAnchorController to run VPS.
    public WayspotAnchorController(IARSession arSession, ILocationService locationService) 
    {
      _arSession = arSession;
      _arSession.SetupLocationService(locationService);
      _arSession.Deinitialized += HandleSessionDeinitialized;
      _wayspotAnchorControllerImplementation = CreateWayspotAnchorController();
    }

    /// Starts the visual position system
    /// @param wayspotAnchorsConfiguration The configuration to start VPS with
    public override void StartVps(IWayspotAnchorsConfiguration wayspotAnchorsConfiguration)
    {
      base.StartVps(wayspotAnchorsConfiguration);
      ARLog._Debug($"Started VPS for Stage ID: {_arSession.StageIdentifier}");
    }

    private void HandleSessionDeinitialized(ARSessionDeinitializedArgs arSessionDeinitializedArgs)
    {
      _arSession.Deinitialized -= HandleSessionDeinitialized;
      _wayspotAnchorControllerImplementation.LocalizationStateUpdated -= HandleLocalizationStateUpdated;
      _wayspotAnchorControllerImplementation.WayspotAnchorsCreated -= HandleWayspotAnchorsCreated;
      _wayspotAnchorControllerImplementation.WayspotAnchorsResolved -= HandleWayspotAnchorsResolved;
      _wayspotAnchorControllerImplementation.WayspotAnchorStatusUpdated -= HandleWayspotAnchorStatusUpdated;

      _wayspotAnchorControllerImplementation.Dispose();
    }

    private _IWayspotAnchorControllerImplementation CreateWayspotAnchorController()
    {
      _IWayspotAnchorControllerImplementation impl;

      switch (_arSession.RuntimeEnvironment)
      {
        case RuntimeEnvironment.Default:
          // will never happen, Default value is invalid value for IARSession.RuntimeEnvironment
          ARLog._Error("Invalid ARSession.RuntimeEnvironment value");
          return null;

        case RuntimeEnvironment.Remote:
          throw new NotImplementedException($"Remote runtime environment not yet supported.");

        case RuntimeEnvironment.Playback:
        case RuntimeEnvironment.LiveDevice:
          impl = new _NativeWayspotAnchorControllerImplementation(_arSession);
          break;

        case RuntimeEnvironment.Mock:
          impl = new _MockWayspotAnchorControllerImplementation(_arSession);
          break;

        default:
          throw new InvalidEnumArgumentException($"Out of range RuntimeEnvironment enum: {_arSession.RuntimeEnvironment}.");
      }

      impl.LocalizationStateUpdated += HandleLocalizationStateUpdated;
      impl.WayspotAnchorsCreated += HandleWayspotAnchorsCreated;
      impl.WayspotAnchorsResolved += HandleWayspotAnchorsResolved;
      impl.WayspotAnchorStatusUpdated += HandleWayspotAnchorStatusUpdated;

      return impl;
    }

  }
}

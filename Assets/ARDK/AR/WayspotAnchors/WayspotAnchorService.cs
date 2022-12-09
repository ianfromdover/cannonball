// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.LocationService;
using LocationServiceStatus = Niantic.ARDK.LocationService.LocationServiceStatus;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  public class WayspotAnchorService : WayspotAnchorServiceBase
  {
    private readonly IARSession _arSession;

    /// Creates a new wayspot anchor service
    /// @param arSession The AR Session used to create the wayspot anchor service
    /// @param locationService The location service used to create the wayspot anchor service
    /// @param wayspotAnchorsConfiguration The configuration of the wayspot anchors
    public WayspotAnchorService
    (
      IARSession arSession,
      ILocationService locationService,
      IWayspotAnchorsConfiguration wayspotAnchorsConfiguration
    )
    {
      _wayspotAnchors = new Dictionary<Guid, IWayspotAnchor>();
      _arSession = arSession;
      _locationService = locationService;
      _wayspotAnchorsConfiguration = wayspotAnchorsConfiguration;

      _wayspotAnchorController = CreateWayspotAnchorController();

      if (_arSession != null)
      {
        _arSession.Deinitialized += HandleSessionDeinitialized;
        _arSession.Paused += HandleARSessionPaused;
        _arSession.Ran += HandleARSessionRan;
      }
    }

    private WayspotAnchorController CreateWayspotAnchorController()
    {
      var wayspotAnchorController = new WayspotAnchorController(_arSession, _locationService);
      wayspotAnchorController.LocalizationStateUpdated += HandleLocalizationStateUpdated;
      wayspotAnchorController.WayspotAnchorsCreated += HandleWayspotAnchorsCreated;

      wayspotAnchorController.StartVps(_wayspotAnchorsConfiguration);

      return wayspotAnchorController;
    }
    
    public override void Dispose()
    {
      if (_arSession != null)
      {
        _arSession.Paused -= HandleARSessionPaused;
        _arSession.Ran -= HandleARSessionRan;
      }

      base.Dispose();
    }

    private void HandleSessionDeinitialized(ARSessionDeinitializedArgs arSessionDeinitializedArgs)
    {
      DestroyWayspotAnchors(_wayspotAnchors.Keys.ToArray());

      _arSession.Deinitialized -= HandleSessionDeinitialized;
      _wayspotAnchorController.LocalizationStateUpdated -= HandleLocalizationStateUpdated;
      _wayspotAnchorController.WayspotAnchorsCreated -= HandleWayspotAnchorsCreated;
    }

    private void HandleARSessionPaused(ARSessionPausedArgs args)
    {
      _wayspotAnchorController.PauseTracking(_wayspotAnchors.Values.ToArray());
    }

    private void HandleARSessionRan(ARSessionRanArgs args)
    {
      _wayspotAnchorController.ResumeTracking(_wayspotAnchors.Values.ToArray());
    }
  }
}

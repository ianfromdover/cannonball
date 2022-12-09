// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;


namespace Niantic.ARDK.AR.WayspotAnchors
{
  public class WayspotAnchorControllerBase
  {
    /// Called when the localization status has changed
    public ArdkEventHandler<LocalizationStateUpdatedArgs> LocalizationStateUpdated;

    /// Called when new anchors have been created
    public ArdkEventHandler<WayspotAnchorsCreatedArgs> WayspotAnchorsCreated;

    /// Called when wayspot anchors report a new position/rotation
    public ArdkEventHandler<WayspotAnchorsResolvedArgs> WayspotAnchorsTrackingUpdated;

    /// Called when the status of wayspot anchors has changed
    public ArdkEventHandler<WayspotAnchorStatusUpdatedArgs> WayspotAnchorStatusUpdated;

    internal _IWayspotAnchorControllerImplementation _wayspotAnchorControllerImplementation;
    protected LocalizationState _localizationState;

    /// Creates a new wayspot anchor API to consume
    /// @param locationService The location service required by the WayspotAnchorControllerBase to run VPS.
    protected WayspotAnchorControllerBase()
    {
    }

    /// Starts the visual position system
    /// @param wayspotAnchorsConfiguration The configuration to start VPS with
    public virtual void StartVps(IWayspotAnchorsConfiguration wayspotAnchorsConfiguration)
    {
      _wayspotAnchorControllerImplementation.StartVPS(wayspotAnchorsConfiguration);
    }

    /// Stops the visual position system
    /// @note This will reset the state and stop all pending creations and trackings
    public void StopVps()
    {
      _wayspotAnchorControllerImplementation.StopVPS();
    }

    /// Creates new wayspot anchors based on position and rotations
    /// @param localPoses The position and rotation used to create new wayspot anchors with
    /// @return The IDs of the newly created wayspot anchors
    public Guid[] CreateWayspotAnchors(params Matrix4x4[] localPoses)
    {
      if (_localizationState != LocalizationState.Localized)
      {
        ARLog._Error
        (
          $"Failed to create wayspot anchor, because the Localization State is {_localizationState}."
        );

        return Array.Empty<Guid>();
      }

      return _wayspotAnchorControllerImplementation.SendWayspotAnchorsCreateRequest(localPoses);
    }

    /// Pauses the tracking of wayspot anchors.  This can be used to conserve resources for wayspot anchors which you currently do not care about,
    /// but may again in the future
    /// @param wayspotAnchors The wayspot anchors to pause tracking for
    public void PauseTracking(params IWayspotAnchor[] wayspotAnchors)
    {
      _wayspotAnchorControllerImplementation.StopResolvingWayspotAnchors(wayspotAnchors);
      foreach (var wayspotAnchor in wayspotAnchors)
      {
        var trackable = ((_IInternalTrackable)wayspotAnchor);
        trackable.SetTrackingEnabled(false);
      }
    }

    /// Resumes the tracking of previously paused wayspot anchors
    /// @param wayspotAnchors The wayspot anchors to resume tracking for
    public void ResumeTracking(params IWayspotAnchor[] wayspotAnchors)
    {
      _wayspotAnchorControllerImplementation.StartResolvingWayspotAnchors(wayspotAnchors);
      foreach (var wayspotAnchor in wayspotAnchors)
      {
        var trackable = ((_IInternalTrackable)wayspotAnchor);
        trackable.SetTrackingEnabled(true);
      }
    }

    /// Restores previously created wayspot anchors via their payloads.
    /// @note
    ///   Anchors will have 'WayspotAnchorStatusCode.Pending' status, where its
    ///   Position and Rotation values are invalid, until they are resolved and
    ///   reach 'WayspotAnchorStatusCode.Success' status.
    /// @param wayspotAnchorPayloads The payloads of the wayspot anchors to restore
    /// @return The restored wayspot anchors
    public IWayspotAnchor[] RestoreWayspotAnchors(params WayspotAnchorPayload[] wayspotAnchorPayloads)
    {
      var wayspotAnchors = new List<IWayspotAnchor>();
      foreach (var wayspotAnchorPayload in wayspotAnchorPayloads)
      {
        byte[] blob = wayspotAnchorPayload._Blob;
        var wayspotAnchor = _WayspotAnchorFactory.Create(blob);
        wayspotAnchors.Add(wayspotAnchor);
      }

      return wayspotAnchors.ToArray();
    }


    protected void HandleLocalizationStateUpdated(LocalizationStateUpdatedArgs args)
    {
      ARLog._DebugFormat
      (
        "VPS LocalizationState updated to {0} (reason: {1})",
        false,
        args.State,
        args.FailureReason
      );

      _localizationState = args.State;
      LocalizationStateUpdated?.Invoke(args);
    }

    protected void HandleWayspotAnchorsCreated(WayspotAnchorsCreatedArgs args)
    {
      WayspotAnchorsCreated?.Invoke(args);
    }

    protected void HandleWayspotAnchorsResolved(WayspotAnchorsResolvedArgs args)
    {
      foreach (var resolution in args.Resolutions)
      {
        var anchor = _WayspotAnchorFactory.GetOrCreateFromIdentifier(resolution.ID);
        ((_IInternalTrackable)anchor).SetTransform(resolution.Position, resolution.Rotation);
      }

      WayspotAnchorsTrackingUpdated?.Invoke(args);
    }

    protected void HandleWayspotAnchorStatusUpdated(WayspotAnchorStatusUpdatedArgs args)
    {
      foreach (var statusUpdate in args.WayspotAnchorStatusUpdates)
      {
        var anchor = _WayspotAnchorFactory.GetOrCreateFromIdentifier(statusUpdate.ID);
        ((_IInternalTrackable)anchor).SetStatusCode(statusUpdate.Code);
      }

      WayspotAnchorStatusUpdated?.Invoke(args);
    }
  }
}

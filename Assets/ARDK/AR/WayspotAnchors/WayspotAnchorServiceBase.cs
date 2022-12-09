using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  public class WayspotAnchorServiceBase:
    IDisposable
  {
    private const string _mustLocaliseBeforeCreatingAnchorErrorMessage = "Must localize before creating wayspot anchors.";

    /// Called when the localization status has changed
    public ArdkEventHandler<LocalizationStateUpdatedArgs> LocalizationStateUpdated;

    protected WayspotAnchorControllerBase _wayspotAnchorController;
    protected ILocationService _locationService;
    protected IWayspotAnchorsConfiguration _wayspotAnchorsConfiguration;
    protected Dictionary<Guid, IWayspotAnchor> _wayspotAnchors;

    /// The current localization state of the WayspotAnchor service
    public LocalizationState LocalizationState { get; private set; }

    /// The localization failure reason, if applicable
    public LocalizationFailureReason LocalizationFailureReason { get; private set; }

    /// Access low-level API
    internal WayspotAnchorControllerBase _WayspotAnchorController
    {
      get
      {
        return _wayspotAnchorController;
      }
    }

    /// Gets all of the wayspot anchors
    /// @return All of the wayspot anchors
    public IWayspotAnchor[] GetAllWayspotAnchors()
    {
      return _wayspotAnchors.Values.ToArray();
    }

    /// Gets a wayspot anchor by its ID
    /// @param id The ID of the wayspot anchor to retrieve
    /// @return The wayspot anchor
    public IWayspotAnchor GetWayspotAnchor(Guid id)
    {
      if (_wayspotAnchors.ContainsKey(id))
        return _wayspotAnchors[id];

      throw new ArgumentException($"No WayspotAnchor with identifier {id} exists.");
    }

    public IWayspotAnchor[] CreateWayspotAnchors(params Matrix4x4[] localPoses)
    {
      if (localPoses.Length == 0)
        throw new ArgumentException("Must supply at least one pose to create an anchor for.");

      var anchors = new IWayspotAnchor[localPoses.Length];

      if (LocalizationState != LocalizationState.Localized)
      {
        ARLog._Error(_mustLocaliseBeforeCreatingAnchorErrorMessage);
        return anchors;
      }

      var ids = _wayspotAnchorController.CreateWayspotAnchors(localPoses);
      for (int i = 0; i < anchors.Length; i++)
      {
        var anchor = _WayspotAnchorFactory.Create(ids[i], localPoses[i]);
        anchors[i] = anchor;

        _wayspotAnchors.Add(anchor.ID, anchor);
      }

      return anchors;
    }

    /// Creates new wayspot anchors
    /// @param callback The callback when the wayspot anchors have been created
    /// @param localPoses The positions and rotations used the create the wayspot anchors
    [Obsolete("Use CreateWayspotAnchors(params Matrix4x4[] localPoses) instead, and subscribe to the returned anchors' events.")]
    public async void CreateWayspotAnchors(Action<IWayspotAnchor[]> callback, params Matrix4x4[] localPoses)
    {
      if (LocalizationState != LocalizationState.Localized)
      {
        ARLog._Error(_mustLocaliseBeforeCreatingAnchorErrorMessage);
        return;
      }

      var wayspotAnchors = await CreateWayspotAnchorsAsync(localPoses);
      callback?.Invoke(wayspotAnchors);
    }

    /// Creates new wayspot anchors. The new wayspot anchor is tracked by default.
    /// @param localPoses The positions and rotations used the create the wayspot anchors
    /// @return The newly created wayspot anchors
    [Obsolete("Use CreateWayspotAnchors(params Matrix4x4[] localPoses) instead, and subscribe to the returned anchors' events.")]
    public async Task<IWayspotAnchor[]> CreateWayspotAnchorsAsync(params Matrix4x4[] localPoses)
    {
      if (LocalizationState != LocalizationState.Localized)
      {
        ARLog._Error(_mustLocaliseBeforeCreatingAnchorErrorMessage);
        return default;
      }

      var ids = _wayspotAnchorController.CreateWayspotAnchors(localPoses);
      var wayspotAnchors = await GetCreatedWayspotAnchors(ids);

      return wayspotAnchors;
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
      var wayspotAnchors = _wayspotAnchorController.RestoreWayspotAnchors(wayspotAnchorPayloads);
      foreach (var anchor in wayspotAnchors)
        _wayspotAnchors.Add(anchor.ID, anchor);

      _wayspotAnchorController.ResumeTracking(wayspotAnchors);
      return wayspotAnchors;
    }

    /// Destroys existing wayspot anchors
    /// @param anchors The wayspot anchors to destroy
    public void DestroyWayspotAnchors(params IWayspotAnchor[] anchors)
    {
      var ids = anchors.Select(a => a.ID);
      DestroyWayspotAnchors(ids.ToArray());
    }

    /// Destroys wayspot anchors by ID
    /// @param ids The IDs of the wayspot anchors to destroy
    public void DestroyWayspotAnchors(params Guid[] ids)
    {
      var wayspotAnchors =
        _wayspotAnchors.Values.Where(a => Array.IndexOf(ids, a.ID) >= 0).ToArray();

      _wayspotAnchorController?.PauseTracking(wayspotAnchors);

      foreach (var id in ids)
      {
        _wayspotAnchors[id].Dispose();
        _wayspotAnchors.Remove(id);
        _WayspotAnchorFactory.Remove(id);
      }
    }

    /// Restarts VPS
    public async void Restart()
    {
      _wayspotAnchorController.StopVps();
      _locationService.Stop();

      while (LocalizationState != LocalizationState.Stopped)
        await Task.Delay(1);

      _locationService.Start();
      _wayspotAnchorController.StartVps(_wayspotAnchorsConfiguration);

      // Resume tracking anchors that have been created
      while (LocalizationState != LocalizationState.Localized)
        await Task.Delay(1);

      var anchors = _wayspotAnchors.Values.ToArray();
      _wayspotAnchorController.ResumeTracking(anchors);
    }

    /// Disposes of the Wayspot Anchor Service
    public virtual void Dispose()
    {
      DestroyWayspotAnchors(_wayspotAnchors.Values.ToArray());
    }

    protected void HandleLocalizationStateUpdated(LocalizationStateUpdatedArgs args)
    {
      LocalizationState = args.State;
      LocalizationFailureReason = args.FailureReason;

      LocalizationStateUpdated?.Invoke(args);
    }

    protected void HandleWayspotAnchorsCreated(WayspotAnchorsCreatedArgs args)
    {
      foreach (var anchor in args.WayspotAnchors)
      {
        // Anchors created/restored using the synchronous methods are added to _wayspotAnchors right away.
        // However, anchors created using the (obsolete) asynchronous methods can only be added
        // once the WayspotAnchorController.WayspotAnchorsCreated event is invoked.
        if (!_wayspotAnchors.ContainsKey(anchor.ID))
          _wayspotAnchors.Add(anchor.ID, anchor);
      }

      _wayspotAnchorController.ResumeTracking(args.WayspotAnchors);
    }

    private async Task<IWayspotAnchor[]> GetCreatedWayspotAnchors(Guid[] ids, int timeoutMilliseconds = 1000)
    {
      IWayspotAnchor[] wayspotAnchors = null;
      var task = TaskUtility.WaitUntil
      (
        () =>
        {
          wayspotAnchors =
            _wayspotAnchors
              .Where(a => ids.Contains(a.Key))
              .Select(a => a.Value)
              .ToArray();

          return wayspotAnchors.Length == ids.Length;
        }
      );

      var timeout = Task.Delay(timeoutMilliseconds);
      await Task.WhenAny(task, timeout);

      if (wayspotAnchors.Length == 0)
        ARLog._WarnRelease("Wayspot anchor creation timed out.");

      return wayspotAnchors;
    }
  }
}

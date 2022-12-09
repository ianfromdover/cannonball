using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal abstract class _MockWayspotAnchorControllerImplementationBase: _IWayspotAnchorControllerImplementation
  {
    private Dictionary<Guid, _MockWayspotAnchor> _wayspotAnchors =
      new Dictionary<Guid, _MockWayspotAnchor>();

    private List<Guid> _resolvedWayspotAnchors = new List<Guid>();
    protected bool _isDisposed;
    private LocalizationState _localizationState;
    private float _timeSinceLastStateUpdate;

    /// Called when the localization state has changed
    public event ArdkEventHandler<LocalizationStateUpdatedArgs> LocalizationStateUpdated;

    /// Called when the status of managed poses has changed
    public event ArdkEventHandler<WayspotAnchorStatusUpdatedArgs> WayspotAnchorStatusUpdated;

    /// Called when new wayspot anchors have been created
    public event ArdkEventHandler<WayspotAnchorsCreatedArgs> WayspotAnchorsCreated;

    /// Called when wayspot anchors have updated their position/rotation
    public event ArdkEventHandler<WayspotAnchorsResolvedArgs> WayspotAnchorsResolved;

    /// Disposes the Mock Wayspot Anchor Controller
    public void Dispose()
    {
      _isDisposed = true;
    }

    public void StartVPS(IWayspotAnchorsConfiguration wayspotAnchorsConfiguration)
    {
      SetLocalizationState(LocalizationState.Initializing, LocalizationFailureReason.None);
      _UpdateLoop.Tick += OnUpdateAchieveLocalization;
    }

    public void StopVPS()
    {
      _UpdateLoop.Tick -= OnUpdateAchieveLocalization;
      SetLocalizationState(LocalizationState.Stopped, LocalizationFailureReason.Canceled);
    }

    public Guid[] SendWayspotAnchorsCreateRequest(params Matrix4x4[] localPoses)
    {
      var createdWayspotAnchors = new Dictionary<Guid, Matrix4x4>();
      var ids = new List<Guid>();
      foreach (var localPose in localPoses)
      {
        var id = Guid.NewGuid();
        ids.Add(id);
        createdWayspotAnchors.Add(id, localPose);
      }

      CreateWayspotAnchorsAsync(createdWayspotAnchors);
      return ids.ToArray();
    }

    private async void CreateWayspotAnchorsAsync(Dictionary<Guid, Matrix4x4> localPoses)
    {
      await SimulateServerWorkAsync();
      var createdWayspotAnchors = new List<IWayspotAnchor>();
      foreach (var anchorData in localPoses)
      {
        var anchor = _WayspotAnchorFactory.GetOrCreateFromIdentifier(anchorData.Key);
        createdWayspotAnchors.Add(anchor);
        _wayspotAnchors.Add(anchor.ID, (_MockWayspotAnchor)anchor);
      }

      var wayspotAnchorsCreatedArgs = new WayspotAnchorsCreatedArgs(createdWayspotAnchors.ToArray());
      WayspotAnchorsCreated?.Invoke(wayspotAnchorsCreatedArgs);

      var statuses = new List<WayspotAnchorStatusUpdate>();
      foreach (var createdWayspotAnchor in createdWayspotAnchors)
      {
        var id = createdWayspotAnchor.ID;
        statuses.Add(new WayspotAnchorStatusUpdate(id, WayspotAnchorStatusCode.Success));
        ((_IInternalTrackable)createdWayspotAnchor).SetStatusCode(WayspotAnchorStatusCode.Success);
      }

      var wayspotAnchorStatusUpdatedArgs = new WayspotAnchorStatusUpdatedArgs(statuses.ToArray());
      WayspotAnchorStatusUpdated?.Invoke(wayspotAnchorStatusUpdatedArgs);
    }

    public void StartResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
      var statuses = new List<WayspotAnchorStatusUpdate>();
      foreach (var anchor in wayspotAnchors)
      {
        _resolvedWayspotAnchors.Add(anchor.ID);
        if (!_wayspotAnchors.ContainsKey(anchor.ID))
        {
          _wayspotAnchors.Add(anchor.ID, (_MockWayspotAnchor)anchor);
          statuses.Add(new WayspotAnchorStatusUpdate(anchor.ID, WayspotAnchorStatusCode.Success));
        }

        ((_IInternalTrackable)anchor).SetStatusCode(WayspotAnchorStatusCode.Success);
      }

      var wayspotAnchorStatusUpdatedArgs = new WayspotAnchorStatusUpdatedArgs(statuses.ToArray());
      WayspotAnchorStatusUpdated?.Invoke(wayspotAnchorStatusUpdatedArgs);
    }

    /// Stops resolving the wayspot anchors
    /// param wayspotAnchors Wayspot anchors to stop resolving
    public void StopResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
      foreach (var anchor in wayspotAnchors)
        _resolvedWayspotAnchors.Remove(anchor.ID);
    }

    protected async void ResolveWayspotAnchorsAsync()
    {
      while (!_isDisposed)
      {
        await SimulateServerWorkAsync();
        if (_localizationState != LocalizationState.Localized)
          continue;

        var resolutions = new List<WayspotAnchorResolvedArgs>();
        foreach (var id in _resolvedWayspotAnchors)
        {
          var anchor = _wayspotAnchors[id];
          ((_IInternalTrackable)anchor).SetTransform(anchor.LastKnownPosition, anchor.LastKnownRotation);

          var resolution = new WayspotAnchorResolvedArgs(id, anchor.LastKnownPosition, anchor.LastKnownRotation);
          resolutions.Add(resolution);
        }

        var wayspotAnchorsResolvedArgs = new WayspotAnchorsResolvedArgs(resolutions.ToArray());
        WayspotAnchorsResolved?.Invoke(wayspotAnchorsResolvedArgs);
      }
    }

    protected void SetLocalizationState(LocalizationState state, LocalizationFailureReason failureReason)
    {
      if (state == _localizationState)
        return;

      _timeSinceLastStateUpdate = 0;
      _localizationState = state;

      var args = new LocalizationStateUpdatedArgs(state, failureReason);
      LocalizationStateUpdated?.Invoke(args);
    }

    private void OnUpdateAchieveLocalization()
    {
      _timeSinceLastStateUpdate += Time.deltaTime;

      if (_timeSinceLastStateUpdate < 1f)
        return;

      switch (_localizationState)
      {
        case LocalizationState.Initializing:
          SetLocalizationState(LocalizationState.Localizing, LocalizationFailureReason.None);
          break;

        case LocalizationState.Localizing:
          SetLocalizationState(LocalizationState.Localized, LocalizationFailureReason.None);
          break;

        case LocalizationState.Localized:
        case LocalizationState.Failed:
        case LocalizationState.Stopped:
          break;
      }
    }

    private async Task SimulateServerWorkAsync()
    {
      await Task.Delay(TimeSpan.FromSeconds(1));
    }
  }
}

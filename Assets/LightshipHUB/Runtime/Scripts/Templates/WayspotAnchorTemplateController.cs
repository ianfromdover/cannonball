// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

namespace Niantic.LightshipHub.Templates
{
  public class WayspotAnchorTemplateController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    [HideInInspector]
    public Text StatusLog;
    [HideInInspector]
    public Text LocalizationStatus;

    private WayspotAnchorService _wayspotAnchorService;
    private IARSession _arSession;
    private LocalizationState _localizationState;

    private readonly HashSet<WayspotAnchorTracker> _wayspotAnchorTrackers =
      new HashSet<WayspotAnchorTracker>();

    private IWayspotAnchorsConfiguration _config;

    private void Awake()
    {
      StatusLog.text = "Initializing Session.";
      OHcontroller.ObjectHolder.AddComponent<ShowTracker>();
    }

    private void OnEnable()
    {
      ARSessionFactory.SessionInitialized += HandleSessionInitialized;
    }

    private void OnDisable()
    {
      ARSessionFactory.SessionInitialized -= HandleSessionInitialized;
    }

    private void OnDestroy()
    {
      if (_wayspotAnchorService != null)
      {
        _wayspotAnchorService.LocalizationStateUpdated -= LocalizationStateUpdated;
        _wayspotAnchorService.Dispose();
      }
    }

     private void Update()
    {
      if (_wayspotAnchorService == null)
      {
        return;
      }
      //Get the pose where you tap on the screen
      var success = TryGetTouchInput(out Matrix4x4 localPose);
      
      if (success)
      {
        if (_wayspotAnchorService.LocalizationState == LocalizationState.Localized)
          PlaceAnchor(localPose); //Create the Wayspot Anchor and place the GameObject
        else
          StatusLog.text = "Must localize before placing anchor.";
      }
    }

    /// Saves all of the existing wayspot anchors
    public void SaveWayspotAnchors()
    {
      if (_wayspotAnchorTrackers.Count > 0)
      {
        var wayspotAnchors = _wayspotAnchorService.GetAllWayspotAnchors();
        var payloads = wayspotAnchors.Select(a => a.Payload);
        WayspotAnchorTemplateDataUtility.SaveLocalPayloads(payloads.ToArray());
      }
      else
      {
        WayspotAnchorTemplateDataUtility.SaveLocalPayloads(Array.Empty<WayspotAnchorPayload>());
      }

      StatusLog.text = $"Saved {_wayspotAnchorTrackers.Count} Wayspot Anchors.";
    }

    /// Loads all of the saved wayspot anchors
    public void LoadWayspotAnchors()
    {
      var payloads = WayspotAnchorTemplateDataUtility.LoadLocalPayloads();
      if (payloads.Length > 0)
      {
        foreach (var payload in payloads)
        {
          var anchors = _wayspotAnchorService.RestoreWayspotAnchors(payload);
          if (anchors.Length == 0)
            return; // error raised in CreateWayspotAnchors

          CreateWayspotAnchorGameObject(anchors[0], Vector3.zero, Quaternion.identity, false);
        }

        StatusLog.text = $"Loaded {_wayspotAnchorTrackers.Count} anchors.";
      }
      else
      {
        StatusLog.text = "No anchors to load.";
      }
    }

    /// Clears all of the active wayspot anchors
    public void ClearAnchorGameObjects()
    {
      if (_wayspotAnchorTrackers.Count == 0)
      {
        StatusLog.text = "No anchors to clear.";
        return;
      }

      foreach (var anchor in _wayspotAnchorTrackers)
      {
        Destroy(anchor.gameObject);
      }

      var wayspotAnchors = _wayspotAnchorTrackers.Select(go => go.WayspotAnchor).ToArray();
      _wayspotAnchorService.DestroyWayspotAnchors(wayspotAnchors);

      _wayspotAnchorTrackers.Clear();
      StatusLog.text = "Cleared Wayspot Anchors.";
    }

    /// Pauses the AR Session
    public void PauseARSession()
    {
      if (_arSession.State == ARSessionState.Running)
      {
        _arSession.Pause();
        StatusLog.text = $"AR Session Paused.";
      }
      else
      {
        StatusLog.text = $"Cannot pause AR Session.";
      }
    }

    /// Resumes the AR Session
    public void ResumeARSession()
    {
      if (_arSession.State == ARSessionState.Paused)
      {
        _arSession.Run(_arSession.Configuration);
        StatusLog.text = $"AR Session Resumed.";
      }
      else
      {
        StatusLog.text = $"Cannot resume AR Session.";
      }
    }

    public void RestartWayspotAnchorService()
    {
      _wayspotAnchorService.Restart();
    }

    private void HandleSessionInitialized(AnyARSessionInitializedArgs anyARSessionInitializedArgs)
    {
      StatusLog.text = "Running Session...";
      _arSession = anyARSessionInitializedArgs.Session;
      _arSession.Ran += HandleSessionRan;
    }

    private void HandleSessionRan(ARSessionRanArgs arSessionRanArgs)
    {
      _arSession.Ran -= HandleSessionRan;
      _wayspotAnchorService = CreateWayspotAnchorService();
      _wayspotAnchorService.LocalizationStateUpdated += OnLocalizationStateUpdated;
      StatusLog.text = "Session Initialized.";
    }

    private void OnLocalizationStateUpdated(LocalizationStateUpdatedArgs args)
    {
      LocalizationStatus.text = "Localization status: " + args.State;
    }

    private WayspotAnchorService CreateWayspotAnchorService()
    {
      var locationService = LocationServiceFactory.Create(_arSession.RuntimeEnvironment);
      locationService.Start();

      if (_config == null)
        _config = WayspotAnchorsConfigurationFactory.Create();

      var wayspotAnchorService =
        new WayspotAnchorService
        (
          _arSession,
          locationService,
          _config
        );

      wayspotAnchorService.LocalizationStateUpdated += LocalizationStateUpdated;

      return wayspotAnchorService;
    }

    private void LocalizationStateUpdated(LocalizationStateUpdatedArgs args)
    {
      LocalizationStatus.text = args.State.ToString();
    }

    private void PlaceAnchor(Matrix4x4 localPose)
    {
      var anchors = _wayspotAnchorService.CreateWayspotAnchors(localPose);
      if (anchors.Length == 0)
        return; // error raised in CreateWayspotAnchors

      var position = localPose.ToPosition();
      var rotation = localPose.ToRotation();
      CreateWayspotAnchorGameObject(anchors[0], position, rotation, true);

      StatusLog.text = "Anchor placed.";
    }

    private GameObject CreateWayspotAnchorGameObject
    (
      IWayspotAnchor anchor,
      Vector3 position,
      Quaternion rotation,
      bool startActive
    )
    {
      var go = Instantiate(OHcontroller.ObjectHolder, position, rotation);

      var tracker = go.GetComponent<WayspotAnchorTracker>();
      if (tracker == null)
      {
        Debug.Log("Anchor prefab was missing WayspotAnchorTracker, so one will be added.");
        tracker = go.AddComponent<WayspotAnchorTracker>();
      }

      tracker.gameObject.SetActive(startActive);
      tracker.AttachAnchor(anchor);
      _wayspotAnchorTrackers.Add(tracker);

      return go;
    }

    private bool TryGetTouchInput(out Matrix4x4 localPose)
    {
      if (_arSession == null || PlatformAgnosticInput.touchCount <= 0)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.IsTouchOverUIObject())
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      if (touch.phase != TouchPhase.Began)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var currentFrame = _arSession.CurrentFrame;
      if (currentFrame == null)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      if (_arSession.RuntimeEnvironment == RuntimeEnvironment.Playback)
      {
        // Playback doesn't support plane detection yet, so instead of hit testing against planes,
        // just place the anchor in front of the camera.
        localPose =
          Matrix4x4.TRS
          (
            OHcontroller.Camera.transform.position + (OHcontroller.Camera.transform.forward * 2),
            Quaternion.identity,
            Vector3.one
          );
      }
      else
      {
        var results = currentFrame.HitTest
        (
          OHcontroller.Camera.pixelWidth,
          OHcontroller.Camera.pixelHeight,
          touch.position,
          ARHitTestResultType.ExistingPlane
        );

        int count = results.Count;
        if (count <= 0)
        {
          localPose = Matrix4x4.zero;
          return false;
        }

        var result = results[0];
        localPose = result.WorldTransform;
      }

      return true;
    }

    public void SetConfig(IWayspotAnchorsConfiguration config)
    {
      _config = config;
    }
  }
}

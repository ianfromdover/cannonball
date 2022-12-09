// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
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
  public class LeaveMessagesTemplateController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    [HideInInspector]
    public Text StatusLog;
    [HideInInspector]
    public Text LocalizationStatus;
    [HideInInspector]
    public MessagePanels MessagePanels;

    private WayspotAnchorService _wayspotAnchorService;
    private IARSession _arSession;
    private LocalizationState _localizationState;

    private readonly HashSet<WayspotAnchorTracker> _wayspotAnchorTrackers =
      new HashSet<WayspotAnchorTracker>();

    private List<string> _wayspotAnchorMessages = new List<string>();

    private IWayspotAnchorsConfiguration _config;

    private Matrix4x4 _localPose;

    private void Awake()
    {
      StatusLog.text = "Initializing Session.";
      OHcontroller.ObjectHolder.AddComponent<ShowTracker>();
      MessagePanels.LeaveMessagePanel.SetActive(false);
      MessagePanels.FindMessagePanel.SetActive(false);
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
        {
          _localPose = localPose;
          MessagePanels.InputMessage.text = "";
          // Ask for message in UI
          MessagePanels.LeaveMessagePanel.SetActive(true);
        }
        else
          StatusLog.text = "Must localize before placing message.";
      } 
    }

    /// Save active messages
    public void SaveMessages()
    {
      var messagesData = new LeaveMessagesUtility.MessagesData();

      if (_wayspotAnchorTrackers.Count > 0)
      {
        var wayspotAnchors = _wayspotAnchorService.GetAllWayspotAnchors();
        var payloads = wayspotAnchors.Select(a => a.Payload);

        messagesData.Payloads = payloads.Select(a => a.Serialize()).ToArray();
        messagesData.Messages = _wayspotAnchorMessages.ToArray();

      }

      LeaveMessagesUtility.SaveLocalMessages(messagesData);

      StatusLog.text = $"Saved {_wayspotAnchorTrackers.Count} Messages.";
    }

    /// Load local messages
    public void LoadMessages()
    {
      var messagesData = LeaveMessagesUtility.LoadLocalMessages();

      var messages = messagesData.Messages;
      var payloads = new List<WayspotAnchorPayload>();

      foreach (var wayspotAnchorPayload in messagesData.Payloads)
      {
        var payload = WayspotAnchorPayload.Deserialize(wayspotAnchorPayload);
        payloads.Add(payload);
      }

      if (payloads.Count > 0)
      {
        for (int i = 0; i < payloads.Count; i++)
        {
          var anchors = _wayspotAnchorService.RestoreWayspotAnchors(payloads[i]);
          if (anchors.Length == 0)
            return; // error raised in CreateWayspotAnchors

          CreateWayspotAnchorGameObject(anchors[0], Vector3.zero, Quaternion.identity, false, messages[i]);
        }

        StatusLog.text = $"Loaded {_wayspotAnchorTrackers.Count} messages.";
      }
      else
      {
        StatusLog.text = "No messages to load.";
      }
    }

    /// Clears all of the active messages
    public void ClearMessages()
    {
      if (_wayspotAnchorTrackers.Count == 0)
      {
        StatusLog.text = "No messages to clear.";
        return;
      }

      foreach (var anchor in _wayspotAnchorTrackers)
      {
        Destroy(anchor.gameObject);
      }
      _wayspotAnchorMessages = new List<string>();

      var wayspotAnchors = _wayspotAnchorTrackers.Select(go => go.WayspotAnchor).ToArray();
      _wayspotAnchorService.DestroyWayspotAnchors(wayspotAnchors);

      _wayspotAnchorTrackers.Clear();
      StatusLog.text = "Messages Cleared.";
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

    //Place anchor after writing message in UI
    public void PlaceAnchor()
    {
      var anchors = _wayspotAnchorService.CreateWayspotAnchors(_localPose);
      if (anchors.Length == 0)
        return; // error raised in CreateWayspotAnchors

      var position = _localPose.ToPosition();
      var rotation = _localPose.ToRotation();

      CreateWayspotAnchorGameObject(anchors[0], position, rotation, true, MessagePanels.InputMessage.text);

      StatusLog.text = "Message placed.";
    }

    private GameObject CreateWayspotAnchorGameObject
    (
      IWayspotAnchor anchor,
      Vector3 position,
      Quaternion rotation,
      bool startActive,
      string message
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
      _wayspotAnchorMessages.Add(message);

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

      // Check if previous anchor was touched
      if (CheckHit(touch))
      {
        StatusLog.text = "Message received.";
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

    private bool CheckHit(Touch touch)
    {
      var worldRay = OHcontroller.Camera.ScreenPointToRay(touch.position);
      RaycastHit hit;

      // Check if object holder with tracker was touched
      if (Physics.Raycast(worldRay, out hit, 1000f))
      {
        ObjectAnimation objAnimation = hit.transform.GetComponentInParent(typeof(ObjectAnimation)) as ObjectAnimation;
        ShowTracker tracker = hit.transform.GetComponentInParent(typeof(ShowTracker)) as ShowTracker;
        Animator animator = hit.transform.GetComponentInParent(typeof(Animator)) as Animator;

        if (tracker != null)
        {
          foreach (var wayspotTracker in _wayspotAnchorTrackers)
          {
            if (wayspotTracker.WayspotAnchor.ID == tracker.WayspotAnchor.ID)
            {
              var index = _wayspotAnchorTrackers.ToList().IndexOf(wayspotTracker);
              MessagePanels.Message.text = _wayspotAnchorMessages[index];
              if (animator != null) 
              {
                // If the object has an animator, write here the name of the animation to reproduce
                animator.SetTrigger("OpenTrigger");
                StartCoroutine(OpenMessage());
              }
              else 
              {
                MessagePanels.FindMessagePanel.SetActive(true);
              }
              return true;
            }
          }
        }
      }
      return false;
    }

    // Show message contained in wayspot anchor
    private IEnumerator OpenMessage() 
    {
      yield return new WaitForSeconds(0.8f);
      MessagePanels.FindMessagePanel.SetActive(true);
    }

    public void SetConfig(IWayspotAnchorsConfiguration config)
    {
      _config = config;
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections;
using System.Text;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.Remote;
using Niantic.ARDK.VirtualStudio.Remote.Data;

using UnityEngine;
using UnityEngine.UI;

using Random = System.Random;

namespace Niantic.ARDK.VirtualStudio.Remote
{
  /// <summary>
  /// Handles the mobile display logic of Remote Connection.
  /// </summary>
  public class RemoteConnectionUI: MonoBehaviour
  {
    [Header("Connection Starting UI")]
    [SerializeField]
    private GameObject _postSelectionUI = null;

    [Header("Connected UI")]
    [SerializeField]
    private Text _connectionStatusText = null;

    [SerializeField]
    private Text _arSessionStatusText = null;

    [SerializeField]
    private Text _networkingStatusText = null;

    [SerializeField]
    private Text _arNetworkingStatusText = null;

    private IARSession _activeARSession;

    private void Awake()
    {
      SubscribeToLifecycleEvents();
      _postSelectionUI.SetActive(true);
      StartConnection(_RemoteConnection.ConnectionMethod.USB);
      
      _RemoteConnection.Deinitialized += Reset;
    }

    private void Reset()
    {
      Camera.main.backgroundColor = Color.blue;
    }

    private void StartConnection(_RemoteConnection.ConnectionMethod connectionMethod)
    {
      _postSelectionUI.SetActive(true);

      // Connect using settings.
      _RemoteConnection.InitIfNone(connectionMethod);
      _RemoteConnection.Connect(null);
    }

    private void Update()
    {
      // UI is not visible when camera feed is rendering
      if (_activeARSession != null && _activeARSession.State == ARSessionState.Running)
        return;

      // Update connection info.
      if (_RemoteConnection.IsConnected)
      {
        _connectionStatusText.text = "Connected to editor!";
        Camera.main.backgroundColor = Color.green;
      }
      else if (_RemoteConnection.IsReady)
      {
        _connectionStatusText.text = "Waiting for connection...";
        Camera.main.backgroundColor = Color.blue;
      }
      else
      {
        _connectionStatusText.text = "Unity Editor disconnected.";
        Camera.main.backgroundColor = Color.gray;
      }
    }

    private void OnDestroy()
    {
      _RemoteConnection.Deinitialize();
    }

    private void SubscribeToLifecycleEvents()
    {
      ARSessionFactory.SessionInitialized +=
        args =>
        {
          ARLog._Debug("[Remote] ARSession Initialized: " + args.Session.StageIdentifier);
          _activeARSession = args.Session;
          _activeARSession.Deinitialized += _ => _activeARSession = null;

          UpdateStatusVisual(_arSessionStatusText, true);

          args.Session.Deinitialized +=
            deinitializedArgs =>
            {
              ARLog._Debug("[Remote] ARSession Deinitialized.");
              UpdateStatusVisual(_arSessionStatusText, false);
            };
        };

      MultipeerNetworkingFactory.NetworkingInitialized +=
        args =>
        {
          ARLog._Debug("[Remote] MultipeerNetworking Initialized: " + args.Networking.StageIdentifier);
          UpdateNetworkingsCount();
          UpdateStatusVisual(_networkingStatusText, true);

          args.Networking.Deinitialized +=
            deinitializedArgs =>
            {
              ARLog._Debug("[Remote] MultipeerNetworking Deinitialized.");

              var networkingsCount = UpdateNetworkingsCount();
              UpdateStatusVisual(_networkingStatusText, networkingsCount > 0);
            };
        };

      ARNetworkingFactory.ARNetworkingInitialized +=
        args =>
        {
          ARLog._Debug("[Remote] ARNetworking Initialized: " + args.ARNetworking.ARSession.StageIdentifier);
          UpdateStatusVisual(_arNetworkingStatusText, true);

          args.ARNetworking.Deinitialized +=
            deinitializedArgs =>
            {
              ARLog._Debug("[Remote] ARNetworking Deinitialized.");
              UpdateStatusVisual(_arNetworkingStatusText, false);
            };
        };
    }

    private readonly Color FADED_WHITE = new Color(1, 1, 1, 0.5f);
    private void UpdateStatusVisual(Text statusText, bool isConstructed)
    {
      if (statusText != null)
      {
        statusText.fontStyle = isConstructed ? FontStyle.Bold : FontStyle.Normal;
        statusText.color = isConstructed ? Color.white : FADED_WHITE;
      }
    }

    private int UpdateNetworkingsCount()
    {
      var networkingsCount = MultipeerNetworkingFactory.Networkings.Count;
      _networkingStatusText.text = "MultipeerNetworking x" + networkingsCount;
      return networkingsCount;
    }
  }
}

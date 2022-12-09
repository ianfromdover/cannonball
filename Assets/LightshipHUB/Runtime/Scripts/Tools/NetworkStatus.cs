// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;

namespace Niantic.LightshipHub.Tools
{
  /// <summary>
  /// A simple class that listens to the status of the network, and modifies a UI element
  /// to reflect the detected network state. Designed to be placed on a UI prefab, to provide
  /// functionality by simply dropping the prefab into the scene.
  /// </summary>
  public class NetworkStatus:
    MonoBehaviour
  {
    private enum NetworkState
    {
      Uninitialized,
      Initialized,
      ConnectedAsHost,
      ConnectedAsPeer,
      FailedToConnect
    }

    /// Optional. Indicates the current session state with a color.
    [SerializeField]
    private Image ConnectedIndicator = null;

    /// Optional. Displays text explaining what the current network state is
    [SerializeField]
    private Text ConnectedIndicatorText = null;

    /// Only print of first X digits of peer id onto screen
    [SerializeField]
    private int PeerIdLimit = 6;

    [SerializeField]
    private Color uninitializedColor, initializedColor, connectedAsHostColor, connectedAsPeerColor, failedToConnectColor;

    private IMultipeerNetworking _networking = null;

    // Used to avoid spamming the log with the same error
    private uint _lastNetworkError = 0;

    private Dictionary<NetworkState, string> _indicatorMessages =
      new Dictionary<NetworkState, string>()
      {
        { NetworkState.Uninitialized, "Not Initialized" },
        { NetworkState.Initialized, "Not Connected" },
        { NetworkState.ConnectedAsHost, "Connected as Host" },
        { NetworkState.ConnectedAsPeer, "Connected as Guest" },
        { NetworkState.FailedToConnect, "Failed to Connect" },
      };

    public void ListenToNetworking(IMultipeerNetworking networking)
    {
      StopListeningToNetworking();

      UpdateIndicator(NetworkState.Initialized);

      _networking = networking;
      _networking.Connected += OnNetworkConnected;
      _networking.ConnectionFailed += OnConnectionDidFailWithError;
      _networking.PeerAdded += OnPeerAdded;
      _networking.PeerRemoved += OnPeerRemoved;
      _networking.Disconnected += OnNetworkDisconnected;
      _networking.Deinitialized += OnNetworkDeinitialized;
    }

    private void StopListeningToNetworking()
    {
      if (_networking == null)
        return;

      _networking.Connected -= OnNetworkConnected;
      _networking.ConnectionFailed -= OnConnectionDidFailWithError;
      _networking.PeerAdded -= OnPeerAdded;
      _networking.PeerRemoved -= OnPeerRemoved;
      _networking.Disconnected -= OnNetworkDisconnected;
      _networking.Deinitialized -= OnNetworkDeinitialized;
    }

    private void Awake()
    {
      UpdateIndicator(NetworkState.Uninitialized);
    }

    private void Start()
    {
      if (_networking == null)
        MultipeerNetworkingFactory.NetworkingInitialized += OnNetworkingInitialized;
    }

    private void OnDestroy()
    {
      MultipeerNetworkingFactory.NetworkingInitialized -= OnNetworkingInitialized;
      StopListeningToNetworking();
    }

    private void OnNetworkingInitialized(AnyMultipeerNetworkingInitializedArgs args)
    {
      // This currently only supports automatically catching the first networking object it sees
      if (_networking != null)
        return;

      Debug.Log("Network session initialized");
      ListenToNetworking(args.Networking);
    }

    private void OnNetworkConnected(ConnectedArgs args)
    {
      var state = args.IsHost ? NetworkState.ConnectedAsHost : NetworkState.ConnectedAsPeer;
      var self = args.Self;
      var msg = string.Format("{0} ({1})", _indicatorMessages[state], self.ToString(PeerIdLimit));

      UpdateIndicator(state, msg);
      Debug.Log(_indicatorMessages[state]);
      Debug.Log(string.Format("\tSelf ID: {0}", self.Identifier));
      Debug.Log(string.Format("\tHost ID: {0}", args.Host.Identifier));
      _lastNetworkError = 0;
    }

    private void OnNetworkDisconnected(DisconnectedArgs args)
    {
      UpdateIndicator(NetworkState.Initialized);
      Debug.Log("Disconnected from network");
    }

    private void OnNetworkDeinitialized(DeinitializedArgs args)
    {
      UpdateIndicator(NetworkState.Uninitialized);
      Debug.Log("Network session was deinitialized");

      StopListeningToNetworking();
      _networking = null;
    }

    private void OnPeerAdded(PeerAddedArgs args)
    {
      Debug.Log("Added: " + args.Peer.ToString(PeerIdLimit));
    }

    private void OnPeerRemoved(PeerRemovedArgs args)
    {
      Debug.Log("Removed: " + args.Peer.ToString(PeerIdLimit));
    }

    private void OnConnectionDidFailWithError(ConnectionFailedArgs args)
    {
      var errorCode = args.ErrorCode;
      if (_lastNetworkError == errorCode)
        return;

      var msg = "Connection Failed: " + errorCode;
      UpdateIndicator(NetworkState.FailedToConnect, msg);
      Debug.Log(msg);
      _lastNetworkError = errorCode;
    }

    private void UpdateIndicator(NetworkState state, string text = null)
    {
      if (ConnectedIndicator != null)
      {
        Color newColor = Color.white;
        switch(state)
        {
          case NetworkState.Uninitialized:
            newColor = uninitializedColor;
            break;
          case NetworkState.Initialized:
            newColor = initializedColor;
            break;
          case NetworkState.ConnectedAsHost:
            newColor = connectedAsHostColor;
            break;
          case NetworkState.ConnectedAsPeer:
            newColor = connectedAsPeerColor;
            break;
          case NetworkState.FailedToConnect:
            newColor = failedToConnectColor;
            break;
          default:
            newColor = Color.white;
            break;
        }
        ConnectedIndicator.color = newColor;
      }

      if (ConnectedIndicatorText != null)
        ConnectedIndicatorText.text = string.IsNullOrEmpty(text) ? _indicatorMessages[state] : text;
    }
  }
}

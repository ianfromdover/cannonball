// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using Niantic.ARDK.AR;
using Niantic.Experimental.ARDK.SharedAR;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.BinarySerialization;
using Niantic.ARDK.Utilities.BinarySerialization.ItemSerializers;
using Niantic.ARDK.Utilities.Extensions;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class MockColocalization :
    IColocalization
  {
    private const string STATE_PREFIX = "state_";
    private INetworking _networking;
    private IDatastore _datastore;
    private IPeerID _self;

    public MockColocalization(INetworking networking, IDatastore datastore) {
        _networking = networking; // Can be null
        _datastore = datastore;
    }

    public void Start() {
        // Send self peer state stable
        if (_networking != null && _datastore != null) {
            _datastore.KeyValueAdded += OnPersistentKVUpdated;
            _datastore.KeyValueUpdated += OnPersistentKVUpdated;
            _networking.ConnectionEvent += OnConnectionStateChanged;

        } else{
          ARLog._Debug("Cannot start Colocalization");
        }
    }

    public void Stop() {
      // TODO: implement
    }

    private void OnPersistentKVUpdated(KeyValuePairArgs args) {
      if (args.Key.StartsWith(STATE_PREFIX))
      {
        var peer = PeerSerialization.PeerFromKey(args.Key);
        if (peer.Equals(_self)) {
          // Potential boiler plate?
          byte[] value = new byte[1024]; // TODO: adjust buf size
          _datastore.GetData(args.Key, ref value);
          var colocalizationState = 
            PeerSerialization.ColocalizationStateFromBytes(new MemoryStream(value));
          var stateArgs = new ColocalizationStateUpdatedArgs
          (
            peer,
            colocalizationState
          );
          var handler = ColocalizationStateUpdated;
          handler?.Invoke(stateArgs);
        }
      }
    }

    private void OnConnectionStateChanged(ConnectionEventArgs args) {
      if (args.connectionEvent == ConnectionEvents.Connected)
      {
        _self = _networking.SelfPeerID;
        _datastore.SetData(
          PeerSerialization.KeyFromPeer(STATE_PREFIX, _self),
          PeerSerialization.BytesFromColocalizationState(ColocalizationState.Colocalized)
        );
      }
    }

    // Stop colocalization 
    public void Pause() {

    }

    public ReadOnlyDictionary<IPeerID, ColocalizationState> ColocalizationStates { get; }
    public ReadOnlyDictionary<IPeerID, Matrix4x4> LatestPeerPoses { get; }
    public ColocalizationFailureReason FailureReason { get; }
    public Matrix4x4 AlignedSpaceOrigin { get; }

#pragma warning disable 0067
    public event ArdkEventHandler<ColocalizationStateUpdatedArgs> ColocalizationStateUpdated;
    public event ArdkEventHandler<PeerPoseReceivedArgs> PeerPoseReceived;
#pragma warning restore 0067

    public void LocalPoseToAligned(Matrix4x4 poseInLocalSpace, out Matrix4x4 poseInAlignedSpace) {
        poseInAlignedSpace = poseInLocalSpace;
    }

    public ColocalizationAlignmentResult AlignedPoseToLocal(IPeerID id, Matrix4x4 poseInAlignedSpace, out Matrix4x4 poseInLocalSpace) {
        poseInLocalSpace = poseInAlignedSpace;
        return ColocalizationAlignmentResult.Success;
    }

    public void Dispose()
    {
      if (_datastore == null)
        return;
      
      _datastore.KeyValueAdded += OnPersistentKVUpdated;
      _datastore.KeyValueUpdated += OnPersistentKVUpdated;
      _networking.ConnectionEvent += OnConnectionStateChanged;
    }
  }
}

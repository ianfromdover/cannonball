// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;

using Niantic.ARDK.Networking;// TODO: remove later
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs; // TODO: remove later
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.BinarySerialization;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// Describes the networking statistics of the current session
  /// @note this is currently not implemented
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct NetworkingStats
  {
    // TODO: define this struct
    public int Rtt { get; set; }
    public float PacketLoss { get; set; }
  }

  /// Describes the result of a network request
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum NetworkingRequestResult : 
    byte
  {
    // TODO: Add more stats
    Success = 0,
    Error = 1
  };

  /// The current connection state of the device to Lightship servers
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ConnectionState : 
    byte
  {
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
  };

  /// Connection events that are fired from INetworking.ConnectionEvent
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ConnectionEvents : 
    byte
  {
    Connected = 0,
    Disconnected = 1,
    ConnectionError = 2 // TODO: more detailed errors
  };

  /// Protocol used for networked messages
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ConnectionType : 
    byte
  {
    UseDefault = 0,
    Reliable = 1,
    Unreliable = 2
  };

  /// Backend to connect to
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum NetworkingBackend : 
    byte
  {
    // Currently hits ARBEs through a shim layer
    NetworkingV0, 
    
    // Mock implementation that returns mock values
    Mock
  };

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct ConnectionStateArgs : 
    IArdkEventArgs
  {
    public ConnectionState connectionState { get; private set; }
    public ConnectionStateArgs(ConnectionState state)
    {
      connectionState = state;
    }
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct ConnectionEventArgs : 
    IArdkEventArgs
  {
    public ConnectionEvents connectionEvent { get; private set; }
    public ConnectionEventArgs(ConnectionEvents connEvent)
    { connectionEvent = connEvent;}
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct PeerIDArgs :
    IArdkEventArgs
  {
    public IPeerID PeerID { get; private set; }
    public PeerIDArgs(IPeerID peerid)
    {
      PeerID = peerid;
    }
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct DataReceivedArgs : 
    IArdkEventArgs
  {
    public IPeer Peer { get; private set; } // TODO: To be removed
    public IPeerID PeerID { get; private set; }
    public uint Tag { get; private set; }
    //public TransportType TransportType { get; private set; } // TODO: needed?
    public int DataLength { get { return _dataArgs.DataLength; } }
    private PeerDataReceivedArgs _dataArgs;

    public DataReceivedArgs(PeerDataReceivedArgs dataArgs)
    {
      Peer = dataArgs.Peer;
      PeerID = new PeerIDv0(Peer);
      Tag = dataArgs.Tag;
      _dataArgs = dataArgs;
    }

    public MemoryStream CreateDataReader() { return _dataArgs.CreateDataReader(); }
    public byte[] CopyData() { return _dataArgs.CopyData(); }
    public void GetData<T>(ref T data)
    {
      var ser = new BinaryDeserializer(_dataArgs.CreateDataReader());
      data = (T)ser.Deserialize();
    }
  }

  // The low level networking interface
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface INetworking :
    IDisposable
  {

    /// Set the default protocol for networked messages on this session 
    /// <param name="connectionType">Protocol to send the data with</param>
    /// @note This is currently unimplemented
    void SetDefaultConnectionType(ConnectionType connectionType);

    /// Send data to the specified peers. Receiving peers will have a DataReceived event fired
    /// <param name="dest">Destination of the message. destination could be peer ID, “server” peer ID, list of peer IDs, or empty for broadcast </param>
    /// <param name="tag">Data tag that peers will receive</param>
    /// <param name="data">Byte[] to send</param>
    /// <param name="connectionType">Protocol to send the data with</param>
    void SendData(List<IPeerID> dest, uint tag, byte[] data, ConnectionType connectionType);
    
    /// Send an object to the specified peers. Receiving peers will have a DataReceived event fired.
    /// <param name="dest">Destination of the message. destination could be peer ID, “server” peer ID, list of peer IDs, or empty for broadcast </param>
    /// <param name="tag">Data tag that peers will receive</param>
    /// <param name="data">Object to send</param>
    /// <param name="connectionType">Protocol to send the data with</param>
    /// @note This is currently unimplemented
    void SendData<T>(List<IPeerID> dest, uint tag, T data, ConnectionType connectionType=ConnectionType.UseDefault);

    /// Get the latest connection state
    /// <returns></returns>
    ConnectionState ConnectionState { get; }

    /// Returns if self is a “server”
    /// <returns>true if this networking is server role, false if this networking is client role</returns>
    /// @note This is currently unimplemented
    bool IsServer { get; }

    /// Return the self Peer ID
    /// <returns>self Peer ID</returns>
    IPeerID SelfPeerID { get; }

    /// Return the server peer ID
    /// <returns>server Peer ID. Invalid peer ID if no server role in the current room</returns>
    /// @note This is currently unimplemented
    IPeerID ServerPeerId { get; }

    /// Get all PeerIDs actively connected to the room
    /// <returns>List of all Peer IDs actively connected to the room</returns>
    List<IPeerID> PeerIDs { get; }

    /// Disconnect the specified peer
    /// Can do only by server.
    /// TODO: Should be async?
    /// <param name="peerID">PeerID of the peer to be kicked out</param>
    /// <returns>result of the request</returns>
    /// @note This is currently unimplemented
    NetworkingRequestResult KickOutPeer(IPeerID peerID);

    /// Get networking stats
    /// RTT, bps, packet loss, etc…
    /// <returns>current network stats struct</returns>
    /// @note This is currently unimplemented
    NetworkingStats NetworkingStats { get; }

    /// Join the networking as a server
    /// @note This is currently unimplemented - joining happens on INetworking creation
    void JoinAsServer(byte[] roomId);

    /// Join the networking as a peer
    /// @note This is currently unimplemented - joining happens on INetworking creation
    void JoinAsPeer(byte[] roomId);

    /// Disconnect from network and datastore
    void Leave();

    /// Get room config of the currrently connected room
    /// RoomConfig include name, ID, geo info, etc.
    /// <returns>the room current of the currrently connected room</returns>
    /// @note This is currently unimplemented - joining happens on INetworking creation
    RoomParams RoomParams { get; }

    // connected, failed, disconnected, Deinitialized
    event ArdkEventHandler<ConnectionEventArgs> ConnectionEvent;

    event ArdkEventHandler<PeerIDArgs> PeerAdded;

    /// Event fired when a peer is removed, either from intentional action, timeout, or error.
    event ArdkEventHandler<PeerIDArgs> PeerRemoved;

    event ArdkEventHandler<DataReceivedArgs> DataReceived;

  }
} // namespace Niantic.ARDK.SharedAR

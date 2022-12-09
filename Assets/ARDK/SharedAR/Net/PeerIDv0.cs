// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

using Niantic.ARDK.Networking;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class PeerIDv0 : 
    IPeerID
  {
    private static readonly Dictionary<Guid, PeerIDv0> _peers;

    static PeerIDv0()
    {
      _peers = new Dictionary<Guid, PeerIDv0>();
    }

    public static IPeerID GetPeerID(Guid peerId)
    {
      return _peers.ContainsKey(peerId) ? _peers[peerId] : null;
    }

    private readonly IPeer _ipeer;
    
    public Guid Identifier
    {
      get => _ipeer.Identifier;
    }

    internal PeerIDv0(IPeer ipeer)
    {
      _ipeer = ipeer;
      if (!_peers.ContainsKey(ipeer.Identifier))
      {
        _peers.Add(ipeer.Identifier, this);
      }
    }

    public override int GetHashCode()
    {
      return _ipeer.GetHashCode();
    }

    public bool Equals(IPeerID info)
    {
      return info != null && Identifier.Equals(info.Identifier);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as IPeerID);
    }
  };
}

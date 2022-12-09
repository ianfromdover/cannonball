// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.IO;
using System.Text;

using Niantic.ARDK.Utilities.BinarySerialization;
using Niantic.ARDK.Utilities.BinarySerialization.ItemSerializers;

using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR {
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public static class PeerSerialization {
    public static IPeerID PeerFromKey(string key)
    {
      // PeerID string is in second half of key
      var peerId = key.Split('_')[1];
      return PeerIDv0.GetPeerID(new Guid(peerId));
    }

    public static string KeyFromPeer(string prefix, IPeerID peer)
    {
      var builder = new StringBuilder();
      builder.Append(prefix);
      builder.Append(peer.ToString());

      return builder.ToString();
    }

    public static byte[] BytesFromColocalizationState(ColocalizationState state)
    {
      using (var stream = new MemoryStream())
      {
        GlobalSerializer.Serialize(stream, state);
        return stream.ToArray();
      }
    }

    public static ColocalizationState ColocalizationStateFromBytes(MemoryStream stream)
    {
      return (ColocalizationState)GlobalSerializer.Deserialize(stream);
    }

    public static Matrix4x4 PoseFromBytes(MemoryStream stream)
    {
      using (var deserializer = new BinaryDeserializer(stream))
      {
        return Matrix4x4Serializer.Instance.Deserialize(deserializer);
      }
    }

    public static byte[] BytesFromPose(Matrix4x4 pose)
    {
      using (var stream = new MemoryStream())
      {
        using (var serializer = new BinarySerializer(stream))
        {
          Matrix4x4Serializer.Instance.Serialize(serializer, pose);
          return stream.ToArray();
        }
      }
    }
  }
}
// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;

using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities.BinarySerialization;
using Niantic.ARDK.Utilities.BinarySerialization.ItemSerializers;

namespace Niantic.LightshipHub.Templates
{
  public class MessagingManager
  {
    private IMultipeerNetworking _networking;
    private SharedSession _controller;
    private readonly MemoryStream _builderMemoryStream = new MemoryStream(24);
    private enum _MessageType : uint
    {
      AskMoveObjectMessage,
      AskAnimateObjectTapMessage,
      AskAnimateObjectDistanceMessage,
      ObjectPositionMessage,
      ObjectScaleMessage,
      ObjectRotationMessage
    }

    internal void InitializeMessagingManager(IMultipeerNetworking networking, SharedSession controller)
    {
      _networking = networking;
      _controller = controller;
      _networking.PeerDataReceived += OnDidReceiveDataFromPeer;
    }

    internal void AskHostToMoveObject(IPeer host, Vector3 position)
    {
      _networking.SendDataToPeer(
          (uint)_MessageType.AskMoveObjectMessage,
          SerializeVector3(position),
          host,
          TransportType.ReliableUnordered
      );
    }

    internal void AskHostToAnimateObjectTap(IPeer host)
    {
      _networking.SendDataToPeer(
          (uint)_MessageType.AskAnimateObjectTapMessage,
          new byte[1], // Empty byte
          host,
          TransportType.ReliableUnordered
      );
    }

    internal void AskHostToAnimateObjectDistance(IPeer host)
    {
      _networking.SendDataToPeer(
          (uint)_MessageType.AskAnimateObjectDistanceMessage,
          new byte[1], // Empty byte
          host,
          TransportType.ReliableUnordered
      );
    }

    internal void BroadcastObjectPosition(Vector3 position)
    {
      _networking.BroadcastData(
          (uint)_MessageType.ObjectPositionMessage,
          SerializeVector3(position),
          TransportType.UnreliableUnordered
      );
    }

    internal void BroadcastObjectScale(Vector3 scale)
    {
      _networking.BroadcastData(
          (uint)_MessageType.ObjectScaleMessage,
          SerializeVector3(scale),
          TransportType.UnreliableUnordered
      );
    }

    internal void BroadcastObjectRotation(Quaternion rotation)
    {
      _networking.BroadcastData(
          (uint)_MessageType.ObjectRotationMessage,
          SerializeQuaternion(rotation),
          TransportType.UnreliableUnordered
      );
    }

    private void OnDidReceiveDataFromPeer(PeerDataReceivedArgs args)
    {
      var data = args.CopyData();
      switch ((_MessageType)args.Tag)
      {
        case _MessageType.AskMoveObjectMessage:
          _controller.SharedObjectHolder.MoveObject(DeserializeVector3(data));
          break;

        case _MessageType.AskAnimateObjectTapMessage:
          _controller.SharedObjectHolder.ObjectInteraction.AnimateObjectTap();
          break;

        case _MessageType.AskAnimateObjectDistanceMessage:
          _controller.SharedObjectHolder.ObjectInteraction.AnimateObjectDistance();
          break;

        case _MessageType.ObjectPositionMessage:
          _controller.SetObjectPosition(DeserializeVector3(data));
          break;

        case _MessageType.ObjectScaleMessage:
          _controller.SetObjectScale(DeserializeVector3(data));
          break;

        case _MessageType.ObjectRotationMessage:
          _controller.SetObjectRotation(DeserializeQuaternion(data));
          break;

        default:
          throw new ArgumentException("Received unknown tag from message");
      }
    }

    internal void Destroy()
    {
      _networking.PeerDataReceived -= OnDidReceiveDataFromPeer;
    }

    private byte[] SerializeVector3(Vector3 vector)
    {
      _builderMemoryStream.Position = 0;
      _builderMemoryStream.SetLength(0);

      using (var binarySerializer = new BinarySerializer(_builderMemoryStream))
        Vector3Serializer.Instance.Serialize(binarySerializer, vector);

      return _builderMemoryStream.ToArray();
    }

    private Vector3 DeserializeVector3(byte[] data)
    {
      using (var readingStream = new MemoryStream(data))
      using (var binaryDeserializer = new BinaryDeserializer(readingStream))
        return Vector3Serializer.Instance.Deserialize(binaryDeserializer);
    }

    private byte[] SerializeQuaternion(Quaternion quat)
    {
      _builderMemoryStream.Position = 0;
      _builderMemoryStream.SetLength(0);

      using (var binarySerializer = new BinarySerializer(_builderMemoryStream))
        QuaternionSerializer.Instance.Serialize(binarySerializer, quat);

      return _builderMemoryStream.ToArray();
    }

    private Quaternion DeserializeQuaternion(byte[] data)
    {
      using (var readingStream = new MemoryStream(data))
      using (var binaryDeserializer = new BinaryDeserializer(readingStream))
        return QuaternionSerializer.Instance.Deserialize(binaryDeserializer);
    }
  }
}

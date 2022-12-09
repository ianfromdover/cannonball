// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Text;

using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal class _MockWayspotAnchor:
    _ThreadCheckedObject,
    IWayspotAnchor,
    _IInternalTrackable
  {
    public ArdkEventHandler<WayspotAnchorResolvedArgs> TrackingStateUpdated { get; set; }

    public event ArdkEventHandler<WayspotAnchorResolvedArgs> TransformUpdated
    {
      add
      {
        _CheckThread();

        _transformUpdated += value;

        if (Status == WayspotAnchorStatusCode.Success)
          value.Invoke(new WayspotAnchorResolvedArgs(ID, LastKnownPosition, LastKnownRotation));
      }
      remove
      {
        _transformUpdated -= value;
      }
    }

    public event ArdkEventHandler<WayspotAnchorStatusUpdate> StatusCodeUpdated
    {
      add
      {
        _CheckThread();

        _statusCodeUpdated += value;
        value.Invoke(new WayspotAnchorStatusUpdate(ID, Status));
      }
      remove
      {
        _statusCodeUpdated -= value;
      }
    }

    public Guid ID { get; }

    /// Whether or not the mock anchor is currently being tracked
    public bool Tracking { get; private set; }

    public WayspotAnchorStatusCode Status { get; private set; } = WayspotAnchorStatusCode.Pending;

    public Vector3 LastKnownPosition { get; private set; }

    public Quaternion LastKnownRotation { get; private set; }

    // Sets whether or not the mock anchor should be tracked
    // Part of _IInternalTrackable interface
    public void SetTrackingEnabled (bool tracking)
    {
      Tracking = tracking;
    }

    // Part of _IInternalTrackable interface
    public void SetTransform(Vector3 position, Quaternion rotation)
    {
      LastKnownPosition = position;
      LastKnownRotation = rotation;
      _transformUpdated?.Invoke(new WayspotAnchorResolvedArgs(ID, LastKnownPosition, LastKnownRotation));
    }

    // Part of _IInternalTrackable interface
    public void SetStatusCode(WayspotAnchorStatusCode statusCode)
    {
      if (Status != statusCode)
      {
        Status = statusCode;
        _statusCodeUpdated?.Invoke(new WayspotAnchorStatusUpdate(ID, Status));
      }
    }

    public _MockWayspotAnchor(Guid id, Matrix4x4 localPose)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));

      ID = id;
      LastKnownPosition = localPose.ToPosition();
      LastKnownRotation = localPose.ToRotation();
    }

    public _MockWayspotAnchor(Guid id)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));

      ID = id;
    }

    public _MockWayspotAnchor(byte[] data)
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(_WayspotAnchorFactory));

      var json = Encoding.UTF8.GetString(data);
      var mockWayspotAnchorData = JsonUtility.FromJson<_MockWayspotAnchorData>(json);

      var success = Guid.TryParse(mockWayspotAnchorData._ID, out Guid identifier);
      if (success)
        ID = identifier;
      else
        throw new ArgumentException("Failed to create wayspot anchor from payload", nameof(data));

      LastKnownPosition =
        new Vector3
        (
          mockWayspotAnchorData._XPosition,
          mockWayspotAnchorData._YPosition,
          mockWayspotAnchorData._ZPosition
        );

      var rotationEuler =
        new Vector3
        (
          mockWayspotAnchorData._XRotation,
          mockWayspotAnchorData._YRotation,
          mockWayspotAnchorData._ZRotation
        );

      LastKnownRotation = Quaternion.Euler(rotationEuler);
    }

    /// Gets the payload of the mock anchor
    /// @note This is a wrapper around the blob of data
    public WayspotAnchorPayload Payload
    {
      get
      {
        string id = ID.ToString();
        var rotation = LastKnownRotation.eulerAngles;
        var mockWayspotAnchorData = new _MockWayspotAnchorData()
        {
          _ID = id,
          _XPosition = LastKnownPosition.x,
          _YPosition = LastKnownPosition.y,
          _ZPosition = LastKnownPosition.z,
          _XRotation = rotation.x,
          _YRotation = rotation.y,
          _ZRotation = rotation.z
        };

        string json = JsonUtility.ToJson(mockWayspotAnchorData);
        byte[] blob = Encoding.UTF8.GetBytes(json);
        var payload = new WayspotAnchorPayload(blob);

        return payload;
      }
    }

    /// The data class used to serialize/deserialize the payload
    [Serializable]
    public class _MockWayspotAnchorData
    {
      public string _ID;
      public float _XPosition;
      public float _YPosition;
      public float _ZPosition;
      public float _XRotation;
      public float _YRotation;
      public float _ZRotation;
    }

    /// Disposes the mock wayspot anchor
    public void Dispose()
    {
    }

    private event ArdkEventHandler<WayspotAnchorResolvedArgs> _transformUpdated = args => {};
    private event ArdkEventHandler<WayspotAnchorStatusUpdate> _statusCodeUpdated = args => {};
  }
}
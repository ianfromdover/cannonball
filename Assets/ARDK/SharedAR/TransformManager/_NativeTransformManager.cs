// Copyright 2022 Niantic, Inc. All Rights Reserved.
#pragma warning disable 0067
using System.Collections.ObjectModel;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.Experimental.ARDK.SharedAR.Transform;
using Niantic.ARDK.Utilities;
using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class _NativeTransformManager :
    ITransformManager
  {
    public event ArdkEventHandler<PeerPoseReceivedArgs> PeerPoseReceived;
    public event ArdkEventHandler<TransformUpdatedArgs> TransformUpdated;
    public event ArdkEventHandler<TransformUpdatedArgs> TransformAdded;
    public event ArdkEventHandler<TransformUpdatedArgs> TransformDeleted;

    public void DisablePoseBroadcasting()
    {
      throw new System.NotImplementedException();
    }

    public void EnabledPoseBroadcasting()
    {
      throw new System.NotImplementedException();
    }

    public void TogglePoseBroadcasting()
    {
      throw new System.NotImplementedException();
    }

    public void SetPoseBroadcasting(bool value)
    {
      throw new System.NotImplementedException();
    }

    public void Update()
    {
      throw new System.NotImplementedException();
    }

    public bool AddTransform(string id, Matrix4x4 transform)
    {
      throw new System.NotImplementedException();
    }

    public bool UpdateTransform(string id, Matrix4x4 transform)
    {
      throw new System.NotImplementedException();
    }

    public Matrix4x4 GetTransform(string id)
    {
      throw new System.NotImplementedException();
    }

    public ReadOnlyCollection<string> GetTransformIds()
    {
      throw new System.NotImplementedException();
    }

    public bool DeleteTransform(string id)
    {
      throw new System.NotImplementedException();
    }

    public void Dispose()
    {
    }
  }
}

#pragma warning restore 0067
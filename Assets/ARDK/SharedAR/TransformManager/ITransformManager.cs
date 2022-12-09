// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.Utilities;

using Matrix4x4 = UnityEngine.Matrix4x4;
namespace Niantic.Experimental.ARDK.SharedAR.Transform
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct TransformUpdatedArgs :
    IArdkEventArgs 
  {
    public string key;
    public Matrix4x4 pose;
    public TransformUpdatedArgs(string _key, Matrix4x4 _pose): this() {
      key = _key;
      pose = _pose;
    }
  }
  
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct TransformRemovedArgs : 
    IArdkEventArgs
  {
    public TransformRemovedArgs(Matrix4x4 _pose): 
      this() 
    {
    }
  }
  
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface ITransformManager :
    IDisposable
  {
    public event ArdkEventHandler<PeerPoseReceivedArgs> PeerPoseReceived;

    public void DisablePoseBroadcasting();

    public void EnabledPoseBroadcasting();

    public void TogglePoseBroadcasting();

    public void SetPoseBroadcasting(bool value);

    public void Update();

    /// <summary>
    /// Set the transform of a specific ID. In this implementation, there is no owner gating.
    /// Other devices will get an event TransformUpdated upon receiving the update
    /// </summary>
    /// <param name="id">ID of the transform you are setting</param>
    /// <param name="transform">Transform in local space</param>
    /// <returns>True if set (always)</returns>
    bool AddTransform(string id, Matrix4x4 transform);
    
    /// <summary>
    /// Update a specified ID and its transform for all devices. In this implementation, there is no owner gating.
    /// Other devices will get an event TransformUpdated upon receiving the update
    /// </summary>
    /// <param name="id">ID of the transform you are setting</param>
    /// <param name="transform">Transform in local space</param>
    /// <returns></returns>
    bool UpdateTransform(string id, Matrix4x4 transform);

    /// <summary>
    /// Delete a specified ID and its transform for all devices. In this implementation, there is no owner gating.
    /// Other devices will get an event TransformDeleted upon receiving the update
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool DeleteTransform(string id);

    /// <summary>
    /// Get the last transform of a specific ID. This will always be in local space
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Matrix4x4 GetTransform(string id);

    /// <summary>
    /// Get a list of all known transform IDs
    /// </summary>
    /// <returns></returns>
    ReadOnlyCollection<string> GetTransformIds();
    
    /// <summary>
    /// Fired upon receiving an updated transform from another peer
    /// This is always in local space
    /// </summary>
    event ArdkEventHandler<TransformUpdatedArgs> TransformUpdated;
    /// <summary>
    /// Fired upon receiving an updated transform from another peer
    /// This is always in local space
    /// </summary>
    event ArdkEventHandler<TransformUpdatedArgs> TransformAdded;

    /// <summary>
    /// Fired upon another peer deleting a transform
    /// </summary>
    event ArdkEventHandler<TransformUpdatedArgs> TransformDeleted;
  }
}
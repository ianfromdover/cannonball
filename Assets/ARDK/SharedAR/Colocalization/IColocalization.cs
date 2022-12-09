// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.ObjectModel;

using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.Utilities;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace Niantic.Experimental.ARDK.SharedAR {
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ColocalizationState
  {
    Unknown = 0,
    Initialized,
    Colocalizing,
    Colocalized,
    LimitedTracking,
    Failed
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ColocalizationFailureReason
  {
    Unknown = 0,
    NetworkingError,
    VPSLocationFailed,
    VPSTimeout,
    VPSSpaceFailure
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum ColocalizationAlignmentResult : byte
  {
    /// <summary>
    /// Returned if local user isn't colocalized or hasn't resolved the Pose Anchor for the
    /// requested peer.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Returned if local user is colocalized and has resolved the Pose Anchor for the
    /// requested peer.
    /// </summary>
    Success
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface IColocalization :
    IDisposable
  {
    // Start Colocalization
    void Start();

    // Stop colocalization 
    void Stop();

    Matrix4x4 AlignedSpaceOrigin { get; }
    
    /// <summary>
    /// Fired upon any peers' (including self) localization state updating
    /// </summary>
    event ArdkEventHandler<ColocalizationStateUpdatedArgs> ColocalizationStateUpdated;

    void LocalPoseToAligned(Matrix4x4 poseInLocalSpace, out Matrix4x4 poseInAlignedSpace);
    ColocalizationAlignmentResult AlignedPoseToLocal(IPeerID id, Matrix4x4 poseInAlignedSpace, out Matrix4x4 poseInLocalSpace);

    // NOT IMPLEMENTED
    ReadOnlyDictionary<IPeerID, ColocalizationState> ColocalizationStates { get; }
  }
}

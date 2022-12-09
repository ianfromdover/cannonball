// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum RoomRequestResult
  {
    Unknown = 0,
    Success,
    Error,
  };

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface IRoom : 
    IDisposable
  {
    // Identifiers
    RoomParams RoomParams { get; }
    string ExperienceId { get; }
    
    byte[] ExperienceInitData { get; }

    // Shared AR Client components
    INetworking Networking { get; }
    IDatastore Datastore { get; }
    IColocalization Colocalization { get; }

    void Leave();
  }
} // namespace Niantic.ARDK.SharedAR
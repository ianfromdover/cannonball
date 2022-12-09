// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System.Collections.Generic;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class Room :
    IRoom
  {
    public RoomParams RoomParams { get; }
    public string ExperienceId { get; }
    public byte[] ExperienceInitData { get; }
    public INetworking Networking { get; }
    public IDatastore Datastore { get; }
    public IColocalization Colocalization { get; }
    
    public void Leave()
    {
      throw new System.NotImplementedException();
    }
    
    public void Dispose()
    {
    }
  }
}
// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.Generic;
using Niantic.ARDK.LocationService;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface IExperienceService
  {
    // Experience authoring
    public Experience CreateExperience(LatLng location, string experienceId);
    void StoreExperienceInitData(string experienceId, byte[] experienceInitData);

    // Experience finding
    public List<Experience> FindExperience(LatLng location);
    byte[] GetExperienceInitData(string experienceId);
    
    // Room operations
    IRoom CreateRoom(Experience template, RoomParams roomParams);
    IRoom CreateRoom(string experienceId, RoomParams roomParams);
    List<IRoom> FindRooms(Experience template);
    List<IRoom> FindRooms(string experienceId);
    IRoom FindRoom(string roomId);
    byte[] JoinRoom(IRoom room);
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.Generic;
using Niantic.ARDK.LocationService;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class MockExperienceService : 
    IExperienceService
  {
    // Writes a list of strings to player prefs. Each of the strings is a player prefs key to get an Experience
    private const string ExperienceListPlayerPrefKey = "MockExperienceServiceListKey";
    
    public Experience CreateExperience(LatLng location, string experienceId)
    {
      throw new System.NotImplementedException();
    }

    public void StoreExperienceInitData(string experienceId, byte[] experienceInitData)
    {
      throw new System.NotImplementedException();
    }

    public List<Experience> FindExperience(LatLng location)
    {
      throw new System.NotImplementedException();
    }

    public byte[] GetExperienceInitData(string experienceId)
    {
      throw new System.NotImplementedException();
    }

    public IRoom CreateRoom(Experience template, RoomParams roomParams)
    {
      throw new System.NotImplementedException();
    }

    public IRoom CreateRoom(string experienceId, RoomParams roomParams)
    {
      throw new System.NotImplementedException();
    }

    public List<IRoom> FindRooms(Experience template)
    {
      throw new System.NotImplementedException();
    }

    public List<IRoom> FindRooms(string experienceId)
    {
      throw new System.NotImplementedException();
    }

    public IRoom FindRoom(string roomId)
    {
      throw new System.NotImplementedException();
    }

    public byte[] JoinRoom(IRoom room)
    {
      throw new System.NotImplementedException();
    }
  }
}

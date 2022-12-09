// Copyright 2022 Niantic, Inc. All Rights Reserved.
namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum RoomVisibility : byte
  {
    Unknown = 0,
    // Publicly visible and can be found through the ExperienceService
    Public,
    // Private room that can only be joined through RoomId
    Private
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum RoomColocalizationMethod : byte
  {
    Unknown = 0,
    Vps = 1,
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class RoomParams
  {
    public string RoomID {get; private set;}
    public RoomVisibility Visibility { get; private set; }
    public int MinPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public RoomColocalizationMethod ColocalizationMethod { get; private set; }

    public RoomParams
    (
      string id,
      RoomVisibility visibility = RoomVisibility.Public,
      RoomColocalizationMethod method = RoomColocalizationMethod.Vps
    )
    {
      RoomID = id;
      Visibility = visibility;
      ColocalizationMethod = method;
    }
    
    public RoomParams
    (
      string id,
      int minPlayers,
      int maxPlayers,
      RoomVisibility visibility = RoomVisibility.Public,
      RoomColocalizationMethod method = RoomColocalizationMethod.Vps
    )
    {
      RoomID = id;
      MinPlayers = minPlayers;
      MaxPlayers = maxPlayers;
      Visibility = visibility;
      ColocalizationMethod = method;
    }
  }
} // namespace Niantic.ARDK.SharedAR
// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.LocationService;

namespace Niantic.Experimental.ARDK.SharedAR
{
  // Defines the parameters of an Experience
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct Experience
  {
    // Application that authored this Experience, maybe this should be hidden
    public string AppId;
    
    // Id used to store or retrieve Experience Init Data to seed persistent experiences
    public string ExperienceId;
    
    // Real world location that the Experience will be played at
    public LatLng Location;
    
    // Type of Experience to play. Used to filter found Experiences in conjunction with proximity to
    //  show/choose Experiences
    public string Kind;
  }
}

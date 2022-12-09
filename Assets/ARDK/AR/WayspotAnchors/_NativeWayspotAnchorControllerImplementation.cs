// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.AR.WayspotAnchors
{
  internal sealed class _NativeWayspotAnchorControllerImplementation:
    _NativeWayspotAnchorControllerImplementationBase
  {
    private readonly IARSession _arSession;

    /// Creates a new native wayspot anchor controller
    /// @param session The session used to create the native wayspot anchor controller
    internal _NativeWayspotAnchorControllerImplementation(IARSession session)
    {
      _NativeAccess.AssertNativeAccessValid();

      _arSession = session;
      _nativeHandle = _NAR_ManagedPoseController_Init(session.StageIdentifier.ToByteArray());
       
      if (_nativeHandle == IntPtr.Zero)
        throw new ArgumentException("nativeHandle can't be Zero.", nameof(_nativeHandle));
    }
    
       /// Starts resolving wayspot anchors.  Resolving anchors will have their position and rotation updates reported via the _wayspotAnchorsResolved
    /// event
    /// @param wayspotAnchors The wayspot anchors to update
    public override void StartResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
        _CheckThread();

        if (!_ValidateARSessionIsAlive())
        {
            throw new Exception("AR Session validation has failed.");
        }
        
        base.StartResolvingWayspotAnchors(wayspotAnchors);
    }

    /// Stops resolving the wayspot anchors
    /// @param wayspotAnchors The wayspot anchors to stop resolving
    public override void StopResolvingWayspotAnchors(params IWayspotAnchor[] wayspotAnchors)
    {
        _CheckThread();

        if (!_ValidateARSessionIsAlive())
            return;
        
        base.StopResolvingWayspotAnchors(wayspotAnchors);
    }
    
    public override void StartVPS(IWayspotAnchorsConfiguration wayspotAnchorsConfiguration)
    {
      _CheckThread();

      if (!_ValidateARSessionIsAlive())
      {
        ARLog._Error("The ARSession is deinitialized, cannot start localization");
        return;
      }

      if (_arSession is _NativeARSession nativeSession)
      {
        if (!nativeSession._IsLocationServiceInitialized)
        {
          ARLog._Error
          (
            "SetupLocationService(locationService) must be called before attempting to localize" +
            " against any available world coordinate space."
          );

          return;
        }
      }
      
      base.StartVPS(wayspotAnchorsConfiguration);
    }
    
    public override void StopVPS()
    {
      _CheckThread();

      if (!_ValidateARSessionIsAlive())
        return;
      
      base.StopVPS();
    }
    
    private bool _ValidateARSessionIsAlive()
    {
      if (_arSession is _NativeARSession nativeSession)
        return !nativeSession.IsDestroyed;

      ARLog._Error
      (
        $"Must use a {nameof(_NativeWayspotAnchorsConfiguration)} with ${nameof(_NativeWayspotAnchorControllerImplementation)}"
      );

      return false;
    }
  }
}

// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public static class ColocalizationFactory
  {
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public class ColocalizationCreatedArgs : 
      IArdkEventArgs
    {
      public IColocalization Colocalization { get; private set; }

      public ColocalizationCreatedArgs(IColocalization colocalization)
      {
        Colocalization = colocalization;
      }
    }

    // TODO : eventually remove this when a sharedArManager is implemented : AR-12779
    public static event ArdkEventHandler<ColocalizationCreatedArgs> ColocalizationCreated;

    public static IColocalization Create(INetworking networking, IARSession arSession)
    {
      var colocalization = new _NativeVPSColocalization(networking, arSession);

      var handler = ColocalizationCreated;
      if (handler != null)
      {
        var args = new ColocalizationCreatedArgs(colocalization);
        handler(args);
      }

      return colocalization;
    }
  }
}

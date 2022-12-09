// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.Utilities;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public static class NetworkingFactory
  {
    public class NetworkingCreatedArgs: IArdkEventArgs
    {
      public INetworking Networking { get; private set; }

      public NetworkingCreatedArgs(INetworking networking)
      {
        Networking = networking;
      }
    }

    public static event ArdkEventHandler<NetworkingCreatedArgs> NetworkingCreated;

    public static INetworking Create()
    {
      var networking = new _NativeNetworking();

      var handler = NetworkingCreated;
      if (handler != null)
      {
        var args = new NetworkingCreatedArgs(networking);
        handler(args);
      }

      return networking;
    }

    public static INetworking Create
    (
      string connectionId,
      NetworkingBackend backend = NetworkingBackend.NetworkingV0
    )
    {
      var networking = new _NativeNetworking(connectionId, backend);

      var handler = NetworkingCreated;
      if (handler != null)
      {
        var args = new NetworkingCreatedArgs(networking);
        handler(args);
      }

      return networking;
    }
  }
}

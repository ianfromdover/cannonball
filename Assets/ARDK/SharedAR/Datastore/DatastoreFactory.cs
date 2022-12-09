// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.Utilities;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public static class DatastoreFactory
  {
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    public class DatastoreCreatedArgs : 
      IArdkEventArgs
    {
      public IDatastore Datastore { get; private set; }

      public DatastoreCreatedArgs(IDatastore datastore)
      {
        Datastore = datastore;
      }
    }

    public static event ArdkEventHandler<DatastoreCreatedArgs> DatastoreCreated;

    public static IDatastore Create(INetworking networking, DatastoreBackend backend = DatastoreBackend.DatastoreV0)
    {
      var datastore = new _NativeDatastore(networking, backend);

      var handler = DatastoreCreated;
      if (handler != null)
      {
        var args = new DatastoreCreatedArgs(datastore);
        handler(args);
      }

      return datastore;
    }
  }
}

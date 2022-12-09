// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.ARDK.Utilities;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// Result of the Datastore operation
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum Result
  {
    Success,
    Fail, // too generic
    NotAuthorized

    // TODO: add more detailed status/error as needed
  };

  /// Lifecycle options for a particular Key/Value pair.
  /// Used to define the behaviour of each entry relative to the peer that set it
  /// @note: Currently, all datastore Key/Values behave as Session-persisted - they will remain
  ///   until explicitly removed or the session times out.
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum LifeCycleOptions
  {
    DuringActive,
    Persisted,
    UntilDisconnected
  };

  /// Contains the key for Key/Value updated events
  /// Use IDatastore.GetData to query the new data associated with this key
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public struct KeyValuePairArgs : 
    IArdkEventArgs
  {
    public string Key { get; set; }
    public KeyValuePairArgs(string key)
    {
      Key = key;
    }
  }
  
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public enum DatastoreBackend : 
    byte
  {
    DatastoreV0, 
    Mock
  };

  /// Server-backed data storage that is associated with sessions or rooms.
  /// Peers can set, update, and delete Key/Value pairs, and have the server notify
  ///   all other peers in the session when updates occur.
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public interface IDatastore :
    IDisposable
  {
    // CRUD operations

    /// Set data into storage
    /// <param name="key">Key of the data</param>
    /// <param name="value">Value to set</param>
    /// <returns>Result of the operation</returns>
    // TODO: should be async? should have other value type?
    Result SetData(string key, byte[] value);
    Result SetData<T>(string key, T value);

    /// Set data into storage
    /// <param name="key">Key of the data</param>
    /// <param name="value">Value to set</param>
    /// <returns>Result of the operation</returns>
    // TODO: should be async? should have other value type?
    Result GetData(string key, ref byte[] value);
    Result GetData<T>(string key, ref T value);

    /// Delete the key-value pair from the storage
    /// <param name="key">Key of the data</param>
    /// <returns>Result of the operation</returns>
    // TODO: should be async?
    Result DeleteData(string key);

    /// Get list of keys under specified tag
    /// <returns>List of keys</returns>
    // TODO: should be async?
    List<string> GetKeys();

    // Configurable options

    /// Claim ownership of data specified by the key
    /// <returns>Result of the operation</returns>
    Result ClaimOwnership(string key);

    /// Set lifecycle of the data specified by the key
    /// <returns>Result of the operation</returns>
    Result SetLifeCycle(string key, LifeCycleOptions options);

    // Events

    /// <summary>
    /// A key was added to the datastore by a peer in the session. Use GetData to get its value
    /// </summary>
    event ArdkEventHandler<KeyValuePairArgs> KeyValueAdded;

    /// <summary>
    /// A key was updated by a peer in the session. Use GetData to get its value
    /// </summary>
    event ArdkEventHandler<KeyValuePairArgs> KeyValueUpdated;

    /// <summary>
    /// A key was deleted by a peer in the session.
    /// </summary>
    event ArdkEventHandler<KeyValuePairArgs> KeyValueDeleted;
  }
} // namespace Niantic.ARDK.SharedAR

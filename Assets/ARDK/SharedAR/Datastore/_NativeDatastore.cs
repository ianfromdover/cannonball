// Copyright 2022 Niantic, Inc. All Rights Reserved.

#pragma warning disable 0067
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using AOT;

using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class _NativeDatastore :
    IDatastore
  {
    private bool _isDestroyed;

    public _NativeDatastore(INetworking networking, DatastoreBackend implementationType = DatastoreBackend.DatastoreV0, string datastoreId = "")
    {
      if (networking is _NativeNetworking nativeNetworking)
      {
        if (!datastoreId.Equals(nativeNetworking.SessionId))
        {
          ARLog._Warn("Joining a datastore Id that is not the same as the networking Id");
        }

        _nativeHandle = (!string.IsNullOrEmpty(datastoreId)) ? 
          _NARDatastore_Init
          (
          nativeNetworking.GetNativeHandle(),
          datastoreId,
          (byte)implementationType
          ) :
          _NARDatastore_Init
          (
          nativeNetworking.GetNativeHandle(),
          nativeNetworking.SessionId,
          (byte)implementationType
          );
        
        SubscribeToNativeCallbacks();

        ARLog._Debug("Created _NativeDatastore");
        GC.AddMemoryPressure(GCPressure);
      }
      else
      {
        ARLog._Error("_NativeDatastore only supports _NativeNetworking");
      }
    }

    public Result SetData(string key, byte[] value)
    {
      return (Result)_NARDatastore_SetData(_nativeHandle, key, value, (ulong)value.Length);
    }

    public Result SetData<T>(string key, T value)
    {
      throw new System.NotImplementedException();
    }

    public Result GetData(string key, ref byte[] value)
    {
      ulong outDataSize = 0;
      byte res = 0;
      var dataPtr = _NARDatastore_GetData(_nativeHandle, key, out res, out outDataSize, (ulong)value.Length);    
      
      Marshal.Copy(dataPtr, value, 0, (int)outDataSize);
      return (Result)res;
    }

    public Result GetData<T>(string key, ref T value)
    {
      throw new System.NotImplementedException();
    }

    public Result DeleteData(string key)
    {
      return (Result)_NARDatastore_DeleteData(_nativeHandle, key);
    }

    // Start with assuming 5 keys * 30 characters per key
    private readonly StringBuilder cachedStringBuilder = new StringBuilder(5 * assumedLengthPerKey);
    private const int assumedLengthPerKey = 30;

    private readonly char[] nullTerminatedChar = new char[]
    {
      '\0'
    };

    public List<string> GetKeys()
    {
      var keyCount = _NARDatastore_GetKeyCount(_nativeHandle);
      cachedStringBuilder.Clear();

      // Raise the max capacity of the stringbuilder if necessary
      if (cachedStringBuilder.MaxCapacity < keyCount * assumedLengthPerKey)
        cachedStringBuilder.EnsureCapacity((int)keyCount * assumedLengthPerKey);

      uint outKeyCount = 0;
      _NARDatastore_GetKeys
        (_nativeHandle, cachedStringBuilder, outKeyCount, (ulong)cachedStringBuilder.Length);

      var nullTerminatedStringList = cachedStringBuilder.ToString();
      var strings = nullTerminatedStringList.Split(nullTerminatedChar);

      return strings.ToList();
    }

    public Result ClaimOwnership(string key)
    {
      return (Result)_NARDatastore_ClaimOwnership(_nativeHandle, key);
    }

    public Result SetLifeCycle(string key, LifeCycleOptions options)
    {
      return (Result)_NARDatastore_SetLifeCycle(_nativeHandle, key, (byte)options);
    }

    public event ArdkEventHandler<KeyValuePairArgs> KeyValueAdded;
    public event ArdkEventHandler<KeyValuePairArgs> KeyValueUpdated;
    public event ArdkEventHandler<KeyValuePairArgs> KeyValueDeleted;

    public void Dispose()
    {
      if (_nativeHandle != IntPtr.Zero)
      {
        _NARDatastore_Release(_nativeHandle);
        GC.RemoveMemoryPressure(GCPressure);
        _nativeHandle = IntPtr.Zero;
        _isDestroyed = true;
      }
    }

    private bool _didSubscribeToNativeCallbacks = false;

    private void SubscribeToNativeCallbacks()
    {
      if (_didSubscribeToNativeCallbacks)
        return;

      lock (this)
      {
        if (_didSubscribeToNativeCallbacks)
          return;

        _NARDatastore_Set_keyValueAddedCallback
        (
          _applicationHandle,
          _nativeHandle,
          _keyValueAddedNative
        );

        _NARDatastore_Set_keyValueUpdatedCallback
        (
          _applicationHandle,
          _nativeHandle,
          _keyValueUpdatedNative
        );

        _NARDatastore_Set_keyValueDeletedCallback
        (
          _applicationHandle,
          _nativeHandle,
          _keyValueDeletedNative
        );

        _didSubscribeToNativeCallbacks = true;
      }
    }

#region Handles
    // Below here are private fields and methods to handle native code and callbacks

    // The pointer to the C++ object handling functionality at the native level
    private IntPtr _nativeHandle;

    private IntPtr _cachedHandleIntPtr = IntPtr.Zero;
    private SafeGCHandle<_NativeDatastore> _cachedHandle;

    // Approx memory size of native object
    // Magic number for 64KB
    private const long GCPressure = 64L * 1024L;

    // Used to round-trip a pointer through c++,
    // so that we can keep our this pointer even in c# functions
    // marshaled and called by native code
    private IntPtr _applicationHandle
    {
      get
      {
        if (_cachedHandleIntPtr != IntPtr.Zero)
          return _cachedHandleIntPtr;

        lock (this)
        {
          if (_cachedHandleIntPtr != IntPtr.Zero)
            return _cachedHandleIntPtr;

          // https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.gchandle.tointptr.aspx
          _cachedHandle = SafeGCHandle.Alloc(this);
          _cachedHandleIntPtr = _cachedHandle.ToIntPtr();
        }

        return _cachedHandleIntPtr;
      }
    }
#endregion

    #region PInvoke
    
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARDatastore_Init
    (
      IntPtr networkHandle,
      string datastoreId,
      byte implementationType
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARDatastore_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARDatastore_SetData
    (
      IntPtr nativeHandle,
      string key,
      byte[] data,
      UInt64 dataSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARDatastore_GetData
    (
      IntPtr nativeHandle,
      string key,
      out byte outResult,
      out UInt64 outDataSize,
      UInt64 maxOutDataSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARDatastore_DeleteData(IntPtr nativeHandle, string key);
   
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern UInt32 _NARDatastore_GetKeyCount(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARDatastore_GetKeys
    (
      IntPtr nativeHandle,
      StringBuilder outKeys,
      UInt32 outKeysCount,
      UInt64 maxOutKeysSize
    );

    // TODO: Add CompareAndSwap when exposing CAS to C#
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARDatastore_ClaimOwnership(IntPtr nativeHandle, string key);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARDatastore_SetLifeCycle
    (
      IntPtr nativeHandle,
      string key,
      byte option
    );

    // Callbacks
    private delegate void _NARDatastore_keyValueAddedCallback(IntPtr context, string key);

    private delegate void _NARDatastore_keyValueUpdatedCallback(IntPtr context, string key);

    private delegate void _NARDatastore_keyValueDeletedCallback(IntPtr context, string key);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARDatastore_Set_keyValueAddedCallback
    (
      IntPtr applicationContext,
      IntPtr nativeHandle,
      _NARDatastore_keyValueAddedCallback cb
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARDatastore_Set_keyValueUpdatedCallback
    (
      IntPtr applicationContext,
      IntPtr nativeHandle,
      _NARDatastore_keyValueUpdatedCallback cb
    );
    
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARDatastore_Set_keyValueDeletedCallback
    (
      IntPtr applicationContext,
      IntPtr nativeHandle,
      _NARDatastore_keyValueDeletedCallback cb
    );

    [MonoPInvokeCallback(typeof(_NARDatastore_keyValueAddedCallback))]
    private static void _keyValueAddedNative
    (
      IntPtr context,
      string key
    )
    {
      ARLog._Debug("Invoked _keyValueAddedNative", true);
      var instance = SafeGCHandle.TryGetInstance<_NativeDatastore>(context);

      if (instance == null || instance._isDestroyed)
      {
        ARLog._Warn("Queued _keyValueAddedNative called after C# instance was destroyed.");
        return;
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance._isDestroyed)
          {
            var msg = "Queued _keyValueAddedNative called after C# instance was destroyed.";
            ARLog._Warn(msg);
            return;
          }

          var handler = instance.KeyValueAdded;
          if (handler != null)
          {
            ARLog._Debug("Surfacing _keyValueAddedNative event");

            var args = new KeyValuePairArgs(key);
            handler(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(_NARDatastore_keyValueUpdatedCallback))]
    private static void _keyValueUpdatedNative
    (
      IntPtr context,
      string key
    )
    {
      ARLog._Debug("Invoked _keyValueUpdatedNative", true);
      var instance = SafeGCHandle.TryGetInstance<_NativeDatastore>(context);

      if (instance == null || instance._isDestroyed)
      {
        ARLog._Warn("Queued _keyValueUpdatedNative called after C# instance was destroyed.");
        return;
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance._isDestroyed)
          {
            var msg = "Queued _keyValueUpdatedNative called after C# instance was destroyed.";
            ARLog._Warn(msg);
            return;
          }

          var handler = instance.KeyValueUpdated;
          if (handler != null)
          {
            ARLog._Debug("Surfacing _keyValueUpdatedNative event");
            var args = new KeyValuePairArgs(key);

            handler(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(_NARDatastore_keyValueDeletedCallback))]
    private static void _keyValueDeletedNative
    (
      IntPtr context,
      string key
    )
    {
      ARLog._Debug("Invoked _keyValueDeletedNative", true);
      var instance = SafeGCHandle.TryGetInstance<_NativeDatastore>(context);

      if (instance == null || instance._isDestroyed)
      {
        ARLog._Warn("Queued _keyValueDeletedNative called after C# instance was destroyed.");
        return;
      }

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance._isDestroyed)
          {
            var msg = "Queued _keyValueDeletedNative called after C# instance was destroyed.";
            ARLog._Warn(msg);
            return;
          }

          var handler = instance.KeyValueDeleted;
          if (handler != null)
          {
            ARLog._Debug("Surfacing _keyValueDeletedNative event");

            var args = new KeyValuePairArgs(key);
            handler(args);
          }
        }
      );
    }
#endregion
  }
}
#pragma warning restore 0067

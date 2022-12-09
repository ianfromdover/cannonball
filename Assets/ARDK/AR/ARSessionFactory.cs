// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;

using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.AR;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.AR
{
  /// Class used to create ARSessions and also to be notified when new ARSessions are created.
  public static class ARSessionFactory
  {
    /// Create an ARSession appropriate for the current device.
    ///
    /// On a mobile device, the attempted order will be LiveDevice, Remote, and finally Mock.
    /// In the Unity Editor, the attempted order will be Remote, then Mock.
    ///
    /// @param stageIdentifier
    ///   The identifier used by the C++ library to connect all related components.
    ///
    /// @returns The created session, or throws if it was not possible to create a session.
    public static IARSession Create(Guid stageIdentifier = default)
    {
      return Create(_VirtualStudioLauncher.SelectedMode, stageIdentifier);
    }

    /// Create an ARSession for the specified RuntimeEnvironment.
    ///
    /// @param env
    ///   The env used to create the session for.
    /// @param stageIdentifier
    ///   The identifier used by the C++ library to connect all related components.
    ///
    /// @returns The created session, or null if it was not possible to create a session.
    public static IARSession Create(RuntimeEnvironment env, Guid stageIdentifier = default)
    {
      if (env == RuntimeEnvironment.Default)
        return Create(_VirtualStudioLauncher.SelectedMode, stageIdentifier);

      if (stageIdentifier == default)
        stageIdentifier = Guid.NewGuid();

      IARSession result;
      switch (env)
      {
        case RuntimeEnvironment.LiveDevice:
          if (_activeSession != null)
            throw new InvalidOperationException("There's another session still active.");

          result = new _NativeARSession(stageIdentifier);
          break;

        case RuntimeEnvironment.Remote:
          result = new _RemoteEditorARSession(stageIdentifier);
          break;

        case RuntimeEnvironment.Playback:
          result = new _NativeARSession(stageIdentifier, true);
          break;

        case RuntimeEnvironment.Mock:
          result = new _MockARSession(stageIdentifier, _VirtualStudioSessionsManager.Instance);
          break;

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, env.GetType());
      }

      _InvokeSessionInitialized(result, isLocal: true);
      return result;
    }

    private static ArdkEventHandler<AnyARSessionInitializedArgs> _sessionInitialized;

    /// Event invoked when a new session is created and initialized.
    public static event ArdkEventHandler<AnyARSessionInitializedArgs> SessionInitialized
    {
      add
      {
        _StaticMemberValidator._FieldIsNullWhenScopeEnds(() => _sessionInitialized);

        _sessionInitialized += value;

        IARSession activeSession;
        lock (_activeSessionLock)
          activeSession = _activeSession;

        if (activeSession != null)
        {
          var args = new AnyARSessionInitializedArgs(activeSession, isLocal: true);
          value(args);
        }
      }
      remove
      {
        _sessionInitialized -= value;
      }
    }

    internal static readonly RuntimeEnvironment[] _defaultBestMatches =
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
      new RuntimeEnvironment[] { RuntimeEnvironment.LiveDevice, RuntimeEnvironment.Remote, RuntimeEnvironment.Mock };
#else
      new RuntimeEnvironment[]
      {
        RuntimeEnvironment.Remote, RuntimeEnvironment.Playback, RuntimeEnvironment.Mock
      };
#endif

    internal static IARSession _CreateVirtualStudioManagedARSession
    (
      RuntimeEnvironment env,
      Guid stageIdentifier,
      bool isLocal,
      _IVirtualStudioSessionsManager virtualStudioManager
    )
    {
      IARSession result;

      switch (env)
      {
        case RuntimeEnvironment.Mock:
        {
          if (virtualStudioManager == null)
            virtualStudioManager = _VirtualStudioSessionsManager.Instance;

          result = new _MockARSession(stageIdentifier, virtualStudioManager);
          break;
        }

        case RuntimeEnvironment.Remote:
          result = new _RemoteEditorARSession(stageIdentifier);
          break;

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, typeof(RuntimeEnvironment));
      }

      ARLog._DebugFormat
      (
        "Created IARSession with env: {0} and stage identifier: {1}",
        false,
        env,
        stageIdentifier
      );

      _InvokeSessionInitialized(result, isLocal);
      return result;
    }

    private static object _activeSessionLock = new object();
    private static IARSession _activeSession;

    // As there is no ConcurrentHashSet at the moment, we use a ConcurrentDictionary and only
    // care about the key, ignoring the value.
    private static ConcurrentDictionary<IARSession, bool> _nonLocalSessions =
      new ConcurrentDictionary<IARSession, bool>(_ReferenceComparer<IARSession>.Instance);

    private static void _InvokeSessionInitialized(IARSession session, bool isLocal)
    {
      var handler = isLocal ? _sessionInitialized : _nonLocalSessionInitialized;

      if (handler != null)
      {
        var args = new AnyARSessionInitializedArgs(session, isLocal);
        handler(args);
      }
      
      if (isLocal)
      {
        _StaticMemberValidator._FieldIsNullWhenScopeEnds(() => _activeSession);

        lock (_activeSessionLock)
        {
          if (_activeSession != null)
            throw new InvalidOperationException("There's another session still active.");

          _activeSession = session;
        }

        session.Deinitialized +=
          (_) =>
          {
            lock (_activeSessionLock)
              if (_activeSession == session)
                _activeSession = null;
          };
      }
      else
      {
        _StaticMemberValidator._CollectionIsEmptyWhenScopeEnds(() => _nonLocalSessions);

        if (!_nonLocalSessions.TryAdd(session, true))
          throw new InvalidOperationException("Duplicated session.");

        session.Deinitialized += (ignored) => _nonLocalSessions.TryRemove(session, out _);
      }
    }

    private static ArdkEventHandler<AnyARSessionInitializedArgs> _nonLocalSessionInitialized;

    internal static event ArdkEventHandler<AnyARSessionInitializedArgs> _NonLocalSessionInitialized
    {
      add
      {
        _StaticMemberValidator._FieldIsNullWhenScopeEnds(() => _nonLocalSessionInitialized);

        _nonLocalSessionInitialized += value;

        // Doing a foreach on a ConcurrentDictionary is safe even if values keep being added or
        // removed during the iteration. Also, it is lock free, so no chance of dead-locks.
        foreach (var session in _nonLocalSessions.Keys)
        {
          var args = new AnyARSessionInitializedArgs(session, isLocal: false);
          value(args);
        }
      }
      remove
      {
        _nonLocalSessionInitialized -= value;
      }
    }
  }
}

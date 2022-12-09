// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Niantic.ARDK.AR;
using Niantic.ARDK.VirtualStudio;
using Niantic.ARDK.VirtualStudio.Networking;
using Niantic.ARDK.VirtualStudio.Networking.Mock;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Collections;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.Remote;

namespace Niantic.ARDK.Networking
{
  /// A class to create new MultipeerNetworking instances as well as to be notified of their
  /// creation.
  public static class MultipeerNetworkingFactory
  {
    /// Initializes the static members of this class that depend on previously initialized members.
    static MultipeerNetworkingFactory()
    {
      _readOnlyNetworkings = _networkings.AsArdkReadOnly();
    }

    /// Create a MultipeerNetworking appropriate for the current device.
    ///
    /// On a mobile device, the attempted order will be LiveDevice, Remote, and finally Mock.
    /// In the Unity Editor, the attempted order will be Remote, then Mock.
    ///
    /// @param stageIdentifier
    ///   The identifier used by the C++ library to connect all related components.
    ///
    /// @returns The created MultipeerNetworking, or throws if it was not possible to create a session.
    public static IMultipeerNetworking Create(Guid stageIdentifier = default(Guid))
    {
      return Create(_VirtualStudioLauncher.SelectedMode, ServerConfiguration.ARBE, stageIdentifier);
    }

    public static IMultipeerNetworking Create
    (
      ServerConfiguration serverConfiguration,
      Guid stageIdentifier = default
    )
    {
      return Create(_VirtualStudioLauncher.SelectedMode, serverConfiguration, stageIdentifier);
    }

    /// Create a MultipeerNetworking with the specified RuntimeEnvironment.
    ///
    /// @param env
    ///   The env used to create the MultipeerNetworking.
    /// @param stageIdentifier
    ///   The identifier used by the C++ library to connect all related components.
    ///
    /// @returns The created MultipeerNetworking, or throws if it was not possible to create a session.
    public static IMultipeerNetworking Create(RuntimeEnvironment env, Guid stageIdentifier = default(Guid))
    {
      var networking = Create(env, ServerConfiguration.ARBE, stageIdentifier);
      if (networking == null)
      {
        throw new NotSupportedException("The provided env is not supported by this build.");
      }

      return networking;
    }

    /// Create a MultipeerNetworking with the specified RuntimeEnvironment.
    ///
    /// @param env
    ///   The env used to create the MultipeerNetworking.
    /// @param serverConfiguration
    ///   The ServerConfiguration that this MultipeerNetworking will use to communicate with ARBEs
    /// @param stageIdentifier
    ///   The identifier used by the C++ library to connect all related components.
    ///
    /// @returns The created MultipeerNetworking, or throws if it was not possible to create a session.
    public static IMultipeerNetworking Create
    (
      RuntimeEnvironment env,
      ServerConfiguration serverConfiguration,
      Guid stageIdentifier = default(Guid)
    )
    {
      if (env == RuntimeEnvironment.Default)
        return Create(_VirtualStudioLauncher.SelectedMode, serverConfiguration, stageIdentifier);

      if (stageIdentifier == default(Guid))
        stageIdentifier = Guid.NewGuid();

      IMultipeerNetworking result;
      switch (env)
      {
        case RuntimeEnvironment.LiveDevice:
          result = new _NativeMultipeerNetworking(serverConfiguration, stageIdentifier);
          break;

        case RuntimeEnvironment.Remote:
          result = new _RemoteEditorMultipeerNetworking(serverConfiguration, stageIdentifier);
          break;

        case RuntimeEnvironment.Mock:
          result = new _MockMultipeerNetworking(stageIdentifier, _VirtualStudioSessionsManager.Instance);
          break;

        default:
          throw new InvalidEnumArgumentException(nameof(env), (int)env, env.GetType());
      }

      _InvokeNetworkingInitialized(result, isLocal: true);
      return result;
    }

    /// A collection of all current networking stacks
    public static IReadOnlyCollection<IMultipeerNetworking> Networkings
    {
      get
      {
        return _readOnlyNetworkings;
      }
    }

    private static ArdkEventHandler<AnyMultipeerNetworkingInitializedArgs> _networkingInitialized;

    /// Event called when a new MultipeerNetworking instance is initialized.
    public static event ArdkEventHandler<AnyMultipeerNetworkingInitializedArgs> NetworkingInitialized
    {
      add
      {
        _StaticMemberValidator._FieldIsNullWhenScopeEnds(() => _networkingInitialized);

        _networkingInitialized += value;

        // If there already exists some networkings, call the event so you don't miss anything
        foreach (var networking in _networkings)
        {
          var args = new AnyMultipeerNetworkingInitializedArgs(networking);
          value(args);
        }
      }
      remove
      {
        _networkingInitialized -= value;
      }
    }

    internal static IMultipeerNetworking _CreateVirtualStudioManagedNetworking
    (
      RuntimeEnvironment env,
      ServerConfiguration configuration,
      Guid stageIdentifier,
      _IVirtualStudioSessionsManager virtualStudioSessionsManager,
      bool isLocal
    )
    {
      IMultipeerNetworking implementation;
      switch (env)
      {
        case RuntimeEnvironment.Mock:
          implementation = new _MockMultipeerNetworking(stageIdentifier, virtualStudioSessionsManager);
          break;

        case RuntimeEnvironment.Remote:
          implementation = new _RemoteEditorMultipeerNetworking(configuration, stageIdentifier);
          break;

        default:
          // Both LiveDevice and Default are invalid cases for this method
          throw new ArgumentOutOfRangeException(nameof(env), env, null);
      }

      _InvokeNetworkingInitialized(implementation, isLocal);
      return implementation;
    }

    private static
      ArdkEventHandler<AnyMultipeerNetworkingInitializedArgs> _nonLocalNetworkingInitialized;

    internal static event
      ArdkEventHandler<AnyMultipeerNetworkingInitializedArgs> _NonLocalNetworkingInitialized
      {
        add
        {
          _StaticMemberValidator._FieldIsNullWhenScopeEnds(() => _nonLocalNetworkingInitialized);

          _nonLocalNetworkingInitialized += value;

          // If there already exists some networkings, call the event so you don't miss anything
          foreach (var networking in _nonLocalNetworkings)
          {
            var args = new AnyMultipeerNetworkingInitializedArgs(networking);
            value(args);
          }
        }
        remove
        {
          _nonLocalNetworkingInitialized -= value;
        }
      }

    #region Implementation
    private static readonly ARDKReadOnlyCollection<IMultipeerNetworking> _readOnlyNetworkings;

    private static readonly HashSet<IMultipeerNetworking> _networkings =
      new HashSet<IMultipeerNetworking>(_ReferenceComparer<IMultipeerNetworking>.Instance);

    private static readonly HashSet<IMultipeerNetworking> _nonLocalNetworkings =
      new HashSet<IMultipeerNetworking>(_ReferenceComparer<IMultipeerNetworking>.Instance);

    private static void _InvokeNetworkingInitialized(IMultipeerNetworking networking, bool isLocal)
    {
      if (SessionExists(networking, isLocal))
      {
        ARLog._WarnFormat
        (
          "An IMultipeerNetworking instance with the StageIdentifier {0} was already initialized.",
          false,
          networking.StageIdentifier
        );

        return;
      }

      ArdkEventHandler<AnyMultipeerNetworkingInitializedArgs> handler;

      if (isLocal)
      {
        ARLog._Debug("Initializing a local session");
        _StaticMemberValidator._CollectionIsEmptyWhenScopeEnds(() => _networkings);

        _networkings.Add(networking);
        networking.Deinitialized += (_) => _networkings.Remove(networking);
        handler = _networkingInitialized;
      }
      else
      {
        ARLog._Debug("Initializing a non-local session");
        _StaticMemberValidator._CollectionIsEmptyWhenScopeEnds(() => _nonLocalNetworkings);

        _nonLocalNetworkings.Add(networking);
        networking.Deinitialized += (_) => _nonLocalNetworkings.Remove(networking);
        handler = _nonLocalNetworkingInitialized;
      }

      if (handler != null)
      {
        var args = new AnyMultipeerNetworkingInitializedArgs(networking);
        handler(args);
      }
    }

    private static bool SessionExists(IMultipeerNetworking networking, bool isLocal)
    {
      if (isLocal)
        return _networkings.Contains(networking);

      return _nonLocalNetworkings.Contains(networking);
    }
    #endregion
  }
}

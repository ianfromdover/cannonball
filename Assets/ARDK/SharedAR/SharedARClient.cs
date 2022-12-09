// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.AR;

namespace Niantic.Experimental.ARDK.SharedAR
{
  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class SharedARClient: 
    IDisposable
  {
    public INetworking Networking { get; private set; }
    public IColocalization Colocalization { get; private set; }
    
    public SharedARClient(IARSession session, string connectionId)
    {
      Networking = NetworkingFactory.Create(connectionId);
      Colocalization = ColocalizationFactory.Create(Networking, session);
    }

    public void Start()
    {
      // Add all subcomponent calls that should run when user wants to start sharedar as a whole
      Colocalization.Start();
    }

    public void Stop()
    {
      // Add all subcomponent calls that should run when user wants to stop sharedar as a whole
      Colocalization.Stop();
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Colocalization.Dispose();
      Networking.Dispose();
      Colocalization = null;
      Networking = null;
    }

    ~SharedARClient()
    {
      Dispose();
    }
  }
}

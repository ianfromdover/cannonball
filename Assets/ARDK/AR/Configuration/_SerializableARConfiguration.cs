// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Niantic.ARDK.AR.Awareness.Depth.Generators;
using Niantic.ARDK.Utilities.Collections;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.AR.Configuration
{
  internal abstract class _SerializableARConfiguration:
    IARConfiguration
  {
    public bool IsLightEstimationEnabled { get; set; }
    public WorldAlignment WorldAlignment { get; set; }

    public IReadOnlyCollection<IARVideoFormat> SupportedVideoFormats
    {
      get
      {
        return EmptyReadOnlyCollection<IARVideoFormat>.Instance;
      }
    }

    public IARVideoFormat VideoFormat
    {
      get
      {
        return null;
      }
      set
      {
      }
    }
    
    public virtual void CopyTo(IARConfiguration target)
    {
      target.IsLightEstimationEnabled = IsLightEstimationEnabled;
      target.WorldAlignment = WorldAlignment;
    }

    void IDisposable.Dispose()
    {
      // Do nothing. This implementation of IARConfiguration is fully managed.
    }
  }
}


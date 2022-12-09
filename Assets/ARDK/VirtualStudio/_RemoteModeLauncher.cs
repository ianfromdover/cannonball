using System.Text.RegularExpressions;

using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.Remote;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio
{
  internal class _RemoteModeLauncher:
    _IVirtualStudioModeLauncher
  {
    private int _imageCompression;
    private int _imageFramerate;
    private int _awarenessFramerate;
    private int _featurePointFramerate;

    public int ImageCompression
    {
      get
      {
        return _imageCompression;
      }
      set
      {
        _RemoteBufferConfiguration.ImageCompression = value;
        _imageCompression = value;
      }
    }

    public int ImageFramerate
    {
      get
      {
        return _imageFramerate;
      }
      set
      {
        _RemoteBufferConfiguration.ImageFramerate = value;
        _imageFramerate = value;
      }
    }

    public int AwarenessFramerate
    {
      get
      {
        return _awarenessFramerate;
      }
      set
      {
        _RemoteBufferConfiguration.AwarenessFramerate = value;
        _awarenessFramerate = value;
      }
    }

    public int FeaturePointFramerate
    {
      get
      {
        return _featurePointFramerate;
      }
      set
      {
        _RemoteBufferConfiguration.FeaturePointFramerate = value;
        _featurePointFramerate = value;
      }
    }

    public _RemoteModeLauncher()
    {
      _imageCompression = _RemoteBufferConfiguration.ImageCompression;
      _imageFramerate = _RemoteBufferConfiguration.ImageFramerate;
      _awarenessFramerate = _RemoteBufferConfiguration.AwarenessFramerate;
      _featurePointFramerate = _RemoteBufferConfiguration.FeaturePointFramerate;
    }

    public void ExitEditMode()
    {
      // do nothing
    }

    public void EnterPlayMode()
    {
      _RemoteConnection.InitIfNone(_RemoteConnection.ConnectionMethod.USB);
      _RemoteConnection.Connect("");
    }

    public void ExitPlayMode()
    {
      // do nothing
    }
  }
}

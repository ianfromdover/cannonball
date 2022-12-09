// Copyright 2021 Niantic, Inc. All Rights Reserved.

using System.Runtime.InteropServices;
using Niantic.ARDK.Internals;
using System;
using System.IO;
using System.Linq;

using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio.AR
{
  internal class _PlaybackDataset
  {
    private int _frameCount = 0;

    public _PlaybackDataset(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(path);

      if (IsValidLocalPath(path))
        _NARPlaybackDataset_Init(path, true);
      else
#if ARDK_IQP
        _NARPlaybackDataset_Init(path, false);
#else
        ARLog._Error("Invalid dataset path.");
#endif

      _frameCount = _NARPlaybackDataset_GetFrameCount();
    }

    private static bool IsValidLocalPath(string path)
    {
      try
      {
        var files = Directory.GetFiles(path);
        return files.Any(s => s.EndsWith("capture.json"));
      }
      catch
      {
        return false;
      }
    }

    public void SwitchDataset(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException(path);

      var launcher = (_PlaybackModeLauncher)_VirtualStudioLauncher.GetOrCreateModeLauncher(RuntimeEnvironment.Playback);
      launcher.DatasetPath = path;

      if (IsValidLocalPath(path))
        _NARPlaybackDataset_Switch(path, true);
#if ARDK_IQP
      else
        _NARPlaybackDataset_Init(path, false);
#else
      else
        ARLog._Error("Invalid dataset path.");
#endif
    }

    public int GetDatasetSize()
    {
      return _frameCount;
    }

    public void Dispose()
    {
      ARLog._Debug("Disposed PlaybackDataset");

      _NARPlaybackDataset_Release();
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARPlaybackDataset_Init(string datasetPath, bool isLocal);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARPlaybackDataset_Switch(string datasetPath, bool isLocal);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARPlaybackDataset_Release();

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern int _NARPlaybackDataset_GetFrameCount();
  }
}
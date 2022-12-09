using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VirtualStudio.AR;

using UnityEngine;

namespace Niantic.ARDK.VirtualStudio
{
  internal class _PlaybackModeLauncher:
    _IVirtualStudioModeLauncher
  {
    private const string DATASET_PATH_KEY = "ARDK_Playback_Dataset_Path";

    private string _datasetPath;
    public string DatasetPath
    {
      get
      {
        return _datasetPath;
      }
      set
      {
        _datasetPath = value;
        PlayerPrefs.SetString(DATASET_PATH_KEY, value);
      }
    }

    public _PlaybackModeLauncher()
    {
      _datasetPath = PlayerPrefs.GetString(DATASET_PATH_KEY, "");
    }

    public void ExitEditMode()
    {

    }

    public void EnterPlayMode()
    {

    }

    public void ExitPlayMode()
    {

    }
  }
}

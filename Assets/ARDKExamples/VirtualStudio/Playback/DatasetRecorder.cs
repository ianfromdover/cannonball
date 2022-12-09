// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using System.Collections;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Recording;
using Niantic.ARDK.LocationService;

using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDKExamples.VirtualStudio
{
  /// Manages the recording of Playback datasets
  public class DatasetRecorder:
    MonoBehaviour
  {
    // Only datasets with location data included can be used to run VPS in Playback Mode.
    [SerializeField]
    private Toggle _captureLocationToggle;

    private IARSession _session = null;
    private ARCapture _capture = null;
    private string _captureName = null;
    private string _temporaryPath = null;
    private string _persistentPath = null;

    private ILocationService _locationService = null;

    private ARCaptureConfig _captureConfig = ARCaptureConfig.Default;

    private void Start()
    {
      ARSessionFactory.SessionInitialized += OnAnyDidInitialize;
      _temporaryPath = Application.persistentDataPath + "/tmp_captures";
      _persistentPath = Application.persistentDataPath + "/captures";
    }

    private void OnDestroy()
    {
      ARSessionFactory.SessionInitialized -= OnAnyDidInitialize;
    }

    private void OnAnyDidInitialize(AnyARSessionInitializedArgs args)
    {
      if (_session != null)
        return;

      Debug.Log("CaptureManager: AR Session initialized");

      _session = args.Session;
      _session.Deinitialized += OnWillDeinitialize;

      if (_captureLocationToggle != null && _captureLocationToggle.isOn)
      {
        _locationService = LocationServiceFactory.Create();
        _session.SetupLocationService(_locationService);
        _locationService.Start(1f, 0.0f);
      }
    }

    private void OnWillDeinitialize(ARSessionDeinitializedArgs args)
    {
      if (_capture != null && _capture.IsRecording())
        _capture.Stop();

      _capture = null;

      _locationService?.Stop();
      Debug.Log("CaptureManager: Unity Location Service stopped");

      _locationService = null;

      _session.Deinitialized -= OnWillDeinitialize;
      _session = null;
      Debug.Log("CaptureManager: AR Session stopped");
    }

    public void StartCapture()
    {
      var stageIdentifier = _session != null ? _session.StageIdentifier : Guid.Empty;
      _capture = new ARCapture(stageIdentifier);

      _captureName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
      _captureConfig.WorkingDirectoryPath = _temporaryPath + "/" + _captureName;
      _captureConfig.ArchivePath = _persistentPath + "/" + _captureName + ".tgz";

      Debug.Log("CaptureManager: Starting a new capture: " + _captureName);
      _capture.Start(_captureConfig);

      StartCoroutine(WatchCaptureStart(_capture));
    }

    public void StopCapture()
    {
      if (_capture == null)
        return;

      Debug.Log("CaptureManager: Stopping the capture");
      _capture.Stop();

      StartCoroutine(WatchCaptureStop(_capture));
    }

    public void DeleteFiles()
    {
      if (Directory.Exists(_temporaryPath))
      {
        Directory.Delete(_temporaryPath, true);
        Debug.Log($"CaptureManager: Successfully deleted files from temporary path : {_temporaryPath}");
      }
      else
      {
        Debug.Log($"CaptureManager: No files to delete from temporary path : {_temporaryPath}");
      }

      if (Directory.Exists(_persistentPath))
      {
        Directory.Delete(_persistentPath, true);
        Debug.Log($"CaptureManager: Successfully deleted files from persistent path : {_persistentPath}");
      }
      else
      {
        Debug.Log($"CaptureManager: No files to delete from persistent path : {_persistentPath}");
      }
    }

    private void OnCaptureStarted()
    {
      _capture.SetApplicationName("ARDK Examples");

      // Example of setting simple client-specific metadata as JSON
      CaptureMetadata metadata =
        new CaptureMetadata
        {
          generatedBy = "CaptureManager.cs",
          captureName = _captureName,
          appId = Application.identifier,
          appVersion = Application.version,
          unityVersion = Application.unityVersion,
        };

      _capture.SetJSONMetadata(JsonUtility.ToJson(metadata));
      Debug.Log("CaptureManager: Added capture metadata");

      // Example of getting the actual paths used by the capture
      var paths = _capture.GetCapturePaths();
      if (paths.WorkingDirectoryPath != null && paths.ArchivePath != null)
      {
        Debug.Log("CaptureManager: Capture working directory path:\n" + paths.WorkingDirectoryPath);
        Debug.Log("CaptureManager: Capture archive path:\n" + paths.ArchivePath);
      }
      else
      {
        Debug.LogError("CaptureManager: Error retrieving the capture paths.");
      }
    }

    private void OnCaptureStopped()
    {
      Debug.Log("CaptureManager: Successfully written archive.");
      _capture = null;
    }

    // Capture Metadata
    [Serializable]
    private class CaptureMetadata
    {
      public string generatedBy;
      public string captureName;
      public string appId;
      public string appVersion;
      public string unityVersion;
    }

    // Coroutines
    private IEnumerator WatchCaptureStart(ARCapture capture)
    {
      if (capture == null)
        yield break;

      while (!capture.IsRecording())
      {
        if (_capture != capture)
        {
          yield break;
        }

        yield return null;
      }

      if (_capture != capture)
      {
        var errorMessage = "A capture has been started before the previous one is successfully " +
                           "configured. Discarding the previous capture.";

        Debug.LogError(errorMessage);
        yield break;
      }

      OnCaptureStarted();
    }

    private IEnumerator WatchCaptureStop(ARCapture capture)
    {
      if (capture == null)
        yield break;

      while (capture.IsRecording())
      {
        if (_capture != capture)
          yield break;

        yield return new WaitForSeconds(0.5f);
      }

      // It is possible that a new capture has been started during the last 0.5 seconds, so
      //   don't null the _capture field if it is not equal to this one. This capture has been
      //   successfully completed, so log a success message.
      if (_capture != capture)
      {
        Debug.Log("Successfully written archive.");
        yield break;
      }

      OnCaptureStopped();
    }

    // For internal use
    public void SetCaptureConfig(ARCaptureConfig config)
    {
      _captureConfig = config;
    }
  }
}

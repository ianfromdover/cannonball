// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_WIN
#define UNITY_STANDALONE_DESKTOP
#endif
#if (UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE_DESKTOP) && !UNITY_EDITOR
#define AR_NATIVE_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Configuration.Authentication;

using Niantic.ARDK.Configuration;
using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Telemetry;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Niantic.ARDK.Internals
{
  /// Controls the startup systems for ARDK.
  public static class StartupSystems
  {
    // Add a destructor to this class to try and catch editor reloads
    private static readonly _Destructor _ = new _Destructor();

    // The pointer to the C++ NarSystemBase handling functionality at the native level
    private static IntPtr _nativeHandle = IntPtr.Zero;

    private const string FileDisablingSuffix = ".DISABLED";

    private static _TelemetryService _telemetryService;

#if UNITY_EDITOR_OSX
    [InitializeOnLoadMethod]
    private static void EditorStartup()
    {
      var nonNativeCompatibilityFilesChangedToBeEnabled = false;
      // Use rsp files to disable the native dll in case of lack of native support
      nonNativeCompatibilityFilesChangedToBeEnabled = EnforceRosettaBasedCompatibility();
      
#if !REQUIRE_MANUAL_STARTUP
      if (!nonNativeCompatibilityFilesChangedToBeEnabled)
      {
        _InitializeNativeLibraries();
      }
      _SetCSharpInitializationMetadata();

#endif
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Startup()
    {
#if AR_NATIVE_SUPPORT
#if !REQUIRE_MANUAL_STARTUP
      _InitializeNativeLibraries();
      _SetCSharpInitializationMetadata();
#endif
#endif
    }
    
    /// <summary>
    /// Allows users to Manually startup and refresh the underlying implementation when required.
    /// Used by internal teams. DO NOT MAKE PRIVATE
    /// </summary>
    public static void ManualStartup()
    {
      _InitializeNativeLibraries();
    }

    /// <summary>
    /// Starts up the ARDK startup systems if they haven't been started yet.
    /// </summary>
    private static void _InitializeNativeLibraries()
    {
#if (AR_NATIVE_SUPPORT || UNITY_EDITOR_OSX)
      // start the telemetry service
      InitializeTelemetry();
      
      try
      {
        // TODO(sxian): Remove the _ROR_CREATE_STARTUP_SYSTEMS() after moving the functionalities to
        // NARSystemBase class.
        // Note, don't put any code before calling _NARSystemBase_Initialize() below, since Narwhal C++
        // _NARSystemBase_Initialize() should be the first API to be called before other components are initialized.
        _ROR_CREATE_STARTUP_SYSTEMS();
      }
      catch (DllNotFoundException e)
      {
        ARLog._DebugFormat("Failed to create ARDK startup systems: {0}", false, e);
      }

      if (_nativeHandle == IntPtr.Zero) {
        _nativeHandle = _InitialiseNarBaseSystemBasedOnOS();
        _CallbackQueue.ApplicationWillQuit += OnApplicationQuit;
      } else {
        ARLog._Error("_nativeHandle is not null, _InitializeNativeLibraries is called twice");
      }
      
#endif
    }

    private static void _SetCSharpInitializationMetadata()
    {
      // The initialization of C# components should happen below.
      _SetAuthenticationParameters();
      SetDeviceMetadata();

      _TelemetryService.RecordEvent(new InitializationEvent()
      {
        InstallMode = GetInstallMode(),
      });
    }

    private static void OnApplicationQuit()
    {
      if (_nativeHandle != IntPtr.Zero)
      {
        _NARSystemBase_Release(_nativeHandle);
        _nativeHandle = IntPtr.Zero;
      }
    }

    private const string AUTH_DOCS_MSG = "For more information, visit the niantic.dev/docs/authentication.html site.";

    internal static void _SetAuthenticationParameters()
    {
      // We always try to find an api key
      var apiKey = string.Empty;
      var authConfigs = Resources.LoadAll<ArdkAuthConfig>("ARDK/ArdkAuthConfig");

      if (authConfigs.Length > 1)
      {
        var errorMessage = "There are multiple ArdkAuthConfigs in Resources/ARDK/ " +
                           "directories, loading the first API key found. Remove extra" +
                           " ArdkAuthConfigs to prevent API key problems. " + AUTH_DOCS_MSG;
        ARLog._Error(errorMessage);
      }
      else if (authConfigs.Length == 0)
      {
        ARLog._Error
        ($"Could not load an ArdkAuthConfig, please add one in a Resources/ARDK/ directory. {AUTH_DOCS_MSG}");
      }
      else
      {
        var authConfig = authConfigs[0];
        apiKey = authConfig.ApiKey;

        if (!string.IsNullOrEmpty(apiKey))
          ArdkGlobalConfig.SetApiKey(apiKey);
      }

      authConfigs = null;
      Resources.UnloadUnusedAssets();

      //Only continue if needed
      if (!ServerConfiguration.AuthRequired)
        return;

      if (string.IsNullOrEmpty(ServerConfiguration.ApiKey))
      {

        if (!string.IsNullOrEmpty(apiKey))
        {
          ServerConfiguration.ApiKey = apiKey;
        }
        else
        {
          ARLog._Error($"No API Key was found. Add it to an ArdkAuthConfig asset. {AUTH_DOCS_MSG}");
        }
      }

#if UNITY_EDITOR
      if (!string.IsNullOrEmpty(apiKey))
      {
        var authResult = ArdkGlobalConfig._VerifyApiKeyWithFeature("feature:unity_editor", isAsync: false);
        if(authResult == NetworkingErrorCode.Ok)
          ARLog._Debug("Successfully authenticated ARDK Api Key");
        else
        {
          ARLog._Error($"Attempted to authenticate ARDK Api Key, but got error: {authResult}");
        }
      }
#endif

      if (!string.IsNullOrEmpty(apiKey))
        ArdkGlobalConfig._VerifyApiKeyWithFeature(GetInstallMode(), isAsync: true);
    }

    private static string GetInstallMode()
    {
      return $"install_mode:{Application.installMode.ToString()}";
    }

    private static void SetDeviceMetadata()
    {
      ArdkGlobalConfig._Internal.SetApplicationId(Application.identifier);
      ArdkGlobalConfig._Internal.SetArdkInstanceId(_ArdkMetadataConfigExtension._CreateFormattedGuid());
    }

    private static IntPtr _InitialiseNarBaseSystemBasedOnOS()
    {
      if (_ArdkPlatformUtility.AreNativeBinariesAvailable)
      {
        return _NARSystemBase_Initialize(_TelemetryService._OnNativeRecordTelemetry);
      }

      return IntPtr.Zero;
    }

    private static readonly Dictionary<string, string> _rosettaFiles = new Dictionary<string, string>()
    {
      {"mcs", "/ARDK/mcs.rsp"},
      {"csc", "/ARDK/csc.rsp"},
    };

    private static bool _rosettaCompatibilityCheckPerformed = false;
    private static bool EnforceRosettaBasedCompatibility()
    {
      if (_rosettaCompatibilityCheckPerformed)
        return false;

#if UNITY_EDITOR
      if (_ArdkPlatformUtility.IsUsingRosetta())
      {
        return _EnableRosettaFiles();
      }

      _DisableRosettaFiles();
#endif
      _rosettaCompatibilityCheckPerformed = true;
      return false;
    }

#if UNITY_EDITOR
    private static bool _EnableRosettaFiles()
    {
      ARLog._Debug("Enabling the files for rosetta compatibility");
      bool fileChanged = false;
      foreach (var rosettaFile in _rosettaFiles)
      {
        var disabledFileName = rosettaFile.Value + FileDisablingSuffix;
        var absolutePath = _GetPathForFile(rosettaFile.Key, disabledFileName);

        if (!string.IsNullOrWhiteSpace(absolutePath))
          fileChanged |= _EnableFileWithRename(absolutePath);
      }

      return fileChanged;
    }

    private static void _DisableRosettaFiles()
    {
      ARLog._Debug("Disabling the files for rosetta compatibility");

      foreach (var rosettaFile in _rosettaFiles)
      {
        var absolutePath = _GetPathForFile(rosettaFile.Key, rosettaFile.Value);

        if (!string.IsNullOrWhiteSpace(absolutePath))
          _DisableFileWithRename(absolutePath);
      }
    }

    private static bool _EnableFileWithRename(string sourceFilePath)
    {
      string sourcePathEnabledFile = sourceFilePath.Substring(0, sourceFilePath.Length - FileDisablingSuffix.Length);

      if (File.Exists(sourcePathEnabledFile))
      {
        ARLog._Debug($"File with name {sourcePathEnabledFile} already exists. So removing disabled file if exists");
        if (File.Exists(sourceFilePath))
        {
          ARLog._Debug($"Deleting {sourceFilePath}");

          File.Delete(sourceFilePath);
          RemoveMetaFile(sourceFilePath);
        }
      }
      else
      {
        ARLog._Debug($"File with name {sourcePathEnabledFile} does not exist. Creating it.");
        File.Move(sourceFilePath, sourcePathEnabledFile);
        RemoveMetaFile(sourceFilePath);
        return true;
      }

      return false;
    }

    private static void _DisableFileWithRename(string sourcePath)
    {
      string newPath = sourcePath + FileDisablingSuffix;

      if (File.Exists(newPath))
      {
        ARLog._Debug($"File with name {newPath} already exists. So cleaning it up");
        File.Delete(newPath);
      }

      File.Move(sourcePath, newPath);
      RemoveMetaFile(sourcePath);
    }
    
    private static void RemoveMetaFile(string sourcePath)
    {
      string metaFilePath = sourcePath + ".meta";

      if(File.Exists(metaFilePath))
        File.Delete(metaFilePath);
    }

    private static string _GetPathForFile(string searchString, string pathInArdk)
    {
      var possibleAssetsGuids = AssetDatabase.FindAssets(searchString);
      foreach (var possibleAssetGuid in possibleAssetsGuids)
      {
        var path = AssetDatabase.GUIDToAssetPath(possibleAssetGuid);

        // sanity check
        if (path.Length < pathInArdk.Length)
          continue;

        // Get the last characters to compare with the pathInArdk string
        var finalSubstring = path.Substring(path.Length - pathInArdk.Length);
        if (finalSubstring.Equals(pathInArdk))
        {
          return path;
        }
      }

      return null;
    }

#endif

    private static void InitializeTelemetry()
    {
      _telemetryService = _TelemetryService.Instance;
      _telemetryService.Start(Application.persistentDataPath);

      _TelemetryHelper.Start();
    }

    private sealed class _Destructor
    {
      // TODO: Inject telemetry service in ctor and do a Flush() in dispose once Flush() is exposed to us in the library
      ~_Destructor()
      {
        _telemetryService.Stop();
        OnApplicationQuit();
      }
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _ROR_CREATE_STARTUP_SYSTEMS();

    [DllImport(_ARDKLibrary.libraryName, CharSet = CharSet.Auto)]
    private static extern IntPtr _NARSystemBase_Initialize(_TelemetryService._ARDKTelemetry_Callback callback);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARSystemBase_Release(IntPtr nativeHandle);
  }
}
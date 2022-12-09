// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.Utilities.VersionUtilities;

using UnityEngine;

namespace Niantic.ARDK.Configuration
{
  internal sealed class _NativeArdkConfig:
    _ArdkGlobalConfigBase
  {
    private const int _NativeCallStringMaxLength = 512;

    private bool _gettingPlatformFirstTime = true;
    
    private string _dbowUrl;
    private string _userId;
    private string _clientId;
    private string _apiKey;
    private string _ardkAppInstanceId;
    private string _contextAwarenessUrl;
    private string _applicationId;
    private string _ardkVersion;
    private string _manufacturer;
    private string _deviceModel;
    private string _platform;
    
    public _NativeArdkConfig()
    {
      ARLog._Debug($"Using config: {nameof(_NativeArdkConfig)}");
    }
    
    public override bool SetUserIdOnLogin(string userId)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetUserId(userId))
      {
        ARLog._Warn("Failed to set the user Id");
        return false;
      }

      _userId = userId;
      return true;
    }

    [Obsolete("This method will not be available externally in a future release.")]
    public override bool SetDbowUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetDBoWUrl(url))
      {
        ARLog._Warn("Failed to set the DBoW URL. It may have already been set.");
        return false;
      }

      // The C++ side actually changes the provided url to include some version information.
      // So, here we just want to clear the cache. On a future get we will get the C++ provided
      // value.
      _dbowUrl = string.Empty;
      return true;
    }

    [Obsolete("This method will not be available externally in a future release.")]
    public override string GetDbowUrl()
    {
      var result = _dbowUrl;
      if (!String.IsNullOrWhiteSpace(result))
        return result;

      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDBoWUrl(stringBuilder, (ulong)stringBuilder.Capacity);

      result = stringBuilder.ToString();
      _dbowUrl = result;
      return result;
    }

    public override bool SetContextAwarenessUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetContextAwarenessUrl(url))
      {
        ARLog._Warn("Failed to set the Context Awareness URL.");
        return false;
      }

      _contextAwarenessUrl = url;
      return true;
    }
    
    [Obsolete("This method will not be available externally in a future release.")]
    public override string GetContextAwarenessUrl()
    {
      /// For security reasons, we will not exposed the default URL
      return _contextAwarenessUrl;
    }

    [Obsolete("This method will not be available externally in a future release.")]
    public override string GetAuthenticationUrl()
    {
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetAuthURL(stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }

    [Obsolete("This method will not be available externally in a future release.")]
    public override bool SetAuthenticationUrl(string url)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetAuthURL(url))
      {
        ARLog._Warn("Failed to set the Authentication URL.");
        return false;
      }

      return true;
    }

    public override NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync)
    {
      var error =
        (NetworkingErrorCode) _NAR_ARDKGlobalConfigHelper_ValidateApiKeyWithFeature(feature, isAsync);

      return error;
    }

    public override bool SetApiKey(string key)
    {
      if (!_NAR_ARDKGlobalConfigHelper_SetApiKey(key))
      {
        ARLog._Warn("Failed to set the API Key.");
        return false;
      }

      _apiKey = key;
      return true;
    }

    public override void SetApplicationId(string bundleId)
    {
      _NAR_ARDKGlobalConfigHelperInternal_SetDataField((uint)_ConfigDataField.ApplicationId, bundleId);
      _applicationId = bundleId;
    }

    public override void SetArdkInstanceId(string instanceId)
    {
      _NAR_ARDKGlobalConfigHelperInternal_SetDataField((uint)_ConfigDataField.ArdkAppInstanceId, instanceId);
      _ardkAppInstanceId = instanceId;
    }

    public override string GetApplicationId()
    {
      if (!string.IsNullOrWhiteSpace(_applicationId))
        return _applicationId;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ApplicationId, stringBuilder, (ulong)stringBuilder.Capacity);

      _applicationId = stringBuilder.ToString();
      return _applicationId;
    }

    public override string GetPlatform()
    {
      if (_gettingPlatformFirstTime)
      {
        _gettingPlatformFirstTime = false;
#if UNITY_EDITOR 
        SetUnityVersion(Application.unityVersion);
#endif
      }

      if (!string.IsNullOrWhiteSpace(_platform))
        return _platform;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.Platform, stringBuilder, (ulong)stringBuilder.Capacity);

      _platform = stringBuilder.ToString();
      return _platform;
    }

    public override string GetManufacturer()
    {
      if (!string.IsNullOrWhiteSpace(_manufacturer))
        return _manufacturer;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.Manufacturer, stringBuilder, (ulong)stringBuilder.Capacity);

      _manufacturer = stringBuilder.ToString();
      return _manufacturer;
    }

    public override string GetDeviceModel()
    {
      if (!string.IsNullOrWhiteSpace(_deviceModel))
        return _deviceModel;
#if UNITY_EDITOR
      _deviceModel = SystemInfo.operatingSystem;
      return _deviceModel;
#else
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.DeviceModel, stringBuilder, (ulong)stringBuilder.Capacity);

      _deviceModel = stringBuilder.ToString();
      return _deviceModel;
#endif
    }

    public override string GetArdkVersion()
    {
      if (!string.IsNullOrWhiteSpace(_ardkVersion))
        return _ardkVersion;
      
      _ardkVersion = ARDKGlobalVersion.GetARDKVersion();
      return _ardkVersion;
    }

    public override string GetUserId()
    {
      if (!string.IsNullOrWhiteSpace(_userId))
        return _userId;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.UserId, stringBuilder, (ulong)stringBuilder.Capacity);

      _userId = stringBuilder.ToString();
      return _userId;
    }

    public override string GetClientId()
    {
      if (!string.IsNullOrWhiteSpace(_clientId))
        return _clientId;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ClientId, stringBuilder, (ulong)stringBuilder.Capacity);

      _clientId = stringBuilder.ToString();
      return _clientId;
    }

    public override string GetArdkAppInstanceId()
    {
      if (!string.IsNullOrWhiteSpace(_ardkAppInstanceId))
        return _ardkAppInstanceId;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetDataField((uint)_ConfigDataField.ArdkAppInstanceId, stringBuilder, (ulong)stringBuilder.Capacity);

      _ardkAppInstanceId = stringBuilder.ToString();
      return _ardkAppInstanceId;
    }

    public override string GetApiKey()
    {
      if (!string.IsNullOrWhiteSpace(_apiKey))
        return _apiKey;
      
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetApiKey(stringBuilder, (ulong)stringBuilder.Capacity);

      _apiKey = stringBuilder.ToString();
      return _apiKey;
    }

    // get the last good jwt token. since jwt tokens last for a limited time, we cannot cache them
    internal string GetJwtToken()
    {
      var stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetJwtToken(stringBuilder, (ulong)stringBuilder.Capacity);

      var result = stringBuilder.ToString();
      return result;
    }
    
    public override string GetTelemetryKey()
    {
      StringBuilder stringBuilder = new StringBuilder(_NativeCallStringMaxLength);
      _NAR_ARDKGlobalConfigHelper_GetTelemetryKey(stringBuilder, stringBuilder.Capacity);
      
      var key = stringBuilder.ToString();
      return key;
    }


    public override ARClientEnvelope.Types.AgeLevel GetAgeLevel()
    {
      var ageLevel = _NAR_ARDKGlobalConfigHelper_GetAgeLevel();
      return (ARClientEnvelope.Types.AgeLevel)ageLevel;
    }
    
    private void SetUnityVersion(string unityVersion)
    {
      _NAR_ARDKGlobalConfigHelper_SetGameEngineVersion(unityVersion);
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern int _NAR_ARDKGlobalConfigHelper_GetAgeLevel();

    // Switch to using a protobuf to pass data back and forth when that is solidified.
    // This is a bit fragile for now
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelperInternal_SetDataField(uint field, string data);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetApiKey(StringBuilder outKey, ulong maxKeySize);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetDataField
    (
      uint field,
      StringBuilder outData,
      ulong maxDataSize
    );
        
    // Set DBoW URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetDBoWUrl(string url);

    // Get DBoW URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetDBoWUrl
    (
      StringBuilder outUrl,
      ulong maxUrlSize
    );

    // Set ContextAwareness URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetContextAwarenessUrl(string url);

    // Set Api Key
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetApiKey(string key);
    
    // Set Auth URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetAuthURL(string key);

    // Attempt to validate the specified feature, with a previously set Api Key.
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern Int32 _NAR_ARDKGlobalConfigHelper_ValidateApiKeyWithFeature(string feature, bool isAsync);
    
    // Get Auth URL
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetAuthURL
    (
      StringBuilder outKey,
      ulong maxKeySize
    );

    // Get last known jwt token
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetJwtToken(StringBuilder outToken, ulong maxTokenSize);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern bool _NAR_ARDKGlobalConfigHelper_SetUserId(string userId);
    
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_GetTelemetryKey(StringBuilder outKey, int maxSize);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NAR_ARDKGlobalConfigHelper_SetGameEngineVersion(string unityVersion);
    
    // Keep this synchronized with ardk_global_config_helper.hpp
    private enum _ConfigDataField : uint
    {
      ApplicationId = 1,
      Platform,
      Manufacturer,
      DeviceModel,
      UserId,
      ClientId,
      DeveloperId,
      ArdkVersion,
      ArdkAppInstanceId
    }
  }
}

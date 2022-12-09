// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.Configuration
{
  /// Global configuration class.
  /// Allows developers to setup different configuration values during runtime.
  /// This exists such that in a live production environment, you can obtain the configuration
  /// settings remotely and set them before running the rest of the application.
  public static class ArdkGlobalConfig
  {
    internal static event Action _LoginChanged;
    
    private static readonly _ArdkGlobalConfigBase _impl;
    
    static ArdkGlobalConfig()
    {
      // Note: We can create a _NativeFeaturePreloader without setting the AccessMode to Native, 
      // and then the preloader will download from default URLs instead of ones set in the ArdkGlobalConfig.
      // There's currently no important use case where that's relevant though, so leaving the bug as known but unresolved.
      _ImplementationType implementationType = _ImplementationType.Native;

      if (!_ArdkPlatformUtility.AreNativeBinariesAvailable)
        implementationType = _ImplementationType.Placeholder;

      switch (implementationType)
      {
        case _ImplementationType.Native:
          _impl = new _NativeArdkConfig();
          break;

        case _ImplementationType.Placeholder:
        default:
          _impl = new _PlaceholderArdkConfig();
          break;
      }
    }
    
    internal static _ArdkGlobalConfigBase _Internal
    {
      get => _impl; 
    }
    
    // TODO AR-12775: Formally move several public URL set/get api's to private
    /// @note
    ///   This method is deprecated and will be removed in a future update.
    [Obsolete("This method is not supported and will be removed in a future release.")]
    public static bool SetDbowUrl(string url)
    {
      return _impl.SetDbowUrl(url);
    }
    
    // TODO AR-12775: Formally move several public URL set/get api's to private
    /// @note
    ///   This method is deprecated and will be removed in a future update.
    [Obsolete("This method is not supported and will be removed in a future release.")]
    public static string GetDbowUrl()
    {
      return _impl.GetDbowUrl();
    }

    // TODO AR-12775: Formally move several public URL set/get api's to private
    /// @note
    ///   This method is deprecated and will be removed in a future update.
    [Obsolete("This method is not supported and will be removed in a future release.")]
    public static string GetContextAwarenessUrl()
    {
      return _impl.GetContextAwarenessUrl();
    }
    
    public static bool SetContextAwarenessUrl(string url)
    {
      return _impl.SetContextAwarenessUrl(url);
    }

    public static bool SetApiKey(string apiKey)
    {
      return _impl.SetApiKey(apiKey);
    }
    
    // TODO AR-12775: Formally move several public URL set/get api's to private
    /// @note
    ///   This method is deprecated and will be removed in a future update.
    [Obsolete("This method is not supported and will be removed in a future release.")]
    public static string GetAuthenticationUrl()
    {
      return _impl.GetAuthenticationUrl();
    }
    
    /// Returns the clientId - a unique identifier generated for the user/device
    /// in cases where a userId is not provided.
    public static string GetClientId()
    {
      return _Internal.GetClientId();
    }
    
    // TODO AR-12775: Formally move several public URL set/get api's to private
    /// @note
    ///   This method is deprecated and will be removed in a future update.
    [Obsolete("This method is not supported and will be removed in a future release.")]
    public static bool SetAuthenticationUrl(string url)
    {
      return _impl.SetAuthenticationUrl(url);
    }

    // returns the last good jwt token from API key validation
    internal static string _GetJwtToken()
    {
      if (_impl is _NativeArdkConfig nativeConfig)
      {
        var token = nativeConfig.GetJwtToken();
        
        // if it is not populated, maybe no auth attempt has been made yet. make a blocking call here
        //  to attempt to get a token
        if (string.IsNullOrEmpty(token))
        {
          _VerifyApiKeyWithFeature("authentication", isAsync: false);

          token = nativeConfig.GetJwtToken();
        }

        return token;
      }

      return null;
    }

    internal static string _GetTelemetryKey()
    {
      return _impl.GetTelemetryKey();
    }

    /// Set the user id associated with the current user.
    /// We strongly recommend generating and using User IDs. Accurate user information allows
    ///  Niantic to support you in maintaining data privacy best practices and allows you to
    ///  understand usage patterns of features among your users. 
    /// ARDK has no strict format or length requirements for User IDs, although the User ID string
    ///  must be a UTF8 string. We recommend avoiding using an ID that maps back directly to the
    ///  user. So, for example, don’t use email addresses, or login IDs. Instead, you should
    ///  generate a unique ID for each user. We recommend generating a GUID.
    /// @param userId String containing the user id.
    /// @returns True if set properly, false if not
    public static bool SetUserIdOnLogin(string userId)
    {
      if (userId == null)
        userId = string.Empty;
    
      return SetUserId(userId);
    }

    

    /// Clear the user id set by |SetUserIdOnLogin|.
    /// @returns True if the user id is cleared properly, false if not
    public static bool ClearUserIdOnLogout()
    {
      return SetUserId(string.Empty);
    }
 
    private static bool SetUserId(string userId)
    {
      var result = _impl.SetUserIdOnLogin(userId);
      if (result)
      {
        _LoginChanged?.Invoke();
      }

      return result;
    }
    
    internal static NetworkingErrorCode _VerifyApiKeyWithFeature(string feature, bool isAsync = true)
    {
      return _impl.VerifyApiKeyWithFeature(feature, isAsync);
    }

    private enum _ImplementationType
    {
      Native = 0,
      Placeholder,
    }
  }
}

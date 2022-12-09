// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;

using Niantic.ARDK.Networking;

namespace Niantic.ARDK.Configuration
{
  internal interface _IArdkConfig
  {
    
    string GetTelemetryKey();

    /// Set the user id associated with the current user.
    bool SetUserIdOnLogin(string userId);
        
    [Obsolete("This method is not supported and will be removed in a future release.")]
    bool SetDbowUrl(string url);

    [Obsolete("This method is not supported and will be removed in a future release.")]
    string GetDbowUrl();
    
    [Obsolete("This method is not supported and will be removed in a future release.")]
    string GetContextAwarenessUrl();

    // This field needs to be able to take in string.Empty since it is required for a lower level to 
    // setup the correct url
    bool SetContextAwarenessUrl(string url);
    
    bool SetApiKey(string key);

    [Obsolete("This method is not supported and will be removed in a future release.")]
    string GetAuthenticationUrl();

    [Obsolete("This method is not supported and will be removed in a future release.")]
    bool SetAuthenticationUrl(string url);

    NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync = true);
  }
}

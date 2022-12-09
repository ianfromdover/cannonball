using System;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.Networking;

namespace Niantic.ARDK.Configuration
{
  // Wrapper as a common reference for all classes wanting to implement _IArdkConfig and _IArdkMetadataConfig
  internal abstract class _ArdkGlobalConfigBase :
    _IArdkConfig,
    _IArdkMetadataConfig
  {
    
    public abstract string GetTelemetryKey();
    
    public abstract bool SetUserIdOnLogin(string userId);

    [Obsolete("This method is not supported and will be removed in a future release.")]
    public abstract bool SetDbowUrl(string url);

    [Obsolete("This method is not supported and will be removed in a future release.")]
    public abstract string GetDbowUrl();

    [Obsolete("This method is not supported and will be removed in a future release.")]
    public abstract string GetContextAwarenessUrl();

    public abstract bool SetContextAwarenessUrl(string url);

    public abstract bool SetApiKey(string key);

    [Obsolete("This method is not supported and will be removed in a future release.")]
    public abstract string GetAuthenticationUrl();

    [Obsolete("This method is not supported and will be removed in a future release.")]
    public abstract bool SetAuthenticationUrl(string url);

    public abstract NetworkingErrorCode VerifyApiKeyWithFeature(string feature, bool isAsync = true);

    public abstract void SetApplicationId(string bundleId);

    public abstract void SetArdkInstanceId(string instanceId);

    public abstract string GetApplicationId();

    public abstract string GetPlatform();

    public abstract string GetManufacturer();

    public abstract string GetDeviceModel();

    public abstract string GetArdkVersion();

    public abstract string GetUserId();

    public abstract string GetClientId();

    public abstract string GetArdkAppInstanceId();

    public abstract string GetApiKey();
    
    public abstract ARClientEnvelope.Types.AgeLevel GetAgeLevel();
  }
}

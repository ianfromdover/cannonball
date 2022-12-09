// Copyright 2022 Niantic, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Text;

using Google.Protobuf;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.ARDK.Configuration.Internal
{
  internal static class _ArdkMetadataConfigExtension
  {
    public const string AuthorizationHeaderKey = "Authorization";
    public const string ClientIdHeaderKey = "x-ardk-clientid";
    public const string UserIdHeaderKey = "x-ardk-userid";
    public const string ArClientEnvelopeKey = "x-ardk-clientenvelope";
    public const string ApplicationIdKey = "x-ardk-application-id";
    public const string FallbackArAgeLevelKey = "x-ardk-agelevel";

    /// Get the common data envelope (client metadata protobuf) serialized into a Json string
    internal static string GetCommonDataEnvelopeAsJson(this _IArdkMetadataConfig metadataConfig)
    {
      // Cannot apply a null value to protobufs, so we need the null checks.
      var proto = new ARCommonMetadata();
      PopulateProtoFields(proto, metadataConfig);
      var protoAsJson = JsonFormatter.Default.Format(proto);
      
      return protoAsJson;
    }
    
    /// Get the common data envelope (client metadata protobuf) serialized into a Json string
    /// Additionally populates the request_id field with a randomly generated UUID
    internal static string GetCommonDataEnvelopeWithRequestIdAsJson(this _IArdkMetadataConfig metadataConfig)
    {
      var proto = new ARCommonMetadata();
      PopulateProtoFields(proto, metadataConfig);

      proto.RequestId = _CreateFormattedGuid();
      var protoAsJson = JsonFormatter.Default.Format(proto);

      return protoAsJson;
    }
    
    internal static ARCommonMetadataStruct GetCommonDataEnvelopeWithRequestIdAsStruct(this _IArdkMetadataConfig metadataConfig)
    {
      var metadata = new ARCommonMetadataStruct
      (
        metadataConfig.GetApplicationId(),
        metadataConfig.GetPlatform(),
        metadataConfig.GetManufacturer(),
        metadataConfig.GetDeviceModel(),
        metadataConfig.GetUserId(),
        metadataConfig.GetClientId(),
        metadataConfig.GetArdkVersion(),
        metadataConfig.GetArdkAppInstanceId(),
        _CreateFormattedGuid()
      );

      return metadata;
    }

    // has to be public because of the test scenes. 
    public static Dictionary<string, string> GetApiGatewayHeaders(this _IArdkMetadataConfig metadataConfig)
    {
      Dictionary<string, string> header = new Dictionary<string, string>();

      header.Add(AuthorizationHeaderKey, metadataConfig.GetApiKey());
      header.Add(ClientIdHeaderKey, metadataConfig.GetClientId()); 
      header.Add(UserIdHeaderKey, metadataConfig.GetUserId());
      header.Add(ApplicationIdKey, metadataConfig.GetApplicationId());
      var encodedString = EncodeAsBase64(metadataConfig.ConstructClientEnvelopeStructAsJson());
      header.Add(ArClientEnvelopeKey, encodedString);
      header.Add(FallbackArAgeLevelKey, metadataConfig.GetAgeLevel().ToString().ToUpper());

      return header;
    }
    
    // Formats as a lower case hex string without "0x" (ie: 0123456789abcdef0123456789abcdef)
    // This is the format used by internal telemetry tooling in the data platform (see AR-10926).
    internal static string _CreateFormattedGuid()
    {
      var guid = Guid.NewGuid();
      return $"{guid.ToString("N").ToLower()}";
    }

    private static string EncodeAsBase64(string stringToEncode)
    {
      byte[] stringToEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(stringToEncode);
      return Convert.ToBase64String(stringToEncodeAsBytes);
    }

    private static string ConstructClientEnvelopeStructAsJson(this _IArdkMetadataConfig metadataConfig)
    {
      ARClientEnvelopeStruct clientEnvelope = new ARClientEnvelopeStruct(metadataConfig.GetAgeLevel());

      return JsonUtility.ToJson(clientEnvelope);
    }

    private static string ConstructClientEnvelopeProtoAsJson(this _IArdkMetadataConfig metadataConfig)
    {
      ARClientEnvelope clientEnvelope = new ARClientEnvelope
      {
        AgeLevel = metadataConfig.GetAgeLevel(),
      };

      return JsonFormatter.Default.Format(clientEnvelope);
    }

    private static void PopulateProtoFields(ARCommonMetadata proto, _IArdkMetadataConfig metadataConfig)
    {
      var manufacturer = metadataConfig.GetManufacturer();
      if (!string.IsNullOrEmpty(manufacturer))
        proto.Manufacturer = manufacturer;

      var appId = metadataConfig.GetApplicationId();
      if (!string.IsNullOrEmpty(appId))
        proto.ApplicationId = appId;

      var appInstanceId = metadataConfig.GetArdkAppInstanceId();
      if (!string.IsNullOrEmpty(appInstanceId))
        proto.ArdkAppInstanceId = appInstanceId;

      var ardkVersion = metadataConfig.GetArdkVersion();
      if (!string.IsNullOrEmpty(ardkVersion))
        proto.ArdkVersion = ardkVersion;

      var clientId = metadataConfig.GetClientId();
      if (!string.IsNullOrEmpty(clientId))
        proto.ClientId = clientId;

      var deviceModel = metadataConfig.GetDeviceModel();
      if (!string.IsNullOrEmpty(deviceModel))
        proto.DeviceModel = deviceModel;

      var platform = metadataConfig.GetPlatform();
      if (!string.IsNullOrEmpty(platform))
        proto.Platform = platform;

      var userId = metadataConfig.GetUserId();
      if (!string.IsNullOrEmpty(userId))
        proto.UserId = userId;
    }
  }

  [Serializable]
  internal struct ARClientEnvelopeStruct
  {
    public string age_level;

    public ARClientEnvelopeStruct(ARClientEnvelope.Types.AgeLevel ageLevel)
    {
      age_level = ageLevel.ToString().ToUpper();
    }
  }

  [Serializable]    
  internal struct ARCommonMetadataStruct 
  {
    public string application_id; 
    public string platform; 
    public string manufacturer; 
    public string device_model; 
    public string user_id; 
    public string client_id; 
    public string ardk_version;
    public string ardk_app_instance_id;
    public string request_id; 

    public ARCommonMetadataStruct
    (
      string applicationID,
      string platform,
      string manufacturer,
      string deviceModel,
      string userID,
      string clientID,
      string ardkVersion,
      string ardkAppInstanceID,
      string requestID
    )
    {
      application_id = applicationID;
      this.platform = platform;
      this.manufacturer = manufacturer;
      device_model = deviceModel;
      user_id = userID;
      client_id = clientID;
      ardk_version = ardkVersion;
      ardk_app_instance_id = ardkAppInstanceID;
      request_id = requestID;
    }
  }
}

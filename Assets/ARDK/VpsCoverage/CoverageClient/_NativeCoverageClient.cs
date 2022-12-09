// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Threading.Tasks;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Configuration;
using Niantic.ARDK.Configuration.Internal;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Telemetry;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;
using Niantic.ARDK.VPSCoverage.GeoserviceMessages;

using UnityEngine;

using LocationInfo = Niantic.ARDK.LocationService.LocationInfo;
using LocationServiceStatus = UnityEngine.LocationServiceStatus;


namespace Niantic.ARDK.VPSCoverage
{
  internal class _NativeCoverageClient: ICoverageClient
  {
    private const string VpsCoverageEndpoint = "https://vps-coverage-api.nianticlabs.com/api/json/v1/";
    private const string CoverageAreasMethodName = "GET_VPS_COVERAGE";
    private const string LocalizationTargetMethodName = "GET_VPS_LOCALIZATION_TARGETS";
    private const string CoverageAreasEndpoint = VpsCoverageEndpoint + CoverageAreasMethodName;
    private const string LocalizationTargetsEndpoint = VpsCoverageEndpoint + LocalizationTargetMethodName;

    internal _NativeCoverageClient()
    {
      _FriendTypeAsserter.AssertCallerIs(typeof(CoverageClientFactory));
    }

    public async Task<CoverageAreasResult> RequestCoverageAreasAsync(LatLng queryLocation, int queryRadius)
    {
      _CoverageAreasRequest request;

      // Server side we use radius == 0 then use max radius, radius < 0 then set radius to 0.
      // Client side we want a to use radius == 0 then radius = 0, radius < 0 then use max radius.
      if (queryRadius == 0)
        queryRadius = -1;
      else if (queryRadius < 0)
        queryRadius = 0;

      var metadata = ArdkGlobalConfig._Internal.GetCommonDataEnvelopeWithRequestIdAsStruct();
      var requestHeaders =  ArdkGlobalConfig._Internal.GetApiGatewayHeaders();

      ARLog._Debug(JsonUtility.ToJson(metadata, true));
      
      if (Input.location.status == LocationServiceStatus.Running)
      {
         int distanceToQuery = (int)queryLocation.Distance(new LatLng(Input.location.lastData));
         request = new _CoverageAreasRequest(queryLocation, queryRadius, distanceToQuery, metadata);
      }
      else
      {
        request = new _CoverageAreasRequest(queryLocation, queryRadius, metadata);
      }

      ReportRequestToTelemetry(CoverageAreasMethodName, request.ar_common_metadata.request_id);

      _HttpResponse<_CoverageAreasResponse> response =
        await _HttpClient.SendPostAsync<_CoverageAreasRequest, _CoverageAreasResponse>
        (
          CoverageAreasEndpoint,
          request,
          requestHeaders
        );

      if (response.Status == ResponseStatus.Success)
      {
        response.Status = _ResponseStatusTranslator.FromString(response.Data.status);
      }
      ReportResponseToTelemetry(CoverageAreasMethodName, request.ar_common_metadata.request_id, response.HttpStatusCode, response.Status);

      CoverageAreasResult result = new CoverageAreasResult(response);

      return result;
    }

    public async Task<CoverageAreasResult> RequestCoverageAreasAsync(LocationInfo queryLocation, int queryRadius)
    {
      return await RequestCoverageAreasAsync(new LatLng(queryLocation), queryRadius);
    }

    public async Task<LocalizationTargetsResult> RequestLocalizationTargetsAsync(string[] targetIdentifiers)
    {
      var metadata = ArdkGlobalConfig._Internal.GetCommonDataEnvelopeWithRequestIdAsStruct();
      var requestHeaders =  ArdkGlobalConfig._Internal.GetApiGatewayHeaders();
      
      ARLog._Debug(JsonUtility.ToJson(metadata, true));

      _LocalizationTargetsRequest request = new _LocalizationTargetsRequest(targetIdentifiers, metadata);
      ReportRequestToTelemetry(LocalizationTargetMethodName, request.ar_common_metadata.request_id);
      
      _HttpResponse<_LocalizationTargetsResponse> response =
        await _HttpClient.SendPostAsync<_LocalizationTargetsRequest, _LocalizationTargetsResponse>
        (
          LocalizationTargetsEndpoint,
          request,
          requestHeaders
        );

      if (response.Status == ResponseStatus.Success)
        response.Status = _ResponseStatusTranslator.FromString(response.Data.status);
      ReportResponseToTelemetry(LocalizationTargetMethodName, request.ar_common_metadata.request_id, response.HttpStatusCode, response.Status);
      
      LocalizationTargetsResult result = new LocalizationTargetsResult(response);

      return result;
    }

    public async void RequestCoverageAreas(LocationInfo queryLocation, int queryRadius, Action<CoverageAreasResult> onAreasReceived)
    {
      CoverageAreasResult result = await RequestCoverageAreasAsync(queryLocation, queryRadius);
      onAreasReceived?.Invoke(result);
    }

    public async void RequestCoverageAreas(LatLng queryLocation, int queryRadius, Action<CoverageAreasResult> onAreasReceived)
    {
      CoverageAreasResult result = await RequestCoverageAreasAsync(queryLocation, queryRadius);
      onAreasReceived?.Invoke(result);
    }

    public async void RequestLocalizationTargets(string[] targetIdentifiers, Action<LocalizationTargetsResult> onTargetsReceived)
    {
      LocalizationTargetsResult result = await RequestLocalizationTargetsAsync(targetIdentifiers);
      onTargetsReceived?.Invoke(result);
    }
    
    private void ReportRequestToTelemetry(string methodName, string requestId)
    {
      try
      {
        _TelemetryService.RecordEvent
        (
          new LightshipServiceEvent()
          {
            IsRequest = true, ApiMethodName = methodName,
          },
          requestId
        );
      }
      catch (Exception e)
      {
        ARLog._Debug($"Error logging vps coverage request {e}");
      }
    }
    
    private void ReportResponseToTelemetry(string methodName, string requestId, long httpStatus, ResponseStatus responseStatus)
    {
      try
      {
        bool isSuccess = false;
        if (httpStatus >= 200 && httpStatus < 300)
          isSuccess = true;
        
        _TelemetryService.RecordEvent(
          new LightshipServiceEvent()
          {
            IsRequest = false,
            ApiMethodName = methodName,
            Success = isSuccess,
            Response = responseStatus.ToString(),
            HttpStatus = httpStatus.ToString(),
          },
          requestId);
      }
      catch (Exception e)
      {
        ARLog._Debug($"Error logging vps coverage response: {e}");
      }
    }
  }
}

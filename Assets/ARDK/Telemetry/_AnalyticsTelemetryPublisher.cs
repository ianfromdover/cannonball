using System;

using Niantic.ARDK.AR.Protobuf;
using Niantic.ARDK.Utilities.Logging;
using Niantic.Platform.Analytics.Telemetry;
using Niantic.Platform.Analytics.Telemetry.Logging;

namespace Niantic.ARDK.Telemetry
{
  internal class _AnalyticsTelemetryPublisher : 
    _ITelemetryPublisher
  {
    private readonly ARDKTelemetryService<ARDKTelemetryOmniProto> _ardkPublisher;
    private const string _NonNativeTelemetryKey = "be3c4f1e-5206-405b-a1c3-046e2883a3f4"; 
    
    public _AnalyticsTelemetryPublisher(string directoryPath, string key, bool registerLogger)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        key = _NonNativeTelemetryKey;
      }

      var builder = new ARDKTelemetryService<ARDKTelemetryOmniProto>.Builder
      (
        ARDKTelemetryService<ARDKTelemetryOmniProto>.AnalyticsEnvironment.ANALYTICS_REL, 
        directoryPath, 
        key
      );

      if (registerLogger)
      {
        var debugOptions = new StartupDebugOptions();
        debugOptions.LogHandler = new _TelemetryLogger();
        debugOptions.LogOptions = LogOptions.All;
        
        builder.SetDebugOptions(debugOptions);
        ARLog._Debug("Registering logger for telemetry.");
      }
      
      _ardkPublisher = builder.Build();
      ARLog._Debug("Successfully created the ardk publisher.");
    }

    public void RecordEvent(ARDKTelemetryOmniProto telemetryEvent)
    {
      try
      {
        _ardkPublisher.RecordEvent(telemetryEvent);
      }
      catch(Exception ex)
      {
        ARLog._Warn($"Posting telemetry failed with the following exception: {ex}");
      }
    }

    private class _TelemetryLogger : ILogHandler
    {
      public void LogMessage(LogLevel logLevel, string message)
      {
        switch (logLevel)
        {
          case LogLevel.Verbose: 
          case LogLevel.Info: 

            ARLog._Debug(message);
            break;
        
          case LogLevel.Warning:
            ARLog._Warn(message);
            break;
          
          case LogLevel.Error: 
          case LogLevel.Fatal:

            ARLog._Error(message);
            break;
        
          default:
            ARLog._Debug(message);
            break;
        }
      }
    }
  }
}

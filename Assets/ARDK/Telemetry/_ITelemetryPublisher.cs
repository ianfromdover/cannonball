using Niantic.ARDK.AR.Protobuf;

namespace Niantic.ARDK.Telemetry
{
  internal interface _ITelemetryPublisher
  {
    public void RecordEvent(ARDKTelemetryOmniProto telemetryEvent);
  }
}

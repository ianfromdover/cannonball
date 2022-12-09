using Niantic.ARDK.AR.Protobuf;

namespace Niantic.ARDK.Telemetry
{
  internal class _DummyTelemetryPublisher : 
    _ITelemetryPublisher
  {
    public void RecordEvent(ARDKTelemetryOmniProto telemetryEvent)
    {
      // do nothing
    }
  }
}

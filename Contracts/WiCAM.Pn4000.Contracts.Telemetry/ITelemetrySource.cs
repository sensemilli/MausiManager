namespace WiCAM.Pn4000.Contracts.Telemetry;

public interface ITelemetrySource
{
	ITelemetryActivity StartActivity(string name);

	ITelemetryActivity ForceNewActivity(string name);
}

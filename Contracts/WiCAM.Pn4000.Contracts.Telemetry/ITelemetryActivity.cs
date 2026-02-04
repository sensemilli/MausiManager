using System;
using System.Diagnostics;

namespace WiCAM.Pn4000.Contracts.Telemetry;

public interface ITelemetryActivity : IDisposable
{
	void SetStatus(ActivityStatusCode code, string description);

	void Stop();
}

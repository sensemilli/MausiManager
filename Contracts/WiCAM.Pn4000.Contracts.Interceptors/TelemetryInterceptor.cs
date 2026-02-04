using Castle.DynamicProxy;
using WiCAM.Pn4000.Contracts.Telemetry;

namespace WiCAM.Pn4000.Contracts.Interceptors;

public class TelemetryInterceptor : IInterceptor
{
    private readonly ITelemetrySource _telemetrySource;

    public TelemetryInterceptor(ITelemetrySource telemetrySource)
    {
        _telemetrySource = telemetrySource;
    }

    public void Intercept(IInvocation invocation)
    {
        using (_telemetrySource.StartActivity(invocation.TargetType.Name + " - " + invocation.Method.Name + "()"))
        {
            invocation.Proceed();
        }
    }
}

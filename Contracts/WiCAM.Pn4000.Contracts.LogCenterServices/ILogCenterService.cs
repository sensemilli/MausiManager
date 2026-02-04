using System;

namespace WiCAM.Pn4000.Contracts.LogCenterServices;

public interface ILogCenterService
{
	event Action<string> OnError;

	event Action<string> OnWarning;

	event Action OnResetStatus;

	void CatchRaport(Exception e);

	void Debug(string info);

	void DispatcherUnhandledException(Exception e);

	void Init();

	bool IsCritivalError();

	void RaportUnhandledException(Exception e);

	void RaportUnobservedTaskException(AggregateException e);

	void SetDebugWindow(IPnDebug pnDebug);

	void SetErrorLamp();
}

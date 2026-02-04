using System;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.LogDataFlow;

public class SimpleLogCenterService : ILogCenterService
{
	public event Action<string> OnError;

	public event Action<string> OnWarning;

	public event Action OnResetStatus;

	public void CatchRaport(Exception e)
	{
	}

	public void Debug(string info)
	{
	}

	public void DispatcherUnhandledException(Exception e)
	{
	}

	public void Init()
	{
	}

	public bool IsCritivalError()
	{
		return false;
	}

	public void RaportUnhandledException(Exception e)
	{
	}

	public void RaportUnobservedTaskException(AggregateException e)
	{
	}

	public void SetDebugWindow(IPnDebug pnDebug)
	{
	}

	public void SetErrorLamp()
	{
	}

	public void SetErrorPopup(IErrorPopup errorPopup)
	{
	}
}

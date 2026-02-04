#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.LogCenterServices.Enum;
using WiCAM.Pn4000.pn4.LogDataFlow;
using WiCAM.Services.Loggers;
using WiCAM.Services.Loggers.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class LogCenterService : ILogCenterService
{
	private IPnDebug pnDebug;

	private PnUsersErrorRaports pnUsersErrorRaports;

	private ErrorLevel lastErrorLevel;

	private IWiLogger _logger;

	private string _lastFile;

	public event Action<string> OnError;

	public event Action<string> OnWarning;

	public event Action OnResetStatus;

	private void VSTrace(string label, string message)
	{
		Trace.WriteLine($"{label}: {message}");
	}

	private void VSTrace(string label, string message, string filePath)
	{
		if (filePath != string.Empty)
		{
			Trace.WriteLine(string.Format("{2}: {0} Raport at {1}", message, filePath, label));
		}
		else
		{
			Trace.WriteLine("PNcatch identical to the previous one.");
		}
	}

	public void Init()
	{
		this.pnUsersErrorRaports = new PnUsersErrorRaports();
		this.pnUsersErrorRaports.ClearHistory();
		this._logger = new WiLogger("*");
	}

	public bool IsCritivalError()
	{
		return false;
	}

	public void CatchRaport(Exception e)
	{
		string text = this.pnUsersErrorRaports.ErrorRaport("CatchRaport", ErrorLevel.Warning, e);
		this.VSTrace("PnCath", e.Message, text);
		this.lastErrorLevel = ErrorLevel.Warning;
		this._lastFile = text;
		this.OnWarning?.Invoke(text);
	}

	public void RaportUnhandledException(Exception e)
	{
		string text = this.pnUsersErrorRaports.ErrorRaport("UnhandledException", ErrorLevel.Error, e);
		this.VSTrace("PnUnhE", string.Empty, text);
		this.lastErrorLevel = ErrorLevel.Error;
		this._lastFile = text;
		this.OnError?.Invoke(text);
	}

	public void RaportUnobservedTaskException(AggregateException e)
	{
		string text = this.pnUsersErrorRaports.ErrorRaport("UnobservedTaskException", ErrorLevel.Error, e);
		string text2 = e.Message;
		using (IEnumerator<Exception> enumerator = e.InnerExceptions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				text2 = string.Concat(str2: enumerator.Current.Message, str0: text2, str1: Environment.NewLine);
			}
		}
		this.VSTrace("PnUnhE", text2, text);
		this.lastErrorLevel = ErrorLevel.Error;
		this._lastFile = text;
		this.OnError?.Invoke(text);
	}

	public void DispatcherUnhandledException(Exception e)
	{
		string text = this.pnUsersErrorRaports.ErrorRaport("DispatcherUnhandledException", ErrorLevel.Error, e);
		this.VSTrace("PnUnhE", e.Message, text);
		this.lastErrorLevel = ErrorLevel.Error;
		this._lastFile = text;
		this.OnError?.Invoke(text);
	}

	public void Debug(string info)
	{
		this.VSTrace("PnDeb", info);
		this.pnDebug?.DebugThat(info);
		if (info != string.Empty)
		{
			this._logger?.Debug(info);
		}
	}

	public void SetDebugWindow(IPnDebug pnDebug)
	{
		this.pnDebug = pnDebug;
	}

	public void SetErrorLamp()
	{
		if (this.lastErrorLevel == ErrorLevel.NotDefine)
		{
			this.OnResetStatus?.Invoke();
		}
		else
		{
			this.OnError?.Invoke(this._lastFile);
		}
	}
}

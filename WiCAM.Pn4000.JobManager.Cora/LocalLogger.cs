using System;
using System.Globalization;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class LocalLogger : ILogger
{
	public void Info(string message)
	{
		Logger.Info(message);
	}

	public void Verbose(string message)
	{
		Logger.Verbose(message);
	}

	public void Error(string message)
	{
		Logger.Error(message);
	}

	public void Warning(string message)
	{
		Logger.Warning(message);
	}

	public void Error(string format, params object[] parameters)
	{
		Error(string.Format(CultureInfo.InvariantCulture, format, parameters));
	}

	public void Info(string format, params object[] parameters)
	{
		Info(string.Format(CultureInfo.InvariantCulture, format, parameters));
	}

	public void Verbose(string format, params object[] parameters)
	{
		Verbose(string.Format(CultureInfo.InvariantCulture, format, parameters));
	}

	public void Warning(string format, params object[] parameters)
	{
		Warning(string.Format(CultureInfo.InvariantCulture, format, parameters));
	}

	public void Indent()
	{
	}

	public void Unindent()
	{
	}

	public void Exception(Exception ex)
	{
		Logger.Error(ex.Message);
		Logger.Error(ex.StackTrace);
	}
}

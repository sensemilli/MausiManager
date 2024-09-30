using System;
using System.Collections.Generic;
using System.Globalization;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class BufferedLogger : IBufferedLogger, ILogger
{
	private enum LogLevel
	{
		Error,
		Info,
		Warning,
		Verbose
	}

	private class SavedMessage
	{
		public LogLevel Level { get; set; }

		public string Message { get; set; }
	}

	private readonly List<SavedMessage> _messages = new List<SavedMessage>();

	public void Error(string message)
	{
		_messages.Add(new SavedMessage
		{
			Level = LogLevel.Error,
			Message = message
		});
	}

	public void Info(string message)
	{
		_messages.Add(new SavedMessage
		{
			Level = LogLevel.Info,
			Message = message
		});
	}

	public void Verbose(string message)
	{
		_messages.Add(new SavedMessage
		{
			Level = LogLevel.Verbose,
			Message = message
		});
	}

	public void Warning(string message)
	{
		_messages.Add(new SavedMessage
		{
			Level = LogLevel.Warning,
			Message = message
		});
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

	public void Write(ILogger logger)
	{
		foreach (SavedMessage message in _messages)
		{
			switch (message.Level)
			{
			case LogLevel.Error:
				logger.Error(message.Message);
				break;
			case LogLevel.Warning:
				logger.Warning(message.Message);
				break;
			case LogLevel.Info:
				logger.Info(message.Message);
				break;
			case LogLevel.Verbose:
				logger.Verbose(message.Message);
				break;
			}
		}
	}

	public void Exception(Exception exception)
	{
		Error(exception.Message);
		Error(exception.StackTrace);
	}

	public void Indent()
	{
	}

	public void Unindent()
	{
	}
}

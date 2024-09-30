using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace WiCAM.Pn4000.JobManager.Helpers;

internal class OutputContentBuilder
{
	private readonly StringBuilder _buffer;

	private readonly Dictionary<Type, Action<string, object>> _actions = new Dictionary<Type, Action<string, object>>();

	public OutputContentBuilder(StringBuilder buffer)
	{
		_buffer = buffer;
		_actions.Add(typeof(double), AddFloatingPoint);
		_actions.Add(typeof(int), AddInteger);
		_actions.Add(typeof(short), AddInteger);
		_actions.Add(typeof(DateTime), AddDateTime);
		_actions.Add(typeof(string), AddString);
	}

	public void AddValue(string key, PropertyInfo property, object item, Type propertyType)
	{
		object value = property.GetValue(item);
		if (_actions.ContainsKey(propertyType))
		{
			_actions[propertyType](key, value);
		}
	}

	private void AddFloatingPoint(string key, object value)
	{
		if (value == null)
		{
			value = 0.0;
		}
		_buffer.Replace(key, string.Format(CultureInfo.InvariantCulture, "{0:0.0000}", (double)value));
	}

	private void AddInteger(string key, object value)
	{
		if (value == null)
		{
			value = 0;
		}
		_buffer.Replace(key, value.ToString());
	}

	private void AddDateTime(string key, object value)
	{
		if (value == null)
		{
			value = DateTime.Today;
		}
		_buffer.Replace(key, string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd}", (DateTime)value));
	}

	private void AddString(string key, object value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		_buffer.Replace(key, value.ToString());
	}
}

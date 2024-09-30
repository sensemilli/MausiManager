using System;
using System.Globalization;
using System.Text;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Helpers;

internal class DatLineManager
{
	public void ReplaceNotExistingLine(StringBuilder sb, string key, string newValue)
	{
		string text = sb.ToString();
		string line = StringHelper.GetLine(text, key, 0);
		if (!string.IsNullOrEmpty(line))
		{
			line = line.Trim();
			int startIndex = text.IndexOf(line, 1, StringComparison.CurrentCultureIgnoreCase);
			string newValue2 = BuildDatLine(key, newValue);
			sb.Replace(line, newValue2, startIndex, line.Length + 5);
		}
		else
		{
			string value = BuildDatLine(key, newValue) + Environment.NewLine;
			sb.Insert(0, value);
		}
	}

	public void ReplaceLine(StringBuilder sb, string key, int begin, double newValue)
	{
		string text = sb.ToString();
		string line = StringHelper.GetLine(text, key, begin);
		if (!string.IsNullOrEmpty(line))
		{
			line = line.Trim();
			int startIndex = text.IndexOf(line, begin + 1, StringComparison.CurrentCultureIgnoreCase);
			string newValue2 = BuildDatLine(key, newValue.ToString("0.00000", CultureInfo.InvariantCulture));
			sb.Replace(line, newValue2, startIndex, line.Length + 5);
		}
	}

	public void ReplaceLine(StringBuilder sb, string key, int begin, int newValue)
	{
		string text = sb.ToString();
		string line = StringHelper.GetLine(text, key, begin);
		if (!string.IsNullOrEmpty(line))
		{
			line = line.Trim();
			int startIndex = text.IndexOf(line, begin + 1, StringComparison.CurrentCultureIgnoreCase);
			string newValue2 = BuildDatLine(key, newValue.ToString());
			sb.Replace(line, newValue2, startIndex, line.Length + 5);
		}
	}

	public string BuildDatLine(string key, string value)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0,-20} = {1}", key, value);
	}
}

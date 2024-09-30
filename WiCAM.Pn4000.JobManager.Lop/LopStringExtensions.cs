using System;

namespace WiCAM.Pn4000.JobManager.Lop;

internal static class LopStringExtensions
{
	public static bool ContainsString(this string value, string search)
	{
		return value.Contains(search, StringComparison.OrdinalIgnoreCase);
	}

	public static string ReadValue(this string input, string delimiter = "=")
	{
		int num = input.IndexOf(delimiter, StringComparison.OrdinalIgnoreCase);
		if (num > -1)
		{
			return input.Substring(num + 1).Trim();
		}
		return string.Empty;
	}

	public static bool IsComment(this string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		return value[0] == '#';
	}
}

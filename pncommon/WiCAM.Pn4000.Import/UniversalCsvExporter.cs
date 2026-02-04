using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WiCAM.Pn4000.Import;

public static class UniversalCsvExporter
{
	public static void ExportCsv<T>(string targetPath, string separator, IEnumerable<T> objectlist)
	{
		File.WriteAllText(targetPath, UniversalCsvExporter.ToCsv(separator, objectlist));
	}

	private static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
	{
		PropertyInfo[] properties = typeof(T).GetProperties();
		string value = string.Join(separator, properties.Select((PropertyInfo f) => f.Name).ToArray());
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(value);
		foreach (T item in objectlist)
		{
			stringBuilder.AppendLine(UniversalCsvExporter.ToCsvFields(separator, properties, item));
		}
		return stringBuilder.ToString();
	}

	private static string ToCsvFields(string separator, PropertyInfo[] fields, object o)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PropertyInfo obj in fields)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(separator);
			}
			object value = obj.GetValue(o);
			if (value != null)
			{
				stringBuilder.Append(value.ToString());
			}
		}
		return stringBuilder.ToString();
	}
}

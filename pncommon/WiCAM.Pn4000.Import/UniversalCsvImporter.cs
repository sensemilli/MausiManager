using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace WiCAM.Pn4000.Import;

public static class UniversalCsvImporter
{
	public static List<T> ImportCSV<T>(string filePath, Dictionary<string, string> shema)
	{
		List<T> list = new List<T>();
		if (File.Exists(filePath))
		{
			using StreamReader streamReader = new StreamReader(filePath);
			int num = 0;
			List<PropertyInfo> list2 = null;
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!(text.Trim() != string.Empty))
				{
					continue;
				}
				if (num == 0)
				{
					string[] columnsFromFirstLine = UniversalCsvImporter.GetColumnsFromFirstLine(text);
					PropertyInfo[] properties = typeof(T).GetProperties();
					list2 = new List<PropertyInfo>();
					string[] array = columnsFromFirstLine;
					foreach (string key in array)
					{
						if (shema.ContainsKey(key))
						{
							list2.Add(UniversalCsvImporter.GetProperty(shema[key], properties));
						}
						else
						{
							list2.Add(null);
						}
					}
				}
				else
				{
					list.Add(UniversalCsvImporter.GetObject<T>(text, list2));
				}
				num++;
			}
			streamReader.Close();
		}
		return list;
	}

	private static T GetObject<T>(string ln, List<PropertyInfo> columnProperty)
	{
		T val = (T)Activator.CreateInstance(typeof(T));
		string[] array = ln.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			PropertyInfo propertyInfo = columnProperty[i];
			if (propertyInfo != null)
			{
				string text = array[i];
				if (propertyInfo.PropertyType == typeof(string))
				{
					propertyInfo.SetValue(val, text);
				}
				if (propertyInfo.PropertyType == typeof(int))
				{
					int result = 0;
					int.TryParse(text, out result);
					propertyInfo.SetValue(val, result);
				}
				if (propertyInfo.PropertyType == typeof(double))
				{
					double result2 = 0.0;
					double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result2);
					propertyInfo.SetValue(val, result2);
				}
			}
		}
		return val;
	}

	private static PropertyInfo GetProperty(string v, PropertyInfo[] properties)
	{
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.Name == v)
			{
				return propertyInfo;
			}
		}
		return null;
	}

	private static string[] GetColumnsFromFirstLine(string ln)
	{
		return ln.Split(';');
	}
}

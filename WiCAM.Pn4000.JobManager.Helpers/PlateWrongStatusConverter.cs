using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.JobManager.Helpers;

public class PlateWrongStatusConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return string.Empty;
		}
		if (parameter == null)
		{
			return string.Empty;
		}
		string format = BuildShortFormat(parameter.ToString());
		return string.Format(CultureInfo.InvariantCulture, format, value.ToString());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public static string BuildShortFormat(string formatString)
	{
		string[] array = formatString.Split((Environment.NewLine + "|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 2)
		{
			return string.Join("   ", array[0], array[1]);
		}
		return array[0];
	}
}

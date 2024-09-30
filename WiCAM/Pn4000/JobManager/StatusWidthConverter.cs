using System;
using System.Globalization;
using System.Windows.Data;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(double), typeof(double))]
public class StatusWidthConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values != null)
		{
			try
			{
				double num = (double)values[0];
				return (double)values[1] * num;
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
		}
		return 0.0;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

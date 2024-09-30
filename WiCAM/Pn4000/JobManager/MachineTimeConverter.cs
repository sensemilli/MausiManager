using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(double), typeof(string))]
public class MachineTimeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 60.0;
		if (value != null)
		{
			double num2 = (double)value;
			int num3 = (int)(num2 / num);
			int num4 = (int)(num2 % num);
			return string.Format(CultureInfo.CurrentCulture, "{0}:{1:00}", num3, num4);
		}
		return "0";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

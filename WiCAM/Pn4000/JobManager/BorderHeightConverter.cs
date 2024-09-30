using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(double), typeof(double))]
public class BorderHeightConverter : IValueConverter
{
	private double _factor = 0.75;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = (double)value;
		if (!double.IsInfinity(num) && !double.IsNaN(num))
		{
			return num * _factor;
		}
		return num;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

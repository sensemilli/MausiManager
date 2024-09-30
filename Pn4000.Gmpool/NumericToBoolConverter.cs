using System;
using System.Globalization;
using System.Windows.Data;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Gmpool
{
	public class NumericToBoolConverter : IValueConverter
	{
		public NumericToBoolConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!ValueTypeHelper.IsNumericType(value.GetType()))
			{
				return false;
			}
			int num = 0;
			if (value is double)
			{
				num = System.Convert.ToInt32(value);
			}
			else if (value is int)
			{
				num = System.Convert.ToInt32(value);
			}
			return num > 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool && (bool)value)
			{
				if (targetType == typeof(double))
				{
					return 1;
				}
				return 1;
			}
			if (targetType == typeof(double))
			{
				return 0;
			}
			return 0;
		}
	}
}
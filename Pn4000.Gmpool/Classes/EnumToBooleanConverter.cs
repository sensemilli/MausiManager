using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	[ValueConversion(typeof(RestPlateFormType), typeof(bool))]
	public class EnumToBooleanConverter : IValueConverter
	{
		public EnumToBooleanConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(bool)value)
			{
				return Binding.DoNothing;
			}
			return parameter;
		}
	}
}
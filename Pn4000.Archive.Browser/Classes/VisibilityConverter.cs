using SmartAssembly.Attributes;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	[DoNotObfuscateType]
	[DoNotPruneType]
	[ValueConversion(typeof(Visibility), typeof(bool))]
	public class VisibilityConverter : IValueConverter
	{
		public VisibilityConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is Visibility && (Visibility)value == Visibility.Visible)
			{
				return true;
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool && (bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Hidden;
		}
	}
}
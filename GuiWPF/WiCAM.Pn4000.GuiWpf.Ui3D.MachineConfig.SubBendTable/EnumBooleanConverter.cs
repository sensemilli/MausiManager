using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public class EnumBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(parameter is string value2))
		{
			return DependencyProperty.UnsetValue;
		}
		if (!System.Enum.IsDefined(value.GetType(), value))
		{
			return DependencyProperty.UnsetValue;
		}
		return System.Enum.Parse(value.GetType(), value2).Equals(value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(parameter is string value2))
		{
			return DependencyProperty.UnsetValue;
		}
		return System.Enum.Parse(targetType, value2);
	}
}

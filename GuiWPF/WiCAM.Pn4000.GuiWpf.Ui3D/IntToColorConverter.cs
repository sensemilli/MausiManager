using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public class IntToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null || !int.TryParse(value.ToString(), out var result))
		{
			return new SolidColorBrush(Colors.WhiteSmoke);
		}
		return result switch
		{
			1 => new SolidColorBrush(Colors.Red), 
			2 => new SolidColorBrush(Colors.Orange), 
			3 => new SolidColorBrush(Colors.GreenYellow), 
			4 => new SolidColorBrush(Colors.LawnGreen), 
			5 => new SolidColorBrush(Colors.LightGreen), 
			_ => new SolidColorBrush(Colors.WhiteSmoke), 
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

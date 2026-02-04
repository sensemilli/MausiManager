using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public class BoolToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		int num = 0;
		if (value.GetType() == typeof(bool))
		{
			num = (((bool)value) ? 1 : 0);
		}
		return num switch
		{
			0 => new SolidColorBrush(Colors.Transparent), 
			1 => new SolidColorBrush(Colors.Red), 
			_ => new SolidColorBrush(Colors.WhiteSmoke), 
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

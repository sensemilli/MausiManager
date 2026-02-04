using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class ImagePathConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Path.Combine(Environment.GetEnvironmentVariable("PNDRIVE") + "/u/pn/pixmap/img", (string)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}

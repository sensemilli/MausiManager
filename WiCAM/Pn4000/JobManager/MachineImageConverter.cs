using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(string), typeof(BitmapImage))]
public class MachineImageConverter : IValueConverter
{
	public BitmapImage Default { get; set; }

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			string text = value.ToString();
			if (IOHelper.FileExists(text))
			{
				return new BitmapImage(new Uri(text, UriKind.RelativeOrAbsolute));
			}
		}
		return Default;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

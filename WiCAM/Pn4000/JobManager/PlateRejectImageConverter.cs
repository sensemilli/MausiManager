using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(string), typeof(BitmapImage))]
public class PlateRejectImageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is PartInfo partInfo)
		{
			string text = Path.Combine(Path.GetDirectoryName(partInfo.Path), partInfo.PART_PIXEL_VIEW_SL);
			if (IOHelper.FileExists(text))
			{
				return new BitmapImage(new Uri(text, UriKind.RelativeOrAbsolute));
			}
		}
		return new BitmapImage();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

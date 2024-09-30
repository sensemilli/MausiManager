using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(short), typeof(Brush))]
public class TextToTimeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		TimeSpan timeSpan = TimeSpan.Zero;
		if (value != null)
		{
			timeSpan = timeSpan.Add(TimeSpan.FromMinutes((double)value));
		}
		return timeSpan;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 0.0;
		if (value != null)
		{
			TimeSpan timeSpan = TimeSpan.Zero;
			string[] array = value.ToString().Split(".,:".ToCharArray());
			if (array.Length > 2)
			{
				timeSpan = timeSpan.Add(TimeSpan.FromHours(StringHelper.ToInt(array[0]))).Add(TimeSpan.FromMinutes(StringHelper.ToInt(array[1]))).Add(TimeSpan.FromSeconds(StringHelper.ToInt(array[2])));
			}
			else if (array.Length > 1)
			{
				timeSpan = timeSpan.Add(TimeSpan.FromMinutes(StringHelper.ToInt(array[0]))).Add(TimeSpan.FromSeconds(StringHelper.ToInt(array[1])));
			}
			else if (array.Length == 1)
			{
				timeSpan = timeSpan.Add(TimeSpan.FromSeconds(StringHelper.ToInt(array[0])));
			}
			num = timeSpan.TotalMinutes;
		}
		return num;
	}
}

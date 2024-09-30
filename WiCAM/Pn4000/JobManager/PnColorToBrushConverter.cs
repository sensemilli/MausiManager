using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.JobManager;

[ValueConversion(typeof(short), typeof(Brush))]
public class PnColorToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is short)
		{
			return PnColors.Instance.SelectColorFromXCOLDF(System.Convert.ToInt32(value));
		}
		return Brushes.White;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

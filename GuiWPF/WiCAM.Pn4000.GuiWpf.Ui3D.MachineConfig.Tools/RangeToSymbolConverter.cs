using System;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class RangeToSymbolConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 2 || values[0] == null || values[1] == null)
		{
			return string.Empty;
		}
		if (double.TryParse(values[0].ToString(), out var result) && double.TryParse(values[1].ToString(), out var result2))
		{
			double num = ((parameter != null) ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture) : 1.0);
			if (!(Math.Abs(result - result2) <= num))
			{
				return " (✗)";
			}
			return " (✓)";
		}
		return string.Empty;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

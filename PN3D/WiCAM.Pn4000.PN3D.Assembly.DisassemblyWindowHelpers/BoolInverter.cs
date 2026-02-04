using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;

public class BoolInverter : MarkupExtension, IValueConverter
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool)
		{
			return !(bool)value;
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return this.Convert(value, targetType, parameter, culture);
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Information;

internal class BdsmExampleEnumListConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return new List<string>(BdsmExampleEnum2String.GetKeys());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PurchasedPartsTypeListConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return new List<string>(PurchasedPartsEnum2String.GetKeys());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}

using System;
using System.Globalization;
using System.Windows.Data;
using WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PurchasedPartsTypeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return PurchasedPartsEnum2String.GetString((PurchasedPartTypesEnum)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return PurchasedPartsEnum2String.GetEnum(value as string);
	}
}

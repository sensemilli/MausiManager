using System;
using System.Globalization;
using System.Windows.Data;
using BendDataSourceModel.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Information;

public class BdsmExampleEnumConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return BdsmExampleEnum2String.GetString((BdsmExampleEnum)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return BdsmExampleEnum2String.GetEnum(value as string);
	}
}

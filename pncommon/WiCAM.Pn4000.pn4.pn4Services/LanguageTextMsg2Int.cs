using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WiCAM.Pn4000.pn4.Interfaces;

namespace WiCAM.Pn4000.pn4.pn4Services;

public sealed class LanguageTextMsg2Int : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (Application.Current is IApplicationDataProvider applicationDataProvider)
			{
				return applicationDataProvider.GetLanguageDictionary().GetMsg2Int((string)parameter);
			}
		}
		catch (Exception)
		{
		}
		return (string)parameter;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException("LanguageTextMSG2INT can only be used for one way conversion.");
	}
}

using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	[ValueConversion(typeof(PropertyInfo), typeof(string))]
	public class ResourceNameConverter : IValueConverter
	{
		public ResourceNameConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			PropertyInfo propertyInfo = value as PropertyInfo;
			if (propertyInfo == null)
			{
				return string.Empty;
			}
			TranslationKeyAttribute customAttribute = propertyInfo.GetCustomAttribute<TranslationKeyAttribute>();
			if (customAttribute == null)
			{
				return propertyInfo.Name;
			}
			return StringResourceHelper.Instance.FindString(customAttribute.Key);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
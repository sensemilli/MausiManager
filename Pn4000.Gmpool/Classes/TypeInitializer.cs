using System;
using System.Reflection;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal static class TypeInitializer
	{
		public static void Initialize(object item)
		{
			PropertyInfo[] properties = item.GetType().GetProperties();
			for (int i = 0; i < (int)properties.Length; i++)
			{
				PropertyInfo propertyInfo = properties[i];
				if (propertyInfo.PropertyType == typeof(string) && propertyInfo.CanWrite)
				{
					propertyInfo.SetValue(item, string.Empty);
				}
			}
		}
	}
}
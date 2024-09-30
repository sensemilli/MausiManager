using System;
using System.Reflection;

namespace WiCAM.Pn4000.JobManager.Classes;

internal class TypeInitializer
{
	public void Initialize(object item)
	{
		Type typeFromHandle = typeof(string);
		PropertyInfo[] properties = item.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.PropertyType == typeFromHandle && propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(item, string.Empty);
			}
		}
	}
}

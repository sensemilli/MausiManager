using System;
using System.Collections.Generic;
using System.Reflection;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;

namespace WiCAM.Pn4000.JobManager;

public class JobStrategyHelper
{
	public ReadJobStrategyInfo CreateReadStrategies(IJobManagerSettings settings)
	{
		return new ReadJobStrategyInfo
		{
			JobStrategy = FromClass<JobInfo>(settings.JobListConfiguration),
			PlateStrategy = FromClass<PlateInfo>(settings.PlateListConfiguration),
			PartStrategy = FromClass<PartInfo>(settings.PartListConfiguration),
			PlatePartStrategy = FromClass<PlatePartInfo>(settings.PlatePartListConfiguration)
		};
	}

	private ReadStrategyInfo FromClass<T>(List<CppConfigurationLineInfo> configuration) where T : class, IDatItem, new()
	{
		ReadStrategyInfo readStrategyInfo = new ReadStrategyInfo
		{
			DatType = typeof(T)
		};
		List<CppConfigurationLineInfo> list = new List<CppConfigurationLineInfo>(configuration);
		CustomAttributeHelper<T, ObligatoryAttribute> customAttributeHelper = new CustomAttributeHelper<T, ObligatoryAttribute>();
		CustomAttributeHelper<T, DatKeyAttribute> customAttributeHelper2 = new CustomAttributeHelper<T, DatKeyAttribute>();
		DatKeyAttribute datKeyAttribute = null;
		foreach (KeyValuePair<PropertyInfo, ObligatoryAttribute> availableAttribute in customAttributeHelper.AvailableAttributes)
		{
			string key = availableAttribute.Key.Name;
			if ((datKeyAttribute = customAttributeHelper2.FindAttribute(availableAttribute.Key)) != null)
			{
				key = datKeyAttribute.Key;
			}
			CppConfigurationLineInfo cppConfigurationLineInfo = null;
			if ((cppConfigurationLineInfo = configuration.Find((CppConfigurationLineInfo x) => x.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))) != null)
			{
				list.Remove(cppConfigurationLineInfo);
			}
			readStrategyInfo.Properties.Add(new ReadPropertyInfo(key, availableAttribute.Key));
		}
		List<PropertyInfo> list2 = new List<PropertyInfo>(customAttributeHelper2.Properties);
		foreach (CppConfigurationLineInfo item in list)
		{
			string text = null;
			PropertyInfo propertyInfo = list2.Find((PropertyInfo x) => x.Name.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase));
			if (propertyInfo == null)
			{
				propertyInfo = FindPropertyByAttributeKeyValue(customAttributeHelper2, item.Key);
				text = item.Key;
			}
			else
			{
				text = propertyInfo.Name;
			}
			if (propertyInfo != null)
			{
				readStrategyInfo.Properties.Add(new ReadPropertyInfo(text, propertyInfo));
			}
		}
		return readStrategyInfo;
	}

	private PropertyInfo FindPropertyByAttributeKeyValue<T>(CustomAttributeHelper<T, DatKeyAttribute> datKeys, string key) where T : class, IDatItem, new()
	{
		foreach (KeyValuePair<PropertyInfo, DatKeyAttribute> availableAttribute in datKeys.AvailableAttributes)
		{
			if (availableAttribute.Value.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
			{
				return availableAttribute.Key;
			}
		}
		return null;
	}
}

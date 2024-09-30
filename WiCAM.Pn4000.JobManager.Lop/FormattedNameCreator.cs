using System;
using System.Collections.Generic;
using System.Reflection;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Classes;
using WiCAM.Pn4000.Jobdata.Interfaces;
using WiCAM.Pn4000.Jobdata.Mapping;

namespace WiCAM.Pn4000.JobManager.Lop;

internal class FormattedNameCreator
{
	private readonly LopTemplateInfo _template;

	private List<string> _usedKeys = new List<string>();

	private Dictionary<string, object> _values = new Dictionary<string, object>();

	private List<PropertyInfo> _jobProperties = new List<PropertyInfo>();

	private Dictionary<string, PropertyInfo> _mappingPlate;

	private Dictionary<string, PropertyInfo> _mappingPlatePart;

	public FormattedNameCreator(LopTemplateInfo template)
	{
		_template = template;
		_usedKeys.AddRange(new KeyFinder(template.NameFormat).FindKeys());
		_jobProperties.AddRange(typeof(JobInfo).GetProperties());
		_mappingPlate = CreateReverseMapping(new MappingCreator().CreateMapping<Plate>().Mapping);
		_mappingPlatePart = CreateReverseMapping(new MappingCreator().CreateMapping<PlatePart>().Mapping);
	}

	public string Create(Plate plateDat, PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		string text = _template.NameFormat;
		IPlatePart platePart = null;
		if (part != null)
		{
			platePart = plateDat.PlatePartList.Find((IPlatePart x) => StringHelper.ToInt(x.PlatePartNumber) == part.PlatePart.PLATE_PART_NUMBER);
		}
		foreach (string usedKey in _usedKeys)
		{
			object obj = FindValue(usedKey, plateDat, plate, platePart);
			if (obj != null)
			{
				text = text.Replace("$" + usedKey + "$", obj.ToString());
			}
		}
		return text;
	}

	private Dictionary<string, PropertyInfo> CreateReverseMapping(Dictionary<PropertyInfo, string> mapping)
	{
		Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();
		foreach (KeyValuePair<PropertyInfo, string> item in mapping)
		{
			dictionary.Add(item.Value, item.Key);
		}
		return dictionary;
	}

	private object FindValue(string key, Plate plateDat, PlateProductionInfo plate, IPlatePart platePart)
	{
		if (_mappingPlate.ContainsKey(key))
		{
			return _mappingPlate[key].GetValue(plateDat, null);
		}
		PropertyInfo propertyInfo = _jobProperties.Find((PropertyInfo x) => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(plate.JobReference, null);
		}
		if (platePart != null && _mappingPlatePart.ContainsKey(key))
		{
			return _mappingPlatePart[key].GetValue(platePart);
		}
		return null;
	}
}

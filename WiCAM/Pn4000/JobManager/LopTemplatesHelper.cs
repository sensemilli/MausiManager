using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.JobManager.Classes;
using WiCAM.Pn4000.JobManager.Lop;

namespace WiCAM.Pn4000.JobManager;

public class LopTemplatesHelper : ILopTemplatesHelper
{
	private readonly ILopTemplateReader _templateReader;

	public static readonly string TypeMatpool = "MATERIAL";

	public static readonly string TypeMatpoolRest = "MATERIAL_REST";

	public static readonly string TypeGapool = "GAPOOL";

	private static readonly string __companyWicam = "PN4000";

	public static readonly string ActionProduced = "PRODUCED";

	public static readonly string ActionStorno = "STORNO";

	public static readonly string ActionReject = "REJECT";

	private readonly Dictionary<string, LopTemplateInfo> _plateTemplates = new Dictionary<string, LopTemplateInfo>();

	private readonly Dictionary<string, LopTemplateInfo> _partTemplates = new Dictionary<string, LopTemplateInfo>();

	private readonly Dictionary<string, LopTemplateInfo> _customerPlateTemplates = new Dictionary<string, LopTemplateInfo>();

	private readonly Dictionary<string, LopTemplateInfo> _customerPartTemplates = new Dictionary<string, LopTemplateInfo>();

	private readonly List<Dictionary<string, LopTemplateInfo>> _dictionaries = new List<Dictionary<string, LopTemplateInfo>>();

	public LopTemplatesHelper(ILopTemplateReader templateReader)
	{
		_templateReader = templateReader;
		_dictionaries.Add(_plateTemplates);
		_dictionaries.Add(_partTemplates);
		_dictionaries.Add(_customerPlateTemplates);
		_dictionaries.Add(_customerPartTemplates);
	}

	public IEnumerable<LopTemplateInfo> FindTemplates(string action, string type)
	{
		List<LopTemplateInfo> list = new List<LopTemplateInfo>();
		foreach (Dictionary<string, LopTemplateInfo> dictionary in _dictionaries)
		{
			foreach (KeyValuePair<string, LopTemplateInfo> item in dictionary)
			{
				if (item.Key.IndexOf(action, StringComparison.CurrentCultureIgnoreCase) > -1 && item.Value.Type.Equals(type, StringComparison.CurrentCultureIgnoreCase))
				{
					list.Add(item.Value);
				}
			}
		}
		return list;
	}

	public IEnumerable<LopTemplateInfo> FindTemplates(string action)
	{
		List<LopTemplateInfo> list = new List<LopTemplateInfo>();
		foreach (Dictionary<string, LopTemplateInfo> dictionary in _dictionaries)
		{
			foreach (KeyValuePair<string, LopTemplateInfo> item in dictionary)
			{
				if (item.Key.IndexOf(action, StringComparison.CurrentCultureIgnoreCase) > -1)
				{
					list.Add(item.Value);
				}
			}
		}
		return list;
	}

	public void PrepareTemplates(string path)
	{
		foreach (LopTemplateInfo item in ReadTemplates(path))
		{
			CheckCompany(item, isMatpool: false);
			UpdateMapping<JobInfo>(item);
			UpdateMapping<ProductionInfo>(item);
			UpdateMapping<PlateInfo>(item);
			UpdateMapping<PartInfo>(item);
			UpdateMapping<PlatePartInfo>(item);
		}
	}

	private void UpdateMapping<T>(LopTemplateInfo template) where T : class
	{
		CustomAttributeHelper<T, DatKeyAttribute> customAttributeHelper = new CustomAttributeHelper<T, DatKeyAttribute>();
		List<PropertyInfo> list = new List<PropertyInfo>(customAttributeHelper.Properties);
		foreach (KeyValuePair<string, PropertyReference> item in template.Mapping.ToList())
		{
			if (item.Value != null)
			{
				continue;
			}
			string propertyName = item.Key.Replace("$", string.Empty);
			PropertyInfo propertyInfo = list.Find((PropertyInfo x) => x.Name == propertyName);
			if (propertyInfo == null)
			{
				DatKeyAttribute attribute = customAttributeHelper.FindAttribute(propertyName);
				if (attribute != null)
				{
					propertyInfo = list.Find((PropertyInfo x) => x.Name == attribute.Key);
				}
				else
				{
					foreach (KeyValuePair<PropertyInfo, DatKeyAttribute> availableAttribute in customAttributeHelper.AvailableAttributes)
					{
						if (availableAttribute.Value.Key.Equals(propertyName))
						{
							propertyInfo = availableAttribute.Key;
							break;
						}
					}
				}
			}
			if (propertyInfo != null)
			{
				template.Mapping[item.Key] = new PropertyReference
				{
					Property = propertyInfo,
					ItemType = typeof(T)
				};
			}
		}
	}

	private void CheckCompany(LopTemplateInfo template, bool isMatpool)
	{
		if (template.Company.Equals(__companyWicam, StringComparison.CurrentCultureIgnoreCase))
		{
			if (isMatpool)
			{
				AddTemplate(_plateTemplates, template);
			}
			else
			{
				AddTemplate(_partTemplates, template);
			}
		}
		else if (isMatpool)
		{
			AddTemplate(_customerPlateTemplates, template);
		}
		else
		{
			AddTemplate(_customerPartTemplates, template);
		}
	}

	private void AddTemplate(Dictionary<string, LopTemplateInfo> dictionary, LopTemplateInfo template)
	{
		string text = string.Join("_", template.LopType, template.Type);
		if (!dictionary.ContainsKey(text))
		{
			dictionary.Add(text, template);
			return;
		}
		Logger.Error("Template {0}' already exists in dictionary", text);
	}

	private IEnumerable<LopTemplateInfo> ReadTemplates(string path)
	{
		List<LopTemplateInfo> list = new List<LopTemplateInfo>();
		Encoding encoding = new FileEncodingHelper().Find(path);
		if (!encoding.Equals(CurrentEncoding.SystemEncoding))
		{
			MessageHelper.Error(string.Format(CultureInfo.InvariantCulture, "Wrong encoding of file '{0}'. \n{1}\nMust be {2}", path, encoding.EncodingName, CurrentEncoding.SystemEncoding.EncodingName));
			return list;
		}
		string text = IOHelper.FileReadAllText(path);
		if (string.IsNullOrEmpty(text))
		{
			MessageHelper.Error(string.Format(CultureInfo.InvariantCulture, "File '{0}' is not found", path));
			return list;
		}
		LopTemplateInfo lopTemplateInfo = new LopTemplateInfo();
		int num = _templateReader.ReadTemplate(lopTemplateInfo, text, 0);
		if (!string.IsNullOrEmpty(lopTemplateInfo.Type))
		{
			list.Add(lopTemplateInfo);
		}
		while (num > 0)
		{
			lopTemplateInfo = new LopTemplateInfo();
			num = _templateReader.ReadTemplate(lopTemplateInfo, text, num);
			if (!string.IsNullOrEmpty(lopTemplateInfo.Type))
			{
				list.Add(lopTemplateInfo);
			}
		}
		return list;
	}
}

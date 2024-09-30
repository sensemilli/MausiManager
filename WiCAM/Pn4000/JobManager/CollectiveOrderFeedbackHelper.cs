using System;
using System.Collections.Generic;
using System.Reflection;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;
using WiCAM.Pn4000.Jobdata.Classes;

namespace WiCAM.Pn4000.JobManager;

public class CollectiveOrderFeedbackHelper
{
	private readonly IProductionHelper _helper;

	private readonly Dictionary<PropertyInfo, PropertyInfo> _reference;

	public CollectiveOrderFeedbackHelper(IProductionHelper helper)
	{
		_helper = helper;
		_reference = BuildReference();
	}

	private static Dictionary<PropertyInfo, PropertyInfo> BuildReference()
	{
		Dictionary<PropertyInfo, PropertyInfo> dictionary = new Dictionary<PropertyInfo, PropertyInfo>();
		List<PropertyInfo> list = new List<PropertyInfo>(typeof(PartInfo).GetProperties());
		PropertyInfo[] properties = typeof(PlatePartCollectiveOrder).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			TranslationKeyAttribute attribute = CustomAttributeHelper.FindCustomAttribute<TranslationKeyAttribute>(propertyInfo);
			if (attribute != null)
			{
				PropertyInfo propertyInfo2 = list.Find((PropertyInfo x) => x.Name.Equals(attribute.Key, StringComparison.CurrentCulture));
				if (propertyInfo2 != null)
				{
					dictionary.Add(propertyInfo, propertyInfo2);
				}
			}
		}
		return dictionary;
	}

	public void ProducePlatePart(PlateProductionInfo plateProduction, PlatePartProductionInfo platePart, IEnumerable<LopTemplateInfo> rejectTemplates, LopTemplateInfo template, Plate plateDat)
	{
		int num = platePart.AmountToProduce;
		int num2 = platePart.AmountToReject;
		if (num > 0)
		{
			foreach (PlatePartCollectiveOrder collectiveOrder in platePart.PlatePart.CollectiveOrders)
			{
				PlatePartProductionInfo platePartProductionInfo = CreatePartProduction(platePart, collectiveOrder);
				int num3 = platePartProductionInfo.PlatePart.PLATE_PART_AMOUNT;
				if (num3 > num)
				{
					num3 = num;
				}
				collectiveOrder.PlatePartProduced += num3;
				_helper.WritePartLop(platePartProductionInfo, num3, template, plateProduction, plateDat);
				num -= num3;
				if (num < 1)
				{
					break;
				}
			}
		}
		if (num2 <= 0)
		{
			return;
		}
		foreach (PlatePartCollectiveOrder collectiveOrder2 in platePart.PlatePart.CollectiveOrders)
		{
			PlatePartProductionInfo platePartProductionInfo2 = CreatePartProduction(platePart, collectiveOrder2);
			int num4 = platePartProductionInfo2.PlatePart.PLATE_PART_AMOUNT;
			if (num4 > num2)
			{
				num4 = num2;
			}
			collectiveOrder2.PlatePartRejected += num4;
			foreach (LopTemplateInfo rejectTemplate in rejectTemplates)
			{
				_helper.WritePartLop(platePartProductionInfo2, num4, rejectTemplate, plateProduction, plateDat);
			}
			num2 -= num4;
			if (num2 < 1)
			{
				break;
			}
		}
	}

	public void StornoPlatePart(PlateProductionInfo plateProduction, LopTemplateInfo template, PlatePartProductionInfo platePart, Plate plateDat)
	{
		int num2 = (platePart.AmountToStorno = platePart.PlatePart.PLATE_PART_AMOUNT * plateProduction.AmountStorno);
		int num3 = num2;
		foreach (PlatePartCollectiveOrder collectiveOrder in platePart.PlatePart.CollectiveOrders)
		{
			PlatePartProductionInfo platePartProductionInfo = CreatePartProduction(platePart, collectiveOrder);
			int num4 = platePartProductionInfo.PlatePart.PLATE_PART_AMOUNT;
			if (num4 > num3)
			{
				num4 = num3;
			}
			collectiveOrder.PlatePartStorno += num4;
			_helper.WritePartLop(platePartProductionInfo, num4, template, plateProduction, plateDat);
			num3 -= num4;
			if (num3 < 1)
			{
				break;
			}
		}
	}

	private PlatePartProductionInfo CreatePartProduction(PlatePartProductionInfo platePart, PlatePartCollectiveOrder order)
	{
		PlatePartInfo obj = new PlatePartInfo
		{
			Plate = platePart.PlatePart.Plate
		};
		PartInfo partInfo = EnumerableHelper.CopyItem(platePart.PlatePart.Part);
		UpdateFromCollectiveOrder(partInfo, order);
		EnumerableHelper.UpdateItem(obj, platePart.PlatePart);
		obj.Part = partInfo;
		return new PlatePartProductionInfo(obj);
	}

	private void UpdateFromCollectiveOrder(PartInfo platePart, PlatePartCollectiveOrder order)
	{
		foreach (KeyValuePair<PropertyInfo, PropertyInfo> item in _reference)
		{
			object value = item.Key.GetValue(order);
			if (value == null)
			{
				continue;
			}
			string text = value.ToString().Trim();
			if (!string.IsNullOrEmpty(text))
			{
				if (item.Value.PropertyType == typeof(string))
				{
					item.Value.SetValue(platePart, value);
				}
				else if (item.Value.PropertyType == typeof(int))
				{
					int num = StringHelper.ToInt(text);
					item.Value.SetValue(platePart, num);
				}
			}
		}
	}
}

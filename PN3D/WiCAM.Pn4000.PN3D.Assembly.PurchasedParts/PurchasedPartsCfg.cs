using System;
using System.Collections.Generic;
using WiCAM.Config;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Services.ConfigProviders.Contracts.Attributes;

namespace WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;

[ConfigSection("PurchasedParts")]
[ConfigScope(ConfigScope.Global)]
public class PurchasedPartsCfg
{
	public bool IgnoreNonHorizontalPlaneConnectedPp { get; set; }

	public bool DetectEverythingAsPpInAssembly { get; set; }

	public bool IsDetectionPrefabricatedPartsByNameActive { get; set; }

	public double MaxDistOfPurchasePartsToSheetMetal { get; set; }

	public List<PurchasedPart> Parts { get; set; } = new List<PurchasedPart>();

	public List<Tuple<int, string>> PartTypes { get; set; }

	public List<Tuple<int, string>> GetDefaultPartTypes(ITranslator translator)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		int num = 1;
		string[] array = new string[2] { "Bold", "Different" };
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new Tuple<int, string>(item2: translator.Translate("l_enum.PrefabricatedPartsEnum." + array[i]), item1: num++));
		}
		return list;
	}
}

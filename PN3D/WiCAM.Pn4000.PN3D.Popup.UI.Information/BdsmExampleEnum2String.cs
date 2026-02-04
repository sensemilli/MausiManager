using System.Windows;
using BendDataSourceModel.Enums;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Information;

internal static class BdsmExampleEnum2String
{
	public static string[] GetKeys()
	{
		return new string[4]
		{
			Application.Current.TryFindResource("l_enum.BdsmExampleEnum.Bdsm1") as string,
			Application.Current.TryFindResource("l_enum.BdsmExampleEnum.Bdsm2") as string,
			Application.Current.TryFindResource("l_enum.BdsmExampleEnum.Bdsm3") as string,
			Application.Current.TryFindResource("l_enum.BdsmExampleEnum.Bdsm4") as string
		};
	}

	public static string GetString(BdsmExampleEnum value)
	{
		return BdsmExampleEnum2String.GetKeys()[(int)value];
	}

	public static BdsmExampleEnum GetEnum(string value)
	{
		string[] keys = BdsmExampleEnum2String.GetKeys();
		for (int i = 0; i < keys.Length; i++)
		{
			if (keys[i] == value)
			{
				return (BdsmExampleEnum)i;
			}
		}
		return BdsmExampleEnum.Bdsm1;
	}
}

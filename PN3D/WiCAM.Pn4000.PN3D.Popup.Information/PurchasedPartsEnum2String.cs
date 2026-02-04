using System.Windows;
using WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

internal static class PurchasedPartsEnum2String
{
	public static string[] GetKeys()
	{
		return new string[3]
		{
			Application.Current.TryFindResource("l_enum.PrefabricatedPartsEnum.None") as string,
			Application.Current.TryFindResource("l_enum.PrefabricatedPartsEnum.Bold") as string,
			Application.Current.TryFindResource("l_enum.PrefabricatedPartsEnum.Different") as string
		};
	}

	public static string GetString(PurchasedPartTypesEnum value)
	{
		return PurchasedPartsEnum2String.GetKeys()[(int)value];
	}

	public static PurchasedPartTypesEnum GetEnum(string value)
	{
		string[] keys = PurchasedPartsEnum2String.GetKeys();
		for (int i = 0; i < keys.Length; i++)
		{
			if (keys[i] == value)
			{
				return (PurchasedPartTypesEnum)i;
			}
		}
		return PurchasedPartTypesEnum.None;
	}
}

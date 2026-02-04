using System;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public static class UIMaterialName
{
	public static string GenerateName(int id, string name)
	{
		if (id < 0)
		{
			return name;
		}
		return $"{id} - {name}";
	}

	public static int GetMaterialIDFromUIString(string v)
	{
		if (v == "*")
		{
			return -1;
		}
		return Convert.ToInt32(v.Split('-')[0].Trim());
	}

	public static string GetMaterialNameFromUIString(string v)
	{
		if (v == "*")
		{
			return "*";
		}
		return v.Split('-')[1].Trim();
	}
}

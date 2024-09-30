using System;
using System.Globalization;

namespace WiCAM.Pn4000.Gmpool
{
	internal static class CS
	{
		public readonly static string GmpoolBrowser;

		public readonly static string MaterialsList;

		public readonly static string MaterialArtsList;

		public readonly static string MaterialsFilter;

		public readonly static string MsgItemAlreadyExist;

		public readonly static string From;

		public readonly static string Till;

		public readonly static string KeyText;

		public readonly static string KeyMaterial;

		public readonly static string KeyMaterialArt;

		public readonly static string HeaderMaterial;

		static CS()
		{
			CS.GmpoolBrowser = "WiCAM.GmpoolBrowser";
			CS.MaterialsList = "MaterialsList";
			CS.MaterialArtsList = "MaterialArts";
			CS.MaterialsFilter = "MaterialsFilter";
			CS.MsgItemAlreadyExist = "MsgItemAlreadyExist";
			CS.From = "From";
			CS.Till = "Till";
			CS.KeyText = "TEXT_";
			CS.KeyMaterial = "MATPOOL_";
			CS.KeyMaterialArt = "MATERIAL_";
			CS.HeaderMaterial = "   KEYWORD                   DESCRIPTION       VISIBLE Y/N  PLACE   WIDTH";
		}

		public static string Begin(string key)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}START", key);
		}

		public static string End(string key)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}END", key);
		}
	}
}
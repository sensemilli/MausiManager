using System;
using System.Text;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	internal class PlateCadgeoHelper
	{
		private readonly static string __platGeo;

		private readonly static string __format;

		private readonly static double __zero;

		private readonly string _path;

		static PlateCadgeoHelper()
		{
			PlateCadgeoHelper.__platGeo = "PLATCADGEO";
			PlateCadgeoHelper.__format = "     3     0     0     1     1     0     1     0 {0,13:0.00000} {1,13:0.00000} {2,13:0.00000} {3,13:0.00000}        .00000";
			PlateCadgeoHelper.__zero = 0;
		}

		public PlateCadgeoHelper()
		{
			this._path = PnPathBuilder.PathInPnHome(new object[] { PlateCadgeoHelper.__platGeo });
		}

		public string WriteGeo(StockMaterialInfo material)
		{
			StringBuilder stringBuilder = new StringBuilder(10000);
			stringBuilder.AppendLine(" V  3.0");
			for (int i = 0; i < 37; i++)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("    .00");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(PlateCadgeoHelper.__platGeo);
			StringHelper.AddWithInvariantCulture(stringBuilder, PlateCadgeoHelper.__format, new object[] { PlateCadgeoHelper.__zero, PlateCadgeoHelper.__zero, PlateCadgeoHelper.__zero, material.MaxY });
			stringBuilder.AppendLine();
			StringHelper.AddWithInvariantCulture(stringBuilder, PlateCadgeoHelper.__format, new object[] { PlateCadgeoHelper.__zero, material.MaxY, material.MaxX, material.MaxY });
			stringBuilder.AppendLine();
			StringHelper.AddWithInvariantCulture(stringBuilder, PlateCadgeoHelper.__format, new object[] { material.MaxX, material.MaxY, material.MaxX, PlateCadgeoHelper.__zero });
			stringBuilder.AppendLine();
			StringHelper.AddWithInvariantCulture(stringBuilder, PlateCadgeoHelper.__format, new object[] { material.MaxX, PlateCadgeoHelper.__zero, PlateCadgeoHelper.__zero, PlateCadgeoHelper.__zero });
			stringBuilder.AppendLine();
			IOHelper.FileDelete(this._path);
			if (!IOHelper.FileWriteAllText(this._path, stringBuilder.ToString()))
			{
				return string.Empty;
			}
			return this._path;
		}
	}
}
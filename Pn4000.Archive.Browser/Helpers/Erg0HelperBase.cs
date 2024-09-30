using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	public abstract class Erg0HelperBase
	{
		protected const string __version20 = " VERSION 2.00";

		protected const string __version30 = " VERSION 3.00";

		protected readonly static string __erg0Name;

		static Erg0HelperBase()
		{
			Erg0HelperBase.__erg0Name = "ERG0";
		}

		protected Erg0HelperBase()
		{
		}

		protected void AddCadgeoName(StringBuilder sb, CadPartInfo ncp)
		{
			if (!string.IsNullOrEmpty(ncp.Extension))
			{
				sb.Append(ncp.PartName);
				return;
			}
			sb.AppendLine(ncp.PartName);
		}

		protected void AppendFilter(StringBuilder sb, string filterKey)
		{
			FilterConfiguration filterConfiguration = DatabaseFilterHelper.Instance.DatabaseFilters.Find((FilterConfiguration x) => x.FilterKey == filterKey);
			if (filterConfiguration == null)
			{
				sb.AppendLine();
				sb.AppendLine(PnPathBuilder.ArDrive);
				return;
			}
			sb.AppendLine(filterConfiguration.FilterKey);
			sb.AppendLine(filterConfiguration.ArDrivePath);
		}
	}
}
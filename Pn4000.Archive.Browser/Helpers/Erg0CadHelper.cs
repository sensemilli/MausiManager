using System;
using System.Collections.Generic;
using System.Text;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	public class Erg0CadHelper : Erg0HelperBase
	{
		public Erg0CadHelper()
		{
		}

		private void Version0(List<CadPartInfo> list, StringBuilder sb)
		{
			foreach (CadPartInfo cadPartInfo in list)
			{
				if (cadPartInfo == null)
				{
					continue;
				}
				sb.AppendLine(cadPartInfo.ArchiveNumber.ToString());
				sb.AppendLine(cadPartInfo.PartName);
			}
		}

		private void Version2(List<CadPartInfo> list, StringBuilder sb)
		{
			sb.AppendLine(" VERSION 2.00");
			foreach (CadPartInfo cadPartInfo in list)
			{
				if (cadPartInfo == null)
				{
					continue;
				}
				sb.AppendLine(cadPartInfo.ArchiveNumber.ToString());
				sb.Append(cadPartInfo.PartName);
				if (DatabaseFilterHelper.Instance.DatabaseFilters == null)
				{
					sb.AppendLine();
					sb.AppendLine(PnPathBuilder.ArDrive);
				}
				else
				{
					base.AppendFilter(sb, cadPartInfo.Filter);
				}
			}
		}

		public void Version3(List<CadPartInfo> list, StringBuilder sb)
		{
			sb.AppendLine(" VERSION 3.00");
			foreach (CadPartInfo cadPartInfo in list)
			{
				if (cadPartInfo == null)
				{
					continue;
				}
				sb.AppendLine(cadPartInfo.ArchiveNumber.ToString());
				sb.AppendLine(cadPartInfo.Path());
				if (DatabaseFilterHelper.Instance.DatabaseFilters == null)
				{
					sb.AppendLine();
					sb.AppendLine(PnPathBuilder.ArDrive);
				}
				else
				{
					base.AppendFilter(sb, cadPartInfo.Filter);
				}
			}
		}

		public void WriteErg0(List<CadPartInfo> list)
		{
			StringBuilder stringBuilder = new StringBuilder(10000);
			if (AppConfiguration.Erg0Format == 2)
			{
				this.Version2(list, stringBuilder);
			}
			else if (AppConfiguration.Erg0Format != 3)
			{
				this.Version0(list, stringBuilder);
			}
			else
			{
				this.Version3(list, stringBuilder);
			}
			stringBuilder.AppendLine();
			IOHelper.FileWriteAllText(Erg0HelperBase.__erg0Name, stringBuilder.ToString());
		}
	}
}
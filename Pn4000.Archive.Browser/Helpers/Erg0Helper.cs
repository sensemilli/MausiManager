using System;
using System.Collections.Generic;
using System.Text;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	internal class Erg0Helper : Erg0HelperBase
	{
		public Erg0Helper()
		{
		}

		public void Write3DPartsErg0(List<CadPartInfo> list)
		{
			(new Erg03DHelper()).WriteErg0(list);
		}

		public void WriteCadPartsErg0(List<CadPartInfo> list)
		{
			(new Erg0CadHelper()).WriteErg0(list);
		}

		public void WriteNcPartsErg0(List<NcPartInfo> list)
		{
			int archiveNumber;
			StringBuilder stringBuilder = new StringBuilder(10000);
			if (AppConfiguration.Erg0Format != 2)
			{
				foreach (NcPartInfo ncPartInfo in list)
				{
					if (ncPartInfo == null)
					{
						continue;
					}
					archiveNumber = ncPartInfo.ArchiveNumber;
					stringBuilder.AppendLine(archiveNumber.ToString());
					stringBuilder.AppendLine(ncPartInfo.PartName);
				}
			}
			else
			{
				stringBuilder.AppendLine(" VERSION 2.00");
				foreach (NcPartInfo ncPartInfo1 in list)
				{
					if (ncPartInfo1 == null)
					{
						continue;
					}
					archiveNumber = ncPartInfo1.ArchiveNumber;
					stringBuilder.AppendLine(archiveNumber.ToString());
					stringBuilder.AppendLine(ncPartInfo1.PartName);
					if (DatabaseFilterHelper.Instance.DatabaseFilters == null)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine(PnPathBuilder.ArDrive);
					}
					else
					{
						base.AppendFilter(stringBuilder, ncPartInfo1.Filter);
					}
				}
			}
			IOHelper.FileWriteAllText(Erg0HelperBase.__erg0Name, stringBuilder.ToString());
		}
	}
}
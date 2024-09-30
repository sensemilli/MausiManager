using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveCad
{
	internal class CadPartDataRepository
	{
		private ListFilter<CadPartInfo> _cadPartFilter;

		private CadPartIoHelper _cadPartsReader;

		private List<CadPartInfo> _filteredCadParts;

		public string ConnectionString
		{
			get
			{
				return this._cadPartsReader.ConnectionString;
			}
		}

		public string DbFilter
		{
			get
			{
				return this._cadPartsReader.DbFilter;
			}
			set
			{
				this._cadPartsReader.DbFilter = value;
			}
		}

		public List<CadPartInfo> Parts
		{
			get
			{
				return this._cadPartsReader.CadParts;
			}
		}

		public CadPartDataRepository()
		{
			this._cadPartFilter = new ListFilter<CadPartInfo>();
			this._cadPartsReader = CadPartIoHelper.Instance;
		}

		public List<CadPartInfo> ByDateInterval(DateTime from, DateTime till)
		{
			return this._filteredCadParts.FindAll((CadPartInfo x) => {
				if (x.LastChanged.Date < from.Date)
				{
					return false;
				}
				return x.LastChanged.Date <= till.Date;
			});
		}

		public List<CadPartInfo> ByFilter(List<FilterInfo> filters)
		{
			List<CadPartInfo> cadPartInfos = this._cadPartFilter.Filter(this._cadPartsReader.CadParts, filters);
			List<CadPartInfo> cadPartInfos1 = cadPartInfos;
			this._filteredCadParts = cadPartInfos;
			return cadPartInfos1;
		}

		public string CadPartDeleteOne(CadPartInfo part)
		{
			if (this._cadPartsReader != null)
			{
				CadPartIoHelper cadPartIoHelper = this._cadPartsReader;
				if (cadPartIoHelper != null)
				{
					return cadPartIoHelper.DeleteOne(part);
				}
			}
			return string.Empty;
		}

		public void DeleteFiles(CadPartInfo part)
		{
			string str = this.PathByType(part, ArchiveFolderType.c2t);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("NC program '{0}' can not be deleted in file system!", new object[] { str });
			}
		}

		public string PathByType(CadPartInfo item, ArchiveFolderType folderType)
		{
			return ArchiveStructureHelper.Instance.PathByType(item.PartName, item.ArchiveNumber, folderType);
		}

		public void ReadAsyncron(int archiveNumber, Action onReadyAction)
		{
			this._cadPartsReader.ReadAsync(archiveNumber, onReadyAction);
		}
	}
}
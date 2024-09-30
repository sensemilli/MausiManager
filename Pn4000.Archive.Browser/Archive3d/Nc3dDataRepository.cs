using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.Archive3d
{
	internal class Nc3dDataRepository
	{
		private readonly ListFilter<CadPartInfo> _n3dPartFilter;

		private readonly CadPartIoHelper _n3dPartsReader;

		private List<CadPartInfo> _filteredN3dParts;

		public string ConnectionString
		{
			get
			{
				return this._n3dPartsReader.ConnectionString;
			}
		}

		public string DbFilter
		{
			get
			{
				return this._n3dPartsReader.DbFilter;
			}
			set
			{
				this._n3dPartsReader.DbFilter = value;
			}
		}

		public List<CadPartInfo> Parts
		{
			get
			{
				return this._n3dPartsReader.CadParts;
			}
		}

		public Nc3dDataRepository()
		{
			this._n3dPartFilter = new ListFilter<CadPartInfo>();
			this._n3dPartsReader = CadPartIoHelper.Instance;
			this._filteredN3dParts = new List<CadPartInfo>();
		}

		public List<CadPartInfo> ByDateInterval(DateTime from, DateTime till)
		{
			return this._filteredN3dParts.FindAll((CadPartInfo x) => {
				if (x.LastChanged.Date < from.Date)
				{
					return false;
				}
				return x.LastChanged.Date <= till.Date;
			});
		}

		public List<CadPartInfo> ByFilter(List<FilterInfo> filters)
		{
			List<CadPartInfo> cadPartInfos = this._n3dPartFilter.Filter(this._n3dPartsReader.CadParts, filters);
			List<CadPartInfo> cadPartInfos1 = cadPartInfos;
			this._filteredN3dParts = cadPartInfos;
			return cadPartInfos1;
		}

		public void DeleteFiles(CadPartInfo part)
		{
			string str = this.PathByType(part, ArchiveFolderType.n3d);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("NC program '{0}' can not be deleted in file system!", new object[] { str });
			}
		}

		public string DeleteOne(CadPartInfo part)
		{
			if (this._n3dPartsReader != null)
			{
				CadPartIoHelper cadPartIoHelper = this._n3dPartsReader;
				if (cadPartIoHelper != null)
				{
					return cadPartIoHelper.DeleteOne(part);
				}
			}
			return string.Empty;
		}

		public string PathByType(CadPartInfo item, ArchiveFolderType folderType)
		{
			return ArchiveStructureHelper.Instance.PathByType(item.PartName, item.ArchiveNumber, folderType);
		}

		public void ReadAsyncron(int archiveNumber, Action onReadyAction)
		{
			this._n3dPartsReader.CadParts.Clear();
			if (archiveNumber != 0)
			{
				this.ReadFromArchive(archiveNumber);
			}
			else
			{
				foreach (ArchiveInfo archiveDatum in ArchiveStructureHelper.Instance.ArchiveData)
				{
					this.ReadFromArchive(archiveDatum.Number);
				}
			}
			onReadyAction();
		}

		private void ReadFromArchive(int archiveNumber)
		{
			string str = ArchiveStructureHelper.Instance.PathByType(string.Empty, archiveNumber, ArchiveFolderType.n3d);
			List<CadPartInfo> cadPartInfos = this._n3dPartsReader.ReadArchiveDirectory(str, archiveNumber);
			if (!EnumerableHelper.IsNullOrEmpty(cadPartInfos))
			{
				CadPartInfo cadPartInfo = cadPartInfos.Find((CadPartInfo x) => x.PartName == "ARCHIV");
				if (cadPartInfo != null)
				{
					cadPartInfos.Remove(cadPartInfo);
				}
			}
			this._n3dPartsReader.CadParts.AddRange(cadPartInfos);
		}
	}
}
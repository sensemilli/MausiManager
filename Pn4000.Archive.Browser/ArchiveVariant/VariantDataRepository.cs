using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Archives.Variants;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveVariant
{
	internal class VariantDataRepository
	{
		private readonly ListFilter<VariantInfo> _varPartFilter;

		private readonly VariantsIoHelper _varPartsReader;

		private List<VariantInfo> _filteredVariantParts;

		public string ConnectionString
		{
			get
			{
				return this._varPartsReader.ConnectionString;
			}
		}

		public string DbFilter
		{
			get
			{
				return this._varPartsReader.DbFilter;
			}
			set
			{
				this._varPartsReader.DbFilter = value;
			}
		}

		public List<VariantInfo> Parts
		{
			get
			{
				return this._varPartsReader.VarParts;
			}
		}

		public VariantDataRepository()
		{
			this._varPartFilter = new ListFilter<VariantInfo>();
			this._varPartsReader = new VariantsIoHelper();
		}

		public List<VariantInfo> ByDateInterval(DateTime from, DateTime till)
		{
			return this._filteredVariantParts.FindAll((VariantInfo x) => {
				if (x.LastChanged.Date < from.Date)
				{
					return false;
				}
				return x.LastChanged.Date <= till.Date;
			});
		}

		public List<VariantInfo> ByFilter(List<FilterInfo> filters)
		{
			List<VariantInfo> variantInfos = this._varPartFilter.Filter(this._varPartsReader.VarParts, filters);
			List<VariantInfo> variantInfos1 = variantInfos;
			this._filteredVariantParts = variantInfos;
			return variantInfos1;
		}

		public void DeleteFiles(VariantInfo part)
		{
		}

		public string DeleteOne(VariantInfo part)
		{
			if (this._varPartsReader != null)
			{
				VariantsIoHelper variantsIoHelper = this._varPartsReader;
				if (variantsIoHelper != null)
				{
					return variantsIoHelper.DeleteOne(part);
				}
			}
			return string.Empty;
		}

		public string PathByType(VariantInfo item, ArchiveFolderType folderType)
		{
			return ArchiveStructureHelper.Instance.PathByType(item.PartName, item.ArchiveNumber, folderType);
		}

		public void ReadAsyncron(Action onReadyAction)
		{
			this._varPartsReader.ReadAsync(0, onReadyAction);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveN2d
{
	internal class NcPartDataRepository
	{
		private readonly ListFilter<NcPartInfo> _ncPartFilter = new ListFilter<NcPartInfo>();

		private readonly INcPartsArchivReader _reader;

		private List<NcPartInfo> _filteredNcParts;

		public string ConnectionString
		{
			get
			{
				return this._reader.ConnectionString;
			}
		}

		public string DbFilter
		{
			get
			{
				return this._reader.DbFilter;
			}
			set
			{
				this._reader.DbFilter = value;
			}
		}

		public bool IsDatabase
		{
			get
			{
				return NcPartsDataReader.Instance.IsInitialised;
			}
		}

		public DateTime MaxDate
		{
			get;
			private set;
		}

		public List<NcPartInfo> Parts
		{
			get
			{
				return this._reader.NcParts;
			}
		}

		public NcPartDataRepository()
		{
			if (this._reader == null)
			{
				if (NcPartsDataReader.Instance.IsInitialised)
				{
					Logger.Info("--- DATABASE");
					this._reader = NcPartsDataReader.Instance;
					return;
				}
				Logger.Info("--- FILESYSTEM");
				this._reader = NcPartIOHelper.Instance;
			}
		}

		public List<NcPartInfo> ByDateInterval(DateTime from, DateTime till)
		{
			return this._filteredNcParts.FindAll((NcPartInfo x) => {
				if (x.SDate.Date < from.Date)
				{
					return false;
				}
				return x.SDate.Date <= till.Date;
			});
		}

		public List<NcPartInfo> ByFilter(List<FilterInfo> filters)
		{
			List<NcPartInfo> ncPartInfos = this._ncPartFilter.Filter(this._reader.NcParts, filters);
			List<NcPartInfo> ncPartInfos1 = ncPartInfos;
			this._filteredNcParts = ncPartInfos;
			List<NcPartInfo> ncPartInfos2 = ncPartInfos1;
			if (!EnumerableHelper.IsNullOrEmpty(ncPartInfos2))
			{
				this.MaxDate = DateTime.Today;
				foreach (NcPartInfo ncPartInfo in ncPartInfos2)
				{
					if (ncPartInfo.SDate <= this.MaxDate)
					{
						continue;
					}
					this.MaxDate = ncPartInfo.SDate;
				}
			}
			return ncPartInfos2;
		}

		public string DeleteOne(long rowId)
		{
			if (this._reader is DataManager)
			{
				DataManager dataManager = this._reader as DataManager;
				if (dataManager != null)
				{
					return dataManager.DeleteFromDb(rowId);
				}
			}
			else if (this._reader is NcPartsDataReader)
			{
				DataManager dataManager1 = new DataManager();
				if (dataManager1.InitializeDatabase())
				{
					return dataManager1.DeleteFromDb(rowId);
				}
			}
			return string.Empty;
		}

		public string Path(NcPartInfo item)
		{
			if (item == null)
			{
				return null;
			}
			return ArchiveStructureHelper.Instance.PathByType(item.PartName, item.ArchiveNumber, ArchiveFolderType.c2d);
		}

		public void ReadAsyncron(int archiveNumber, Action onReadyAction, MainWindow wnd)
		{
			if (!(this._reader is NcPartsDataReader))
			{
				this._reader.ReadAsync(archiveNumber, onReadyAction);
				return;
			}
			NcPartsDataReader ncPartsDataReader = this._reader as NcPartsDataReader;
			wnd.ArchiveFilter.IsEnabled = false;
			wnd.Cursor = Cursors.Wait;
			int num = ncPartsDataReader.FindPartsAmount(archiveNumber);
			wnd.StatusText.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("MessageFilesAmount"), 0, num);
			ncPartsDataReader.InitializeWindow(wnd);
			ncPartsDataReader.ReadAsyncron(archiveNumber, onReadyAction);
		}
	}
}
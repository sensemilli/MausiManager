using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Database;
using WiCAM.Pn4000.JobManager;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	internal class NcPartsDataReader : SqlServerHelperBase, INcPartsArchivReader, IArchiveReader
	{
		private BackgroundWorker _bw;

		private Action _onCompletedAction;

		private int _reportRuns;

		private int _archiveNumber;

		private MainWindow _wnd;

		private int _partsAmount;

		private static NcPartsDataReader _instance;

		public string DbFilter
		{
			get;
			set;
		}

		public static NcPartsDataReader Instance
		{
			get
			{
				if (NcPartsDataReader._instance == null)
				{
					Logger.Verbose("Initialize NcPartsDataReader");
					NcPartsDataReader._instance = new NcPartsDataReader();
				}
				return NcPartsDataReader._instance;
			}
		}

		public bool IsInitialised
		{
			get;
			private set;
		}

		public List<NcPartInfo> NcParts
		{
			get
			{
				return JustDecompileGenerated_get_NcParts();
			}
			set
			{
				JustDecompileGenerated_set_NcParts(value);
			}
		}

		private List<NcPartInfo> JustDecompileGenerated_NcParts_k__BackingField;

		public List<NcPartInfo> JustDecompileGenerated_get_NcParts()
		{
			return this.JustDecompileGenerated_NcParts_k__BackingField;
		}

		private void JustDecompileGenerated_set_NcParts(List<NcPartInfo> value)
		{
			this.JustDecompileGenerated_NcParts_k__BackingField = value;
		}

		private NcPartsDataReader()
		{
			SqlDatabaseConfigurationInfo dbConfiguration = SqlDatabaseConfigurationInfo.DbConfiguration;
			dbConfiguration.InitializeFromPndbcfFile(WiCAM.Pn4000.Database.CS.DbTypeArchiv);
			if (!string.IsNullOrEmpty(dbConfiguration.ConnectionString))
			{
				base.ConnectionString = dbConfiguration.ConnectionString;
				this.DbFilter = dbConfiguration.Filter;
				this.IsInitialised = true;
			}
			this.NcParts = new List<NcPartInfo>();
			this._bw = new BackgroundWorker()
			{
				WorkerReportsProgress = true,
				WorkerSupportsCancellation = true
			};
			this._bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Bw_RunWorkerCompleted);
			this._bw.ProgressChanged += new ProgressChangedEventHandler(this.Bw_ProgressChanged);
			this._bw.DoWork += new DoWorkEventHandler(this.Bw_DoWork);
		}

		private void Bw_DoWork(object sender, DoWorkEventArgs e)
		{
			string str = this.FindNcPartSelectCommand(false);
			int num = 3000;
			Logger.Info(str);
			Logger.Info("Bw_DoWork : archiveNumber={0}", new object[] { this._archiveNumber });
			if (this._partsAmount > num)
			{
				base.StrongTypedListTwoBlocksBgw<NcPartInfo>(str, SqlServerHelperBase.CreateParameters(new string[] { "ArchiveNumber", "Filter" }, new object[] { this._archiveNumber, this.DbFilter }), CommandType.Text, this._bw, e, num);
				return;
			}
			base.StrongTypedListTwoBlocksBgw<NcPartInfo>(str, SqlServerHelperBase.CreateParameters(new string[] { "ArchiveNumber", "Filter" }, new object[] { this._archiveNumber, this.DbFilter }), CommandType.Text, this._bw, e, 1);
		}

		private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			List<NcPartInfo> userState = e.UserState as List<NcPartInfo>;
			if (!EnumerableHelper.IsNullOrEmpty(userState))
			{
				this.NcParts.AddRange(userState);
			}
			if (this._reportRuns != 0)
			{
				int num = this._reportRuns;
			}
			else
			{
				this._wnd.Cursor = null;
			}
			this._reportRuns++;
			if (this._onCompletedAction != null)
			{
				this._onCompletedAction();
			}
		}

		private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this._wnd.ArchiveFilter.IsEnabled = true;
			Logger.Verbose("BW complete");
		}

		private string FindNcPartSelectCommand(bool isRowsCounter = false)
		{
			SqlCrudInfo sqlCrudInfo = NcPartInfo.CrudData();
			if (!isRowsCounter)
			{
				if (this._archiveNumber > 0)
				{
					if (!string.IsNullOrEmpty(this.DbFilter))
					{
						return sqlCrudInfo.Command("A1F1");
					}
					return sqlCrudInfo.Command("A1F0");
				}
				if (!string.IsNullOrEmpty(this.DbFilter))
				{
					return sqlCrudInfo.Command("A0F1");
				}
				return sqlCrudInfo.Command("A0F0");
			}
			if (this._archiveNumber > 0)
			{
				if (!string.IsNullOrEmpty(this.DbFilter))
				{
					return sqlCrudInfo.Command("CtrA1F1");
				}
				return sqlCrudInfo.Command("CtrA1F0");
			}
			if (!string.IsNullOrEmpty(this.DbFilter))
			{
				return sqlCrudInfo.Command("CtrA0F1");
			}
			return sqlCrudInfo.Command("CtrA0F0");
		}

		public int FindPartsAmount(int archiveNumber)
		{
			this._archiveNumber = archiveNumber;
			string str = this.FindNcPartSelectCommand(true);
			this._partsAmount = 0;
			object obj = base.ExecuteScalarCommand(str, SqlServerHelperBase.CreateParameters(new string[] { "ArchiveNumber" }, new object[] { this._archiveNumber }), CommandType.Text);
			if (obj != null)
			{
				this._partsAmount = StringHelper.ToInt(obj);
			}
			return this._partsAmount;
		}

		public void InitializeWindow(MainWindow wnd)
		{
			this._wnd = wnd;
		}

		public void ReadAsyncron(int archiveNumber, Action onReadyAction)
		{
			if (!this._bw.IsBusy)
			{
				this.NcParts.Clear();
				this._archiveNumber = archiveNumber;
				this._onCompletedAction = onReadyAction;
				this._reportRuns = 0;
				Logger.Info("ReadAsyncron : archiveNumber={0}", new object[] { archiveNumber });
				this._bw.RunWorkerAsync();
			}
		}

        public void ReadAsync(int archiveNumber, Action onReadyAction)
        {
            ((IArchiveReader)Instance).ReadAsyncron(archiveNumber, onReadyAction);
        }

        string WiCAM.Pn4000.Archives.IArchiveReader.ConnectionString
		{
			get
			{
				return base.ConnectionString;
			}
		}
        

    }
}
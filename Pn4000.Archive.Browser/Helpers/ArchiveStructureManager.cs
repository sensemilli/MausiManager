using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls.ArchivesControl;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	internal class ArchiveStructureManager : ViewModelBase
	{
		private static WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager __instance;

		private int _archiveNumber;

		public static Action ArchivesAreReadHandler;

		public readonly ArchiveTreeFilterControlViewModel _model;

		private ArchiveTreeFilterControl _treeControl;

		public int ArchiveNumber
		{
			get
			{
				return this._archiveNumber;
			}
			set
			{
				this._archiveNumber = value;
				base.NotifyPropertyChanged("ArchiveNumber");
				this.SelectedArchiv = this._model.ArchivesCollection.ToList<ArchiveInfo>().Find((ArchiveInfo x) => x.Number == this._archiveNumber);
			}
		}

		public static WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager Instance
		{
			get
			{
				if (WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.__instance == null)
				{
					Logger.Verbose("Initialize ArchiveStructureManager");
					WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.__instance = new WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager();
				}
				return WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.__instance;
			}
		}

		public ArchiveInfo SelectedArchiv
		{
			get
			{
				return this._model.SelectedArchiv;
			}
			set
			{
				this._model.SelectedArchiv = value;
				base.NotifyPropertyChanged("SelectedArchiv");
			}
		}

		private ArchiveStructureManager()
		{
			ArchiveTreeFilterControlViewModel.ArchivesAreReadHandler = WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.ArchivesAreReadHandler;
			this._model = new ArchiveTreeFilterControlViewModel();
			this._model.Texts.TextSearchArchive = StringResourceHelper.Instance.FindString("Archive");
			this._model.Texts.TextSearchSubArchive = StringResourceHelper.Instance.FindString("SubArchive");
			this._model.Texts.TextSelectAll = StringResourceHelper.Instance.FindString("SelectAll");
		}

		public void ChangeDatabaseFilter(FilterConfiguration filter, Action<object> workCompletedHandler)
		{
			if (!string.IsNullOrEmpty(filter.ArDrivePath))
			{
				PnPathBuilder.ChangeArDrive(filter.ArDrivePath);
			}
			this._model.ReadArchivesStructure();
		}

		public void ChangeSelectedArchive(int archiveNumber)
		{
			this.ArchiveNumber = archiveNumber;
			this._model.SelectArchive(archiveNumber);
		}

		public void ExpandAll()
		{
			if (this._treeControl != null)
			{
				this._treeControl.ExpandAll();
			}
		}

		public void ExpandSelected()
		{
			this._treeControl.ExpandParent(this.SelectedArchiv);
		}

		public void Initialize(ArchiveTreeFilterControl treeControl)
		{
			this._treeControl = treeControl;
			this._model.InitializeModel(treeControl);
		}
	}
}
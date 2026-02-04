using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig;
using Microsoft.Win32;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupSelectFilesForImportViewModel : PopupViewModelBase
{
	private ICommand _selectFile;

	private ICommand _selectGeometrySourceFolder;

	private ICommand _selectGeometryTargetFolder;

	private ImportObject _selectedImportObject;

	private IPnPathService _pnPathService;

	private BendMachine _bendMachine;

	private string _toolType;

	private Action _closeAction;

	public ICommand SelectFileCommand => this._selectFile ?? (this._selectFile = new RelayCommand(delegate
	{
		this.SelectFile();
	}));

	public ICommand SelectGeometrySourceFolderCommand => this._selectGeometrySourceFolder ?? (this._selectGeometrySourceFolder = new RelayCommand(delegate
	{
		this.SelectGeometrySourceFolder();
	}));

	public ICommand SelectGeometryTargetFolderCommand => this._selectGeometryTargetFolder ?? (this._selectGeometryTargetFolder = new RelayCommand(delegate
	{
		this.SelectGeometryTargetFolder();
	}));

	public ImportObject SelectedImportObject
	{
		get
		{
			return this._selectedImportObject;
		}
		set
		{
			this._selectedImportObject = value;
			this.OnPropertyChanged("SelectedImportObject");
		}
	}

	public PopupSelectFilesForImportViewModel(IPnPathService pnPathService, BendMachine bendMachine, string toolType, Action closeAction)
	{
		this._pnPathService = pnPathService;
		this._bendMachine = bendMachine;
		this._toolType = toolType;
		this._closeAction = closeAction;
		this.SelectedImportObject = new ImportObject();
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
	}

	private void SelectFile()
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "ARV files (*.arv)|*.arv|MDB files (*.mdb)|*.mdb",
			Title = "Select config-file.",
			Multiselect = true
		};
		if (openFileDialog.ShowDialog().Value)
		{
			if (Path.GetExtension(openFileDialog.FileNames.FirstOrDefault()).ToLower() == ".mdb")
			{
				this.SelectedImportObject = new MdbImportObject
				{
					DataSourceFiles = new ObservableCollection<string>(openFileDialog.FileNames),
					Type = SourceType.MDB
				};
			}
			else
			{
				this.SelectedImportObject = new ImportObject
				{
					DataSourceFiles = new ObservableCollection<string>(openFileDialog.FileNames),
					Type = SourceType.ARV
				};
			}
		}
	}

	private void SelectGeometrySourceFolder()
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = (string.IsNullOrEmpty(this.SelectedImportObject.DataSourceFiles.FirstOrDefault()) ? this._pnPathService.PNHOMEDRIVE : Path.GetDirectoryName(this.SelectedImportObject.DataSourceFiles.FirstOrDefault())),
			Description = "Select source folder for mdb geometry-files."
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			((MdbImportObject)this.SelectedImportObject).GeometrySourceFolder = folderBrowserDialog.SelectedPath;
		}
	}

	private void SelectGeometryTargetFolder()
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = this._bendMachine.MachinePath + "\\Tools\\",
			Description = "Select folder to store geometry-files."
		};
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			this.SelectedImportObject.GeometryTargetFolder = folderBrowserDialog.SelectedPath;
		}
	}

	private static bool CanCancelClick(object obj)
	{
		return true;
	}

	private void CancelClick(object obj)
	{
		base.CloseView();
	}

	private static bool CanOkClick(object obj)
	{
		return true;
	}

	private void OkClick(object obj)
	{
		ObservableCollection<string> dataSourceFiles = this.SelectedImportObject.DataSourceFiles;
		if (dataSourceFiles != null && dataSourceFiles.Count > 0)
		{
			switch (this._toolType)
			{
			case "Punches":
				this.ImportPunches(this.SelectedImportObject);
				break;
			case "Dies":
				this.ImportDies(this.SelectedImportObject);
				break;
			case "Adapters":
				this.ImportAdapters(this.SelectedImportObject);
				break;
			case "UpperHolders":
			case "LowerHolders":
				this.SelectedImportObject.PunchGeometryTargetFolder = this._bendMachine.MachinePath + this._bendMachine.HolderGeometry;
				this.ImportHolders(this.SelectedImportObject);
				break;
			}
		}
		this._closeAction?.Invoke();
		base.CloseView();
	}

	private void ImportPunches(ImportObject importObject)
	{
		switch (importObject.Type)
		{
		case SourceType.ARV:
		{
			foreach (string dataSourceFile in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfPunch(dataSourceFile, importObject.GeometryTargetFolder, this._bendMachine);
			}
			break;
		}
		case SourceType.MDB:
		{
			foreach (string dataSourceFile2 in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfPunchFromMdb(dataSourceFile2, importObject.GeometryTargetFolder, null, ((MdbImportObject)importObject).GeometrySourceFolder, this._bendMachine);
			}
			break;
		}
		}
	}

	private void ImportDies(ImportObject importObject)
	{
		switch (importObject.Type)
		{
		case SourceType.ARV:
		{
			foreach (string dataSourceFile in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfDie(dataSourceFile, importObject.GeometryTargetFolder, this._bendMachine);
			}
			break;
		}
		case SourceType.MDB:
		{
			foreach (string dataSourceFile2 in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfDieFromMdb(dataSourceFile2, importObject.GeometryTargetFolder, null, ((MdbImportObject)importObject).GeometrySourceFolder, this._bendMachine);
			}
			break;
		}
		}
	}

	private void ImportAdapters(ImportObject importObject)
	{
		switch (importObject.Type)
		{
		case SourceType.ARV:
		{
			foreach (string dataSourceFile in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfAdapter(dataSourceFile, importObject.GeometryTargetFolder, this._bendMachine);
			}
			break;
		}
		case SourceType.MDB:
		{
			foreach (string dataSourceFile2 in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfAdapterFromMdb(dataSourceFile2, importObject.GeometryTargetFolder, null, ((MdbImportObject)importObject).GeometrySourceFolder, this._bendMachine);
			}
			break;
		}
		}
	}

	private void ImportHolders(ImportObject importObject)
	{
		switch (importObject.Type)
		{
		case SourceType.ARV:
		{
			foreach (string dataSourceFile in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfHolder(dataSourceFile, importObject.GeometryTargetFolder, this._bendMachine);
			}
			break;
		}
		case SourceType.MDB:
		{
			foreach (string dataSourceFile2 in importObject.DataSourceFiles)
			{
				ImportTrumpf.ImportTrumpfHolderFromMdb(dataSourceFile2, importObject.GeometryTargetFolder, importObject.PunchGeometryTargetFolder, ((MdbImportObject)importObject).GeometrySourceFolder, this._bendMachine);
			}
			break;
		}
		}
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		this._bendMachine = null;
		this.SelectedImportObject = null;
	}
}

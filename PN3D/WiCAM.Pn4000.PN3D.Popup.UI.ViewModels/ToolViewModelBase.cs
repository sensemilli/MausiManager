using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DxfToCadGeo;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.HealCadGeo;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.LvdConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig;
using Microsoft.Win32;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Helpers.Util;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ToolViewModelBase : ViewModelBase
{
	protected IGlobals _globals;

	private readonly IPnPathService _pnPathService;

	private readonly IModelFactory _modelFactory;

	private ICommand _loadCadGeo;

	private ICommand _importHeelToolTrumpfWzg;

	private ICommand _importMeasureToolTrumpfWzg;

	private ICommand _commonImportToolConfig;

	private ToolConfigModel _toolConfigModel;

	private int _lastFilterIndexInImportOpenFileDialog;

	private FrameworkElement _imageProfile;

	private FrameworkElement _imagePart;

	private const double ToolImageHeight = 424.0;

	private const double ToolImageWidth = 413.0;

	public IToolExpert Tools { get; set; }

	public BendMachine BendMachine { get; set; }

	public ToolConfigModel ToolConfigModel
	{
		get
		{
			return this._toolConfigModel;
		}
		set
		{
			this._toolConfigModel = value;
			base.NotifyPropertyChanged("ToolConfigModel");
		}
	}

	protected object LastSelectedObject { get; set; }

	protected string SelectedType { get; set; }

	public Action CloseActionToolImport { get; set; }

	public ICommand LoadCadGeoCommand => this._loadCadGeo ?? (this._loadCadGeo = new RelayCommand((Action<object>)delegate
	{
		this.LoadCadGeo_Click();
	}));

	public ICommand ImportTrumpfHeelToolWzg => this._importHeelToolTrumpfWzg ?? (this._importHeelToolTrumpfWzg = new RelayCommand((Action<object>)delegate
	{
		this.CreateHeelToolByWzg();
	}));

	public ICommand ImportTrumpfMeasureToolWzg => this._importMeasureToolTrumpfWzg ?? (this._importMeasureToolTrumpfWzg = new RelayCommand((Action<object>)delegate
	{
		this.CreateMeasureToolByWzg();
	}));

	public ICommand CommonImportToolConfigCommand => this._commonImportToolConfig ?? (this._commonImportToolConfig = new RelayCommand((Action<object>)delegate
	{
		this.CommonImportToolConfig();
	}));

	public FrameworkElement ImageProfile
	{
		get
		{
			return this._imageProfile;
		}
		set
		{
			this._imageProfile = value;
			base.NotifyPropertyChanged("ImageProfile");
		}
	}

	public FrameworkElement ImagePart
	{
		get
		{
			return this._imagePart;
		}
		set
		{
			this._imagePart = value;
			base.NotifyPropertyChanged("ImagePart");
		}
	}

	public ToolViewModelBase(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IModelFactory modelFactory)
	{
		this._globals = globals;
		this._pnPathService = pnPathService;
		this._modelFactory = modelFactory;
		int? num = mainWindowDataProvider.GetRecentlyUsedImportRecords().FirstOrDefault()?.ArchiveID;
		this._lastFilterIndexInImportOpenFileDialog = (num.HasValue ? num.Value : 0);
		this.ImageProfile = new Canvas
		{
			Height = 424.0,
			Width = 413.0
		};
		this.ImagePart = new Screen3D();
	}

	private string CreateAllImportFiltersForOpenFileDialog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.RadanXml") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.RadanMdb") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.BystronicXml") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.TrumpfArv") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.TrumpfMdb") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.WzgProfileHeel") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.WzgProfileMeasureTool") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.Dxf") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.LvdXml") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.DelemXml") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.SafanXml") as string);
		return stringBuilder.ToString();
	}

	private void CommonImportToolConfig()
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = this.CreateAllImportFiltersForOpenFileDialog(),
			Title = this._modelFactory.Resolve<ITranslator>().Translate("l_popup.PopupMachineConfig.l_filter.ImportDialogName"),
			FilterIndex = this._lastFilterIndexInImportOpenFileDialog,
			Multiselect = true
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return;
		}
		try
		{
			switch (openFileDialog.FilterIndex)
			{
			case 1:
				this.ContinueImportForRadanXmlFiles(openFileDialog.FileNames);
				break;
			case 2:
				this.ContinueImportForRadanMdbFiles(openFileDialog.FileNames);
				break;
			case 3:
				this.ContinueImportForBystronicXmlFiles(openFileDialog.FileNames);
				break;
			case 4:
				this.ContinueImportForTrumpfArvFiles(openFileDialog.FileNames);
				break;
			case 5:
				this.ContinueImportForTrumpfMdbFiles(openFileDialog.FileNames);
				break;
			case 6:
				this.ContinueImportForWzgProfileHeelFiles(openFileDialog.FileNames);
				break;
			case 7:
				this.ContinueImportForWzgProfileMeasureToolFiles(openFileDialog.FileNames);
				break;
			case 8:
				this.ContinueImportForDxfFiles(openFileDialog.FileNames);
				break;
			case 9:
				this.ContinueImportForLvdXmlFiles(openFileDialog.FileNames);
				break;
			case 10:
				this.ContinueImportForDelemXmlFiles(openFileDialog.FileNames);
				break;
			case 11:
				this.ContinueImportForSafanXmlFiles(openFileDialog.FileNames);
				break;
			}
		}
		catch (Exception ex)
		{
			this._globals.MessageDisplay.ShowTranslatedErrorMessage(ex.Message);
		}
		if (openFileDialog.FilterIndex != this._lastFilterIndexInImportOpenFileDialog)
		{
			this._modelFactory.Resolve<IMainWindowDataProvider>().AddRecentlyUsedRecord(new RecentlyUsedRecord
			{
				FileName = "ImportIndex",
				ArchiveID = openFileDialog.FilterIndex,
				Type = "Imp3D"
			});
		}
		this._lastFilterIndexInImportOpenFileDialog = openFileDialog.FilterIndex;
	}

	public virtual void UpdateSelectedItemGeometry()
	{
	}

	public string GetAndCheckTargetFolder(string type)
	{
		string text = this.BendMachine.MachinePath;
		switch (type)
		{
		case "Punches":
			text += this.BendMachine.UpperToolsGeometry;
			this.GenerateTargetFolder(text);
			break;
		case "Dies":
			text += this.BendMachine.LowerToolsGeometry;
			this.GenerateTargetFolder(text);
			break;
		case "Adapters":
			text += this.BendMachine.AdapterGeometry;
			this.GenerateTargetFolder(text);
			break;
		case "UpperHolders":
		case "LowerHolders":
			text += this.BendMachine.HolderGeometry;
			this.GenerateTargetFolder(text);
			break;
		}
		return text;
	}

	private string CheckLetestImportType()
	{
		if (this.LastSelectedObject == null || string.IsNullOrEmpty(this.SelectedType))
		{
			global::System.Windows.MessageBox.Show("Please select item for update geometry and import file again");
			return string.Empty;
		}
		return this.GetAndCheckTargetFolder(this.SelectedType);
	}

	private void UpdateLatesSelectedObjectWithNewGeometry(string fileName)
	{
		GeometryFileDataViewModel geometryFile = new GeometryFileDataViewModel
		{
			Name = fileName
		};
		this.LastSelectedObject.ToString().Split('.').Last();
		bool flag = true;
		if (this.LastSelectedObject is PunchProfileViewModel)
		{
			((PunchProfileViewModel)this.LastSelectedObject).GeometryFile = geometryFile;
		}
		else if (this.LastSelectedObject is DieProfileViewModel)
		{
			((DieProfileViewModel)this.LastSelectedObject).GeometryFile = geometryFile;
		}
		else if (this.LastSelectedObject is AdapterProfileViewModel)
		{
			((AdapterProfileViewModel)this.LastSelectedObject).GeometryFile = geometryFile;
		}
		else
		{
			global::System.Windows.MessageBox.Show("Item type not support in import dxf");
			flag = false;
		}
		if (flag)
		{
			this.UpdateSelectedItemGeometry();
		}
	}

	private void ContinueImportForDxfFiles(string[] fileNames)
	{
		string text = this.CheckLetestImportType();
		if (!(text == string.Empty) && fileNames.Length >= 0)
		{
			if (fileNames.Length > 1)
			{
				global::System.Windows.MessageBox.Show("Only 1st selected file will be using");
			}
			this.UpdateLatesSelectedObjectWithNewGeometry(DxfToCadGeo.Convert(fileNames[0], text, this._modelFactory));
		}
	}

	private void LoadCadGeo_Click()
	{
		string hardcodeGeometryTargetFolder = this.CheckLetestImportType();
		if (!(hardcodeGeometryTargetFolder == string.Empty))
		{
			ArchiveBrowser br = new ArchiveBrowser();
			br.Start(delegate
			{
				string fileName = HealCadGeo_.CopyAndHeal(br.FullPath, hardcodeGeometryTargetFolder);
				this.UpdateLatesSelectedObjectWithNewGeometry(fileName);
			});
		}
	}

	private void ContinueImportForWzgProfileMeasureToolFiles(string[] fileNames)
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.WzgFront") as string),
			Title = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.ImportDialogName") as string)
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return;
		}
		global::Microsoft.Win32.OpenFileDialog openFileDialog2 = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.WzgDisk") as string),
			Title = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.ImportDialogName") as string)
		};
		if (!openFileDialog2.ShowDialog().Value)
		{
			return;
		}
		foreach (string profileWzgFile in fileNames)
		{
			try
			{
				WzgLoader.CombineWzgToMeasureTool(profileWzgFile, openFileDialog.FileName, openFileDialog2.FileName, "D:\\myModelFinished.c3mo");
			}
			catch (Exception)
			{
			}
		}
	}

	private void ContinueImportForWzgProfileHeelFiles(string[] fileNames)
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.WzgHeel") as string),
			Title = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.ImportDialogName") as string)
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return;
		}
		foreach (string profileWzgFile in fileNames)
		{
			try
			{
				WzgLoader.CombineWzgToHeelTool(profileWzgFile, openFileDialog.FileName, "D:\\myModelFinished.c3mo");
			}
			catch (Exception)
			{
			}
		}
	}

	private void ContinueImportForTrumpfMdbFiles(string[] fileNames)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = Path.GetDirectoryName(fileNames[0]),
			Description = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.SourceGeometryDialogName") as string)
		};
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string text = this.BendMachine.MachinePath;
		string selectedPath = folderBrowserDialog.SelectedPath;
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				text += this.BendMachine.UpperToolsGeometry;
				this.GenerateTargetFolder(text);
				ImportTrumpf.ImportTrumpfPunchFromMdb(filePath, text, null, selectedPath, this.BendMachine);
				break;
			case "Dies":
				text += this.BendMachine.LowerToolsGeometry;
				this.GenerateTargetFolder(text);
				ImportTrumpf.ImportTrumpfDieFromMdb(filePath, text, null, selectedPath, this.BendMachine);
				break;
			case "Adapters":
				text += this.BendMachine.AdapterGeometry;
				this.GenerateTargetFolder(text);
				ImportTrumpf.ImportTrumpfAdapterFromMdb(filePath, text, null, selectedPath, this.BendMachine);
				break;
			case "UpperHolders":
			case "LowerHolders":
			{
				string punchGeometryTargetFolder = text + this.BendMachine.UpperToolsGeometry;
				text += this.BendMachine.HolderGeometry;
				this.GenerateTargetFolder(text);
				ImportTrumpf.ImportTrumpfHolderFromMdb(filePath, text, punchGeometryTargetFolder, selectedPath, this.BendMachine);
				break;
			}
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void GenerateTargetFolder(string targetFolder)
	{
		if (!Directory.Exists(targetFolder))
		{
			Directory.CreateDirectory(targetFolder);
		}
	}

	private void ContinueImportForTrumpfArvFiles(string[] fileNames)
	{
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				ImportTrumpf.ImportTrumpfPunch(filePath, this.BendMachine.MachinePath + this.BendMachine.UpperToolsGeometry, this.BendMachine);
				break;
			case "Dies":
				ImportTrumpf.ImportTrumpfDie(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine);
				break;
			case "Hems":
				ImportTrumpf.ImportTrumpfHem(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine);
				break;
			case "Adapters":
				ImportTrumpf.ImportTrumpfAdapter(filePath, this.BendMachine.MachinePath + this.BendMachine.AdapterGeometry, this.BendMachine);
				break;
			case "UpperHolders":
			case "LowerHolders":
				ImportTrumpf.ImportTrumpfHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void ContinueImportForBystronicXmlFiles(string[] fileNames)
	{
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				ImportBystronic.ImportBystronicPunch(filePath, this.BendMachine.MachinePath + this.BendMachine.UpperToolsGeometry, this.BendMachine);
				break;
			case "Dies":
				ImportBystronic.ImportBystronicDie(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine);
				break;
			case "Adapters":
				ImportBystronic.ImportBystronicAdapter(filePath, this.BendMachine.MachinePath + this.BendMachine.AdapterGeometry, this.BendMachine);
				break;
			case "UpperHolders":
				ImportBystronic.ImportBystronicPunchHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			case "LowerHolders":
				ImportBystronic.ImportBystronicDieHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void ContinueImportForRadanXmlFiles(string[] fileNames)
	{
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				ImportRadan.ImportRadanPunch(filePath, this.BendMachine.MachinePath + this.BendMachine.UpperToolsGeometry, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "Dies":
				ImportRadan.ImportRadanDie(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "Adapters":
				ImportRadan.ImportRadanAdapter(filePath, this.BendMachine.MachinePath + this.BendMachine.AdapterGeometry, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "UpperHolders":
				ImportRadan.ImportRadanPunchHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "LowerHolders":
				ImportRadan.ImportRadanDieHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void ContinueImportForRadanMdbFiles(string[] fileNames)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
		{
			SelectedPath = Path.GetDirectoryName(fileNames[0]),
			Description = (global::System.Windows.Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.SourceGeometryDialogName") as string)
		};
		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string text = this.BendMachine.MachinePath;
		string selectedPath = folderBrowserDialog.SelectedPath;
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				text += this.BendMachine.UpperToolsGeometry;
				this.GenerateTargetFolder(text);
				ImportRadan.ImportRadanPunchFromMdb(filePath, text, null, selectedPath, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "Dies":
				text += this.BendMachine.LowerToolsGeometry;
				this.GenerateTargetFolder(text);
				ImportRadan.ImportRadanDieFromMdb(filePath, text, null, selectedPath, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "Adapters":
				text += this.BendMachine.AdapterGeometry;
				this.GenerateTargetFolder(text);
				ImportRadan.ImportRadanAdapterFromMdb(filePath, text, null, selectedPath, this.BendMachine, this._pnPathService, this._modelFactory);
				break;
			case "UpperHolders":
			{
				string punchGeometryTargetFolder2 = text + this.BendMachine.UpperToolsGeometry;
				text += this.BendMachine.HolderGeometry;
				ImportRadan.ImportRadanHolderFromMdb(filePath, text, punchGeometryTargetFolder2, selectedPath, this.BendMachine, this._pnPathService);
				break;
			}
			case "LowerHolders":
			{
				string punchGeometryTargetFolder = text + this.BendMachine.UpperToolsGeometry;
				text += this.BendMachine.HolderGeometry;
				this.GenerateTargetFolder(text);
				ImportRadan.ImportRadanHolderFromMdb(filePath, text, punchGeometryTargetFolder, selectedPath, this.BendMachine, this._pnPathService);
				break;
			}
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void ContinueImportForLvdXmlFiles(string[] fileNames)
	{
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				ImportLvd.ImportLvdPunch(filePath, this.BendMachine.MachinePath + this.BendMachine.UpperToolsGeometry, this.BendMachine);
				break;
			case "Dies":
				ImportLvd.ImportLvdDie(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine);
				break;
			case "Adapters":
				ImportLvd.ImportLvdAdapter(filePath, this.BendMachine.MachinePath + this.BendMachine.AdapterGeometry, this.BendMachine);
				break;
			case "UpperHolders":
				ImportLvd.ImportLvdPunchHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			case "LowerHolders":
				ImportLvd.ImportLvdDieHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private void ContinueImportForDelemXmlFiles(string[] fileNames)
	{
		Mouse.OverrideCursor = global::System.Windows.Input.Cursors.Wait;
		foreach (string filePath in fileNames)
		{
			switch (this.SelectedType)
			{
			case "Punches":
				ImportDelem.ImportDelemPunch(filePath, this.BendMachine.MachinePath + this.BendMachine.UpperToolsGeometry, this.BendMachine);
				break;
			case "Dies":
				ImportDelem.ImportDelemDie(filePath, this.BendMachine.MachinePath + this.BendMachine.LowerToolsGeometry, this.BendMachine);
				break;
			case "Adapters":
				ImportDelem.ImportDelemAdapter(filePath, this.BendMachine.MachinePath + this.BendMachine.AdapterGeometry, this.BendMachine);
				break;
			case "UpperHolders":
				ImportDelem.ImportDelemPunchHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			case "LowerHolders":
				ImportDelem.ImportDelemDieHolder(filePath, this.BendMachine.MachinePath + this.BendMachine.HolderGeometry, this.BendMachine);
				break;
			}
		}
		this.CloseActionToolImport?.Invoke();
		this.ToolConfigModel = new ToolConfigModel(this.BendMachine, this._globals.Materials);
		Mouse.OverrideCursor = null;
	}

	private bool CreateHeelToolByWzg()
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "WZG files (*.wzg)|*.wzg",
			Title = "Select profile-wzg-file",
			Multiselect = true
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return false;
		}
		global::Microsoft.Win32.OpenFileDialog openFileDialog2 = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "WZG files (*.wzg)|*.wzg",
			Title = "Select heel-wzg-file",
			Multiselect = true
		};
		if (!openFileDialog2.ShowDialog().Value)
		{
			return false;
		}
		try
		{
			return WzgLoader.CombineWzgToHeelTool(openFileDialog.FileName, openFileDialog2.FileName, "D:\\myModelFinished.c3mo");
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool CreateMeasureToolByWzg()
	{
		global::Microsoft.Win32.OpenFileDialog openFileDialog = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "WZG files (*.wzg)|*.wzg",
			Title = "Select profile-wzg-file",
			Multiselect = true
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return false;
		}
		global::Microsoft.Win32.OpenFileDialog openFileDialog2 = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "WZG files (*.wzg)|*.wzg",
			Title = "Select front-wzg-file",
			Multiselect = true
		};
		if (!openFileDialog2.ShowDialog().Value)
		{
			return false;
		}
		global::Microsoft.Win32.OpenFileDialog openFileDialog3 = new global::Microsoft.Win32.OpenFileDialog
		{
			Filter = "WZG files (*.wzg)|*.wzg",
			Title = "Select disc-wzg-file",
			Multiselect = true
		};
		if (!openFileDialog3.ShowDialog().Value)
		{
			return false;
		}
		try
		{
			return WzgLoader.CombineWzgToMeasureTool(openFileDialog.FileName, openFileDialog2.FileName, openFileDialog3.FileName, "D:\\myModelFinished.c3mo");
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void ContinueImportForSafanXmlFiles(string[] ofdFileNames)
	{
		throw new NotImplementedException();
	}
}

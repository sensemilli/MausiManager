using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using BendDataBase.Enums;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.AmadaConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.BendTable;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.BendTable;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Unfold.BendTable.MdbImport;
using MdbImporter;
using Microsoft.Win32;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.BendTable.Contracts;
using WiCAM.Pn4000.BendTable.DataClasses;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;
using WiCAM.Pn4000.Import;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class PopupBendParameterViewModel : PopupViewModelBase
{
	public SubPopupForPopup SubPopup;

	private PopupBendParameterModel _model;

	private readonly IScopedFactorio _modelFactory;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IMaterialManager _materials;

	private readonly IConfigProvider _configProvider;

	private readonly IPnCommandBasics _pnCommandBasics;

	private IBendMachineSimulation _bendMachine;

	private readonly IMachineHelper _machineHelper;

	private TabItem _actualTab;

	private bool _isCopyButtonEnabled;

	private bool _isAddNewButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private ToolConfigModel _toolConfigModel;

	private Action<PopupBendParameterViewModel> _closeAction;

	private double _dialogOpacity;

	private ChangedConfigType _changedConfigType;

	private ChangesResult _changesResult;

	private int _lastFilterIndexInImportOpenFileDialog;

	private readonly IGlobals _globals;

	private readonly ITranslator _translator;

	private Func<BendParamType> _getFixedValue;

	public ToolGroupsViewModel ToolGroupsViewModel { get; set; }

	public PunchesViewModel PunchesViewModel { get; set; }

	public DiesViewModel DiesViewModel { get; set; }

	public DieHemViewModel DieHemViewModel { get; set; }

	public PreferredProfilesViewModel PreferredProfilesViewModel { get; set; }

	public BendDataBaseViewModel BendDataBaseViewModel { get; set; }

	public Visibility ToolTabsVisible { get; set; }

	public TabItem ActualTab
	{
		get
		{
			return _actualTab;
		}
		set
		{
			_actualTab = value;
			OnPropertyChanged("ActualTab");
			SetEditorEnableRules();
		}
	}

	public bool IsCopyButtonEnabled
	{
		get
		{
			return _isCopyButtonEnabled;
		}
		set
		{
			_isCopyButtonEnabled = value;
			OnPropertyChanged("IsCopyButtonEnabled");
		}
	}

	public bool IsAddNewButtonEnabled
	{
		get
		{
			return _isAddNewButtonEnabled;
		}
		set
		{
			_isAddNewButtonEnabled = value;
			OnPropertyChanged("IsAddNewButtonEnabled");
		}
	}

	public bool IsDeleteButtonEnabled
	{
		get
		{
			return _isDeleteButtonEnabled;
		}
		set
		{
			_isDeleteButtonEnabled = value;
			OnPropertyChanged("IsDeleteButtonEnabled");
		}
	}

	public bool IsEditButtonEnabled
	{
		get
		{
			return _isEditButtonEnabled;
		}
		set
		{
			_isEditButtonEnabled = value;
			OnPropertyChanged("IsEditButtonEnabled");
		}
	}

	public bool IsOkButtonEnabled
	{
		get
		{
			return _isOkButtonEnabled;
		}
		set
		{
			_isOkButtonEnabled = value;
			OnPropertyChanged("IsOkButtonEnabled");
		}
	}

	public bool IsCancelButtonEnabled
	{
		get
		{
			return _isCancelButtonEnabled;
		}
		set
		{
			_isCancelButtonEnabled = value;
			OnPropertyChanged("IsCancelButtonEnabled");
		}
	}

	public bool IsSaveButtonEnabled
	{
		get
		{
			return _isSaveButtonEnabled;
		}
		set
		{
			_isSaveButtonEnabled = value;
			OnPropertyChanged("IsSaveButtonEnabled");
		}
	}

	public ToolConfigModel ToolConfigModel
	{
		get
		{
			return _toolConfigModel;
		}
		set
		{
			_toolConfigModel = value;
			OnPropertyChanged("ToolConfigModel");
		}
	}

	public double DialogOpacity
	{
		get
		{
			return _dialogOpacity;
		}
		set
		{
			_dialogOpacity = value;
			OnPropertyChanged("DialogOpacity");
		}
	}

	public PopupBendParameterViewModel(IGlobals globals, ITranslator translator, PopupBendParameterModel popupBendParameterModel, IScopedFactorio modelFactory, IImportMaterialMapper importMaterialMapper, IMaterialManager materials, IMachineHelper machineHelper, IConfigProvider configProvider, IPnCommandBasics pnCommandBasics)
	{
		_globals = globals;
		_translator = translator;
		_model = popupBendParameterModel;
		_modelFactory = modelFactory;
		_importMaterialMapper = importMaterialMapper;
		_materials = materials;
		_machineHelper = machineHelper;
		_configProvider = configProvider;
		_pnCommandBasics = pnCommandBasics;
	}

	public void Init(IDoc3d doc, IBendMachineSimulation bendMachineConfig, Action<PopupBendParameterViewModel> closeAction = null)
	{
		_closeAction = closeAction;
		_bendMachine = bendMachineConfig;
		_model.Init(doc, bendMachineConfig);
		_getFixedValue = () => _model.P3DBendTableValueShowType;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		DialogOpacity = generalUserSettingsConfig.DialogOpacity;
		ToolTabsVisible = Visibility.Collapsed;
		base.Button0_DeleteVisibility = Visibility.Visible;
		base.Button1_EditVisibility = Visibility.Visible;
		base.Button2_AddVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button7_LearnVisibility = Visibility.Visible;
		base.Button8_CopyVisibility = Visibility.Visible;
		base.Button15_PrintVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button7_Learn = System.Windows.Application.Current.FindResource("l_popup.Button7_Save") as string;
		base.Button15_Print = System.Windows.Application.Current.FindResource("l_popup.PopupBendParameter.ImportFromExtern") as string;
		base.Button7_LearnPath = System.Windows.Application.Current.FindResource("popup.Button_SavePath") as Geometry;
		base.Button15_PrintPath = System.Windows.Application.Current.FindResource("popup.Button_ImportPath") as Geometry;
		base.Button0_DeleteClick = new RelayCommand<object>(DeleteButtonClick, CanDeleteButtonClick);
		base.Button1_EditClick = new RelayCommand<object>(EditButtonClick, CanEditButtonClick);
		base.Button2_AddClick = new RelayCommand<object>(AddNewButtonClick, CanAddNewButtonClick);
		base.Button5_CancelClick = new RelayCommand<object>(CancelButtonClick, CanCancelButtonClick);
		base.Button7_LearnClick = new RelayCommand<object>(ApplyButtonClick, CanApplyButtonClick);
		base.Button8_CopyClick = new RelayCommand<object>(CopyButtonClick, CanCopyButtonClick);
		base.Button10_GraphicsClick = new RelayCommand<object>(ExportCsvButtonClick);
		base.Button15_PrintClick = new RelayCommand<object>(ImportExternButtonClick, CanImportExternButtonClick);
		base.Button16_OkClick = new RelayCommand<object>(OkButtonClick, CanOkButtonClick);
		LoadViewModels();
	}

	private static bool CanImportExternButtonClick(object obj)
	{
		return true;
	}

	private string CreateAllImportFiltersForOpenFileDialog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.PnCsv") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.LvdCsv") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.TrumpfMdbOnlyK") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.TrumpfMdbKAndTools") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.AmadaXml") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.DelemTab") as string);
		stringBuilder.Append("|");
		stringBuilder.Append(System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.BystronicXml") as string);
		return stringBuilder.ToString();
	}

	private void ImportExternButtonClick(object obj)
	{
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = CreateAllImportFiltersForOpenFileDialog(),
			Title = (System.Windows.Application.Current.FindResource("l_popup.PopupBendTable.l_filter.ImportDialogName") as string),
			FilterIndex = _lastFilterIndexInImportOpenFileDialog
		};
		if (!openFileDialog.ShowDialog().Value)
		{
			return;
		}
		string text = openFileDialog.FileNames.FirstOrDefault();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		switch (openFileDialog.FilterIndex)
		{
		case 1:
			ImportCsvPN(text);
			break;
		case 2:
			ImportCsvLVD(text);
			break;
		case 3:
			if (_model.Doc?.BendMachineConfig != null)
			{
				ImportMdb(text);
			}
			break;
		case 4:
			if (_model.Doc?.BendMachineConfig != null)
			{
				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
				{
					Description = "Select source folder for mdb geometry-files."
				};
				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					ImportMdb(text, folderBrowserDialog.SelectedPath);
				}
			}
			break;
		case 5:
			ImportAmada(text);
			break;
		case 6:
			ImportDelemTab(text);
			break;
		case 7:
			ImportBystronicXml(text);
			break;
		}
		_lastFilterIndexInImportOpenFileDialog = openFileDialog.FilterIndex;
	}

	private void ImportCsvPN(string path)
	{
		Dictionary<string, string> shema = new Dictionary<string, string>
		{
			{ "MaterialId", "Material" },
			{ "MaterialGroupId", "MaterialGroup" },
			{ "Material3DGroup", "Material3DGroup" },
			{ "Thickness", "Thickness" },
			{ "Angle", "Angle" },
			{ "Radius", "Radius" },
			{ "KFactor", "KFactor" },
			{ "BA", "BA" },
			{ "BD", "BD" },
			{ "Din", "DinLength" },
			{ "MinRadius", "MinRadius" },
			{ "MaxRadius", "MaxRadius" },
			{ "VWidth", "VWidth" },
			{ "CornerRadius", "CornerRadius" },
			{ "VAngle", "VAngle" },
			{ "PunchRadius", "PunchRadius" },
			{ "Tag", "Tag" },
			{ "BendLengthMin", "BendLengthMin" },
			{ "BendLengthMax", "BendLengthMax" }
		};
		List<VisualBendZoneDataBaseItem> list = UniversalCsvImporter.ImportCSV<VisualBendZoneDataBaseItem>(path, shema);
		int num = 0;
		List<VisualBendZoneDataBaseItem> list2 = new List<VisualBendZoneDataBaseItem>();
		foreach (VisualBendZoneDataBaseItem item in list)
		{
			double result;
			bool num2 = double.TryParse(item.KFactor, out result);
			double result2;
			bool flag = double.TryParse(item.BA, out result2);
			double result3;
			bool flag2 = double.TryParse(item.BD, out result3);
			double result4;
			bool flag3 = double.TryParse(item.DinLength, out result4);
			if (num2 || flag || flag2 || flag3)
			{
				list2.Add(item);
				num++;
			}
		}
		list2.ForEach(delegate(VisualBendZoneDataBaseItem i)
		{
			i.SetFixedValueFunc(_getFixedValue);
		});
		HashSet<VisualBendZoneDataBaseItem> hashSet = new HashSet<VisualBendZoneDataBaseItem>(BendDataBaseViewModel.Items.Concat(list2));
		bool flag4 = false;
		foreach (VisualBendZoneDataBaseItem item2 in hashSet)
		{
			item2.Material3DGroup = _model.Globals.Materials.GetClosestMaterial3DGroupName(item2.Material3DGroup);
			if (item2.Material3DGroup == _materials.MaterialErrorString)
			{
				flag4 = true;
			}
		}
		if (flag4)
		{
			string caption = System.Windows.Application.Current.TryFindResource("l_popup.PopupBendParameter.Question") as string;
			string message = System.Windows.Application.Current.TryFindResource("l_popup.PopupBendParameter.MaterialConvertionError") as string;
			if (SubPopup.ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}
		}
		BendDataBaseViewModel.Items = hashSet.ToObservableCollection();
		_globals.MessageDisplay.ShowInformationMessage(_translator.Translate("l_popup.PopupBendParameter.InfoAddedItems", num, list.Count));
	}

	private bool MaterialMapping<T>(IEnumerable<T> items, Func<T, string> getMaterial, out Dictionary<string, int> matMapping)
	{
		List<(string, int, int)> list = (from x in items.Select(getMaterial)
			group x by x into gr
			select (matImport: gr.Key, matPn: _materials.GetMaterial3DGroupNameByMaterialName(gr.Key), count: gr.Count())).ToList();
		matMapping = list.ToDictionary<(string, int, int), string, int>(((string matImport, int matPn, int count) x) => x.matImport, ((string matImport, int matPn, int count) x) => x.matPn);
		if (list.Any<(string, int, int)>(((string matImport, int matPn, int count) x) => x.matPn < 0))
		{
			PopupMaterialMappingViewModel popupMaterialMappingViewModel = new PopupMaterialMappingViewModel(list, _materials.Material3DGroup, _translator);
			PopupMaterialMappingView popupMaterialMappingView = new PopupMaterialMappingView();
			popupMaterialMappingView.DataContext = popupMaterialMappingViewModel;
			popupMaterialMappingView.ShowDialog();
			if (popupMaterialMappingView.CancelByUser)
			{
				return false;
			}
			matMapping = popupMaterialMappingViewModel.GetResult();
		}
		return true;
	}

	private void ImportCsvLVD(string path)
	{
		Dictionary<string, string> shema = new Dictionary<string, string>
		{
			{ "MaterialNorm", "Material" },
			{ "MaterialNormID", "Material" },
			{ "Thickness", "Thickness" },
			{ "BendAngle", "Angle" },
			{ "Ri", "Radius" },
			{ "Ba", "BD" },
			{ "DieVWidth", "VWidth" },
			{ "DieRollRadius", "CornerRadius" },
			{ "DieAngle", "VAngle" },
			{ "PunchRadius", "PunchRadius" },
			{ "Punch", "Punch" },
			{ "Die", "Die" },
			{ "Sb", "SpringBack" }
		};
		IMessageLogGlobal messageDisplay = _globals.MessageDisplay;
		List<BendZoneDataBaseItemLVD> list = UniversalCsvImporter.ImportCSV<BendZoneDataBaseItemLVD>(path, shema);
		foreach (BendZoneDataBaseItemLVD item in list)
		{
			item.SpringBack = 0.0 - Math.Abs(item.SpringBack);
		}
		if (MaterialMapping(list, (BendZoneDataBaseItemLVD x) => x.Material, out Dictionary<string, int> matMapping))
		{
			List<VisualBendZoneDataBaseItem> source = ConvertLvdToPn(list, matMapping);
			source = source.Where((VisualBendZoneDataBaseItem i) => double.TryParse(i.KFactor, out var _) || double.TryParse(i.BA, out var _) || double.TryParse(i.BD, out var _) || double.TryParse(i.DinLength, out var _)).ToList();
			source.ForEach(delegate(VisualBendZoneDataBaseItem i)
			{
				i.SetFixedValueFunc(_getFixedValue);
			});
			HashSet<VisualBendZoneDataBaseItem> enumerable = new HashSet<VisualBendZoneDataBaseItem>(BendDataBaseViewModel.Items.Concat(source));
			BendDataBaseViewModel.Items = enumerable.ToObservableCollection();
			messageDisplay.ShowInformationMessage(_translator.Translate("l_popup.PopupBendParameter.InfoAddedItems", source.Count, list.Count));
		}
	}

	private List<VisualBendZoneDataBaseItem> ConvertLvdToPn(List<BendZoneDataBaseItemLVD> items, Dictionary<string, int> matMapping)
	{
		List<VisualBendZoneDataBaseItem> list = new List<VisualBendZoneDataBaseItem>();
		foreach (BendZoneDataBaseItemLVD item in items)
		{
			bool flag = false;
			int num = matMapping[item.Material];
			if (num < 0)
			{
				continue;
			}
			string material3DGroupName = _model.Globals.Materials.GetMaterial3DGroupName(num);
			if (material3DGroupName == _materials.MaterialErrorString)
			{
				flag = true;
			}
			if (flag)
			{
				string caption = System.Windows.Application.Current.TryFindResource("l_popup.PopupBendParameter.Question") as string;
				string message = System.Windows.Application.Current.TryFindResource("l_popup.PopupBendParameter.MaterialConvertionError") as string;
				if (SubPopup.ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				{
					continue;
				}
			}
			try
			{
				VisualBendZoneDataBaseItem visualBendZoneDataBaseItem = new VisualBendZoneDataBaseItem(_getFixedValue)
				{
					Material3DGroup = material3DGroupName,
					Thickness = convertNumber(item.Thickness),
					Angle = (180.0 - Convert.ToDouble(item.Angle.Replace(',', '.'), CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
					Radius = convertNumber(item.Radius),
					MinRadius = "*",
					MaxRadius = "*",
					VWidth = convertNumber(item.VWidth),
					VAngle = convertNumber(item.VAngle),
					CornerRadius = convertNumber(item.CornerRadius),
					PunchRadius = convertNumber(item.PunchRadius),
					PunchAngle = "*",
					Tag = string.Empty,
					BendLengthMin = "*",
					BendLengthMax = "*",
					SpringBack = (0.0 - item.SpringBack) * Math.PI / 180.0
				};
				if (!string.IsNullOrEmpty(item.BD))
				{
					visualBendZoneDataBaseItem.DinLength = (0.0 - Convert.ToDouble(item.BD.Replace(',', '.'), CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
					list.Add(visualBendZoneDataBaseItem);
				}
			}
			catch
			{
			}
		}
		return list;
		static string convertNumber(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return "*";
			}
			return str.Replace(',', '.');
		}
	}

	private void ImportBystronicXml(string path)
	{
		BystronicBendTable bystronicBendTable = new BystronicBendTable().Deserialize(path);
		List<VisualBendZoneDataBaseItem> list = new List<VisualBendZoneDataBaseItem>();
		string closestMaterial3DGroupName = _materials.GetClosestMaterial3DGroupName(bystronicBendTable.Material.Name);
		BBXMaterialDataTable[] dataTables = bystronicBendTable.Material.DataTables;
		foreach (BBXMaterialDataTable bBXMaterialDataTable in dataTables)
		{
			BBXMaterialDataTableDTEntry[] dTEntries = bBXMaterialDataTable.DTEntries;
			foreach (BBXMaterialDataTableDTEntry bBXMaterialDataTableDTEntry in dTEntries)
			{
				VisualBendZoneDataBaseItem item = new VisualBendZoneDataBaseItem(_getFixedValue)
				{
					Material3DGroup = closestMaterial3DGroupName,
					Thickness = new DoubleValidType(bBXMaterialDataTable.SheetThickness).GetString(),
					Angle = new DoubleValidType(bBXMaterialDataTableDTEntry.BendAngle).GetString(),
					Radius = new DoubleValidType(bBXMaterialDataTable.SheetThickness).GetString(),
					MinRadius = "*",
					MaxRadius = "*",
					VWidth = new DoubleValidType(bBXMaterialDataTable.DieOpeningWidth).GetString(),
					VAngle = "*",
					CornerRadius = new DoubleValidType(bBXMaterialDataTable.DieOpeningRadius).GetString(),
					PunchRadius = new DoubleValidType(bBXMaterialDataTable.PunchRadius).GetString(),
					PunchAngle = "*",
					BendLengthMin = "*",
					BendLengthMax = "*",
					Tag = string.Empty,
					BD = new DoubleValidType(bBXMaterialDataTableDTEntry.WG).GetString(),
					IsChanged = false
				};
				list.Add(item);
			}
		}
		HashSet<VisualBendZoneDataBaseItem> enumerable = new HashSet<VisualBendZoneDataBaseItem>(BendDataBaseViewModel.Items.Concat(list));
		BendDataBaseViewModel.Items = enumerable.ToObservableCollection();
	}

	private void ImportDelemTab(string path)
	{
		string text = Path.Combine(Path.GetDirectoryName(path), "userpreferences.dld");
		if (!File.Exists(text))
		{
			System.Windows.MessageBox.Show("File " + text + " not exists!");
			return;
		}
		List<DelemBendTableItem> list = DelemBendTable.Import(path, text);
		if (!MaterialMapping(list, (DelemBendTableItem x) => x.MaterialName, out Dictionary<string, int> matMapping))
		{
			return;
		}
		List<VisualBendZoneDataBaseItem> list2 = new List<VisualBendZoneDataBaseItem>();
		foreach (DelemBendTableItem item2 in list)
		{
			string empty = string.Empty;
			int num = matMapping[item2.MaterialName];
			if (num >= 0)
			{
				empty = _model.Globals.Materials.GetMaterial3DGroupName(num);
				VisualBendZoneDataBaseItem item = new VisualBendZoneDataBaseItem(_getFixedValue)
				{
					Material3DGroup = empty,
					Thickness = new DoubleValidType(item2.Thickness).GetString(),
					Angle = new DoubleValidType(180.0 - item2.Angle).GetString(),
					Radius = new DoubleValidType(item2.Thickness).GetString(),
					MinRadius = "*",
					MaxRadius = "*",
					VWidth = new DoubleValidType(item2.V).GetString(),
					VAngle = new DoubleValidType(item2.P).GetString(),
					CornerRadius = new DoubleValidType(item2.D).GetString(),
					PunchRadius = new DoubleValidType(item2.Radius).GetString(),
					PunchAngle = "*",
					BendLengthMin = "*",
					BendLengthMax = "*",
					Tag = string.Empty,
					DinLength = new DoubleValidType(item2.C).GetString(),
					IsChanged = false
				};
				list2.Add(item);
			}
		}
		list2 = list2.Where((VisualBendZoneDataBaseItem i) => double.TryParse(i.KFactor, out var _)).ToList();
		HashSet<VisualBendZoneDataBaseItem> enumerable = new HashSet<VisualBendZoneDataBaseItem>(BendDataBaseViewModel.Items.Concat(list2));
		BendDataBaseViewModel.Items = enumerable.ToObservableCollection();
	}

	private void ImportAmada(string path)
	{
		ArrayOfBendInfo arrayOfBendInfo = AmadaBendTable.Import(path);
		List<VisualBendZoneDataBaseItem> list = new List<VisualBendZoneDataBaseItem>();
		ArrayOfBendInfoBendInfo[] bendInfo = arrayOfBendInfo.BendInfo;
		foreach (ArrayOfBendInfoBendInfo arrayOfBendInfoBendInfo in bendInfo)
		{
			DeductionInfo[] deductionInfoList = arrayOfBendInfoBendInfo.DeductionInfoList;
			foreach (DeductionInfo deductionInfo in deductionInfoList)
			{
				int material3DGroupNameByMaterialName = _materials.GetMaterial3DGroupNameByMaterialName(arrayOfBendInfoBendInfo.MaterialName);
				string name = _materials.GetMaterial3DGroupName(material3DGroupNameByMaterialName);
				if (material3DGroupNameByMaterialName < 0)
				{
					material3DGroupNameByMaterialName = _importMaterialMapper.GetMaterialId(arrayOfBendInfoBendInfo.MaterialName);
					IMaterialArt materialById = _materials.GetMaterialById(material3DGroupNameByMaterialName);
					name = ((materialById != null) ? _materials.GetMaterial3DGroupName(materialById.MaterialGroupForBendDeduction) : string.Empty);
				}
				int material3DGroupIdByName = _materials.GetMaterial3DGroupIdByName(name);
				VisualBendZoneDataBaseItem item = new VisualBendZoneDataBaseItem(_getFixedValue)
				{
					Material3DGroup = new IntValidType(material3DGroupIdByName).GetString(),
					Thickness = new DoubleValidType(arrayOfBendInfoBendInfo.Thickness).GetString(),
					Angle = new DoubleValidType(deductionInfo.Angle).GetString(),
					Radius = new DoubleValidType(arrayOfBendInfoBendInfo.IR).GetString(),
					MinRadius = "*",
					MaxRadius = "*",
					VWidth = new DoubleValidType(arrayOfBendInfoBendInfo.VWidth).GetString(),
					VAngle = "*",
					CornerRadius = "*",
					PunchRadius = "*",
					PunchAngle = "*",
					BendLengthMin = "*",
					BendLengthMax = "*",
					Tag = arrayOfBendInfoBendInfo.Comment,
					DinLength = new DoubleValidType(0.0 - deductionInfo.DeductionValue).GetString(),
					IsChanged = false
				};
				list.Add(item);
			}
		}
		list = list.Where((VisualBendZoneDataBaseItem i) => double.TryParse(i.KFactor, out var _)).ToList();
		HashSet<VisualBendZoneDataBaseItem> enumerable = new HashSet<VisualBendZoneDataBaseItem>(BendDataBaseViewModel.Items.Concat(list));
		BendDataBaseViewModel.Items = enumerable.ToObservableCollection();
	}

	private void ExportCsvButtonClick(object obj)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			IEnumerable<BendTableEntityCsv> objectlist = BendDataBaseViewModel.Items.Select((VisualBendZoneDataBaseItem entity) => new BendTableEntityCsv
			{
				MaterialId = "*",
				MaterialGroupId = "",
				Material3DGroup = entity.Material3DGroup,
				Thickness = new DoubleValidType(entity.Thickness).GetString(),
				Angle = new DoubleValidType(entity.Angle).GetString(),
				Radius = new DoubleValidType(entity.Radius).GetString(),
				KFactor = new DoubleValidType(entity.KFactor).GetString(),
				BA = new DoubleValidType(entity.BA).GetString(),
				BD = new DoubleValidType(entity.BD).GetString(),
				Din = new DoubleValidType(entity.DinLength).GetString(),
				MinRadius = new DoubleValidType(entity.MinRadius).GetString(),
				MaxRadius = new DoubleValidType(entity.MaxRadius).GetString(),
				VWidth = new DoubleValidType(entity.VWidth).GetString(),
				VAngle = new DoubleValidType(entity.VAngle).GetString(),
				CornerRadius = new DoubleValidType(entity.CornerRadius).GetString(),
				PunchRadius = new DoubleValidType(entity.PunchRadius).GetString(),
				BendLengthMin = new DoubleValidType(entity.BendLengthMin).GetString(),
				BendLengthMax = new DoubleValidType(entity.BendLengthMax).GetString(),
				Tag = entity.Tag
			});
			UniversalCsvExporter.ExportCsv(folderBrowserDialog.SelectedPath + "\\bendTable.csv", ";", objectlist);
		}
	}

	private bool DoubleHasValue(double value)
	{
		if (!double.IsNaN(value))
		{
			return !double.IsInfinity(value);
		}
		return false;
	}

	private void ImportMdb(string path, string geometrySourceFolder = null)
	{
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(path, "AKF_ABKANTFAKTOR", typeof(AKF_ABKANTFAKTOR));
		IEnumerable<object> punchGroupEntries = UniversalMdbImporter.ImportTable(path, "WZG_OW_GRUPPE", typeof(WZG_OW_GRUPPE));
		List<object> list = UniversalMdbImporter.ImportTable(path, "WZG_OW", typeof(WZG_OW)).ToList();
		List<object> list2 = UniversalMdbImporter.ImportTable(path, "WZG_OW_ATTRIBUTE", typeof(WZG_OW_ATTRIBUTE)).ToList();
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(path, "WZG_TASTSCHEIBEN", typeof(WZG_TASTSCHEIBEN));
		IEnumerable<object> dieGroupEntries = UniversalMdbImporter.ImportTable(path, "WZG_MA_GRUPPE", typeof(WZG_MA_GRUPPE));
		IEnumerable<object> enumerable2 = UniversalMdbImporter.ImportTable(path, "WZG_MA", typeof(WZG_MA));
		IEnumerable<object> enumerable3 = UniversalMdbImporter.ImportTable(path, "WZG_MA_ATTRIBUTE", typeof(WZG_MA_ATTRIBUTE));
		IEnumerable<object> enumerable4 = UniversalMdbImporter.ImportTable(path, "WZG_FALZ", typeof(WZG_FALZ));
		IEnumerable<object> enumerable5 = UniversalMdbImporter.ImportTable(path, "WZG_FALZ_ATTRIBUTE", typeof(WZG_FALZ_ATTRIBUTE));
		List<AKF_ABKANTFAKTOR> list3 = (from x in source
			select (AKF_ABKANTFAKTOR)x into e
			where e.AKF_VERFAHREN == 2 || e.AKF_VERFAHREN == 0
			select e).ToList();
		if (!MaterialMapping(list3, (AKF_ABKANTFAKTOR x) => x.AKF_MATERIAL_NAME, out Dictionary<string, int> matMapping))
		{
			return;
		}
		(from e in list3
			select GetNewBendTableEntity(e, punchGroupEntries, dieGroupEntries, matMapping) into x
			where x != null
			select x).ToList();
		throw new NotImplementedException();
	}

	private IBendTableEntity GetNewBendTableEntity(AKF_ABKANTFAKTOR akf, IEnumerable<object> punchGroupEntries, IEnumerable<object> dieGroupEntries, Dictionary<string, int> matMapping)
	{
		WZG_OW_GRUPPE wZG_OW_GRUPPE = (WZG_OW_GRUPPE)punchGroupEntries.FirstOrDefault((object e) => ((WZG_OW_GRUPPE)e).WZG_OW_GRUPPE_ID == akf.AKF_OW_GRUPPE);
		WZG_MA_GRUPPE wZG_MA_GRUPPE = (WZG_MA_GRUPPE)dieGroupEntries.FirstOrDefault((object e) => ((WZG_MA_GRUPPE)e).WZG_MA_GRUPPE_ID == akf.AKF_MA_GRUPPE);
		double num = ((akf.AKF_WINKEL == 180.0) ? 180.0 : (180.0 - akf.AKF_WINKEL));
		double init = BendDataCalculator.KFactorFromDinLength(akf.AKF_MATERIAL_DICKE, num, akf.AKF_RADIUS, akf.AKF_FAKTOR);
		int num2 = matMapping[akf.AKF_MATERIAL_NAME];
		if (num2 < 0)
		{
			return null;
		}
		IntValidType material3DGroupID = new IntValidType(num2);
		return new BendTableEntity
		{
			Material3DGroupID = material3DGroupID,
			Thickness = new DoubleValidType(akf.AKF_MATERIAL_DICKE),
			Angle = new DoubleValidType(num),
			R = new DoubleValidType(akf.AKF_RADIUS),
			KFactor = new DoubleValidType(init),
			MinRadius = new DoubleValidType("*"),
			MaxRadius = new DoubleValidType("*"),
			VWidth = ((wZG_MA_GRUPPE != null) ? new DoubleValidType(wZG_MA_GRUPPE.WZG_MA_GESENKWEITE) : new DoubleValidType("*")),
			CornerRadius = new DoubleValidType("*"),
			PunchRadius = ((wZG_OW_GRUPPE != null) ? new DoubleValidType(wZG_OW_GRUPPE.WZG_OW_RADIUS) : new DoubleValidType("*")),
			PunchAngle = new DoubleValidType("*"),
			BendLengthMin = new DoubleValidType("*"),
			BendLengthMax = new DoubleValidType("*"),
			SpringBack = null,
			Tag = ""
		};
	}

	private void LoadViewModels()
	{
		BendMachine bendMachine = _bendMachine?.BendMachine;
		ToolConfigModel toolModel = new ToolConfigModel(bendMachine, _materials);
		if (_bendMachine != null)
		{
			PunchesViewModel = _modelFactory.Resolve<PunchesViewModel>();
			PunchesViewModel.Init(bendMachine, toolModel);
			PunchesViewModel.DataChanged = dataChangedAction;
			PunchesViewModel.PropertyChanged += PropertyChanged;
			DiesViewModel = _modelFactory.Resolve<DiesViewModel>();
			DiesViewModel.Init(bendMachine, toolModel);
			DiesViewModel.DataChanged = dataChangedAction;
			DiesViewModel.PropertyChanged += PropertyChanged;
			DieHemViewModel = _modelFactory.Resolve<DieHemViewModel>();
			DieHemViewModel.Init(bendMachine, toolModel);
			DieHemViewModel.DataChanged = dataChangedAction;
			DieHemViewModel.PropertyChanged += PropertyChanged;
			ToolGroupsViewModel = _modelFactory.Resolve<ToolGroupsViewModel>();
			ToolGroupsViewModel.Init(bendMachine, toolModel);
			ToolGroupsViewModel.DataChanged = dataChangedAction;
			ToolGroupsViewModel.PropertyChanged += PropertyChanged;
			PunchesViewModel.CloseActionToolImport = delegate
			{
				toolModel = new ToolConfigModel(bendMachine, _materials);
				ToolGroupsViewModel.ToolConfigModel = toolModel;
				PunchesViewModel.ToolConfigModel = toolModel;
				DiesViewModel.ToolConfigModel = toolModel;
				DieHemViewModel.ToolConfigModel = toolModel;
				PreferredProfilesViewModel.ToolConfigModel = toolModel;
				PunchesViewModel.LoadTools();
			};
			DiesViewModel.CloseActionToolImport = delegate
			{
				toolModel = new ToolConfigModel(bendMachine, _materials);
				ToolGroupsViewModel.ToolConfigModel = toolModel;
				PunchesViewModel.ToolConfigModel = toolModel;
				DiesViewModel.ToolConfigModel = toolModel;
				DieHemViewModel.ToolConfigModel = toolModel;
				PreferredProfilesViewModel.ToolConfigModel = toolModel;
				DiesViewModel.LoadTools();
			};
			DieHemViewModel.CloseActionToolImport = delegate
			{
				toolModel = new ToolConfigModel(bendMachine, _materials);
				ToolGroupsViewModel.ToolConfigModel = toolModel;
				PunchesViewModel.ToolConfigModel = toolModel;
				DieHemViewModel.ToolConfigModel = toolModel;
				DiesViewModel.ToolConfigModel = toolModel;
				PreferredProfilesViewModel.ToolConfigModel = toolModel;
				DieHemViewModel.LoadTools();
			};
			ToolTabsVisible = Visibility.Visible;
		}
		BendDataBaseViewModel = _modelFactory.Resolve<BendDataBaseViewModel>().Init(toolModel, _bendMachine);
		BendDataBaseViewModel.DataChanged = dataChangedAction;
		BendDataBaseViewModel.PropertyChanged += PropertyChanged;
		OnPropertyChanged("PunchesViewModel");
		OnPropertyChanged("DiesViewModel");
		OnPropertyChanged("DieHemViewModel");
		OnPropertyChanged("PreferredProfilesViewModel");
		OnPropertyChanged("ToolGroupsViewModel");
		OnPropertyChanged("BendDataBaseViewModel");
		void dataChangedAction(ChangedConfigType changed)
		{
			_changedConfigType |= changed;
		}
	}

	private new void PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "IsDeleteButtonEnabled" || e.PropertyName == "IsAddNewButtonEnabled" || e.PropertyName == "IsAddButtonEnabled" || e.PropertyName == "IsCopyButtonEnabled" || e.PropertyName == "IsEditButtonEnabled" || e.PropertyName == "IsOkButtonEnabled" || e.PropertyName == "IsCancelButtonEnabled" || e.PropertyName == "IsSaveButtonEnabled" || e.PropertyName == "SelectedIndexProfiles" || e.PropertyName == "SelectedIndexParts" || e.PropertyName == "SelectedIndexPunchGroups" || e.PropertyName == "SelectedIndexDieGroups")
		{
			SetEditorEnableRules();
		}
	}

	public void SetEditorEnableRules()
	{
		switch (ActualTab?.Name)
		{
		case "BendTableTab":
			IsAddNewButtonEnabled = !BendDataBaseViewModel.IsToolEditorMode && BendDataBaseViewModel.IsAddButtonEnabled;
			IsCopyButtonEnabled = !BendDataBaseViewModel.IsToolEditorMode && BendDataBaseViewModel.IsCopyButtonEnabled;
			IsDeleteButtonEnabled = !BendDataBaseViewModel.IsToolEditorMode && BendDataBaseViewModel.IsDeleteButtonEnabled;
			IsEditButtonEnabled = false;
			IsOkButtonEnabled = true;
			IsCancelButtonEnabled = !BendDataBaseViewModel.IsToolEditorMode;
			IsSaveButtonEnabled = true;
			break;
		case "GroupsTab":
			IsAddNewButtonEnabled = ToolGroupsViewModel.IsAddButtonEnabled;
			IsCopyButtonEnabled = ToolGroupsViewModel.IsCopyButtonEnabled;
			IsDeleteButtonEnabled = ToolGroupsViewModel.IsDeleteButtonEnabled;
			IsEditButtonEnabled = ToolGroupsViewModel.IsEditButtonEnabled;
			IsOkButtonEnabled = ToolGroupsViewModel.IsOkButtonEnabled;
			IsCancelButtonEnabled = ToolGroupsViewModel.IsCancelButtonEnabled;
			IsSaveButtonEnabled = ToolGroupsViewModel.IsSaveButtonEnabled;
			break;
		case "PreferredToolsTab":
			IsAddNewButtonEnabled = PreferredProfilesViewModel.IsAddButtonEnabled;
			IsDeleteButtonEnabled = PreferredProfilesViewModel.IsDeleteButtonEnabled;
			IsCopyButtonEnabled = PreferredProfilesViewModel.IsCopyButtonEnabled;
			IsEditButtonEnabled = PreferredProfilesViewModel.IsEditButtonEnabled;
			IsOkButtonEnabled = PreferredProfilesViewModel.IsOkButtonEnabled;
			IsCancelButtonEnabled = PreferredProfilesViewModel.IsCancelButtonEnabled;
			IsSaveButtonEnabled = PreferredProfilesViewModel.IsSaveButtonEnabled;
			break;
		case "PunchesTab":
			IsAddNewButtonEnabled = PunchesViewModel.IsAddButtonEnabled;
			IsDeleteButtonEnabled = PunchesViewModel.IsDeleteButtonEnabled;
			IsCopyButtonEnabled = PunchesViewModel.IsCopyButtonEnabled;
			IsEditButtonEnabled = PunchesViewModel.IsEditButtonEnabled;
			IsOkButtonEnabled = PunchesViewModel.IsOkButtonEnabled;
			IsCancelButtonEnabled = PunchesViewModel.IsCancelButtonEnabled;
			IsSaveButtonEnabled = PunchesViewModel.IsSaveButtonEnabled;
			break;
		case "DiesTab":
			IsAddNewButtonEnabled = DiesViewModel.IsAddButtonEnabled;
			IsDeleteButtonEnabled = DiesViewModel.IsDeleteButtonEnabled;
			IsCopyButtonEnabled = DiesViewModel.IsCopyButtonEnabled;
			IsEditButtonEnabled = DiesViewModel.IsEditButtonEnabled;
			IsEditButtonEnabled = DiesViewModel.IsEditButtonEnabled;
			IsOkButtonEnabled = DiesViewModel.IsOkButtonEnabled;
			IsCancelButtonEnabled = DiesViewModel.IsCancelButtonEnabled;
			IsSaveButtonEnabled = DiesViewModel.IsSaveButtonEnabled;
			break;
		case "HemsTab":
			IsAddNewButtonEnabled = DieHemViewModel.IsAddButtonEnabled;
			IsDeleteButtonEnabled = DieHemViewModel.IsDeleteButtonEnabled;
			IsCopyButtonEnabled = DieHemViewModel.IsCopyButtonEnabled;
			IsEditButtonEnabled = DieHemViewModel.IsEditButtonEnabled;
			IsOkButtonEnabled = DieHemViewModel.IsOkButtonEnabled;
			IsCancelButtonEnabled = DieHemViewModel.IsCancelButtonEnabled;
			IsSaveButtonEnabled = DieHemViewModel.IsSaveButtonEnabled;
			break;
		default:
			IsCopyButtonEnabled = false;
			IsAddNewButtonEnabled = false;
			IsDeleteButtonEnabled = false;
			IsEditButtonEnabled = false;
			IsOkButtonEnabled = true;
			IsCancelButtonEnabled = true;
			IsSaveButtonEnabled = true;
			break;
		}
	}

	private static bool CanApplyButtonClick(object obj)
	{
		return true;
	}

	private bool CanDeleteButtonClick(object obj)
	{
		return IsDeleteButtonEnabled;
	}

	private bool CanAddNewButtonClick(object obj)
	{
		return IsAddNewButtonEnabled;
	}

	private bool CanCopyButtonClick(object obj)
	{
		return IsCopyButtonEnabled;
	}

	private static bool CanCancelButtonClick(object obj)
	{
		return true;
	}

	private static bool CanOkButtonClick(object obj)
	{
		return true;
	}

	private bool CanEditButtonClick(object obj)
	{
		return IsEditButtonEnabled;
	}

	private void DeleteButtonClick(object obj)
	{
		switch (ActualTab.Name)
		{
		case "GroupsTab":
			if (ToolGroupsViewModel.LastSelectedType == typeof(PunchGroup))
			{
				ToolGroupsViewModel.Delete_Click(ToolGroupsViewModel.SelectedPunchGroup);
			}
			else if (ToolGroupsViewModel.LastSelectedType == typeof(DieGroup))
			{
				ToolGroupsViewModel.Delete_Click(ToolGroupsViewModel.SelectedDieGroup);
			}
			break;
		case "BendTableTab":
			BendDataBaseViewModel.DeleteButtonClick();
			break;
		case "PreferredToolsTab":
			PreferredProfilesViewModel.DeleteButtonClick();
			break;
		case "PunchesTab":
			if (PunchesViewModel.LastSelectedType == typeof(PunchPart))
			{
				PunchesViewModel.Delete_Click(PunchesViewModel.SelectedPart);
			}
			else if (PunchesViewModel.LastSelectedType == typeof(PunchProfile))
			{
				PunchesViewModel.Delete_Click(PunchesViewModel.SelectedProfile);
			}
			else if (PunchesViewModel.LastSelectedType == typeof(SensorDisk))
			{
				PunchesViewModel.Delete_Click(PunchesViewModel.SelectedDisk);
			}
			break;
		case "DiesTab":
			if (DiesViewModel.LastSelectedType == typeof(DiePart))
			{
				DiesViewModel.Delete_Click(DiesViewModel.SelectedPart);
			}
			else if (DiesViewModel.LastSelectedType == typeof(DieProfile))
			{
				DiesViewModel.Delete_Click(DiesViewModel.SelectedProfile);
			}
			break;
		case "HemsTab":
			if (DieHemViewModel.LastSelectedType == typeof(HemPart))
			{
				DieHemViewModel.Delete_Click(DieHemViewModel.SelectedPart);
			}
			else if (DieHemViewModel.LastSelectedType == typeof(HemProfile))
			{
				DieHemViewModel.Delete_Click(DieHemViewModel.SelectedProfile);
			}
			break;
		}
		SetEditorEnableRules();
	}

	private void AddNewButtonClick(object obj)
	{
		switch (ActualTab.Name)
		{
		case "GroupsTab":
			if (ToolGroupsViewModel.LastSelectedType == typeof(PunchGroup))
			{
				ToolGroupsViewModel.AddPunchGroup_Click();
			}
			else if (ToolGroupsViewModel.LastSelectedType == typeof(DieGroup))
			{
				ToolGroupsViewModel.AddDieGroup_Click();
			}
			break;
		case "BendTableTab":
			BendDataBaseViewModel.AddButtonClick();
			break;
		case "PreferredToolsTab":
			PreferredProfilesViewModel.AddButtonClick();
			break;
		case "PunchesTab":
			if (PunchesViewModel.LastSelectedType == typeof(PunchPart))
			{
				PunchesViewModel.AddPart_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(PunchProfile))
			{
				PunchesViewModel.AddProfile_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(SensorDisk))
			{
				PunchesViewModel.AddDisk_Click();
			}
			break;
		case "DiesTab":
			if (DiesViewModel.LastSelectedType == typeof(DiePart))
			{
				DiesViewModel.AddPart_Click();
			}
			else if (DiesViewModel.LastSelectedType == typeof(DieProfile))
			{
				DiesViewModel.AddProfile_Click();
			}
			break;
		case "HemsTab":
			if (DieHemViewModel.LastSelectedType == typeof(HemPart))
			{
				DieHemViewModel.AddPart_Click();
			}
			else if (DieHemViewModel.LastSelectedType == typeof(HemProfile))
			{
				DieHemViewModel.AddProfile_Click();
			}
			break;
		}
		SetEditorEnableRules();
	}

	private void EditButtonClick(object obj)
	{
		switch (ActualTab.Name)
		{
		case "GroupsTab":
			if (ToolGroupsViewModel.LastSelectedType == typeof(PunchGroup))
			{
				ToolGroupsViewModel.EditPunchGroup_Click();
			}
			else if (ToolGroupsViewModel.LastSelectedType == typeof(DieGroup))
			{
				ToolGroupsViewModel.EditDieGroup_Click();
			}
			break;
		case "PreferredToolsTab":
			PreferredProfilesViewModel.EditButtonClick();
			break;
		case "PunchesTab":
			if (PunchesViewModel.LastSelectedType == typeof(PunchPart))
			{
				PunchesViewModel.EditPart_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(PunchProfile))
			{
				PunchesViewModel.EditProfile_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(SensorDisk))
			{
				PunchesViewModel.EditDisk_Click();
			}
			break;
		case "DiesTab":
			if (DiesViewModel.LastSelectedType == typeof(DiePart))
			{
				DiesViewModel.EditPart_Click();
			}
			else if (DiesViewModel.LastSelectedType == typeof(DieProfile))
			{
				DiesViewModel.EditProfile_Click();
			}
			break;
		case "HemsTab":
			if (DieHemViewModel.LastSelectedType == typeof(HemPart))
			{
				DieHemViewModel.EditPart_Click();
			}
			else if (DieHemViewModel.LastSelectedType == typeof(HemProfile))
			{
				DieHemViewModel.EditProfile_Click();
			}
			break;
		}
		SetEditorEnableRules();
	}

	private void CopyButtonClick(object obj)
	{
		switch (ActualTab.Name)
		{
		case "GroupsTab":
			if (ToolGroupsViewModel.LastSelectedType == typeof(PunchGroup))
			{
				ToolGroupsViewModel.CopyPunchGroup_Click();
			}
			else if (ToolGroupsViewModel.LastSelectedType == typeof(DieGroup))
			{
				ToolGroupsViewModel.CopyDieGroup_Click();
			}
			break;
		case "BendTableTab":
			BendDataBaseViewModel.CopyButtonClick();
			break;
		case "PreferredToolsTab":
			PreferredProfilesViewModel.CopyButtonClick();
			break;
		case "PunchesTab":
			if (PunchesViewModel.LastSelectedType == typeof(PunchPart))
			{
				PunchesViewModel.CopyPart_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(PunchProfile))
			{
				PunchesViewModel.CopyProfile_Click();
			}
			else if (PunchesViewModel.LastSelectedType == typeof(SensorDisk))
			{
				PunchesViewModel.CopyDisk_Click();
			}
			break;
		case "DiesTab":
			if (DiesViewModel.LastSelectedType == typeof(DiePart))
			{
				DiesViewModel.CopyPart_Click();
			}
			else if (DiesViewModel.LastSelectedType == typeof(DieProfile))
			{
				DiesViewModel.CopyProfile_Click();
			}
			break;
		case "HemsTab":
			if (DieHemViewModel.LastSelectedType == typeof(HemPart))
			{
				DieHemViewModel.CopyPart_Click();
			}
			else if (DieHemViewModel.LastSelectedType == typeof(HemProfile))
			{
				DieHemViewModel.CopyProfile_Click();
			}
			break;
		}
		SetEditorEnableRules();
	}

	private void CancelButtonClick(object obj)
	{
		if (ActualTab.Name == "BendTableTab")
		{
			if (BendDataBaseViewModel.SelectPreferredProfileVisible != 0)
			{
				if (_closeAction != null)
				{
					_closeAction(this);
				}
				else
				{
					CloseView();
				}
			}
			else if (BendDataBaseViewModel.SelectPreferredProfileVisible == Visibility.Visible)
			{
				BendDataBaseViewModel.CancelButtonClick();
				return;
			}
		}
		else if (_closeAction != null)
		{
			_closeAction(this);
		}
		else
		{
			CloseView();
		}
		ToolGroupsViewModel?.Dispose();
		PunchesViewModel?.Dispose();
		DiesViewModel?.Dispose();
		DieHemViewModel?.Dispose();
		BendDataBaseViewModel?.Dispose();
	}

	private void OkButtonClick(object obj)
	{
		if (ActualTab.Name == "BendTableTab" && BendDataBaseViewModel.SelectPreferredProfileVisible == Visibility.Visible)
		{
			BendDataBaseViewModel.SetSelectedProfile();
		}
		else
		{
			CloseLikeOk();
		}
	}

	private void ApplyButtonClick(object obj)
	{
		Save();
	}

	private void CloseLikeOk()
	{
		Save();
		if (_changedConfigType != 0)
		{
			PopupCheckConfigChangedViewModel dataContext = new PopupCheckConfigChangedViewModel(_model.Globals, delegate(bool result)
			{
				if (result)
				{
					CheckChanges();
					ApplyChanges();
				}
			});
			PopupCheckConfigChangedView popupCheckConfigChangedView = new PopupCheckConfigChangedView();
			popupCheckConfigChangedView.DataContext = dataContext;
			popupCheckConfigChangedView.ShowDialog();
		}
		if (_closeAction != null)
		{
			_closeAction(this);
		}
		else
		{
			CloseView();
		}
		ToolGroupsViewModel?.Dispose();
		PunchesViewModel?.Dispose();
		DiesViewModel?.Dispose();
		DieHemViewModel?.Dispose();
		BendDataBaseViewModel?.Dispose();
	}

	private void Save()
	{
		BendDataBaseViewModel?.Save();
		ToolGroupsViewModel?.Refresh();
		PunchesViewModel?.Save();
		DiesViewModel?.Save();
		DieHemViewModel?.Save();
		PreferredProfilesViewModel?.UpdateFieldsAndSave();
		if (_bendMachine != null)
		{
			_machineHelper.Serialize(_bendMachine.BendMachine.MachinePath, _bendMachine.BendMachine);
		}
		LoadViewModels();
	}

	private void CheckChanges()
	{
		foreach (ChangedConfigType value in System.Enum.GetValues(typeof(ChangedConfigType)))
		{
			if ((_changedConfigType & value) <= ChangedConfigType.BendTable)
			{
				continue;
			}
			switch (value)
			{
			case ChangedConfigType.BendTable:
				_changesResult = ChangesResult.ReunfoldModel;
				return;
			case ChangedConfigType.BendSequence:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.PreferredProfiles:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.PostProcessor:
				if (_changesResult <= ChangesResult.ReloadMachine)
				{
					_changesResult = ChangesResult.ReloadMachine;
				}
				break;
			case ChangedConfigType.MachineConfig:
				if (_changesResult <= ChangesResult.ReloadMachine)
				{
					_changesResult = ChangesResult.ReloadMachine;
				}
				break;
			case ChangedConfigType.MaterialMapping:
				if (_changesResult <= ChangesResult.ReloadMachine)
				{
					_changesResult = ChangesResult.ReloadMachine;
				}
				break;
			case ChangedConfigType.ToolMapping:
				if (_changesResult <= ChangesResult.ReloadMachine)
				{
					_changesResult = ChangesResult.ReloadMachine;
				}
				break;
			case ChangedConfigType.Punches:
			case ChangedConfigType.Dies:
			case ChangedConfigType.Tools:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.PunchGroup:
			case ChangedConfigType.DieGroup:
			case ChangedConfigType.ToolGroups:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.UpperAdapter:
			case ChangedConfigType.LowerAdapter:
			case ChangedConfigType.Adapters:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.UpperHolder:
			case ChangedConfigType.LowerHolder:
			case ChangedConfigType.Holders:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			case ChangedConfigType.LeftFinger:
			case ChangedConfigType.RightFinger:
			case ChangedConfigType.Fingers:
				if (_changesResult <= ChangesResult.RecalculateFingers)
				{
					_changesResult = ChangesResult.RecalculateFingers;
				}
				break;
			case ChangedConfigType.UpperClampingSystem:
			case ChangedConfigType.LowerClampingSystem:
			case ChangedConfigType.ClampingSystem:
				if (_changesResult <= ChangesResult.RecalculateTools)
				{
					_changesResult = ChangesResult.RecalculateTools;
				}
				break;
			default:
				_changesResult = ChangesResult.NoChanges;
				break;
			}
		}
	}

	private void ApplyChanges()
	{
		IPnCommandArg pnCommandArg = _pnCommandBasics.CreateCommandArg(_model.Doc);
		switch (_changesResult)
		{
		case ChangesResult.ReunfoldModel:
		{
			bool hasToolSetups = _model.Doc.HasToolSetups;
			bool hasFingers = _model.Doc.HasFingers;
			_model.RibbonCommands.SetBendMachine(pnCommandArg);
			if (_model.Doc.HasModel)
			{
				_model.Doc.UpdateDoc();
			}
			if (hasToolSetups)
			{
				_model.RibbonCommands.SetTools(pnCommandArg);
			}
			if (hasFingers)
			{
				_model.RibbonCommands.SetFingers(pnCommandArg);
			}
			break;
		}
		case ChangesResult.ReloadMachine:
		{
			bool hasToolSetups2 = _model.Doc.HasToolSetups;
			bool hasFingers2 = _model.Doc.HasFingers;
			_model.RibbonCommands.SetBendMachine(pnCommandArg);
			if (hasToolSetups2)
			{
				_model.RibbonCommands.SetTools(pnCommandArg);
			}
			if (hasFingers2)
			{
				_model.RibbonCommands.SetFingers(pnCommandArg);
			}
			break;
		}
		case ChangesResult.RecalculateTools:
			if (_model.Doc.HasToolSetups)
			{
				_model.RibbonCommands.SetTools(pnCommandArg);
			}
			else
			{
				_model.Doc.UpdateDoc();
			}
			break;
		case ChangesResult.RecalculateFingers:
			if (_model.Doc.HasFingers)
			{
				_model.RibbonCommands.SetFingers(pnCommandArg);
			}
			break;
		case ChangesResult.NoChanges:
		case ChangesResult.RecalculateFingers | ChangesResult.RecalculateTools:
		case ChangesResult.RecalculateFingers | ChangesResult.ReloadMachine:
		case ChangesResult.RecalculateTools | ChangesResult.ReloadMachine:
		case ChangesResult.RecalculateFingers | ChangesResult.RecalculateTools | ChangesResult.ReloadMachine:
			break;
		}
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			CloseLikeOk();
		}
	}
}

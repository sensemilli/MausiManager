using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupUnfoldSettingViewModel : PopupViewModelBase
{
	public class PurchasedPartTypeViewModel : ViewModelBase
	{
		private string _desc;

		private int _index;

		public string Desc
		{
			get
			{
				return this._desc;
			}
			set
			{
				if (this._desc != value)
				{
					this._desc = value;
					base.NotifyPropertyChanged("Desc");
				}
			}
		}

		public int Index
		{
			get
			{
				return this._index;
			}
			set
			{
				if (this._index != value)
				{
					this._index = value;
					base.NotifyPropertyChanged("Index");
				}
			}
		}
	}

	public class PrefabricatedPartViewModel : ViewModelBase, IPrefabricatedPart
	{
		public string Name { get; set; }

		public string NameBinding
		{
			get
			{
				return this.Name;
			}
			set
			{
				if (this.NameSearchType == IPrefabricatedPart.SearchTypes.RegexCompare)
				{
					Regex.IsMatch("", value, RegexOptions.IgnoreCase);
				}
				this.Name = value;
			}
		}

		public int Type { get; set; } = 1;

		public bool IsMountedBeforeBending { get; set; }

		public bool IgnoreAtCollision
		{
			get
			{
				return !this.IsMountedBeforeBending;
			}
			set
			{
				this.IsMountedBeforeBending = !value;
			}
		}

		public string[] AdditionalValues { get; set; }

		public List<(string propName, int propIdx)> AdditionalHeaderNames { get; } = new List<(string, int)>();

		public IEnumerable<(string propName, string propValue)> AdditionalProperties => this.AdditionalHeaderNames.Select(((string propName, int propIdx) x) => (propName: x.propName, this.AdditionalValues[x.propIdx]));

		public IPrefabricatedPart.SearchTypes NameSearchType { get; set; }

		public IPrefabricatedPart.SearchTypes NameSearchTypeBinding
		{
			get
			{
				return this.NameSearchType;
			}
			set
			{
				if (this.NameSearchType == IPrefabricatedPart.SearchTypes.RegexCompare)
				{
					Regex.IsMatch("", this.Name, RegexOptions.IgnoreCase);
				}
				this.NameSearchType = value;
			}
		}

		public PrefabricatedPartViewModel()
		{
			this.AdditionalValues = new string[0];
		}

		public PrefabricatedPartViewModel(IPrefabricatedPart part, Dictionary<string, int> additionalHeaders)
		{
			this.Name = part.Name;
			this.IsMountedBeforeBending = part.IsMountedBeforeBending;
			this.Type = part.Type;
			this.NameSearchType = part.NameSearchType;
			this.AdditionalValues = new string[additionalHeaders.Count];
			foreach (var additionalProperty in part.AdditionalProperties)
			{
				int num = additionalHeaders[additionalProperty.propName.ToLowerInvariant()];
				this.AdditionalValues[num] = additionalProperty.propValue;
				this.AdditionalHeaderNames.Add((additionalProperty.propName, num));
			}
		}
	}

	public class LegendEntry : ViewModelBase
	{
		private int _no;

		public Func<int, bool> CheckNewNo;

		public int No
		{
			get
			{
				return this._no;
			}
			set
			{
				if (this._no != value && value > 0 && this.CheckNewNo(value))
				{
					this._no = value;
					base.NotifyPropertyChanged("No");
				}
			}
		}

		public string Desc { get; set; }

		public bool Required { get; set; }

		public LegendEntry()
		{
		}

		public LegendEntry(int no, string desc, bool required)
		{
			this._no = no;
			this.Desc = desc;
			this.Required = required;
		}
	}

	public class ColorSetting : ViewModelBase
	{
		public Action<ColorSetting, ModelColors3DConfig> SaveAction;

		private CfgColor _color;

		private bool _overwrite = true;

		public ICommand ColorChangeCommand { get; set; }

		public string Desc { get; set; }

		public CfgColor Color
		{
			get
			{
				return this._color;
			}
			set
			{
				if (this._color != value)
				{
					this._color = value;
					base.NotifyPropertyChanged("Color");
				}
			}
		}

		public Visibility OverwriteOptionVisibility { get; init; }

		public bool Overwrite
		{
			get
			{
				return this._overwrite;
			}
			set
			{
				if (this._overwrite != value)
				{
					this._overwrite = value;
					base.NotifyPropertyChanged("Overwrite");
				}
			}
		}
	}

	private PopupUnfoldSettingModel _model;

	private readonly IPrefabricatedPartsManager _prefabricatedPartsManager;

	private readonly ITranslator _translator;

	private readonly IUnitConverter _unitconverter;

	private IBendMachineSummary _selectedMachine;

	private bool _searchSpecialVisibleFaceColor;

	public SubPopupForPopup subPopup;

	private bool _ignoreNonHorizontalPlaneConnectedPp;

	private bool _isDetectionPrefabricatedPartsByNameActive;

	private bool _detectEverythingAsPpInAssembly;

	private double _maxDistOfPurchasePartsToSheetMetal;

	public SimValidationConfig simValidationConfig { get; set; }

	public General3DConfig general3DConfig { get; set; }

	public Macro3DConfig macro3DConfig { get; set; }

	public ModelColors3DConfig modelColors3DConfig { get; set; }

	public ReconstructIrregularBendsConfig ReconstructBendsConfig { get; set; }

	public IValidationSettingsViewModel ValidationViewModel { get; set; }

	public IMacroConfigViewModel MacroConfigViewModel { get; set; }

	public ColorSetting SpecialVisibleFaceColor { get; } = new ColorSetting();

	public bool SearchSpecialVisibleFaceColor
	{
		get
		{
			return this._searchSpecialVisibleFaceColor;
		}
		set
		{
			if (this._searchSpecialVisibleFaceColor != value)
			{
				this._searchSpecialVisibleFaceColor = value;
				this.OnPropertyChanged("SearchSpecialVisibleFaceColor");
				this.OnPropertyChanged("SpecialVisibleFaceColorVisible");
			}
		}
	}

	public bool? ReconstructBendsAutoAfterImport
	{
		get
		{
			if (this.ReconstructBendsConfig.ReconstructAfterImport.HasValue)
			{
				return this.ReconstructBendsConfig.ReconstructAfterImport == 1;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				this.ReconstructBendsConfig.ReconstructAfterImport = (value.Value ? 1 : 0);
			}
			else
			{
				this.ReconstructBendsConfig.ReconstructAfterImport = null;
			}
		}
	}

	public Visibility SpecialVisibleFaceColorVisible
	{
		get
		{
			if (!this._searchSpecialVisibleFaceColor)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public ObservableCollection<IBendMachineSummary> Machines { get; set; }

	public IBendMachineSummary SelectedMachine
	{
		get
		{
			return this._selectedMachine;
		}
		set
		{
			this._selectedMachine = value;
			this.OnPropertyChanged("SelectedMachine");
		}
	}

	public bool IgnoreNonHorizontalPlaneConnectedPp
	{
		get
		{
			return this._ignoreNonHorizontalPlaneConnectedPp;
		}
		set
		{
			if (this._ignoreNonHorizontalPlaneConnectedPp != value)
			{
				this._ignoreNonHorizontalPlaneConnectedPp = value;
				this.OnPropertyChanged("IgnoreNonHorizontalPlaneConnectedPp");
			}
		}
	}

	public bool IsDetectionPrefabricatedPartsByNameActive
	{
		get
		{
			return this._isDetectionPrefabricatedPartsByNameActive;
		}
		set
		{
			if (this._isDetectionPrefabricatedPartsByNameActive != value)
			{
				this._isDetectionPrefabricatedPartsByNameActive = value;
				this.OnPropertyChanged("IsDetectionPrefabricatedPartsByNameActive");
			}
		}
	}

	public bool DetectEverythingAsPpInAssembly
	{
		get
		{
			return this._detectEverythingAsPpInAssembly;
		}
		set
		{
			if (this._detectEverythingAsPpInAssembly != value)
			{
				this._detectEverythingAsPpInAssembly = value;
				this.OnPropertyChanged("DetectEverythingAsPpInAssembly");
			}
		}
	}

	public double MaxDistOfPurchasePartsToSheetMetal
	{
		get
		{
			return this._maxDistOfPurchasePartsToSheetMetal;
		}
		set
		{
			if (this._maxDistOfPurchasePartsToSheetMetal != value)
			{
				this._maxDistOfPurchasePartsToSheetMetal = value;
				this.OnPropertyChanged("MaxDistOfPurchasePartsToSheetMetalUI");
				this.OnPropertyChanged("MaxDistOfPurchasePartsToSheetMetal");
			}
		}
	}

	public double MaxDistOfPurchasePartsToSheetMetalUI
	{
		get
		{
			return this._unitconverter.Length.ToUi(this._maxDistOfPurchasePartsToSheetMetal, null);
		}
		set
		{
			double num = this._unitconverter.Length.FromUi(value, null);
			if (num != this._maxDistOfPurchasePartsToSheetMetal)
			{
				this._maxDistOfPurchasePartsToSheetMetal = num;
				this.OnPropertyChanged("MaxDistOfPurchasePartsToSheetMetalUI");
			}
		}
	}

	public string MaxDistOfPurchasePartsToSheetMetalUnit => this._unitconverter.Length.Unit;

	public RadObservableCollection<PrefabricatedPartViewModel> PurchasedPartList { get; } = new RadObservableCollection<PrefabricatedPartViewModel>();

	public RadObservableCollection<PurchasedPartTypeViewModel> PurchasedPartTypeList { get; } = new RadObservableCollection<PurchasedPartTypeViewModel>();

	public RadObservableCollection<ComboboxEntry<IPrefabricatedPart.SearchTypes>> NameSearchTypes { get; } = new RadObservableCollection<ComboboxEntry<IPrefabricatedPart.SearchTypes>>();

	public RelayCommand CmdAddNewPurchasedPartType { get; }

	public RelayCommand CmdDeletePurchasedPartType { get; }

	public RelayCommand CmdImportPrefabricatedParts { get; }

	public RelayCommand CmdResetPrefabricatedParts { get; }

	public RelayCommand CmdDeleteAll { get; }

	public RelayCommand CmdDeleteRow { get; }

	public List<string> AdditionalHeaders { get; private set; } = new List<string>();

	public RadObservableCollection<LegendEntry> LegendEntries { get; }

	public bool LegendCheckBeforeSave { get; set; }

	public bool LegendCheckBeforePp { get; set; }

	public int BaseColorPn
	{
		get
		{
			return this.macro3DConfig.BaseColorPn;
		}
		set
		{
			this.macro3DConfig.BaseColorPn = value;
			this.OnPropertyChanged("BaseColorPn");
		}
	}

	public int TubeCutColorPn
	{
		get
		{
			return this.macro3DConfig.TubeCutColorPn;
		}
		set
		{
			this.macro3DConfig.TubeCutColorPn = value;
			this.OnPropertyChanged("TubeCutColorPn");
		}
	}

	public int BendPosColorPn
	{
		get
		{
			return this.macro3DConfig.BendPosColorPn;
		}
		set
		{
			this.macro3DConfig.BendPosColorPn = value;
			this.OnPropertyChanged("BendPosColorPn");
		}
	}

	public int BendNegColorPn
	{
		get
		{
			return this.macro3DConfig.BendNegColorPn;
		}
		set
		{
			this.macro3DConfig.BendNegColorPn = value;
			this.OnPropertyChanged("BendNegColorPn");
		}
	}

	public int MacroPosColorPn
	{
		get
		{
			return this.macro3DConfig.MacroPosColorPn;
		}
		set
		{
			this.macro3DConfig.MacroPosColorPn = value;
			this.OnPropertyChanged("MacroPosColorPn");
		}
	}

	public int MacroNegColorPn
	{
		get
		{
			return this.macro3DConfig.MacroNegColorPn;
		}
		set
		{
			this.macro3DConfig.MacroNegColorPn = value;
			this.OnPropertyChanged("MacroNegColorPn");
		}
	}

	public int DeepeningPosColorPn
	{
		get
		{
			return this.macro3DConfig.DeepeningPosColorPn;
		}
		set
		{
			this.macro3DConfig.DeepeningPosColorPn = value;
			this.OnPropertyChanged("DeepeningPosColorPn");
		}
	}

	public int DeepeningNegColorPn
	{
		get
		{
			return this.macro3DConfig.DeepeningNegColorPn;
		}
		set
		{
			this.macro3DConfig.DeepeningNegColorPn = value;
			this.OnPropertyChanged("DeepeningNegColorPn");
		}
	}

	public int BlindHoleSideColorPn
	{
		get
		{
			return this.macro3DConfig.BlindHoleSideColorPn;
		}
		set
		{
			this.macro3DConfig.BlindHoleSideColorPn = value;
			this.OnPropertyChanged("BlindHoleSideColorPn");
		}
	}

	public int EmbossedPosColorPn
	{
		get
		{
			return this.macro3DConfig.EmbossedPosColorPn;
		}
		set
		{
			this.macro3DConfig.EmbossedPosColorPn = value;
			this.OnPropertyChanged("EmbossedPosColorPn");
		}
	}

	public int EmbossedNegColorPn
	{
		get
		{
			return this.macro3DConfig.EmbossedNegColorPn;
		}
		set
		{
			this.macro3DConfig.EmbossedNegColorPn = value;
			this.OnPropertyChanged("EmbossedNegColorPn");
		}
	}

	public int EmbStampPosColorPn
	{
		get
		{
			return this.macro3DConfig.EmbStampPosColorPn;
		}
		set
		{
			this.macro3DConfig.EmbStampPosColorPn = value;
			this.OnPropertyChanged("EmbStampPosColorPn");
		}
	}

	public int EmbStampNegColorPn
	{
		get
		{
			return this.macro3DConfig.EmbStampNegColorPn;
		}
		set
		{
			this.macro3DConfig.EmbStampNegColorPn = value;
			this.OnPropertyChanged("EmbStampNegColorPn");
		}
	}

	public int EmbStampSpecialColorPn
	{
		get
		{
			return this.macro3DConfig.EmbStampSpecialColorPn;
		}
		set
		{
			this.macro3DConfig.EmbStampSpecialColorPn = value;
			this.OnPropertyChanged("EmbStampSpecialColorPn");
		}
	}

	public int UnknownMacroPosColorPn => this.macro3DConfig.UnknownMacroPosColorPn;

	public int UnknownMacroNegColorPn => this.macro3DConfig.UnknownMacroNegColorPn;

	public int PurchasedPartsPosColorPn
	{
		get
		{
			return this.macro3DConfig.PurchasedPartsPosColorPn;
		}
		set
		{
			this.macro3DConfig.PurchasedPartsPosColorPn = value;
			this.OnPropertyChanged("PurchasedPartsPosColorPn");
		}
	}

	public int PurchasedPartsNegColorPn
	{
		get
		{
			return this.macro3DConfig.PurchasedPartsNegColorPn;
		}
		set
		{
			this.macro3DConfig.PurchasedPartsNegColorPn = value;
			this.OnPropertyChanged("PurchasedPartsNegColorPn");
		}
	}

	public int CutOutMarkerColorPn
	{
		get
		{
			return this.macro3DConfig.CutOutMarkerColor;
		}
		set
		{
			this.macro3DConfig.CutOutMarkerColor = value;
			this.OnPropertyChanged("CutOutMarkerColorPn");
		}
	}

	public int ChamferPosColorPn
	{
		get
		{
			return this.macro3DConfig.ChamferPosColorPn;
		}
		set
		{
			this.macro3DConfig.ChamferPosColorPn = value;
			this.OnPropertyChanged("ChamferPosColorPn");
		}
	}

	public int ChamferNegColorPn
	{
		get
		{
			return this.macro3DConfig.ChamferNegColorPn;
		}
		set
		{
			this.macro3DConfig.ChamferNegColorPn = value;
			this.OnPropertyChanged("ChamferNegColorPn");
		}
	}

	public ICommand BaseColorPnCommand { get; set; }

	public ICommand TubeCutColorPnCommand { get; set; }

	public ICommand BendPosColorPnCommand { get; set; }

	public ICommand BendNegColorPnCommand { get; set; }

	public ICommand MacroPosColorPnCommand { get; set; }

	public ICommand MacroNegColorPnCommand { get; set; }

	public ICommand DeepeningPosColorPnCommand { get; set; }

	public ICommand DeepeningNegColorPnCommand { get; set; }

	public ICommand BlindHoleSideColorPnCommand { get; set; }

	public ICommand EmbossedPosColorPnCommand { get; set; }

	public ICommand EmbossedNegColorPnCommand { get; set; }

	public ICommand PurchasedPartsPosColorPnCommand { get; set; }

	public ICommand PurchasedPartsNegColorPnCommand { get; set; }

	public ICommand EmbStampPosColorPnCommand { get; set; }

	public ICommand EmbStampNegColorPnCommand { get; set; }

	public ICommand EmbStampSpecialColorPnCommand { get; set; }

	public ICommand CutOutMarkerColorPnCommand { get; set; }

	public ICommand ChamferPosColorPnCommand { get; set; }

	public ICommand ChamferNegColorPnCommand { get; set; }

	public ObservableCollection<ColorSetting> PartColors3D { get; set; }

	public ObservableCollection<ColorSetting> MacroColors3D { get; set; }

	public ObservableCollection<ColorSetting> MachineColors3D { get; set; }

	public ObservableCollection<ColorSetting> EditOrderColors3D { get; set; }

	public event Action<List<string>> ReloadPrefabricatedDynColumns;

	private void FromModelToUi()
	{
		this.Machines = this._model.Machines;
		this.general3DConfig = this._model.General3DConfig;
		this.simValidationConfig = this._model.SimValidationConfig;
		this.macro3DConfig = this._model.Macro3DConfig;
		this.modelColors3DConfig = this._model.ModelColors3DConfig;
		this.ReconstructBendsConfig = this._model.ReconstructBendsConfig;
		this.SpecialVisibleFaceColor.Color = this._model.AnalyzeConfig.SpecialVisibleFaceColor;
		this.SpecialVisibleFaceColor.ColorChangeCommand = new RelayCommand((Action<object>)delegate
		{
			CfgColor color = this.SpecialVisibleFaceColor.Color.Clone();
			if (this.subPopup.SelectCfgColor(color))
			{
				this.SpecialVisibleFaceColor.Color = color;
			}
		});
		this.SearchSpecialVisibleFaceColor = this._model.AnalyzeConfig.SearchSpecialVisibleFaceColor;
		this.Construct_MacroSettings();
		this.Construct_ModelColors3D();
		this.SelectedMachine = ((this.general3DConfig.P3D_UseDefaultMachine && this.general3DConfig.P3D_Default_MachineId > -1) ? this.Machines.FirstOrDefault((IBendMachineSummary x) => x.MachineNo == this.general3DConfig.P3D_Default_MachineId) : null);
		IPrefabricatedPartsManager prefabricatedPartsManager = this._prefabricatedPartsManager;
		this.IgnoreNonHorizontalPlaneConnectedPp = prefabricatedPartsManager.IgnoreNonHorizontalPlaneConnectedPp;
		this.IsDetectionPrefabricatedPartsByNameActive = prefabricatedPartsManager.IsDetectionPrefabricatedPartsByNameActive;
		this.DetectEverythingAsPpInAssembly = prefabricatedPartsManager.DetectEverythingAsPpInAssembly;
		this.MaxDistOfPurchasePartsToSheetMetal = prefabricatedPartsManager.MaxDistOfPurchasePartsToSheetMetal;
		this.ReLoadPrefabricatedParts();
		this.LegendLoad();
		this.MacroConfigViewModel.Load(this);
	}

	private void FromUiToModel()
	{
		this.GetReferenceBack_ModelColors3D();
		this._model.AnalyzeConfig.SearchSpecialVisibleFaceColor = this.SearchSpecialVisibleFaceColor;
		this._model.AnalyzeConfig.SpecialVisibleFaceColor = this.SpecialVisibleFaceColor.Color;
		int num = this.SelectedMachine?.MachineNo ?? (-1);
		this.general3DConfig.P3D_Default_MachineId = (this.general3DConfig.P3D_UseDefaultMachine ? num : (-1));
		this.ValidationViewModel.Save();
		this.MacroConfigViewModel.Save(this._model);
		HashSet<string> usedPartNames = new HashSet<string>();
		HashSet<string> badRegex = new HashSet<string>();
		this._prefabricatedPartsManager.SetData(this.PurchasedPartTypeList.Select((PurchasedPartTypeViewModel x) => (typeId: x.Index, typeDesc: x.Desc)), this.PurchasedPartList.Where(delegate(PrefabricatedPartViewModel x)
		{
			if (!string.IsNullOrEmpty(x.Name))
			{
				if (x.NameSearchType == IPrefabricatedPart.SearchTypes.RegexCompare)
				{
					try
					{
						Regex.IsMatch("", x.Name, RegexOptions.IgnoreCase);
					}
					catch (Exception)
					{
						badRegex.Add(x.Name);
						return false;
					}
				}
				else if (!usedPartNames.Add(x.Name.ToLowerInvariant()))
				{
					return false;
				}
				return true;
			}
			return false;
		}));
		this._prefabricatedPartsManager.SetConfig(this.IgnoreNonHorizontalPlaneConnectedPp, this.IsDetectionPrefabricatedPartsByNameActive, this.DetectEverythingAsPpInAssembly, this.MaxDistOfPurchasePartsToSheetMetal);
		this.LegendSave();
		this._model.PushToConfigProvider();
		this.MacroConfigViewModel.Save(this._model);
	}

	public PopupUnfoldSettingViewModel(IPrefabricatedPartsManager prefabricatedPartsManager, ITranslator translator, IMacroConfigViewModel macroConfigViewModel, IUnitConverter unitconverter, IValidationSettingsViewModel validationSettingsViewModel)
	{
		this._prefabricatedPartsManager = prefabricatedPartsManager;
		this._translator = translator;
		this.MacroConfigViewModel = macroConfigViewModel;
		this._unitconverter = unitconverter;
		this.ValidationViewModel = validationSettingsViewModel;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		this.CmdAddNewPurchasedPartType = new RelayCommand(AddNewPurchasedPartType);
		this.CmdDeletePurchasedPartType = new RelayCommand(DeletePurchasedPartType, DeletePurchasedPartTypeCanExecute);
		this.CmdImportPrefabricatedParts = new RelayCommand(ImportPrefabricatedParts);
		this.CmdResetPrefabricatedParts = new RelayCommand(ReLoadPrefabricatedParts);
		this.CmdDeleteAll = new RelayCommand(DeleteAll);
		this.CmdDeleteRow = new RelayCommand(DeleteRow, DeleteRowCanExecute);
		this.LegendEntries = new RadObservableCollection<LegendEntry>();
		this.LegendEntries.CollectionChanged += LegendEntries_CollectionChanged;
	}

	public void Init(string pnDrive, PopupUnfoldSettingModel model)
	{
		this._model = model;
		this.ValidationViewModel.Init(model, pnDrive);
		this.NameSearchTypes.Clear();
		this.NameSearchTypes.Add(new ComboboxEntry<IPrefabricatedPart.SearchTypes>(this._translator.Translate("l_popup.PopupUnfoldSetting.SearchTypes.Exact"), IPrefabricatedPart.SearchTypes.EqualCompare));
		this.NameSearchTypes.Add(new ComboboxEntry<IPrefabricatedPart.SearchTypes>(this._translator.Translate("l_popup.PopupUnfoldSetting.SearchTypes.Regex"), IPrefabricatedPart.SearchTypes.RegexCompare));
		this.FromModelToUi();
	}

	private static bool CanCancelClick(object obj)
	{
		return true;
	}

	private bool CanAlwaysBeClick(object obj)
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
		base.CloseView();
		this.FromUiToModel();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			this.FromUiToModel();
		}
	}

	private void AddNewPurchasedPartType(object obj)
	{
		this.PurchasedPartTypeList.Add(new PurchasedPartTypeViewModel
		{
			Desc = "",
			Index = this.PurchasedPartTypeList.Select((PurchasedPartTypeViewModel x) => x.Index).DefaultIfEmpty(0).Max() + 1
		});
	}

	private void DeletePurchasedPartType(object obj)
	{
		if (obj is PurchasedPartTypeViewModel item)
		{
			this.PurchasedPartTypeList.Remove(item);
		}
	}

	private void DeleteAll(object obj)
	{
		this.PurchasedPartTypeList.Clear();
		this.PurchasedPartList.Clear();
	}

	private void DeleteRow(object obj)
	{
		if (obj is PrefabricatedPartViewModel item)
		{
			this.PurchasedPartList.Remove(item);
		}
		else if (obj is LegendEntry item2)
		{
			this.LegendEntries.Remove(item2);
		}
	}

	private void ImportPrefabricatedParts(object obj)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = this._translator.Translate("l_popup.PopupUnfoldSetting.PurchasedPartImportTt"),
			Filter = "XML|*.xml",
			DereferenceLinks = true
		};
		if (openFileDialog.ShowDialog() == true)
		{
			this._prefabricatedPartsManager.ImportXml(openFileDialog.FileName);
			this.ReLoadPrefabricatedParts();
		}
	}

	private bool DeletePurchasedPartTypeCanExecute(object obj)
	{
		PurchasedPartTypeViewModel vm = obj as PurchasedPartTypeViewModel;
		if (vm != null)
		{
			return this.PurchasedPartList.All((PrefabricatedPartViewModel x) => x.Type != vm.Index);
		}
		return false;
	}

	private bool DeleteRowCanExecute(object obj)
	{
		return true;
	}

	private void ReLoadPrefabricatedParts()
	{
		IPrefabricatedPartsManager prefabricatedPartsManager = this._prefabricatedPartsManager;
		this.PurchasedPartTypeList.Clear();
		this.PurchasedPartTypeList.AddRange(prefabricatedPartsManager.GetPartTypesOrdered().Select<(int, string), PurchasedPartTypeViewModel>(delegate((int typeId, string typeDesc) tuple)
		{
			PurchasedPartTypeViewModel purchasedPartTypeViewModel = new PurchasedPartTypeViewModel();
			(purchasedPartTypeViewModel.Index, purchasedPartTypeViewModel.Desc) = tuple;
			return purchasedPartTypeViewModel;
		}));
		this.PurchasedPartList.Clear();
		List<IPrefabricatedPart> list = prefabricatedPartsManager.AllParts().ToList();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (IPrefabricatedPart item in list)
		{
			foreach (var additionalProperty in item.AdditionalProperties)
			{
				hashSet.Add(additionalProperty.propName);
			}
		}
		this.AdditionalHeaders = hashSet.ToList();
		int idx = 0;
		Dictionary<string, int> additionalIndex = this.AdditionalHeaders.ToDictionary((string x) => x.ToLowerInvariant(), (string x) => idx++);
		this.PurchasedPartList.AddRange(list.Select((IPrefabricatedPart part) => new PrefabricatedPartViewModel(part, additionalIndex)));
		this.ReloadPrefabricatedDynColumns?.Invoke(this.AdditionalHeaders);
	}

	private void LegendEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems == null)
		{
			return;
		}
		foreach (LegendEntry newItem in e.NewItems)
		{
			newItem.CheckNewNo = CheckNewLegendNo;
			if (newItem.No == 0)
			{
				newItem.No = this.LegendEntries.Select((LegendEntry x) => x.No).DefaultIfEmpty(0).Max() + 1;
			}
		}
	}

	private bool CheckNewLegendNo(int number)
	{
		return this.LegendEntries.All((LegendEntry x) => x.No != number);
	}

	private void LegendLoad()
	{
		this.LegendEntries.Clear();
		this.LegendEntries.AddRange(this._model.UserCommentsConfig.UserCommentsRequired.Select<KeyValuePair<int, string>, LegendEntry>((KeyValuePair<int, string> x) => new LegendEntry(x.Key, x.Value, required: true)));
		this.LegendEntries.AddRange(this._model.UserCommentsConfig.UserCommentsOptional.Select<KeyValuePair<int, string>, LegendEntry>((KeyValuePair<int, string> x) => new LegendEntry(x.Key, x.Value, required: false)));
		foreach (LegendEntry legendEntry in this.LegendEntries)
		{
			legendEntry.CheckNewNo = CheckNewLegendNo;
		}
		this.LegendCheckBeforeSave = this._model.UserCommentsConfig.CheckBeforeSave;
		this.LegendCheckBeforePp = this._model.UserCommentsConfig.CheckBeforePp;
		this.OnPropertyChanged("LegendCheckBeforePp");
		this.OnPropertyChanged("LegendCheckBeforeSave");
	}

	private void LegendSave()
	{
		this._model.UserCommentsConfig.UserCommentsRequired = this.LegendEntries.Where((LegendEntry x) => x.Required).ToDictionary((LegendEntry x) => x.No, (LegendEntry x) => x.Desc);
		this._model.UserCommentsConfig.UserCommentsOptional = this.LegendEntries.Where((LegendEntry x) => !x.Required).ToDictionary((LegendEntry x) => x.No, (LegendEntry x) => x.Desc);
		this._model.UserCommentsConfig.CheckBeforeSave = this.LegendCheckBeforeSave;
		this._model.UserCommentsConfig.CheckBeforePp = this.LegendCheckBeforePp;
	}

	private void Construct_MacroSettings()
	{
		this.BaseColorPnCommand = new RelayCommand<object>(BaseColorPnClick, CanAlwaysBeClick);
		this.TubeCutColorPnCommand = new RelayCommand<object>(TubeCutColorPnClick, CanAlwaysBeClick);
		this.BendPosColorPnCommand = new RelayCommand<object>(BendPosColorPnClick, CanAlwaysBeClick);
		this.BendNegColorPnCommand = new RelayCommand<object>(BendNegColorPnClick, CanAlwaysBeClick);
		this.MacroPosColorPnCommand = new RelayCommand<object>(MacroPosColorPnClick, CanAlwaysBeClick);
		this.MacroNegColorPnCommand = new RelayCommand<object>(MacroNegColorPnClick, CanAlwaysBeClick);
		this.DeepeningPosColorPnCommand = new RelayCommand<object>(DeepeningPosColorPnClick, CanAlwaysBeClick);
		this.DeepeningNegColorPnCommand = new RelayCommand<object>(DeepeningNegColorPnClick, CanAlwaysBeClick);
		this.BlindHoleSideColorPnCommand = new RelayCommand<object>(BlindHoleSideColorPnClick, CanAlwaysBeClick);
		this.EmbossedPosColorPnCommand = new RelayCommand<object>(EmbossedPosColorPnClick, CanAlwaysBeClick);
		this.EmbossedNegColorPnCommand = new RelayCommand<object>(EmbossedNegColorPnClick, CanAlwaysBeClick);
		this.PurchasedPartsPosColorPnCommand = new RelayCommand<object>(PurchasedPartsPosColorPnClick, CanAlwaysBeClick);
		this.PurchasedPartsNegColorPnCommand = new RelayCommand<object>(PurchasedPartsNegColorPnClick, CanAlwaysBeClick);
		this.EmbStampPosColorPnCommand = new RelayCommand<object>(EmbStampPosColorPnClick, CanAlwaysBeClick);
		this.EmbStampNegColorPnCommand = new RelayCommand<object>(EmbStampNegColorPnClick, CanAlwaysBeClick);
		this.EmbStampSpecialColorPnCommand = new RelayCommand<object>(EmbStampSpecialColorPnClick, CanAlwaysBeClick);
		this.CutOutMarkerColorPnCommand = new RelayCommand<object>(CutOutMarkerColorPnClick, CanAlwaysBeClick);
		this.ChamferPosColorPnCommand = new RelayCommand<object>(ChamferPosColorPnClick, CanAlwaysBeClick);
		this.ChamferNegColorPnCommand = new RelayCommand<object>(ChamferNegColorPnClick, CanAlwaysBeClick);
	}

	private void EmbossedPosColorPnClick(object obj)
	{
		this.EmbossedPosColorPn = this.subPopup.SelectColorPnIntEdition(this.EmbossedPosColorPn);
	}

	private void EmbossedNegColorPnClick(object obj)
	{
		this.EmbossedNegColorPn = this.subPopup.SelectColorPnIntEdition(this.EmbossedNegColorPn);
	}

	private void BlindHoleSideColorPnClick(object obj)
	{
		this.BlindHoleSideColorPn = this.subPopup.SelectColorPnIntEdition(this.BlindHoleSideColorPn);
	}

	private void DeepeningNegColorPnClick(object obj)
	{
		this.DeepeningNegColorPn = this.subPopup.SelectColorPnIntEdition(this.DeepeningNegColorPn);
	}

	private void DeepeningPosColorPnClick(object obj)
	{
		this.DeepeningPosColorPn = this.subPopup.SelectColorPnIntEdition(this.DeepeningPosColorPn);
	}

	private void MacroNegColorPnClick(object obj)
	{
		this.MacroNegColorPn = this.subPopup.SelectColorPnIntEdition(this.MacroNegColorPn);
	}

	private void MacroPosColorPnClick(object obj)
	{
		this.MacroPosColorPn = this.subPopup.SelectColorPnIntEdition(this.MacroPosColorPn);
	}

	private void BendNegColorPnClick(object obj)
	{
		this.BendNegColorPn = this.subPopup.SelectColorPnIntEdition(this.BendNegColorPn);
	}

	private void BendPosColorPnClick(object obj)
	{
		this.BendPosColorPn = this.subPopup.SelectColorPnIntEdition(this.BendPosColorPn);
	}

	private void TubeCutColorPnClick(object obj)
	{
		this.TubeCutColorPn = this.subPopup.SelectColorPnIntEdition(this.TubeCutColorPn);
	}

	private void BaseColorPnClick(object obj)
	{
		this.BaseColorPn = this.subPopup.SelectColorPnIntEdition(this.BaseColorPn);
	}

	private void PurchasedPartsPosColorPnClick(object obj)
	{
		this.PurchasedPartsPosColorPn = this.subPopup.SelectColorPnIntEdition(this.PurchasedPartsPosColorPn);
	}

	private void PurchasedPartsNegColorPnClick(object obj)
	{
		this.PurchasedPartsNegColorPn = this.subPopup.SelectColorPnIntEdition(this.PurchasedPartsNegColorPn);
	}

	private void EmbStampPosColorPnClick(object obj)
	{
		this.EmbStampPosColorPn = this.subPopup.SelectColorPnIntEdition(this.EmbStampPosColorPn);
	}

	private void EmbStampNegColorPnClick(object obj)
	{
		this.EmbStampNegColorPn = this.subPopup.SelectColorPnIntEdition(this.EmbStampNegColorPn);
	}

	private void EmbStampSpecialColorPnClick(object obj)
	{
		this.EmbStampSpecialColorPn = this.subPopup.SelectColorPnIntEdition(this.EmbStampSpecialColorPn);
	}

	private void CutOutMarkerColorPnClick(object obj)
	{
		this.CutOutMarkerColorPn = this.subPopup.SelectColorPnIntEdition(this.CutOutMarkerColorPn);
	}

	private void ChamferPosColorPnClick(object obj)
	{
		this.ChamferPosColorPn = this.subPopup.SelectColorPnIntEdition(this.ChamferPosColorPn);
	}

	private void ChamferNegColorPnClick(object obj)
	{
		this.ChamferNegColorPn = this.subPopup.SelectColorPnIntEdition(this.ChamferNegColorPn);
	}

	public void Construct_ModelColors3D()
	{
		this.GenerateColors();
	}

	private void GetReferenceBack_ModelColors3D()
	{
		foreach (ColorSetting item in this.PartColors3D)
		{
			item.SaveAction(item, this.modelColors3DConfig);
		}
		foreach (ColorSetting item2 in this.MacroColors3D)
		{
			item2.SaveAction(item2, this.modelColors3DConfig);
		}
		foreach (ColorSetting item3 in this.MachineColors3D)
		{
			item3.SaveAction(item3, this.modelColors3DConfig);
		}
		foreach (ColorSetting item4 in this.EditOrderColors3D)
		{
			item4.SaveAction(item4, this.modelColors3DConfig);
		}
	}

	private void GenerateColors()
	{
		ObservableCollection<ColorSetting> observableCollection = new ObservableCollection<ColorSetting>();
		ObservableCollection<ColorSetting> observableCollection2 = new ObservableCollection<ColorSetting>();
		ObservableCollection<ColorSetting> observableCollection3 = new ObservableCollection<ColorSetting>();
		ObservableCollection<ColorSetting> observableCollection4 = new ObservableCollection<ColorSetting>();
		observableCollection.Add(this.CreateColorSetting("VisibleFaceColor", this.modelColors3DConfig.VisibleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.VisibleFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("VisibleSideColor", this.modelColors3DConfig.VisibleSideColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.VisibleSideColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("FlatGroupTopBottomFaceColor", this.modelColors3DConfig.FlatGroupTopBottomFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.FlatGroupTopBottomFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("FlatGroupConnectingFaceColor", this.modelColors3DConfig.FlatGroupConnectingFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.FlatGroupConnectingFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("BendGroupTopBottomFaceColor", this.modelColors3DConfig.BendGroupTopBottomFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.BendGroupTopBottomFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("BendGroupConnectingFaceColor", this.modelColors3DConfig.BendGroupConnectingFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.BendGroupConnectingFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("BendGroupNoDeductionValueFound", this.modelColors3DConfig.BendGroupNoDeductionValueFound, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.BendGroupNoDeductionValueFound = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("BendZoneAdjustRadiusErrorColor", this.modelColors3DConfig.BendZoneAdjustRadiusErrorColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.BendZoneAdjustRadiusErrorColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("PurchasedPartFaceColor", this.modelColors3DConfig.PurchasedPartFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.PurchasedPartFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("NotBendableFaceColor", this.modelColors3DConfig.NotBendableFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.NotBendableFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("CutOutMarker", this.modelColors3DConfig.CutOutColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.CutOutColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("SelectionFacePrimary", this.modelColors3DConfig.SelectionHighlightFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.SelectionHighlightFaceColor = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("SelectionEdgePrimary", this.modelColors3DConfig.SelectedObjectHighlightBorderColorPrimary, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.SelectedObjectHighlightBorderColorPrimary = col.Color;
		}));
		observableCollection.Add(this.CreateColorSetting("SelectionEdgeSecondary", this.modelColors3DConfig.SelectedObjectHighlightBorderColorSecondary, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.SelectedObjectHighlightBorderColorSecondary = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroSimpleHole", this.modelColors3DConfig.MacroSimpleHoleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroSimpleHoleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroStepDrilling", this.modelColors3DConfig.MacroStepDrillingFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroStepDrillingFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroCounterSink", this.modelColors3DConfig.MacroCounterSinkFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroCounterSinkFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroTwoSidedCounterSink", this.modelColors3DConfig.MacroTwoSidedCounterSinkFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroTwoSidedCounterSinkFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedCounterSink", this.modelColors3DConfig.MacroEmbossedCounterSinkFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedCounterSinkFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroBolt", this.modelColors3DConfig.MacroBoltFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroBoltFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroLouver", this.modelColors3DConfig.MacroLouverFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroLouverFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroBridgeLance", this.modelColors3DConfig.MacroBridgeFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroBridgeFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroBorder", this.modelColors3DConfig.MacroBorderColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroBorderColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroChamfer", this.modelColors3DConfig.MacroChamferFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroChamferFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroDeepening", this.modelColors3DConfig.MacroDeepeningFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroDeepeningFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroBlindHole", this.modelColors3DConfig.MacroBlindHoleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroBlindHoleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroConicBlindHole", this.modelColors3DConfig.MacroConicBlindHoleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroConicBlindHoleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroSphericalBlindHole", this.modelColors3DConfig.MacroSphericalBlindHoleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroSphericalBlindHoleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroLance", this.modelColors3DConfig.MacroLanceFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroLanceFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedCircle", this.modelColors3DConfig.MacroEmbossedCircleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedCircleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedLine", this.modelColors3DConfig.MacroEmbossedLineFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedLineFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedSquare", this.modelColors3DConfig.MacroEmbossedSquareFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedSquareFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedRectangle", this.modelColors3DConfig.MacroEmbossedRectangleFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedRectangleFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedSquareRounded", this.modelColors3DConfig.MacroEmbossedSquareRoundedFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedSquareRoundedFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedRectangleRounded", this.modelColors3DConfig.MacroEmbossedRectangleRoundedFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedRectangleRoundedFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossedFreeform", this.modelColors3DConfig.MacroEmbossedFreeformFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossedFreeformFaceColor = col.Color;
		}));
		observableCollection2.Add(this.CreateColorSetting("MacroEmbossmentStamp", this.modelColors3DConfig.MacroEmbossmentStampFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.MacroEmbossmentStampFaceColor = col.Color;
		}));
		observableCollection3.Add(this.CreateColorSetting("Fingers", this.modelColors3DConfig.FingerColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.FingerColor = col.Color;
			config.OverwriteFingerColor = col.Overwrite;
		}, this.modelColors3DConfig.OverwriteFingerColor, Visibility.Visible));
		observableCollection3.Add(this.CreateColorSetting("BendAids", this.modelColors3DConfig.BendAidsColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.BendAidsColor = col.Color;
			config.OverwriteBendAidsColor = col.Overwrite;
		}, this.modelColors3DConfig.OverwriteBendAidsColor, Visibility.Visible));
		observableCollection3.Add(this.CreateColorSetting("ToolFaceColor", this.modelColors3DConfig.ToolFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolFaceColor = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection3.Add(this.CreateColorSetting("ToolHighlightFaceColor", this.modelColors3DConfig.ToolHighlightFaceColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolHighlightFaceColor = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection3.Add(this.CreateColorSetting("ToolHighlightEdgeColor", this.modelColors3DConfig.ToolHighlightEdgeColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolHighlightEdgeColor = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection3.Add(this.CreateColorSetting("ToolFaceColorError", this.modelColors3DConfig.ToolFaceColorError, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolFaceColorError = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection3.Add(this.CreateColorSetting("ToolHighlightFaceColorError", this.modelColors3DConfig.ToolHighlightFaceColorError, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolHighlightFaceColorError = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection3.Add(this.CreateColorSetting("ToolHighlightEdgeColorError", this.modelColors3DConfig.ToolHighlightEdgeColorError, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.ToolHighlightEdgeColorError = col.Color;
		}, this.modelColors3DConfig.OverwriteBendAidsColor));
		observableCollection4.Add(this.CreateColorSetting("EditOrderHoverColor", this.modelColors3DConfig.EditOrderHoverColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderHoverColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderFullyBentColor", this.modelColors3DConfig.EditOrderFullyBentColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderFullyBentColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderHoverAndFullyBentColor", this.modelColors3DConfig.EditOrderHoverAndFullyBentColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderHoverAndFullyBentColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderDefaultColor", this.modelColors3DConfig.EditOrderDefaultColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderDefaultColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderReverseColor", this.modelColors3DConfig.EditOrderReverseColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderReverseColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderMergeColor", this.modelColors3DConfig.EditOrderMergeColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderMergeColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderSplitColor", this.modelColors3DConfig.EditOrderSplitColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderSplitColor = col.Color;
		}));
		observableCollection4.Add(this.CreateColorSetting("EditOrderAmbiguousColor", this.modelColors3DConfig.EditOrderAmbiguousColor, delegate(ColorSetting col, ModelColors3DConfig config)
		{
			config.EditOrderAmbiguousColor = col.Color;
		}));
		this.PartColors3D = observableCollection;
		this.MacroColors3D = observableCollection2;
		this.MachineColors3D = observableCollection3;
		this.EditOrderColors3D = observableCollection4;
	}

	private ColorSetting CreateColorSetting(string title, CfgColor color, Action<ColorSetting, ModelColors3DConfig> saveAction, bool overwrite = true, Visibility overwriteVisibility = Visibility.Collapsed)
	{
		ColorSetting colorProp = new ColorSetting
		{
			OverwriteOptionVisibility = overwriteVisibility
		};
		colorProp.Color = color;
		colorProp.ColorChangeCommand = new RelayCommand((Action<object>)delegate
		{
			CfgColor color2 = colorProp.Color.Clone();
			if (this.subPopup.SelectCfgColor(color2))
			{
				colorProp.Color = color2;
			}
		});
		colorProp.SaveAction = saveAction;
		colorProp.Overwrite = overwrite;
		colorProp.Desc = this._translator.Translate("l_popup.PnInterfaceSettings." + title);
		return colorProp;
	}
}

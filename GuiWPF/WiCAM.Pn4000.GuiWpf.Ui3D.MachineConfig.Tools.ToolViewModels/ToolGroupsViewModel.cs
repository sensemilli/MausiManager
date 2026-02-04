using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class ToolGroupsViewModel : ToolViewModelBase, ICopyTab, IDeleteTab, IAddTab, IEditTab
{
	private ObservableCollection<UpperToolViewModel> _punchGroupTools;

	private ObservableCollection<LowerToolViewModel> _dieGroupTools;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private UpperToolGroupViewModel _selectedPunchGroup;

	private LowerToolGroupViewModel _selectedDieGroup;

	private ICommand _selectClick;

	private ICommand _punchesOrderChangedCommand;

	private ICommand _diesOrderChangedCommand;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _punchBorderBrush;

	private Brush _dieBorderBrush;

	private UpperToolViewModel _selectedPrimaryToolPunch;

	private LowerToolViewModel _selectedPrimaryToolDie;

	private bool _singleSelected = true;

	private bool _isCopyButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private FrameworkElement _imagePart;

	private bool _showGroupswithNoAmount;

	private List<LowerToolGroupViewModel> _dieGroupView;

	private List<UpperToolGroupViewModel> _punchGroupView = new List<UpperToolGroupViewModel>();

	private ChangedConfigType _changed;
    private ITranslator _translator;
    private IDrawToolProfiles _drawToolProfiles;
    private IConfigProvider _configProvider;
    private IModelFactory _modelFactory;

    public FrameworkElement EditScreen
	{
		get
		{
			return _editScreen;
		}
		set
		{
			_editScreen = value;
			NotifyPropertyChanged("EditScreen");
		}
	}

	public Visibility EditScreenVisible
	{
		get
		{
			return _editScreenVisible;
		}
		set
		{
			_editScreenVisible = value;
			NotifyPropertyChanged("EditScreenVisible");
		}
	}

	public ICommand SelectClick => _selectClick ?? (_selectClick = new RelayCommand(SelectType));

	public ICommand PunchesOrderChangedCommand => _punchesOrderChangedCommand ?? (_punchesOrderChangedCommand = new RelayCommand((Action<object>)delegate
	{
		RefreshPunchPriorities();
	}));

	public ICommand DiesOrderChangedCommand => _diesOrderChangedCommand ?? (_diesOrderChangedCommand = new RelayCommand((Action<object>)delegate
	{
		RefreshDiePriorities();
	}));

	public ICommand KeyDownDelete => _keyDownDelete ?? (_keyDownDelete = new RelayCommand(Delete_Click));

	public Type LastSelectedType
	{
		get
		{
			return _lastSelectedType;
		}
		set
		{
			_lastSelectedType = value;
			NotifyPropertyChanged("LastSelectedType");
			if (_lastSelectedType == typeof(DieGroup))
			{
				PunchBorderBrush = new SolidColorBrush(Colors.Transparent);
				DieBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = SelectedPunchGroup;
			}
			else if (_lastSelectedType == typeof(PunchGroup))
			{
				PunchBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				DieBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = SelectedDieGroup;
			}
		}
	}

	public UpperToolGroupViewModel SelectedPunchGroup
	{
		get
		{
			return _selectedPunchGroup;
		}
		set
		{
			_selectedPunchGroup = value;
			NotifyPropertyChanged("SelectedPunchGroup");
			SelectType("PunchGroups");
			RefreshPunchPreviews();
			Parallel.ForEach((IEnumerable<UpperToolGroupViewModel>)base.ToolConfigModel.UpperGroups, (Action<UpperToolGroupViewModel>)delegate
			{
				_ = SelectedPunchGroup;
			});
			UpperToolGroupViewModel selectedPunchGroup = SelectedPunchGroup;
			if (selectedPunchGroup != null && selectedPunchGroup.PrimaryToolId >= 0)
			{
				SelectedPrimaryToolPunch = base.ToolConfigModel.UpperTools.FirstOrDefault((UpperToolViewModel p) => p.ID == SelectedPunchGroup.PrimaryToolId);
			}
			SetEditorEnableRules();
		}
	}

	public LowerToolGroupViewModel SelectedDieGroup
	{
		get
		{
			return _selectedDieGroup;
		}
		set
		{
			_selectedDieGroup = value;
			NotifyPropertyChanged("SelectedDieGroup");
			SelectType("DieGroups");
			RefreshDiePreviews();
			Parallel.ForEach((IEnumerable<LowerToolGroupViewModel>)base.ToolConfigModel.LowerGroups, (Action<LowerToolGroupViewModel>)delegate
			{
				_ = SelectedDieGroup;
			});
			LowerToolGroupViewModel selectedDieGroup = SelectedDieGroup;
			if (selectedDieGroup != null && selectedDieGroup.PrimaryToolId >= 0)
			{
				SelectedPrimaryToolDie = base.ToolConfigModel.LowerTools.FirstOrDefault((LowerToolViewModel p) => p.ID == SelectedDieGroup.PrimaryToolId);
			}
			SetEditorEnableRules();
		}
	}

	public ObservableCollection<object> SelectedPunchGroups { get; internal set; }

	public ObservableCollection<object> SelectedDieGroups { get; internal set; }

	public Brush PunchBorderBrush
	{
		get
		{
			return _punchBorderBrush;
		}
		set
		{
			_punchBorderBrush = value;
			NotifyPropertyChanged("PunchBorderBrush");
		}
	}

	public Brush DieBorderBrush
	{
		get
		{
			return _dieBorderBrush;
		}
		set
		{
			_dieBorderBrush = value;
			NotifyPropertyChanged("DieBorderBrush");
		}
	}

	public UpperToolViewModel SelectedPrimaryToolPunch
	{
		get
		{
			return _selectedPrimaryToolPunch;
		}
		set
		{
			_selectedPrimaryToolPunch = value;
			NotifyPropertyChanged("SelectedPrimaryToolPunch");
			if (SelectedPrimaryToolPunch != null)
			{
				SelectedPunchGroup.PrimaryToolId = SelectedPrimaryToolPunch?.ID;
				SelectedPunchGroup.PrimaryToolName = SelectedPrimaryToolPunch.Name;
			}
		}
	}

	public LowerToolViewModel SelectedPrimaryToolDie
	{
		get
		{
			return _selectedPrimaryToolDie;
		}
		set
		{
			_selectedPrimaryToolDie = value;
			NotifyPropertyChanged("SelectedPrimaryToolDie");
			if (SelectedPrimaryToolDie != null)
			{
				SelectedDieGroup.PrimaryToolId = SelectedPrimaryToolDie?.ID;
				SelectedDieGroup.PrimaryToolName = SelectedPrimaryToolDie.Name;
			}
		}
	}

	public ObservableCollection<UpperToolViewModel> PunchGroupTools
	{
		get
		{
			return _punchGroupTools;
		}
		set
		{
			if (_punchGroupTools != value)
			{
				_punchGroupTools = value;
				NotifyPropertyChanged("PunchGroupTools");
			}
		}
	}

	public ObservableCollection<LowerToolViewModel> DieGroupTools
	{
		get
		{
			return _dieGroupTools;
		}
		set
		{
			if (_dieGroupTools != value)
			{
				_dieGroupTools = value;
				NotifyPropertyChanged("DieGroupTools");
			}
		}
	}

	public bool SingleSelected
	{
		get
		{
			return _singleSelected;
		}
		set
		{
			_singleSelected = value;
			NotifyPropertyChanged("SingleSelected");
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
			NotifyPropertyChanged("IsCopyButtonEnabled");
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
			NotifyPropertyChanged("IsDeleteButtonEnabled");
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
			NotifyPropertyChanged("IsEditButtonEnabled");
		}
	}

	public bool IsAddButtonEnabled
	{
		get
		{
			return _isAddButtonEnabled;
		}
		set
		{
			_isAddButtonEnabled = value;
			NotifyPropertyChanged("IsAddButtonEnabled");
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
			NotifyPropertyChanged("IsOkButtonEnabled");
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
			NotifyPropertyChanged("IsCancelButtonEnabled");
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
			NotifyPropertyChanged("IsSaveButtonEnabled");
		}
	}

	public bool ShowGroupsWithNoAmount
	{
		get
		{
			return _showGroupswithNoAmount;
		}
		set
		{
			_showGroupswithNoAmount = value;
			NotifyPropertyChanged("ShowGroupsWithNoAmount");
			RefreshViews();
		}
	}

	public List<LowerToolGroupViewModel> DieGroupView
	{
		get
		{
			return _dieGroupView;
		}
		set
		{
			_dieGroupView = value;
			NotifyPropertyChanged("DieGroupView");
		}
	}

	public List<UpperToolGroupViewModel> PunchGroupView
	{
		get
		{
			return _punchGroupView;
		}
		set
		{
			_punchGroupView = value;
			NotifyPropertyChanged("PunchGroupView");
		}
	}

	public new FrameworkElement ImagePart
	{
		get
		{
			return _imagePart;
		}
		set
		{
			_imagePart = value;
			NotifyPropertyChanged("ImagePart");
		}
	}

	public ObservableCollection<RadMenuItem> PunchGroupContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> DieGroupContextMenuItems { get; set; }

	public List<ComboboxEntry<CornerType>> CornerTypes { get; private set; }

	public ToolGroupsViewModel(IConfigProvider _configProvider, IModelFactory _modelFactory, IDrawToolProfiles _drawToolProfiles, ITranslator _translator)
		: base(_modelFactory, _translator)
	{
		this._translator = _translator ?? throw new ArgumentNullException(nameof(_translator));
		this._drawToolProfiles = _drawToolProfiles ?? throw new ArgumentNullException(nameof(_drawToolProfiles));
		this._configProvider = _configProvider ?? throw new ArgumentNullException(nameof(_configProvider));
		this._modelFactory = _modelFactory ?? throw new ArgumentNullException(nameof(_modelFactory));
    }

	public void Init(MachineToolsViewModel toolConfigModel, IToolExpert tools = null)
	{
		base.Init(toolConfigModel);
		CornerTypes = _translator.GetTranslatedComboboxEntries<CornerType>().ToList();
		ImagePart = new Canvas
		{
			Height = 1.0,
			Width = 1.0
		};
		ImagePart.Loaded += Image3DOnLoaded;
		PunchGroupTools = new ObservableCollection<UpperToolViewModel>();
		DieGroupTools = new ObservableCollection<LowerToolViewModel>();
		InitializePunchGroupContextMenuItems();
		InitializeDieGroupContextMenuItems();
		if (base.ToolConfigModel.LowerGroups.Count > 0)
		{
			SelectedDieGroup = null;
			LowerToolGroupViewModel selectedDieGroup = SelectedDieGroup;
			if (selectedDieGroup != null && selectedDieGroup.PrimaryToolId >= 0)
			{
				SelectedPrimaryToolDie = base.ToolConfigModel.LowerTools.FirstOrDefault((LowerToolViewModel p) => p.ID == SelectedDieGroup.PrimaryToolId);
			}
		}
		SelectedDieGroup = base.ToolConfigModel.LowerGroups.FirstOrDefault();
		if (base.ToolConfigModel.UpperGroups.Count > 0)
		{
			SelectedPunchGroup = null;
			UpperToolGroupViewModel selectedPunchGroup = SelectedPunchGroup;
			if (selectedPunchGroup != null && selectedPunchGroup.PrimaryToolId >= 0)
			{
				SelectedPrimaryToolPunch = base.ToolConfigModel.UpperTools.FirstOrDefault((UpperToolViewModel p) => p.ID == SelectedPunchGroup.PrimaryToolId);
			}
		}
		SelectedPunchGroup = base.ToolConfigModel.UpperGroups.FirstOrDefault();
		RefreshViews();
	}

	private void RefreshViews()
	{
		DieGroupView = base.ToolConfigModel.LowerGroups.Where(DieGroupFilter).ToList();
		PunchGroupView = base.ToolConfigModel.UpperGroups.Where(PunchGroupFilter).ToList();
	}

	private bool DieGroupFilter(LowerToolGroupViewModel lowerGroup)
	{
		if (ShowGroupsWithNoAmount)
		{
			return true;
		}
		HashSet<MultiToolViewModel> multiTools = (from x in base.ToolConfigModel.LowerTools
			where x.Group == lowerGroup
			select x.MultiTool).ToHashSet();
		return base.ToolConfigModel.LowerPieces.Where((LowerToolPieceViewModel x) => multiTools.Contains(x.MultiTool)).Any((LowerToolPieceViewModel x) => x.TotalAmount > 0);
	}

	private bool PunchGroupFilter(UpperToolGroupViewModel upperGroup)
	{
		if (ShowGroupsWithNoAmount)
		{
			return true;
		}
		HashSet<MultiToolViewModel> multiTools = (from x in base.ToolConfigModel.UpperTools
			where x.Group == upperGroup
			select x.MultiTool).ToHashSet();
		return base.ToolConfigModel.UpperPieces.Where((UpperToolPieceViewModel x) => multiTools.Contains(x.MultiTool)).Any((UpperToolPieceViewModel x) => x.TotalAmount > 0);
	}

	private void InitializePunchGroupContextMenuItems()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		PunchGroupContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyPunchGroup_Click)
		};
		PunchGroupContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditPunchGroup_Click)
		};
		PunchGroupContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeletePunchGroup_Click)
		};
		PunchGroupContextMenuItems.Add(item3);
	}

	private void InitializeDieGroupContextMenuItems()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		DieGroupContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyDieGroup_Click)
		};
		DieGroupContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditDieGroup_Click)
		};
		DieGroupContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteDieGroup_Click)
		};
		DieGroupContextMenuItems.Add(item3);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		SelectedDieGroup = base.ToolConfigModel.LowerGroups.FirstOrDefault();
		SelectedPunchGroup = base.ToolConfigModel.UpperGroups.FirstOrDefault();
	}

	public void Refresh()
	{
		foreach (UpperToolGroupViewModel group in base.ToolConfigModel.UpperGroups)
		{
			IEnumerable<UpperToolViewModel> enumerable = base.ToolConfigModel.UpperTools.Where((UpperToolViewModel p) => p.Group.ID == group.ID);
			foreach (UpperToolViewModel item in enumerable)
			{
				item.Group = group;
			}
			UpperToolViewModel upperToolViewModel = enumerable.OrderBy((UpperToolViewModel p) => p.Priority).FirstOrDefault();
			if (upperToolViewModel != null)
			{
				group.PrimaryToolId = upperToolViewModel.ID;
				group.PrimaryToolName = upperToolViewModel.Name;
			}
			else
			{
				group.PrimaryToolId = -1;
				group.PrimaryToolName = null;
			}
		}
		foreach (LowerToolGroupViewModel group2 in base.ToolConfigModel.LowerGroups)
		{
			IEnumerable<LowerToolViewModel> enumerable2 = base.ToolConfigModel.LowerTools.Where((LowerToolViewModel p) => p.Group.ID == group2.ID);
			foreach (LowerToolViewModel item2 in enumerable2)
			{
				item2.Group = group2;
			}
			LowerToolViewModel lowerToolViewModel = enumerable2.OrderBy((LowerToolViewModel p) => p.Priority).FirstOrDefault();
			if (lowerToolViewModel != null)
			{
				group2.PrimaryToolId = lowerToolViewModel?.ID;
				group2.PrimaryToolName = lowerToolViewModel.Name;
			}
			else
			{
				group2.PrimaryToolId = -1;
				group2.PrimaryToolName = null;
			}
		}
	}

	private void RefreshPunchPreviews()
	{
		List<UpperToolViewModel> list = new List<UpperToolViewModel>();
		foreach (UpperToolViewModel item in base.ToolConfigModel.UpperTools.Where((UpperToolViewModel p) => p?.Group?.ID == SelectedPunchGroup?.ID))
		{
			list.Add(item);
			item.ImageProfile = new Canvas
			{
				Height = 93.0,
				Width = 92.0
			};
			MeasureAndArrange(item.ImageProfile);
			item.Load2DPreview((Canvas)item.ImageProfile, _drawToolProfiles);
		}
		PunchGroupTools = list.OrderBy((UpperToolViewModel p) => p.Priority).ToObservableCollection();
	}

	private void RefreshDiePreviews()
	{
		List<LowerToolViewModel> list = new List<LowerToolViewModel>();
		foreach (LowerToolViewModel item in base.ToolConfigModel.LowerTools.Where((LowerToolViewModel p) => p.Group?.ID == SelectedDieGroup?.ID))
		{
			list.Add(item);
			item.ImageProfile = new Canvas
			{
				Height = 93.0,
				Width = 92.0
			};
			MeasureAndArrange(item.ImageProfile);
			item.Load2DPreview((Canvas)item.ImageProfile, _drawToolProfiles);
		}
		DieGroupTools = list.OrderBy((LowerToolViewModel p) => p.Priority).ToObservableCollection();
	}

	private static void MeasureAndArrange(UIElement element)
	{
		element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		element.Arrange(new Rect(0.0, 0.0, element.DesiredSize.Width, element.DesiredSize.Height));
	}

	private void RefreshPunchPriorities()
	{
		for (int i = 0; i < PunchGroupTools.Count; i++)
		{
			PunchGroupTools[i].Priority = i + 1;
		}
		UpperToolViewModel upperToolViewModel = PunchGroupTools.FirstOrDefault();
		if (upperToolViewModel == null)
		{
			SelectedPunchGroup.PrimaryToolId = -1;
			SelectedPunchGroup.PrimaryToolName = null;
		}
		else
		{
			SelectedPunchGroup.PrimaryToolId = upperToolViewModel.ID;
			SelectedPunchGroup.PrimaryToolName = upperToolViewModel.Name;
		}
	}

	private void RefreshDiePriorities()
	{
		for (int i = 0; i < DieGroupTools.Count; i++)
		{
			DieGroupTools[i].Priority = i + 1;
		}
		LowerToolViewModel lowerToolViewModel = DieGroupTools.FirstOrDefault();
		if (lowerToolViewModel == null)
		{
			SelectedDieGroup.PrimaryToolId = -1;
			SelectedDieGroup.PrimaryToolName = null;
		}
		else
		{
			SelectedDieGroup.PrimaryToolId = lowerToolViewModel.ID;
			SelectedDieGroup.PrimaryToolName = lowerToolViewModel.Name;
		}
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "PunchGroups"))
		{
			if (text == "DieGroups")
			{
				LastSelectedType = typeof(DieGroup);
				base.LastSelectedObject = SelectedDieGroup;
			}
		}
		else
		{
			LastSelectedType = typeof(PunchGroup);
			base.LastSelectedObject = SelectedPunchGroup;
		}
	}

	public void AddButtonClick()
	{
		if (LastSelectedType == typeof(PunchGroup))
		{
			AddPunchGroup_Click();
		}
		else if (LastSelectedType == typeof(DieGroup))
		{
			AddDieGroup_Click();
		}
	}

	public void AddPunchGroup_Click()
	{
		UpperToolGroupViewModel selectedPunchGroup = base.ToolConfigModel.CreateUpperToolGroup("");
		SelectedPunchGroup = selectedPunchGroup;
		_changed = ChangedConfigType.PunchGroup;
	}

	public void AddDieGroup_Click()
	{
		LowerToolGroupViewModel lowerToolGroupViewModel = base.ToolConfigModel.CreateLowerToolGroup();
		lowerToolGroupViewModel.Name = "";
		SelectedDieGroup = lowerToolGroupViewModel;
		_changed = ChangedConfigType.DieGroup;
	}

	public void CopyButtonClick()
	{
		if (LastSelectedType == typeof(PunchGroup))
		{
			CopyPunchGroup_Click();
		}
		else if (LastSelectedType == typeof(DieGroup))
		{
			CopyDieGroup_Click();
		}
	}

	public void CopyPunchGroup_Click()
	{
		if (SelectedPunchGroup != null)
		{
			UpperToolGroupViewModel upperToolGroupViewModel = (UpperToolGroupViewModel)SelectedPunchGroup.Copy();
			upperToolGroupViewModel.ClearID();
			base.ToolConfigModel.UpperGroups.Add(upperToolGroupViewModel);
			SelectedPunchGroup = base.ToolConfigModel.UpperGroups.LastOrDefault();
			_changed = ChangedConfigType.PunchGroup;
		}
	}

	public void CopyDieGroup_Click()
	{
		if (SelectedDieGroup != null)
		{
			LowerToolGroupViewModel lowerToolGroupViewModel = (LowerToolGroupViewModel)SelectedDieGroup.Copy();
			lowerToolGroupViewModel.ClearID();
			base.ToolConfigModel.LowerGroups.Add(lowerToolGroupViewModel);
			SelectedDieGroup = base.ToolConfigModel.LowerGroups.LastOrDefault();
			_changed = ChangedConfigType.DieGroup;
		}
	}

	public void DeleteButtonClick()
	{
		if (LastSelectedType == typeof(PunchGroup))
		{
			Delete_Click(SelectedPunchGroup);
		}
		else if (LastSelectedType == typeof(DieGroup))
		{
			Delete_Click(SelectedDieGroup);
		}
	}

	public void Delete_Click(object param)
	{
		if (param == null)
		{
			return;
		}
		if (!(param is UpperToolGroupViewModel))
		{
			if (param is LowerToolGroupViewModel && SelectedDieGroups.Any())
			{
				DeleteDieGroup_Click();
			}
		}
		else if (SelectedPunchGroups.Any())
		{
			DeletePunchGroup_Click();
		}
	}

	private void DeletePunchGroup_Click()
	{
		int num = base.ToolConfigModel.UpperGroups.IndexOf((UpperToolGroupViewModel)SelectedPunchGroups.Last()) + 1 - SelectedPunchGroups.Count();
		base.ToolConfigModel.DeleteUpperGroup((UpperToolGroupViewModel)SelectedPunchGroups.Last());
		if (num >= base.ToolConfigModel.UpperGroups.Count)
		{
			num = base.ToolConfigModel.UpperGroups.Count - 1;
		}
		SetEditorEnableRules();
		if (num < 0)
		{
			SelectedPunchGroup = null;
		}
		else
		{
			SelectedPunchGroup = base.ToolConfigModel.UpperGroups[num];
		}
		_changed = ChangedConfigType.PunchGroup;
	}

	private void DeleteDieGroup_Click()
	{
		int num = base.ToolConfigModel.LowerGroups.IndexOf((LowerToolGroupViewModel)SelectedDieGroups.Last()) + 1 - SelectedDieGroups.Count();
		base.ToolConfigModel.DeleteLowerGroup((LowerToolGroupViewModel)SelectedDieGroups.Last());
		if (num >= base.ToolConfigModel.LowerGroups.Count)
		{
			num = base.ToolConfigModel.LowerGroups.Count - 1;
		}
		SetEditorEnableRules();
		if (num < 0)
		{
			SelectedDieGroup = null;
		}
		else
		{
			SelectedDieGroup = base.ToolConfigModel.LowerGroups[num];
		}
		_changed = ChangedConfigType.DieGroup;
	}

	public void EditButtonClick()
	{
		if (LastSelectedType == typeof(PunchGroup))
		{
			EditPunchGroup_Click();
		}
		else if (LastSelectedType == typeof(DieGroup))
		{
			EditDieGroup_Click();
		}
	}

	public void EditPunchGroup_Click()
	{
		Action<bool, bool> closeAction = delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.UpperGroups.IndexOf(SelectedPunchGroup);
				base.ToolConfigModel.UpperGroups.Remove(SelectedPunchGroup);
				base.ToolConfigModel.UpperGroups.Insert(index, ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as UpperToolGroupViewModel);
				SelectedPunchGroup = base.ToolConfigModel.UpperGroups[index];
			}
			if (close)
			{
				((EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				EditScreen = null;
				SetEditorEnableRules();
			}
		};
		EditScreenViewModel dataContext = new EditScreenViewModel(((IEnumerable<UpperToolGroupViewModel>)base.ToolConfigModel.UpperGroups).Select((Func<UpperToolGroupViewModel, ToolItemViewModel>)((UpperToolGroupViewModel i) => i)), SelectedPunchGroup, isUpper: false, base.ToolConfigModel, _configProvider, closeAction, _drawToolProfiles);
		EditScreenView editScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		if (EditScreen != null)
		{
			((EditScreen as EditScreenView)?.DataContext as EditScreenViewModel)?.Dispose();
			EditScreen = null;
		}
		EditScreen = editScreen;
		EditScreenVisible = Visibility.Visible;
		SetEditorEnableRules();
	}

	public void EditDieGroup_Click()
	{
		Action<bool, bool> closeAction = delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.LowerGroups.IndexOf(SelectedDieGroup);
				base.ToolConfigModel.LowerGroups.Remove(SelectedDieGroup);
				base.ToolConfigModel.LowerGroups.Insert(index, ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as LowerToolGroupViewModel);
				SelectedDieGroup = base.ToolConfigModel.LowerGroups[index];
			}
			if (close)
			{
				((EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				EditScreen = null;
				SetEditorEnableRules();
			}
		};
		EditScreenViewModel dataContext = new EditScreenViewModel(((IEnumerable<LowerToolGroupViewModel>)base.ToolConfigModel.LowerGroups).Select((Func<LowerToolGroupViewModel, ToolItemViewModel>)((LowerToolGroupViewModel i) => i)), SelectedDieGroup, isUpper: false, base.ToolConfigModel, _configProvider, closeAction, _drawToolProfiles);
		EditScreenView editScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		if (EditScreen != null)
		{
			((EditScreen as EditScreenView)?.DataContext as EditScreenViewModel)?.Dispose();
			EditScreen = null;
		}
		EditScreen = editScreen;
		EditScreenVisible = Visibility.Visible;
		SetEditorEnableRules();
	}

	public void SetEditorEnableRules()
	{
		if (EditScreen != null)
		{
			IsAddButtonEnabled = false;
			IsCopyButtonEnabled = false;
			IsDeleteButtonEnabled = false;
			IsEditButtonEnabled = false;
			IsOkButtonEnabled = false;
			IsCancelButtonEnabled = false;
			IsSaveButtonEnabled = false;
			return;
		}
		if (LastSelectedType == typeof(DieGroup))
		{
			ObservableCollection<object> selectedDieGroups = SelectedDieGroups;
			SingleSelected = selectedDieGroups == null || selectedDieGroups.Count() <= 1;
			if (SelectedDieGroup == null)
			{
				IsCopyButtonEnabled = false;
				IsDeleteButtonEnabled = false;
				IsEditButtonEnabled = false;
				IsAddButtonEnabled = true;
				IsOkButtonEnabled = true;
				IsCancelButtonEnabled = true;
				IsSaveButtonEnabled = true;
				return;
			}
			IsCopyButtonEnabled = SelectedDieGroup != null && SingleSelected;
			IsEditButtonEnabled = SelectedDieGroup != null && SingleSelected;
			IsDeleteButtonEnabled = SelectedDieGroup != null;
		}
		else if (LastSelectedType == typeof(PunchGroup))
		{
			ObservableCollection<object> selectedPunchGroups = SelectedPunchGroups;
			SingleSelected = selectedPunchGroups == null || selectedPunchGroups.Count() <= 1;
			if (SelectedPunchGroup == null)
			{
				IsCopyButtonEnabled = false;
				IsDeleteButtonEnabled = false;
				IsEditButtonEnabled = false;
				IsAddButtonEnabled = true;
				IsOkButtonEnabled = true;
				IsCancelButtonEnabled = true;
				IsSaveButtonEnabled = true;
				return;
			}
			IsCopyButtonEnabled = SelectedPunchGroup != null && SingleSelected;
			IsEditButtonEnabled = SelectedPunchGroup != null && SingleSelected;
			IsDeleteButtonEnabled = SelectedPunchGroup != null;
		}
		else
		{
			IsCopyButtonEnabled = false;
			IsDeleteButtonEnabled = false;
			IsEditButtonEnabled = false;
		}
		IsAddButtonEnabled = true;
		IsOkButtonEnabled = true;
		IsCancelButtonEnabled = true;
		IsSaveButtonEnabled = true;
	}

	public void Dispose()
	{
		ImagePart = null;
	}
}

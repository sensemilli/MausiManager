using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class PopupMachineConfigViewModel : ViewModelBase
{
	private PopupMachineConfigModel _model;

	private double _dialogOpacity;

	private TabItem _selectedTab;

	private TabItem _selectedToolsTab;

	private TabItem _selectedHoldersTab;

	private ICommand _loadData;

	private ICommand _cancelCommand;

	private ICommand _okCommand;

	private ICommand _addCommand;

	private ICommand _deleteCommand;

	private ICommand _copyCommand;

	private ICommand _saveCommand;

	private ICommand _editCommand;

	private Action<PopupMachineConfigViewModel> _closeAction;

	private readonly IPnCommandBasics _pnCommandBasics;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IMachineBendFactory _machineBendFactory;

	private bool _isCopyButtonEnabled;

	private bool _isAddNewButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private ChangedConfigType _changedConfigType;

	private ChangesResult _changesResult;

	private IGlobals _globals;

	private readonly IModelFactory _modelFactory;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IDoEvents _doEvents;

	public double DialogOpacity
	{
		get
		{
			return _dialogOpacity;
		}
		set
		{
			_dialogOpacity = value;
			NotifyPropertyChanged("DialogOpacity");
		}
	}

	public TabItem SelectedTab
	{
		get
		{
			return _selectedTab;
		}
		set
		{
			_selectedTab = value;
			NotifyPropertyChanged("SelectedTab");
			SetEditorEnableRules();
		}
	}

	public TabItem SelectedToolsTab
	{
		get
		{
			return _selectedToolsTab;
		}
		set
		{
			_selectedToolsTab = value;
			NotifyPropertyChanged("SelectedToolsTab");
			SetEditorEnableRules(_selectedToolsTab);
		}
	}

	public TabItem SelectedHoldersTab
	{
		get
		{
			return _selectedHoldersTab;
		}
		set
		{
			_selectedHoldersTab = value;
			NotifyPropertyChanged("SelectedHoldersTab");
			SetEditorEnableRules(_selectedHoldersTab);
		}
	}

	public BendMachine BendMachineOld { get; set; }

	public IBendMachine BendMachine { get; set; }

	public ICommand LoadDataCommand => _loadData ?? (_loadData = new RelayCommand((Action<object>)delegate
	{
		LoadDataCall();
	}));

	public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand((Action<object>)delegate
	{
		CloseLikeCancel();
	}));

	public ICommand OkCommand => _okCommand ?? (_okCommand = new RelayCommand((Action<object>)delegate
	{
		CloseLikeOk();
	}));

	public ICommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand((Action<object>)delegate
	{
		Add();
	}));

	public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand((Action<object>)delegate
	{
		Delete();
	}));

	public ICommand CopyCommand => _copyCommand ?? (_copyCommand = new RelayCommand((Action<object>)delegate
	{
		Copy();
	}));

	public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand((Action<object>)delegate
	{
		Save();
	}));

	public ICommand EditCommand => _editCommand ?? (_editCommand = new RelayCommand((Action<object>)delegate
	{
		Edit();
	}));

	public MachineToolsViewModel MachineToolsViewModel { get; set; }

	public WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels.ToolGeneralViewModel ToolGeneralViewModel { get; set; }

	public WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels.ToolGroupsViewModel MachineToolsGroupsViewModel { get; set; }

	public UpperToolsViewModel UpperToolsViewModel { get; set; }

	public LowerToolsViewModel LowerToolsViewModel { get; set; }

	public LowerToolExtensionsViewModel LowerToolExtensionsViewModel { get; set; }

	public DieHemViewModel DieHemViewModel { get; set; }

	public PreferredProfilesViewModel PreferredProfilesViewModel { get; set; }

	public AssignAliasViewModel AssignAliasViewModel { get; set; }

	public FingerStopGeneralViewModel FingerStopGeneralViewModel { get; set; }

	public FingerViewModel LeftFingerViewModel { get; set; }

	public FingerViewModel RightFingerViewModel { get; set; }

	public UpperAdaptersViewModel UpperAdapterViewModel { get; set; }

	public LowerAdaptersViewModel LowerAdapterViewModel { get; set; }

	public UpperHolderViewModel UpperHolderViewModel { get; set; }

	public LowerHolderViewModel LowerHolderViewModel { get; set; }

	public ClampingSystemViewModel ClampingSystemViewModel { get; set; }

	public MappingViewModel MappingViewModel { get; set; }

	public MaterialsViewModel MaterialsViewModel { get; set; }

	public MachineConfigViewModel MachineConfigViewModel { get; set; }

	public BendSequenceEditorViewModel BendSequenceEditorViewModel { get; set; }

	public PPSettingsViewModel PPSettingsViewModel { get; set; }

	public BendDataBaseViewModel BendDataBaseViewModel { get; set; }

	public IBendTableViewModel BendTableViewModel { get; set; }

	public IMachineUnfoldSettingsViewModel MachineUnfoldSettingsViewModel { get; set; }

	public CrowningViewModel CrowningViewModel { get; set; }

	public PressForceHemViewModel PressForceHemViewModel { get; set; }

	public bool IsCopyButtonEnabled => _isCopyButtonEnabled;

	public bool IsAddNewButtonEnabled => _isAddNewButtonEnabled;

	public bool IsDeleteButtonEnabled => _isDeleteButtonEnabled;

	public bool IsEditButtonEnabled => _isEditButtonEnabled;

	public bool IsOkButtonEnabled => _isOkButtonEnabled;

	public bool IsCancelButtonEnabled => _isCancelButtonEnabled;

	public bool IsSaveButtonEnabled => _isSaveButtonEnabled;

	public Visibility DeleteButtonVisibility { get; private set; }

	public Visibility AddButtonVisibility { get; private set; }

	public Visibility CopyButtonVisibility { get; private set; }

	public Visibility EditButtonVisibility { get; private set; }

	private void LoadDataCall()
	{
	}

	public PopupMachineConfigViewModel(PopupMachineConfigModel model, IGlobals globals, IModelFactory modelFactory, IConfigProvider configProvider, IPnCommandBasics pnCommandBasics, IMainWindowBlock mainWindowBlock, IMachineBendFactory machineBendFactory, IGlobalToolStorage toolStorage, IDoEvents doEvents)
	{
		_model = model;
		_globals = globals;
		_modelFactory = modelFactory;
		_pnCommandBasics = pnCommandBasics;
		_mainWindowBlock = mainWindowBlock;
		_machineBendFactory = machineBendFactory;
		_toolStorage = toolStorage;
		_doEvents = doEvents;
		GeneralUserSettingsConfig generalUserSettingsConfig = configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		DialogOpacity = generalUserSettingsConfig.DialogOpacity;
	}

	public void Init(IDoc3d doc, IBendMachine bendMachine, Action<PopupMachineConfigViewModel> closeAction)
	{
		_model.Init(doc, bendMachine);
		_closeAction = closeAction;
		BendMachineOld = null;
		BendMachine = bendMachine;
		LoadViewModels();
	}

	private void LoadViewModels()
	{
		Dispose();
		ToolConfigModel toolConfigModel = new ToolConfigModel(BendMachineOld, _globals.Materials);
		BendTableViewModel = _modelFactory.Resolve<IBendTableViewModel>();
		BendTableViewModel.Init(BendMachine);
		MachineToolsViewModel = _modelFactory.Resolve<MachineToolsViewModel>().Init(BendMachine);
		MachineToolsViewModel machineToolsViewModel = MachineToolsViewModel;
		machineToolsViewModel.ChangedAction = (Action<ChangedConfigType>)Delegate.Combine(machineToolsViewModel.ChangedAction, new Action<ChangedConfigType>(dataChangedAction));
		UpperToolsViewModel = _modelFactory.Resolve<UpperToolsViewModel>().Init(MachineToolsViewModel);
		UpperToolsViewModel.PropertyChanged += PropertyChanged;
		LowerToolsViewModel = _modelFactory.Resolve<LowerToolsViewModel>().Init(MachineToolsViewModel);
		LowerToolsViewModel.PropertyChanged += PropertyChanged;
		LowerToolExtensionsViewModel = _modelFactory.Resolve<LowerToolExtensionsViewModel>().Init(MachineToolsViewModel);
		LowerToolExtensionsViewModel.PropertyChanged += PropertyChanged;
		ToolGeneralViewModel = _modelFactory.Resolve<WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels.ToolGeneralViewModel>().Init(MachineToolsViewModel);
		ToolGeneralViewModel.PropertyChanged += PropertyChanged;
		PreferredProfilesViewModel = _modelFactory.Resolve<PreferredProfilesViewModel>();
		PreferredProfilesViewModel.Init(BendMachine, MachineToolsViewModel, toolConfigModel, _model.Doc);
		PreferredProfilesViewModel.DataChanged = dataChangedAction;
		PreferredProfilesViewModel.PropertyChanged += PropertyChanged;
		AssignAliasViewModel = _modelFactory.Resolve<AssignAliasViewModel>();
		AssignAliasViewModel.Init(BendMachine, MachineToolsViewModel, toolConfigModel, _model.Doc);
		AssignAliasViewModel.DataChanged = dataChangedAction;
		AssignAliasViewModel.PropertyChanged += PropertyChanged;
		MachineToolsGroupsViewModel = _modelFactory.Resolve<WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels.ToolGroupsViewModel>();
		MachineToolsGroupsViewModel.Init(MachineToolsViewModel);
		MachineToolsGroupsViewModel.PropertyChanged += PropertyChanged;
		UpperAdapterViewModel = _modelFactory.Resolve<UpperAdaptersViewModel>();
		UpperAdapterViewModel.Init(MachineToolsViewModel);
		UpperAdapterViewModel.PropertyChanged += PropertyChanged;
		LowerAdapterViewModel = _modelFactory.Resolve<LowerAdaptersViewModel>();
		LowerAdapterViewModel.Init(MachineToolsViewModel);
		LowerAdapterViewModel.PropertyChanged += PropertyChanged;
		FingerStopGeneralViewModel = new FingerStopGeneralViewModel(BendMachine.FingerStopSettings)
		{
			DataChanged = dataChangedAction
		};
		LeftFingerViewModel = _modelFactory.Resolve<FingerViewModel>().Init(BendMachine, FingerStopType.LeftFinger);
		LeftFingerViewModel.DataChanged = dataChangedAction;
		RightFingerViewModel = _modelFactory.Resolve<FingerViewModel>().Init(BendMachine, FingerStopType.RightFinger);
		RightFingerViewModel.DataChanged = dataChangedAction;
		MaterialsViewModel = _modelFactory.Resolve<MaterialsViewModel>();
		MaterialsViewModel.Init(BendMachine.PpMappings.MaterialMapping);
		MaterialsViewModel.DataChanged = dataChangedAction;
		MappingViewModel = _modelFactory.Resolve<MappingViewModel>();
		MappingViewModel.Init(BendMachine, MachineToolsViewModel, MaterialsViewModel);
		MappingViewModel.DataChanged = dataChangedAction;
		MachineConfigViewModel machineConfigViewModel = _modelFactory.Resolve<MachineConfigViewModel>();
		machineConfigViewModel.Init(BendMachine, delegate
		{
			BendSequenceEditorViewModel = _modelFactory.Resolve<BendSequenceEditorViewModel>().Init(BendMachine.ToolCalculationSettings);
			BendSequenceEditorViewModel.DataChanged = dataChangedAction;
		});
		machineConfigViewModel.DataChanged = dataChangedAction;
		MachineConfigViewModel = machineConfigViewModel;
		BendSequenceEditorViewModel = _modelFactory.Resolve<BendSequenceEditorViewModel>().Init(BendMachine.ToolCalculationSettings);
		BendSequenceEditorViewModel.DataChanged = dataChangedAction;
		PPSettingsViewModel = _modelFactory.Resolve<PPSettingsViewModel>();
		PPSettingsViewModel.Init(BendMachine);
		PPSettingsViewModel.DataChanged = dataChangedAction;
		MachineUnfoldSettingsViewModel = _modelFactory.Resolve<IMachineUnfoldSettingsViewModel>();
		MachineUnfoldSettingsViewModel.Init(BendMachine, dataChangedAction);
		CrowningViewModel = _modelFactory.Resolve<CrowningViewModel>();
		CrowningViewModel.Init(BendMachine);
		PressForceHemViewModel = _modelFactory.Resolve<PressForceHemViewModel>();
		PressForceHemViewModel.Init(BendMachine);
		NotifyPropertyChanged("ToolGeneralViewModel");
		NotifyPropertyChanged("UpperToolsViewModel");
		NotifyPropertyChanged("LowerToolsViewModel");
		NotifyPropertyChanged("LowerToolExtensionsViewModel");
		NotifyPropertyChanged("DieHemViewModel");
		NotifyPropertyChanged("PreferredProfilesViewModel");
		NotifyPropertyChanged("MachineToolsGroupsViewModel");
		NotifyPropertyChanged("FingerStopGeneralViewModel");
		NotifyPropertyChanged("LeftFingerViewModel");
		NotifyPropertyChanged("RightFingerViewModel");
		NotifyPropertyChanged("UpperAdapterViewModel");
		NotifyPropertyChanged("LowerAdapterViewModel");
		NotifyPropertyChanged("UpperHolderViewModel");
		NotifyPropertyChanged("LowerHolderViewModel");
		NotifyPropertyChanged("ClampingSystemViewModel");
		NotifyPropertyChanged("MappingViewModel");
		NotifyPropertyChanged("MaterialsViewModel");
		NotifyPropertyChanged("MachineConfigViewModel");
		NotifyPropertyChanged("BendSequenceEditorViewModel");
		NotifyPropertyChanged("PPSettingsViewModel");
		NotifyPropertyChanged("BendDataBaseViewModel");
		NotifyPropertyChanged("MachineUnfoldSettingsViewModel");
		NotifyPropertyChanged("CrowningViewModel");
		NotifyPropertyChanged("PressForceHemViewModel");
		NotifyPropertyChanged("MachineToolsViewModel");
		NotifyPropertyChanged("AssignAliasViewModel");
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

	private void Save()
	{
		if (!CanSaveData())
		{
			return;
		}
		_mainWindowBlock.InitWait();
		if (SelectedTab.Name == "BendTableTab")
		{
			BendDataBaseViewModel bendDataBaseViewModel = BendDataBaseViewModel;
			if (bendDataBaseViewModel != null && bendDataBaseViewModel.SelectPreferredProfileVisible == Visibility.Visible)
			{
				BendDataBaseViewModel.SetSelectedProfile();
				goto IL_005b;
			}
		}
		SaveData();
		LoadViewModels();
		goto IL_005b;
		IL_005b:
		_mainWindowBlock.CloseWait();
	}

	private bool CanSaveData()
	{
		if (BendTableViewModel.CanSave() && MachineToolsViewModel.CanSave() && PreferredProfilesViewModel.CanSave())
		{
			return MappingViewModel.CanSave();
		}
		return false;
	}

	private void SaveData()
	{
		BendTableViewModel?.Save(BendMachine);
		MachineToolsViewModel?.Save(BendMachine.ToolConfig, BendMachine);
		BendSequenceEditorViewModel?.Save();
		PPSettingsViewModel?.Save();
		MachineUnfoldSettingsViewModel?.Save();
		CrowningViewModel?.Save();
		PressForceHemViewModel?.Save();
		MachineConfigViewModel?.Save(BendMachine);
		FingerStopGeneralViewModel?.Save(BendMachine.FingerStopSettings);
		MaterialsViewModel?.Save(BendMachine.PpMappings.MaterialMapping);
		LeftFingerViewModel?.Save();
		RightFingerViewModel?.Save();
		PreferredProfilesViewModel?.Save();
		_machineBendFactory.SaveMachine(BendMachine);
		_ = _changedConfigType & ChangedConfigType.BendSequence;
		_ = 1;
		_ = _changedConfigType & ChangedConfigType.Fingers;
		_ = 1;
		_toolStorage.Save();
	}

	private void CloseLikeOk()
	{
		if (!CanSaveData())
		{
			return;
		}
		_mainWindowBlock.InitWait();
		_doEvents.DoEvents();
		SaveData();
		if (_model.Doc.HasModel && _changedConfigType != 0)
		{
			PopupCheckConfigChangedViewModel dataContext = new PopupCheckConfigChangedViewModel(_model.Globals, delegate(bool result)
			{
				_mainWindowBlock.InitWait();
				_doEvents.DoEvents();
				if (result)
				{
					CheckChanges();
					ApplyChanges();
				}
			});
			PopupCheckConfigChangedView obj = new PopupCheckConfigChangedView
			{
				DataContext = dataContext
			};
			_mainWindowBlock.CloseWait();
			obj.ShowDialog();
		}
		_closeAction?.Invoke(this);
		_mainWindowBlock.BlockUI_Unblock();
		_mainWindowBlock.CloseWait();
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
			case ChangedConfigType.Tools:
			case ChangedConfigType.MachineConfig:
			case ChangedConfigType.PostProcessor:
			case ChangedConfigType.MaterialMapping:
			case ChangedConfigType.ToolMapping:
				if (_changesResult <= ChangesResult.ReloadMachine)
				{
					_changesResult = ChangesResult.ReloadMachine;
				}
				break;
			case ChangedConfigType.PreferredProfiles:
			case ChangedConfigType.Punches:
			case ChangedConfigType.Dies:
			case ChangedConfigType.UpperAdapter:
			case ChangedConfigType.LowerAdapter:
			case ChangedConfigType.Adapters:
			case ChangedConfigType.UpperHolder:
			case ChangedConfigType.LowerHolder:
			case ChangedConfigType.Holders:
			case ChangedConfigType.PunchGroup:
			case ChangedConfigType.DieGroup:
			case ChangedConfigType.ToolGroups:
			case ChangedConfigType.UpperClampingSystem:
			case ChangedConfigType.LowerClampingSystem:
			case ChangedConfigType.ClampingSystem:
			case ChangedConfigType.BendSequence:
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
				_model.Doc.ResetFingers();
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

	private void CloseLikeCancel()
	{
		CloseView();
	}

	private void CloseView()
	{
		_closeAction?.Invoke(this);
		_mainWindowBlock.BlockUI_Unblock();
	}

	private void SetEditorEnableRules(TabItem tabItem = null)
	{
		object? activeViewModel = GetActiveViewModel(tabItem);
		_isEditButtonEnabled = false;
		_isCopyButtonEnabled = false;
		_isDeleteButtonEnabled = false;
		_isAddNewButtonEnabled = false;
		_isOkButtonEnabled = true;
		_isCancelButtonEnabled = true;
		_isSaveButtonEnabled = true;
		if (activeViewModel is IEditTab editTab)
		{
			_isEditButtonEnabled = editTab.IsEditButtonEnabled;
			EditButtonVisibility = Visibility.Visible;
		}
		else
		{
			EditButtonVisibility = Visibility.Hidden;
		}
		if (activeViewModel is ICopyTab copyTab)
		{
			_isCopyButtonEnabled = copyTab.IsCopyButtonEnabled;
			CopyButtonVisibility = Visibility.Visible;
		}
		else
		{
			CopyButtonVisibility = Visibility.Hidden;
		}
		if (activeViewModel is IDeleteTab deleteTab)
		{
			_isDeleteButtonEnabled = deleteTab.IsDeleteButtonEnabled;
			DeleteButtonVisibility = Visibility.Visible;
		}
		else
		{
			DeleteButtonVisibility = Visibility.Hidden;
		}
		if (activeViewModel is IAddTab addTab)
		{
			_isAddNewButtonEnabled = addTab.IsAddButtonEnabled;
			AddButtonVisibility = Visibility.Visible;
		}
		else
		{
			AddButtonVisibility = Visibility.Hidden;
		}
		NotifyPropertyChanged("DeleteButtonVisibility");
		NotifyPropertyChanged("AddButtonVisibility");
		NotifyPropertyChanged("CopyButtonVisibility");
		NotifyPropertyChanged("EditButtonVisibility");
		NotifyPropertyChanged("IsEditButtonEnabled");
		NotifyPropertyChanged("IsCopyButtonEnabled");
		NotifyPropertyChanged("IsDeleteButtonEnabled");
		NotifyPropertyChanged("IsAddNewButtonEnabled");
		NotifyPropertyChanged("IsOkButtonEnabled");
		NotifyPropertyChanged("IsCancelButtonEnabled");
		NotifyPropertyChanged("IsSaveButtonEnabled");
	}

	private object? GetActiveViewModel(TabItem tabItem = null)
	{
		if (tabItem == null)
		{
			tabItem = SelectedTab;
		}
		object result = null;
		if (tabItem.Content is TabControl tabControl)
		{
			if (tabControl.SelectedContent is FrameworkElement frameworkElement)
			{
				result = frameworkElement.DataContext;
			}
		}
		else if (tabItem.Content is FrameworkElement frameworkElement2)
		{
			result = frameworkElement2.DataContext;
		}
		return result;
	}

	private void Copy()
	{
		if (GetActiveViewModel() is ICopyTab copyTab)
		{
			copyTab.CopyButtonClick();
		}
		SetEditorEnableRules();
	}

	private void Delete()
	{
		if (GetActiveViewModel() is IDeleteTab deleteTab)
		{
			deleteTab.DeleteButtonClick();
		}
		SetEditorEnableRules();
	}

	private void Add()
	{
		if (GetActiveViewModel() is IAddTab addTab)
		{
			addTab.AddButtonClick();
		}
		SetEditorEnableRules();
	}

	private void Edit()
	{
		if (GetActiveViewModel() is IEditTab editTab)
		{
			editTab.EditButtonClick();
		}
		SetEditorEnableRules();
	}

	public void Dispose()
	{
		BendDataBaseViewModel?.Dispose();
		MachineToolsGroupsViewModel?.Dispose();
		UpperToolsViewModel?.Dispose();
		LowerToolsViewModel?.Dispose();
		LowerToolExtensionsViewModel?.Dispose();
		DieHemViewModel?.Dispose();
		LeftFingerViewModel?.Dispose();
		RightFingerViewModel?.Dispose();
		UpperAdapterViewModel?.Dispose();
		LowerAdapterViewModel?.Dispose();
		UpperHolderViewModel?.Dispose();
		LowerHolderViewModel?.Dispose();
		ClampingSystemViewModel?.Dispose();
		PreferredProfilesViewModel?.Dispose();
		MappingViewModel?.Dispose();
		MaterialsViewModel?.Dispose();
		MachineConfigViewModel?.Dispose();
		BendSequenceEditorViewModel?.Dispose();
		PPSettingsViewModel?.Dispose();
		MachineUnfoldSettingsViewModel?.Dispose();
	}
}

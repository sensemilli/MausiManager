using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class PreferredProfilesViewModel : ToolViewModelBase, ICopyTab, IDeleteTab, IAddTab, IEditTab
{
	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private readonly IMaterialManager _materialManager;

	private readonly IConfigProvider _configProvider;

	private readonly ITranslator _translator;

	private readonly IGlobalToolStorage _globalToolStorage;

	private IDoc3d _doc;

	private int _material3DGroupIndex;

	private PreferredProfileViewModel _selectedProfile;

	private ICommand _keyDownDelete;

	private bool _singleSelected = true;

	private bool _isCopyButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private ChangedConfigType _changed;

	public Action<ChangedConfigType> DataChanged;

	private RadObservableCollection<AlternativeToolsetViewModel> _alternativeTools;

	private PreferredProfileViewModel _alternativeProfile;

	public IUnitConverter UnitConverter { get; }

	public int? Material3DGroupID { get; set; }

	public double Thickness { get; set; }

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

	public int Material3DGroupIndex
	{
		get
		{
			return _material3DGroupIndex;
		}
		set
		{
			_material3DGroupIndex = value;
			NotifyPropertyChanged("Material3DGroupIndex");
		}
	}

	public ObservableCollection<Material3DGroupViewModel> Material3DGroups { get; set; }

	public PreferredProfileViewModel SelectedProfile
	{
		get
		{
			return _selectedProfile;
		}
		set
		{
			_selectedProfile = value;
			Parallel.ForEach(PreferredProfiles, delegate(PreferredProfileViewModel item)
			{
				if (item != _selectedProfile)
				{
					item.IsSelected = false;
				}
			});
			NotifyPropertyChanged("SelectedProfile");
			ItemSelectionChangeNotify();
			RefreshAlternativeTools(_selectedProfile);
		}
	}

	public new IBendMachine BendMachine { get; set; }

	public MachineToolsViewModel MachineVm { get; set; }

	public ICommand KeyDownDelete => _keyDownDelete ?? (_keyDownDelete = new RelayCommand(DeleteButtonClick));

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

	public ObservableCollection<object> SelectedItems { get; internal set; }

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

	public ObservableCollection<RadMenuItem> PreferredProfileContextMenuItems { get; set; }

	public RadObservableCollection<PreferredProfileViewModel> Profiles { get; } = new RadObservableCollection<PreferredProfileViewModel>();

	public RadObservableCollection<AlternativeToolsetViewModel> AlternativeTools
	{
		get
		{
			return _alternativeTools;
		}
		set
		{
			if (_alternativeTools != value)
			{
				_alternativeTools = value;
				NotifyPropertyChanged("AlternativeTools");
			}
		}
	}

	public RadObservableCollection<PreferredProfileViewModel> PreferredProfiles { get; } = new RadObservableCollection<PreferredProfileViewModel>();

	public List<WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<BendMethod>> BendMethods { get; } = new List<WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<BendMethod>>();

	public RadObservableCollection<PreferredProfileListViewModel> PreferredProfileLists { get; } = new RadObservableCollection<PreferredProfileListViewModel>();

	private PreferredProfileListViewModel _selectedPreferredProfileList { get; set; }

	public PreferredProfileListViewModel SelectedPreferredProfileList
	{
		get
		{
			return _selectedPreferredProfileList;
		}
		set
		{
			if (_selectedPreferredProfileList != value)
			{
				_selectedPreferredProfileList = value;
				FillPreferredProfiles();
				SelectedProfile = PreferredProfiles.FirstOrDefault();
				NotifyPropertyChanged("SelectedPreferredProfileList");
			}
		}
	}

	private Dictionary<PreferredProfileViewModel, RadObservableCollection<AlternativeToolsetViewModel>> AlternativeChangedTools { get; } = new Dictionary<PreferredProfileViewModel, RadObservableCollection<AlternativeToolsetViewModel>>();

	public ObservableCollection<UpperToolGroupViewModel> PunchGroups => MachineVm.UpperGroups;

	public ObservableCollection<LowerToolGroupViewModel> DieGroups => MachineVm.LowerGroups;

	public ObservableCollection<UpperToolViewModel> Punches => MachineVm.UpperTools;

	public ObservableCollection<LowerToolViewModel> Dies => MachineVm.LowerTools;

	public ObservableCollection<LowerToolExtensionViewModel> DieExtensions => MachineVm.LowerToolsExtensions;

	public PreferredProfilesViewModel(IGlobals globals, IMaterialManager materialManager, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory, ITranslator translator, IUnitConverter unitConverter, IGlobalToolStorage globalToolStorage)
		: base(globals, mainWindowDataProvider, pnPathService, modelFactory)
	{
		_materialManager = materialManager;
		_configProvider = configProvider;
		_translator = translator;
		_globalToolStorage = globalToolStorage;
		UnitConverter = unitConverter;
	}

	public void Init(IBendMachine bendMachine, MachineToolsViewModel machineVm, ToolConfigModel toolConfigModel, IDoc3d doc)
	{
		_doc = doc;
		BendMachine = bendMachine;
		MachineVm = machineVm;
		base.ToolConfigModel = toolConfigModel;
		Thickness = _doc.Thickness;
		Material3DGroupID = _doc.Material?.MaterialGroupForBendDeduction;
		InitializePreferredProfileContextMenuItems();
		FillCombos();
		SelectedPreferredProfileList = PreferredProfileLists.FirstOrDefault((PreferredProfileListViewModel x) => x.List == bendMachine.PreferredProfileList) ?? PreferredProfileLists.Last();
	}

	private void InitializePreferredProfileContextMenuItems()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		PreferredProfileContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyButtonClick)
		};
		PreferredProfileContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditButtonClick)
		};
		PreferredProfileContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = (object)new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteButtonClick)
		};
		PreferredProfileContextMenuItems.Add(item3);
	}

	private void FillCombos()
	{
		Material3DGroups = new ObservableCollection<Material3DGroupViewModel>
		{
			new Material3DGroupViewModel(null)
			{
				Name = "*",
				Number = -1
			}
		};
		foreach (IMaterialUnf material3DGroup in _materialManager.Material3DGroups)
		{
			Material3DGroups.Add(new Material3DGroupViewModel(material3DGroup));
		}
		BendMethods.Clear();
		foreach (BendMethod value in System.Enum.GetValues(typeof(BendMethod)))
		{
			BendMethods.Add(new WiCAM.Pn4000.GuiWpf.UiBasic.ComboboxEntry<BendMethod>(_translator.Translate("l_enum.BendMethod." + value), value));
		}
		PreferredProfileLists.Clear();
		foreach (IPreferredProfileList allPreferredProfileList in _globalToolStorage.GetAllPreferredProfileLists())
		{
			PreferredProfileLists.Add(new PreferredProfileListViewModel
			{
				Description = allPreferredProfileList.Description,
				List = allPreferredProfileList
			});
		}
		PreferredProfileLists.Add(new PreferredProfileListViewModel
		{
			Description = _translator.Translate("l_popup.PreferredToolsView.NewList")
		});
	}

	private void FillPreferredProfiles()
	{
		PreferredProfiles.SuspendNotifications();
		PreferredProfiles.Clear();
		if (SelectedPreferredProfileList.List != null)
		{
			foreach (IPreferredProfile profile in from i in SelectedPreferredProfileList.List.PreferredProfiles
				orderby i.MaterialGroupID, i.Thickness, i.MinRadius, i.MinAngle
				select i)
			{
				PreferredProfiles.Add(new PreferredProfileViewModel(this, profile, Material3DGroups.FirstOrDefault((Material3DGroupViewModel m) => m.Number == profile.MaterialGroupID)));
			}
		}
		PreferredProfiles.ResumeNotifications();
	}

	public void UpdateFieldsAndSave()
	{
		foreach (BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles.PreferredProfileViewModel profile in base.ToolConfigModel.PreferredProfiles)
		{
			PunchGroupViewModel punchGroup = base.ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == profile.PunchGroupId);
			DieGroupViewModel dieGroup = base.ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == profile.DieGroupId);
			profile.PunchGroup = punchGroup;
			profile.DieGroup = dieGroup;
		}
		Save();
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
		ObservableCollection<object> selectedItems = SelectedItems;
		SingleSelected = selectedItems == null || selectedItems.Count() <= 1;
		if (SelectedProfile == null)
		{
			IsCopyButtonEnabled = false;
			IsDeleteButtonEnabled = false;
			IsEditButtonEnabled = false;
			IsAddButtonEnabled = true;
			IsOkButtonEnabled = true;
			IsCancelButtonEnabled = true;
			IsSaveButtonEnabled = true;
		}
		else
		{
			IsCopyButtonEnabled = SelectedProfile != null && SingleSelected;
			IsEditButtonEnabled = SelectedProfile != null && SingleSelected;
			IsDeleteButtonEnabled = SelectedProfile != null;
			IsAddButtonEnabled = true;
			IsOkButtonEnabled = true;
			IsCancelButtonEnabled = true;
			IsSaveButtonEnabled = true;
		}
	}

	private void ItemSelectionChangeNotify()
	{
		SetEditorEnableRules();
	}

	public void AddButtonClick()
	{
		PreferredProfileViewModel preferredProfileViewModel = new PreferredProfileViewModel(this);
		PreferredProfiles.Add(preferredProfileViewModel);
		SelectedProfile = preferredProfileViewModel;
	}

	public void CopyButtonClick()
	{
		ImmutableArray<PreferredProfileViewModel>.Enumerator enumerator = SelectedItems.OfType<PreferredProfileViewModel>().ToImmutableArray().GetEnumerator();
		while (enumerator.MoveNext())
		{
			PreferredProfileViewModel item = enumerator.Current.Dupplicate();
			PreferredProfiles.Add(item);
		}
	}

	public void DeleteButtonClick()
	{
		PreferredProfiles.RemoveRange(SelectedItems.OfType<PreferredProfileViewModel>().ToImmutableArray());
		_changed |= ChangedConfigType.PreferredProfiles;
	}

	private void RefreshAlternativeTools(PreferredProfileViewModel profile)
	{
		if (_alternativeProfile != profile)
		{
			_alternativeProfile = profile;
			if (_alternativeProfile != null)
			{
				AlternativeTools = _alternativeProfile.AlternativeToolsVm;
			}
			else
			{
				AlternativeTools = null;
			}
		}
	}

	public void EditButtonClick()
	{
		throw new NotImplementedException();
	}

	public bool CanSave()
	{
		if (!PreferredProfiles.All((PreferredProfileViewModel x) => x.CanSave()))
		{
			return false;
		}
		return true;
	}

	public void Save()
	{
		IPreferredProfileList preferredProfileList = SelectedPreferredProfileList.List ?? _globalToolStorage.CreatePreferredProfileList();
		preferredProfileList.Description = SelectedPreferredProfileList.Description;
		preferredProfileList.PreferredProfiles = PreferredProfiles.Select((PreferredProfileViewModel x) => x.SaveToPreferredProfile()).ToList();
		BendMachine.PreferredProfileList = preferredProfileList;
		_changed = ChangedConfigType.PreferredProfiles;
		DataChanged?.Invoke(_changed);
	}

	public void Dispose()
	{
	}
}

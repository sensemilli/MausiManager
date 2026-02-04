using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts;
using Label = System.Windows.Controls.Label;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine.SelectMachine;

public class SelectMachineViewModel : PopupViewModelBase, ISelectMachineViewModel
{
	private IDoc3d _doc;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly IFactorio _factorio;

	private readonly ITranslator _translator;

	private readonly IProfilesHelper _profilesHelper;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly IMaterialManager _materials;

	private Canvas _imageUnfoldModel;

	private Dictionary<int, HashSet<Polygon>> _faceDict;

	private Dictionary<int, HashSet<Polygon>> _holeDict;

	private ICombinedBendDescriptorInternal _mouseActiveBend;

	private double _scale;

	private double _dx;

	private double _dy;

	private double _dWidth;

	private double _dHeight;

	private readonly Brush _b1 = Brushes.LightSteelBlue;

	private readonly Brush _b2 = Brushes.SteelBlue;

	private readonly Brush _b3 = Brushes.Blue;

	private readonly Brush _b4 = Brushes.Green;

	private readonly Brush _l1 = Brushes.Black;

	private readonly Brush _l2 = Brushes.DarkBlue;

	private readonly Brush _l3 = Brushes.DarkGreen;

	private MachineSummaryViewModel _selectedMachine;

	private PreferredProfileViewModel _preferredProfile;

	private FrameworkElement _imagePunchProfile;

	private FrameworkElement _imageDieProfile;

	private FrameworkElement _imageDefaultPunchProfile;

	private FrameworkElement _imageDefaultDieProfile;

	private ICommand _selectDoubleClick;

	private ICommand _modification;

	private ICommand _rightClickCommand;

	private ICommand _enterKeyDown;

	private Visibility _editorVisible = Visibility.Collapsed;

	private Visibility _editorNotVisible;

	private Visibility _preferredProfilePreviewVisible = Visibility.Collapsed;

	private int _material3DGroupIndex;

	private PunchGroupViewModel _punchGroup;

	private DieGroupViewModel _dieGroup;

	private Dictionary<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> _fittingProfilesDict;

	private KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> _profileToUse;

	private ObservableCollection<ICombinedBendDescriptorInternal> _selectedBends;

	private bool _changeMat = true;

	private Action<ISelectMachineViewModel> _closeAction;

	private bool _isAddNewButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isOkButtonEnabled;

	private Dictionary<IBendDescriptor, Label> _bendZoneLabelDict;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IUndo3dService _undo3dService;

	private ToolConfigModel _toolConfigModel;

	private DieProfileViewModel _primaryDie;

	private PunchProfileViewModel _primaryPunch;

	private Visibility _loadingGeometryVisibility;

	private bool _isBackgroundLoadingCompleted;

	private Thread _loadThread;

	private bool _useDefaultTools = true;

	private bool _useSuggestedTools;

	private string _thickness;

	private string _minAngle;

	private string _maxAngle;

	private string _minRadius;

	private string _maxRadius;

	private RecentlyUsedRecord _recentlyUsedMachine;

	private double _dialogOpacity;

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

	public Canvas ImageUnfoldModel
	{
		get
		{
			return _imageUnfoldModel;
		}
		set
		{
			_imageUnfoldModel = value;
			OnPropertyChanged("ImageUnfoldModel");
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
			OnPropertyChanged("Material3DGroupIndex");
			Material3DGroupIndexChanged(_material3DGroupIndex);
		}
	}

	public ObservableCollection<string> Material3DGroupNames { get; set; }

	public PunchGroupViewModel PunchGroup
	{
		get
		{
			return _punchGroup;
		}
		set
		{
			_punchGroup = value;
			OnPropertyChanged("PunchGroup");
			if (PreferredProfile != null)
			{
				PreferredProfile.PunchGroup = _punchGroup;
			}
			PreferredProfileChanged();
		}
	}

	public DieGroupViewModel DieGroup
	{
		get
		{
			return _dieGroup;
		}
		set
		{
			_dieGroup = value;
			OnPropertyChanged("DieGroup");
			if (PreferredProfile != null)
			{
				PreferredProfile.DieGroup = _dieGroup;
			}
			PreferredProfileChanged();
		}
	}

	public Dictionary<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> FittingProfilesDict
	{
		get
		{
			return _fittingProfilesDict;
		}
		set
		{
			_fittingProfilesDict = value;
			OnPropertyChanged("FittingProfilesDict");
		}
	}

	public ObservableCollection<ICombinedBendDescriptorInternal> SelectedBends
	{
		get
		{
			return _selectedBends;
		}
		set
		{
			_selectedBends = value;
			HighLightLabels();
			OnPropertyChanged("SelectedBends");
		}
	}

	public KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> ProfileToUse
	{
		get
		{
			return _profileToUse;
		}
		set
		{
			if (_profileToUse.Value != null)
			{
				_profileToUse.Value.PropertyChanged -= Value_PropertyChanged;
			}
			_profileToUse = value;
			SelectedBends?.Clear();
			if (_profileToUse.Value != null)
			{
				_profileToUse.Value.PropertyChanged += Value_PropertyChanged;
				IEnumerable<KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>> source = FittingProfilesDict.Where<KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>>((KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> x) => x.Value.Item2 == _profileToUse.Value.Item2);
				SelectedBends = (from bend in source.Where(delegate(KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> bend)
					{
						KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> keyValuePair = bend;
						return keyValuePair.Key != null;
					})
					select _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _profileToUse.Key.Order)).ToObservableCollection();
				if (_profileToUse.Value != null)
				{
					PreferredProfileViewModel item = _profileToUse.Value.Item2;
					if (item != null && item.Id >= 0)
					{
						PunchGroupViewModel punchGroup = ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == _profileToUse.Value.Item2.PunchGroupId);
						SelectedPunchGroupChanged(punchGroup);
						DieGroupViewModel dieGroup = ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == _profileToUse.Value.Item2.DieGroupId);
						SelectedDieGroupChanged(dieGroup);
						goto IL_014f;
					}
				}
				SelectedPunchGroupChanged(null);
				SelectedDieGroupChanged(null);
			}
			goto IL_014f;
			IL_014f:
			OnPropertyChanged("ProfileToUse");
		}
	}

	public ObservableCollection<MachineSummaryViewModel> Machines { get; set; }

	public MachineSummaryViewModel SelectedMachine
	{
		get
		{
			return _selectedMachine;
		}
		set
		{
			if (value != null)
			{
				_selectedMachine = value;
				OnPropertyChanged("SelectedMachine");
				CheckForPreferredTools();
				UpDateButtonEnable();
			}
		}
	}

	public BendMachine? SelectedMachineExtended => null;

	public PreferredProfileViewModel PreferredProfile
	{
		get
		{
			return _preferredProfile;
		}
		set
		{
			_preferredProfile = value;
			OnPropertyChanged("PreferredProfile");
			if (_preferredProfile != null)
			{
				Thickness = _preferredProfile.Thickness.ToString(CultureInfo.InvariantCulture);
				MinAngle = _preferredProfile.MinAngle.ToString(CultureInfo.InvariantCulture);
				MaxAngle = _preferredProfile.MaxAngle.ToString(CultureInfo.InvariantCulture);
				MinRadius = _preferredProfile.MinRadius.ToString(CultureInfo.InvariantCulture);
				MaxRadius = _preferredProfile.MaxRadius.ToString(CultureInfo.InvariantCulture);
				DieGroup = ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == _preferredProfile.DieGroupId);
				PunchGroup = ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == _preferredProfile.PunchGroupId);
			}
			else
			{
				Thickness = "";
				MinAngle = "";
				MaxAngle = "";
				MinRadius = "";
				MaxRadius = "";
				DieGroup = null;
				PunchGroup = null;
			}
			PreferredProfileChanged();
			UpDateButtonEnable();
		}
	}

	public FrameworkElement ImagePunchProfile
	{
		get
		{
			return _imagePunchProfile;
		}
		set
		{
			_imagePunchProfile = value;
			OnPropertyChanged("ImagePunchProfile");
		}
	}

	public FrameworkElement ImageDieProfile
	{
		get
		{
			return _imageDieProfile;
		}
		set
		{
			_imageDieProfile = value;
			OnPropertyChanged("ImageDieProfile");
		}
	}

	public FrameworkElement ImageDefaultPunchProfile
	{
		get
		{
			return _imageDefaultPunchProfile;
		}
		set
		{
			_imageDefaultPunchProfile = value;
			OnPropertyChanged("ImageDefaultPunchProfile");
		}
	}

	public FrameworkElement ImageDefaultDieProfile
	{
		get
		{
			return _imageDefaultDieProfile;
		}
		set
		{
			_imageDefaultDieProfile = value;
			OnPropertyChanged("ImageDefaultDieProfile");
		}
	}

	public ICommand SelectDoubleClick => _selectDoubleClick ?? (_selectDoubleClick = new RelayCommand((Action<object>)delegate
	{
		_doc.MachinePath = SelectedMachine?.MachinePath;
		CloseLikeOk();
	}));

	public ICommand ModificationCommand => _modification ?? (_modification = new RelayCommand((Action<object>)delegate
	{
		Modification();
	}));

	public ICommand RightClickCommand => _rightClickCommand ?? (_rightClickCommand = new RelayCommand((Action<object>)delegate
	{
		_doc.MachinePath = SelectedMachine?.MachinePath;
		CloseLikeOk();
	}));

	public ICommand EnterKeyDown => _enterKeyDown ?? (_enterKeyDown = new RelayCommand(EnterKey));

	public Visibility EditorVisible
	{
		get
		{
			return _editorVisible;
		}
		set
		{
			_editorVisible = value;
			if (_editorVisible == Visibility.Visible)
			{
				EditorNotVisible = Visibility.Collapsed;
			}
			else
			{
				EditorNotVisible = Visibility.Visible;
			}
			OnPropertyChanged("EditorVisible");
			UpDateButtonEnable();
		}
	}

	public Visibility EditorNotVisible
	{
		get
		{
			return _editorNotVisible;
		}
		set
		{
			_editorNotVisible = value;
			OnPropertyChanged("EditorNotVisible");
			UpDateButtonEnable();
		}
	}

	public Visibility PreferredProfilePreviewVisible
	{
		get
		{
			return _preferredProfilePreviewVisible;
		}
		set
		{
			_preferredProfilePreviewVisible = value;
			OnPropertyChanged("PreferredProfilePreviewVisible");
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
			PreferredProfile = null;
		}
	}

	public DieProfileViewModel PrimaryDie
	{
		get
		{
			return _primaryDie;
		}
		set
		{
			_primaryDie = value;
			OnPropertyChanged("PrimaryDie");
		}
	}

	public PunchProfileViewModel PrimaryPunch
	{
		get
		{
			return _primaryPunch;
		}
		set
		{
			_primaryPunch = value;
			OnPropertyChanged("PrimaryPunch");
		}
	}

	public Visibility LoadingGeometryVisibility
	{
		get
		{
			return _loadingGeometryVisibility;
		}
		set
		{
			if (_loadingGeometryVisibility != value)
			{
				_loadingGeometryVisibility = value;
				OnPropertyChanged("LoadingGeometryVisibility");
			}
		}
	}

	public bool IsBackgroundLoadingCompleted
	{
		get
		{
			return _isBackgroundLoadingCompleted;
		}
		set
		{
			_isBackgroundLoadingCompleted = value;
			OnPropertyChanged("IsBackgroundLoadingCompleted");
			if (_isBackgroundLoadingCompleted)
			{
				LoadingGeometryVisibility = Visibility.Collapsed;
			}
			else
			{
				LoadingGeometryVisibility = Visibility.Visible;
			}
		}
	}

	public bool UseDefaultTools
	{
		get
		{
			return _useDefaultTools;
		}
		set
		{
			_useDefaultTools = value;
			OnPropertyChanged("UseDefaultTools");
			CheckForPreferredTools();
		}
	}

	public bool UseSuggestedTools
	{
		get
		{
			return _useSuggestedTools;
		}
		set
		{
			_useSuggestedTools = value;
			OnPropertyChanged("UseSuggestedTools");
			CheckForPreferredTools();
		}
	}

	public string Thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			if (PreferredProfile != null && !string.IsNullOrEmpty(value))
			{
				PreferredProfile.Thickness = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			_thickness = value;
			OnPropertyChanged("Thickness");
		}
	}

	public string MinAngle
	{
		get
		{
			return _minAngle;
		}
		set
		{
			if (PreferredProfile != null && !string.IsNullOrEmpty(value))
			{
				PreferredProfile.MinAngle = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			_minAngle = value;
			OnPropertyChanged("MinAngle");
		}
	}

	public string MaxAngle
	{
		get
		{
			return _maxAngle;
		}
		set
		{
			if (PreferredProfile != null && !string.IsNullOrEmpty(value))
			{
				PreferredProfile.MaxAngle = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			_maxAngle = value;
			OnPropertyChanged("MaxAngle");
		}
	}

	public string MinRadius
	{
		get
		{
			return _minRadius;
		}
		set
		{
			if (PreferredProfile != null && !string.IsNullOrEmpty(value))
			{
				PreferredProfile.MinRadius = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			_minRadius = value;
			OnPropertyChanged("MinRadius");
		}
	}

	public string MaxRadius
	{
		get
		{
			return _maxRadius;
		}
		set
		{
			if (PreferredProfile != null && !string.IsNullOrEmpty(value))
			{
				PreferredProfile.MaxRadius = Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			_maxRadius = value;
			OnPropertyChanged("MaxRadius");
		}
	}

	public SelectMachineViewModel(IFactorio factorio, IMainWindowDataProvider mainWindowDataProvider, ITranslator translator, IProfilesHelper profilesHelper, IConfigProvider configProvider, IPnPathService pathService, IMaterialManager materials, IDrawToolProfiles drawToolProfiles, IMainWindowBlock mainWindowBlock, IUndo3dService undo3dService)
	{
		_factorio = factorio;
		_mainWindowDataProvider = mainWindowDataProvider;
		_translator = translator;
		_profilesHelper = profilesHelper;
		_configProvider = configProvider;
		_pathService = pathService;
		_materials = materials;
		_drawToolProfiles = drawToolProfiles;
		_mainWindowBlock = mainWindowBlock;
		_undo3dService = undo3dService;
	}

	public ISelectMachineViewModel Init(IDoc3d doc, IEnumerable<BendMachine> machines, IEnumerable<IBendMachineSummary> machineSummaries, Action<ISelectMachineViewModel> closeAction = null)
	{
		_doc = doc;
		_recentlyUsedMachine = _mainWindowDataProvider.GetRecentlyUsedMachineRecords().FirstOrDefault();
		_closeAction = closeAction;
		Machines = new ObservableCollection<MachineSummaryViewModel>(from x in machineSummaries
			where x != null
			select new MachineSummaryViewModel(x));
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		DialogOpacity = generalUserSettingsConfig.DialogOpacity;
		Initialize();
		LoadMaterials();
		if (Machines.Count > 0)
		{
			if (_recentlyUsedMachine != null)
			{
				SelectedMachine = Machines.FirstOrDefault((MachineSummaryViewModel m) => m.Number == _recentlyUsedMachine.ArchiveID);
			}
			if (SelectedMachine == null)
			{
				MachineSummaryViewModel machineSummaryViewModel2 = (SelectedMachine = Machines.First());
			}
		}
		if (PreferredProfilePreviewVisible == Visibility.Visible)
		{
			LoadGeometryInBackground();
		}
		UpDateButtonEnable();
		return this;
	}

	private void Initialize()
	{
		FittingProfilesDict = new Dictionary<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>();
		SelectedBends = new ObservableCollection<ICombinedBendDescriptorInternal>();
		EditorVisible = Visibility.Collapsed;
		base.Button0_DeleteVisibility = Visibility.Visible;
		base.Button1_EditVisibility = Visibility.Visible;
		base.Button2_AddVisibility = Visibility.Visible;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button0_DeleteClick = new RelayCommand<object>(DeleteClick, CanDeleteClick);
		base.Button1_EditClick = new RelayCommand<object>(EditClick, CanEditClick);
		base.Button2_AddClick = new RelayCommand<object>(AddClick, CanAddClick);
		base.Button5_CancelClick = new RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button16_OkClick = new RelayCommand<object>(OkClick, CanOkClick);
		ImageUnfoldModel = new Canvas
		{
			Width = 728.0,
			Height = 432.0,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		ImagePunchProfile = new Canvas
		{
			Height = 364.0,
			Width = 477.0,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		ImageDieProfile = new Canvas
		{
			Height = 364.0,
			Width = 477.0,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		ImageDefaultPunchProfile = new Canvas
		{
			Height = 192.0,
			Width = 173.0,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		ImageDefaultDieProfile = new Canvas
		{
			Height = 192.0,
			Width = 173.0,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		MeasureAndArrange(ImageUnfoldModel);
		MeasureAndArrange(ImagePunchProfile);
		MeasureAndArrange(ImageDieProfile);
		MeasureAndArrange(ImageDefaultPunchProfile);
		MeasureAndArrange(ImageDefaultDieProfile);
		IDoc3d doc = _doc;
		PreferredProfilePreviewVisible = ((doc == null || !(doc.EntryModel3D.Shells?.Count > 0)) ? Visibility.Collapsed : Visibility.Visible);
	}

	public IBendMachineSummary GetSelectedMachine()
	{
		return SelectedMachine.Summary;
	}

	private static void MeasureAndArrange(UIElement element)
	{
		element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		element.Arrange(new Rect(0.0, 0.0, element.DesiredSize.Width, element.DesiredSize.Height));
	}

	private void LoadMaterials()
	{
		Material3DGroupNames = new ObservableCollection<string>();
		foreach (KeyValuePair<int, string> item in _materials.Material3DGroup)
		{
			Material3DGroupNames.Add(item.Value);
		}
	}

	private void CheckForPreferredTools()
	{
		if (ProfileToUse.Value != null)
		{
			ProfileToUse.Value.Item1 = null;
			ProfileToUse.Value.Item2 = null;
		}
		PreferredProfile = null;
		((Canvas)ImageDefaultPunchProfile).Children.Clear();
		((Canvas)ImageDefaultDieProfile).Children.Clear();
		((Canvas)ImagePunchProfile).Children.Clear();
		((Canvas)ImageDieProfile).Children.Clear();
		ToolConfigModel = ((SelectedMachineExtended == null) ? null : new ToolConfigModel(SelectedMachineExtended, _materials));
		if (_doc != null && ToolConfigModel != null)
		{
			LoadFittingProfiles();
		}
	}

	private void LoadFittingProfiles()
	{
		if (FittingProfilesDict == null)
		{
			return;
		}
		FittingProfilesDict = new Dictionary<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>();
		if (ProfileToUse.Value != null)
		{
			ProfileToUse.Value.Item1 = null;
			ProfileToUse.Value.Item2 = null;
		}
		if (_doc == null || UseSuggestedTools)
		{
			return;
		}
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _doc.CombinedBendDescriptors)
		{
			ObservableCollection<PreferredProfileViewModel> observableCollection = new ObservableCollection<PreferredProfileViewModel>();
			foreach (PreferredProfileViewModel preferredProfile in ToolConfigModel.PreferredProfiles)
			{
				if (Math.Abs(preferredProfile.Thickness - _doc.Thickness) < 0.0001 && preferredProfile.Material3DGroupID == _doc.Material?.MaterialGroupForBendDeduction && CheckMinMaxRadius(combinedBendDescriptor[0].BendParams.OriginalRadius, preferredProfile))
				{
					observableCollection.Add(preferredProfile);
				}
			}
			observableCollection = (from x in observableCollection
				group x by x.Id into x
				select x.First()).ToObservableCollection();
			FittingProfilesDict.Add(combinedBendDescriptor, new VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>(observableCollection, observableCollection.FirstOrDefault()));
		}
		FittingProfilesDict = new Dictionary<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>(FittingProfilesDict);
		ProfileToUse = FittingProfilesDict.FirstOrDefault();
	}

	private bool CheckMinMaxRadius(double radius, PreferredProfileViewModel profile)
	{
		double num = Math.Abs(radius);
		if (!(Math.Abs(profile.MaxRadius - profile.MinRadius) > 0.0) || !(profile.MinRadius >= 0.0) || !(profile.MaxRadius > 0.0))
		{
			return true;
		}
		if (num >= profile.MinRadius)
		{
			return num <= profile.MaxRadius;
		}
		return false;
	}

	private void LoadGeometryInBackground()
	{
		_loadThread = new Thread((ThreadStart)delegate
		{
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				Draw();
				IsBackgroundLoadingCompleted = true;
			});
		})
		{
			Priority = ThreadPriority.Normal,
			IsBackground = true,
			Name = "GeometryLoaderThread"
		};
		_loadThread.Start();
	}

	private static bool CanCancelClick(object obj)
	{
		return true;
	}

	private void CancelClick(object obj)
	{
		if (EditorVisible != 0)
		{
			if (_closeAction != null)
			{
				_closeAction(this);
			}
			else
			{
				CloseView();
			}
			_mainWindowBlock.BlockUI_Unblock(_doc);
		}
		else if (EditorVisible == Visibility.Visible)
		{
			EditorVisible = Visibility.Collapsed;
		}
	}

	private bool CanOkClick(object obj)
	{
		return IsOkButtonEnabled;
	}

	private bool CanEditClick(object obj)
	{
		return IsEditButtonEnabled;
	}

	private void OkClick(object obj)
	{
		CloseLikeOk();
	}

	private void EnterKey(object obj)
	{
		if ((obj as RadGridView).RowInEditMode == null)
		{
			CloseLikeOk();
		}
	}

	private void EditClick(object obj)
	{
		PreferredProfile = ToolConfigModel.PreferredProfiles.FirstOrDefault();
		if (PreferredProfile == null)
		{
			Material3DGroupIndex = -1;
			PunchGroup = null;
			DieGroup = null;
		}
		EditorVisible = Visibility.Visible;
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			CloseLikeOk();
		}
	}

	public Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> GetPreferedToolsForCombinedBends()
	{
		Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> result = new Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile>();
		if (UseDefaultTools && ProfileToUse.Value != null)
		{
			VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel> value = ProfileToUse.Value;
			if (value != null && value.Item2?.Id >= 0)
			{
				foreach (KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> item2 in FittingProfilesDict)
				{
					ObservableCollection<PreferredProfileViewModel> item = item2.Value.Item1;
					if (item != null && item.Count > 0)
					{
						throw new NotImplementedException();
					}
				}
			}
		}
		return result;
	}

	private void CloseLikeOk()
	{
		if (EditorVisible != 0)
		{
			IBendMachine? bendMachine = _doc.BendMachine;
			_doc.MachinePath = SelectedMachine?.MachinePath;
			IBendMachine bendMachine2 = _doc.BendMachine;
			if (bendMachine != bendMachine2)
			{
				_undo3dService.Save(_doc, _translator.Translate("Undo3d.SetBendMachine", bendMachine2?.MachineNo, bendMachine2?.Name));
			}
			if (_closeAction != null)
			{
				_closeAction(this);
			}
			else
			{
				CloseView();
			}
			_mainWindowBlock.BlockUI_Unblock(_doc);
		}
		else if (EditorVisible == Visibility.Visible)
		{
			throw new NotImplementedException();
		}
	}

	private bool CanAddClick(object obj)
	{
		return IsAddNewButtonEnabled;
	}

	private void AddClick(object obj)
	{
		throw new NotImplementedException();
	}

	private bool CanDeleteClick(object obj)
	{
		return IsDeleteButtonEnabled;
	}

	private void DeleteClick(object obj)
	{
		throw new NotImplementedException();
	}

	private void PreferredProfileChanged()
	{
		((Canvas)ImageDefaultPunchProfile).Children.Clear();
		((Canvas)ImageDefaultDieProfile).Children.Clear();
		((Canvas)ImagePunchProfile).Children.Clear();
		((Canvas)ImageDieProfile).Children.Clear();
		if (PreferredProfile == null)
		{
			return;
		}
		DieGroupViewModel dieGroup = ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel x) => x.ID == PreferredProfile.DieGroupId);
		if (dieGroup != null)
		{
			DieGroupViewModel dieGroupViewModel = ToolConfigModel.DieGroups.First((DieGroupViewModel d) => d.ID == dieGroup.ID && d.Name == dieGroup.Name);
			if (dieGroupViewModel.ID != dieGroup.ID)
			{
				DieGroup = dieGroupViewModel;
			}
		}
		DieProfileViewModel dieProfileViewModel = ToolConfigModel.DieProfiles.FirstOrDefault((DieProfileViewModel x) => x.ID == dieGroup?.PrimaryToolId);
		if (dieProfileViewModel != null && !string.IsNullOrEmpty(dieProfileViewModel.GeometryFile.Name) && !dieProfileViewModel.GeometryFile.Name.EndsWith(".n3d"))
		{
			_drawToolProfiles.LoadDiePreview2D(dieProfileViewModel.DieProfile, (Canvas)ImageDieProfile, SelectedMachineExtended);
			OnPropertyChanged("ImageDieProfile");
		}
		PunchGroupViewModel punchGroup = ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel x) => x.ID == PreferredProfile.PunchGroupId);
		if (punchGroup != null)
		{
			PunchGroupViewModel punchGroupViewModel = ToolConfigModel.PunchGroups.First((PunchGroupViewModel p) => p.ID == punchGroup.ID && p.Name == punchGroup.Name);
			if (punchGroup.ID != punchGroupViewModel.ID)
			{
				PunchGroup = punchGroupViewModel;
			}
		}
		PunchProfileViewModel punchProfileViewModel = ToolConfigModel.PunchProfiles.FirstOrDefault((PunchProfileViewModel x) => x.ID == punchGroup?.PrimaryToolId);
		if (punchProfileViewModel != null && !string.IsNullOrEmpty(punchProfileViewModel.GeometryFile.Name) && !punchProfileViewModel.GeometryFile.Name.EndsWith(".n3d"))
		{
			_drawToolProfiles.LoadPunchPreview2D(punchProfileViewModel.PunchProfile, (Canvas)ImagePunchProfile, SelectedMachineExtended);
			OnPropertyChanged("ImagePunchProfile");
		}
		_changeMat = false;
		Material3DGroupIndex = Material3DGroupNames.IndexOf(PreferredProfile.Material3DGroupName);
		_changeMat = true;
	}

	private void Material3DGroupIndexChanged(int index)
	{
		if (!_changeMat || PreferredProfile == null || index > _materials.MaterialList.Count - 1)
		{
			return;
		}
		if (index == -1)
		{
			PreferredProfile.MaterialGroup3D = null;
			OnPropertyChanged("PreferredProfile");
			return;
		}
		KeyValuePair<int, string> group = _materials.Material3DGroup.ElementAt(index);
		if (group.Value != PreferredProfile.Material3DGroupName)
		{
			PreferredProfile.MaterialGroup3D = new Material3DGroupViewModel(_materials.Material3DGroups.FirstOrDefault((IMaterialUnf m) => m.Number == group.Key));
			Material3DGroupIndex = Material3DGroupNames.IndexOf(PreferredProfile.Material3DGroupName);
			OnPropertyChanged("PreferredProfile");
		}
	}

	private void SelectedPunchGroupChanged(PunchGroupViewModel punchGroup)
	{
		if (ImageDefaultPunchProfile == null)
		{
			return;
		}
		((Canvas)ImageDefaultPunchProfile).Children.Clear();
		OnPropertyChanged("ImageDefaultPunchProfile");
		if (punchGroup != null && ProfileToUse.Value != null)
		{
			if (punchGroup != null)
			{
				ProfileToUse.Value.Item2.PunchGroup = punchGroup;
				PrimaryPunch = ToolConfigModel.PunchProfiles.FirstOrDefault((PunchProfileViewModel p) => p.ID == punchGroup.PrimaryToolId);
				if (PrimaryPunch != null)
				{
					if (!string.IsNullOrEmpty(PrimaryPunch.GeometryFile.Name) && !PrimaryPunch.GeometryFile.Name.EndsWith(".n3d"))
					{
						_drawToolProfiles.LoadPunchPreview2D(PrimaryPunch.PunchProfile, (Canvas)ImageDefaultPunchProfile, SelectedMachineExtended);
					}
				}
				else
				{
					((Canvas)ImageDefaultPunchProfile).Children.Clear();
				}
			}
		}
		else
		{
			PrimaryPunch = null;
			((Canvas)ImageDefaultPunchProfile).Children.Clear();
		}
		OnPropertyChanged("ImageDefaultPunchProfile");
		OnPropertyChanged("ProfileToUse");
	}

	private void SelectedDieGroupChanged(DieGroupViewModel dieGroup)
	{
		if (ImageDefaultDieProfile == null)
		{
			return;
		}
		((Canvas)ImageDefaultDieProfile).Children.Clear();
		OnPropertyChanged("ImageDefaultDieProfile");
		if (dieGroup != null && ProfileToUse.Value != null)
		{
			if (dieGroup != null)
			{
				ProfileToUse.Value.Item2.DieGroup = dieGroup;
				PrimaryDie = ToolConfigModel.DieProfiles.FirstOrDefault((DieProfileViewModel p) => p.ID == dieGroup.PrimaryToolId);
				if (PrimaryDie != null)
				{
					if (!string.IsNullOrEmpty(PrimaryDie.GeometryFile.Name) && !PrimaryDie.GeometryFile.Name.EndsWith(".n3d"))
					{
						_drawToolProfiles.LoadDiePreview2D(PrimaryDie.DieProfile, (Canvas)ImageDefaultDieProfile, SelectedMachineExtended);
					}
				}
				else
				{
					((Canvas)ImageDefaultDieProfile).Children.Clear();
				}
			}
		}
		else
		{
			PrimaryDie = null;
			((Canvas)ImageDefaultDieProfile).Children.Clear();
		}
		OnPropertyChanged("ImageDefaultDieProfile");
		OnPropertyChanged("ProfileToUse");
	}

	private void UpDateButtonEnable()
	{
		IsAddNewButtonEnabled = EditorVisible == Visibility.Visible;
		IsEditButtonEnabled = EditorVisible != 0 && SelectedMachine != null;
		IsDeleteButtonEnabled = ((EditorVisible != 0) ? (SelectedMachine != null) : (ToolConfigModel.PreferredProfiles != null && EditorVisible == Visibility.Visible && ToolConfigModel.PreferredProfiles.IndexOf(PreferredProfile) > -1));
		IsOkButtonEnabled = SelectedMachine != null;
	}

	private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		IEnumerable<KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>> enumerable = FittingProfilesDict?.Where((KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> x) => x.Value.Item1 != null && x.Value.Item2 == _profileToUse.Value.Item2);
		if (enumerable != null)
		{
			SelectedBends = (from bend in enumerable.Where(delegate(KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> bend)
				{
					KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> keyValuePair = bend;
					return keyValuePair.Key != null;
				})
				select _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == ProfileToUse.Key.Order)).ToObservableCollection();
		}
		else
		{
			SelectedBends = new ObservableCollection<ICombinedBendDescriptorInternal>();
		}
		if (ProfileToUse.Value != null)
		{
			PreferredProfileViewModel item = ProfileToUse.Value.Item2;
			if (item != null && item.Id >= 0)
			{
				PunchGroupViewModel punchGroup = ToolConfigModel?.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == ProfileToUse.Value.Item2.PunchGroupId);
				SelectedPunchGroupChanged(punchGroup);
				DieGroupViewModel dieGroup = ToolConfigModel?.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == ProfileToUse.Value.Item2.DieGroupId);
				SelectedDieGroupChanged(dieGroup);
				return;
			}
		}
		SelectedPunchGroupChanged(null);
		SelectedDieGroupChanged(null);
	}

	private void Draw()
	{
		Freeze();
		GetCurrentGeometries();
	}

	private void GetCurrentGeometries()
	{
		if (ImageUnfoldModel.Children.Count > 0)
		{
			return;
		}
		ImageUnfoldModel.Children.Clear();
		_faceDict = new Dictionary<int, HashSet<Polygon>>();
		_holeDict = new Dictionary<int, HashSet<Polygon>>();
		if (_doc == null)
		{
			return;
		}
		List<(Face, Model)> list = _doc.UnfoldModel3D.GetAllFaceModelsWithFaceGroup().ToList();
		if (list.Count < 1)
		{
			return;
		}
		CalculateCurrentScreenParameters();
		foreach (var item3 in list)
		{
			Face item = item3.Item1;
			Model item2 = item3.Item2;
			Matrix4d worldMatrix = item.Shell.GetWorldMatrix(item2);
			HashSet<Polygon> hashSet = new HashSet<Polygon>();
			HashSet<Polygon> hashSet2 = new HashSet<Polygon>();
			Brush currentBrushForFace = GetCurrentBrushForFace(item, _doc);
			Polygon polygon = new Polygon
			{
				StrokeThickness = 1.0,
				Fill = currentBrushForFace,
				Stroke = currentBrushForFace,
				StrokeMiterLimit = 0.0,
				StrokeLineJoin = PenLineJoin.Miter
			};
			foreach (Vertex item4 in item.BoundaryEdgesCcw.SelectMany((FaceHalfEdge e) => e.Vertices))
			{
				Vector3d v = item4.Pos;
				worldMatrix.TransformInPlace(ref v);
				polygon.Points.Add(GetPointProjection(v.X, v.Y));
			}
			ImageUnfoldModel.Children.Add(polygon);
			hashSet.Add(polygon);
			foreach (List<FaceHalfEdge> item5 in item.HoleEdgesCw)
			{
				Polygon polygon2 = new Polygon
				{
					StrokeThickness = 1.0,
					Fill = new SolidColorBrush(Colors.White),
					Stroke = currentBrushForFace,
					StrokeMiterLimit = 0.0,
					StrokeLineJoin = PenLineJoin.Miter
				};
				foreach (Vertex item6 in item5.SelectMany((FaceHalfEdge e) => e.Vertices))
				{
					Vector3d v2 = item6.Pos;
					worldMatrix.TransformInPlace(ref v2);
					polygon2.Points.Add(GetPointProjection(v2.X, v2.Y));
				}
				ImageUnfoldModel.Children.Add(polygon2);
				hashSet2.Add(polygon2);
			}
			_faceDict.Add(item.ID, hashSet);
			_holeDict.Add(item.ID, hashSet2);
			foreach (FaceHalfEdge item7 in item.BoundaryEdgesCcw)
			{
				Polyline polyline = new Polyline
				{
					Stroke = _l1
				};
				foreach (Vertex vertex in item7.Vertices)
				{
					Vector3d v3 = vertex.Pos;
					worldMatrix.TransformInPlace(ref v3);
					polyline.Points.Add(GetPointProjection(v3.X, v3.Y));
				}
				ImageUnfoldModel.Children.Add(polyline);
			}
		}
		FillLabels();
	}

	private Brush GetCurrentBrushForFace(Face face, IDoc3d doc)
	{
		if (face.FaceGroup.IsBendingZone)
		{
			if (!(Math.Round(face.FaceGroup.ConvexAxis.OpeningAngle * 180.0 / Math.PI, 6) > 0.0))
			{
				return _b3;
			}
			return _b4;
		}
		if (face.FaceGroup.ID == doc.VisibleFaceGroupId)
		{
			return _b2;
		}
		return _b1;
	}

	private Point GetPointProjection(double x, double y)
	{
		return new Point((int)(x * _scale + _dx), (int)(_dHeight - y * _scale - _dy));
	}

	private void FillLabels()
	{
		_bendZoneLabelDict = new Dictionary<IBendDescriptor, Label>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _doc.CombinedBendDescriptors)
		{
			foreach (IBendDescriptor item in combinedBendDescriptor.Enumerable)
			{
				if (_bendZoneLabelDict.TryGetValue(item, out Label value))
				{
					value.Content = (string)value.Content + $", {combinedBendDescriptor.Order + 1}";
					continue;
				}
				Label label = new Label
				{
					Foreground = new SolidColorBrush(Colors.Black),
					Content = $"{combinedBendDescriptor.Order + 1}",
					Background = new SolidColorBrush(Colors.Wheat),
					Margin = new Thickness(5.0),
					BorderThickness = new Thickness(1.0),
					BorderBrush = new SolidColorBrush(Colors.Black)
				};
				label.MouseEnter += Text_MouseEnter;
				label.MouseLeave += Text_MouseLeave;
				_bendZoneLabelDict.Add(item, label);
				Model unfoldFaceGroupModel = item.BendParams.UnfoldFaceGroupModel;
				Shell? shell = unfoldFaceGroupModel.Shells.FirstOrDefault();
				AABB<Vector3d> boundingBox = shell.AABBTree.Root.BoundingBox;
				Vector3d v = boundingBox.Min;
				Vector3d v2 = boundingBox.Max;
				Matrix4d worldMatrix = shell.GetWorldMatrix(unfoldFaceGroupModel);
				worldMatrix.TransformInPlace(ref v);
				worldMatrix.TransformInPlace(ref v2);
				TextCanvasCenter(label, GetPointProjection((v.X + v2.X) / 2.0, (v.Y + v2.Y) / 2.0));
				ImageUnfoldModel?.Children.Add(label);
			}
		}
	}

	private static void TextCanvasCenter(UIElement T, Point p)
	{
		T.Measure(new Size(double.MaxValue, double.MaxValue));
		Canvas.SetLeft(T, p.X - T.DesiredSize.Width / 2.0);
		Canvas.SetTop(T, p.Y - T.DesiredSize.Height / 2.0);
	}

	private void CalculateCurrentScreenParameters()
	{
		ImageUnfoldModel.Measure(new Size(ImageUnfoldModel.ActualWidth, ImageUnfoldModel.ActualHeight));
		ImageUnfoldModel.Arrange(new Rect(0.0, 0.0, ImageUnfoldModel.DesiredSize.Width, ImageUnfoldModel.DesiredSize.Height));
		_dWidth = ImageUnfoldModel.ActualWidth - 20.0;
		_dHeight = ImageUnfoldModel.ActualHeight - 20.0;
		Pair<Vector3d, Vector3d> boundary = _doc.UnfoldModel3D.GetBoundary(Matrix4d.Identity);
		Vector3d item = boundary.Item1;
		Vector3d item2 = boundary.Item2;
		double scale = _dWidth / Math.Abs(item2.X - item.X);
		double num = _dHeight / Math.Abs(item2.Y - item.Y);
		_scale = scale;
		if (num < _scale)
		{
			_scale = num;
		}
		double num2 = item.X * _scale;
		double num3 = item.Y * _scale;
		double num4 = item2.X * _scale;
		double num5 = item2.Y * _scale;
		_dx = _dWidth / 2.0 - (num2 + (num4 - num2) / 2.0) + 10.0;
		_dy = _dHeight / 2.0 - (num3 + (num5 - num3) / 2.0) - 10.0;
	}

	private void Freeze()
	{
		_b1.Freeze();
		_b2.Freeze();
		_b3.Freeze();
		_b4.Freeze();
		_l1.Freeze();
		_l2.Freeze();
		_l3.Freeze();
	}

	private void Text_MouseLeave(object sender, MouseEventArgs e)
	{
		if (!SelectedBends.Any((ICombinedBendDescriptorInternal x) => x.Order == _mouseActiveBend.Order))
		{
			UpdateAllColorsAndTexts();
			HighLightLabels();
			_mouseActiveBend = null;
		}
	}

	private void Text_MouseEnter(object sender, MouseEventArgs e)
	{
		_mouseActiveBend = GetBendForLabel(sender as Label);
		if (_mouseActiveBend == null)
		{
			return;
		}
		ProfileToUse = FittingProfilesDict.FirstOrDefault<KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>>>((KeyValuePair<ICombinedBendDescriptorInternal, VisualPair<ObservableCollection<PreferredProfileViewModel>, PreferredProfileViewModel>> i) => i.Key == _mouseActiveBend);
		foreach (IBendDescriptor item in _mouseActiveBend.Enumerable)
		{
			_bendZoneLabelDict[item].Background = _b4;
		}
	}

	private ICombinedBendDescriptorInternal GetBendForLabel(Label label)
	{
		return _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal cbf) => cbf.Enumerable.Any((IBendDescriptor bz) => _bendZoneLabelDict[bz] == label));
	}

	private void HighLightLabels()
	{
		if (_faceDict == null || SelectedBends == null)
		{
			return;
		}
		UpdateAllColorsAndTexts();
		foreach (ICombinedBendDescriptorInternal selectedBend in SelectedBends)
		{
			foreach (IBendDescriptor item in selectedBend.Enumerable)
			{
				_bendZoneLabelDict[item].Background = _b4;
			}
		}
	}

	private void UpdateAllColorsAndTexts()
	{
		foreach (Face item in _doc.UnfoldModel3D.GetAllFacesWithFaceGroup())
		{
			SetColorForAllPolygonsAtFace(item, GetCurrentBrushForFace(item, _doc), new SolidColorBrush(Colors.White));
		}
		FillLabels();
	}

	private void SetColorForAllPolygonsAtFace(Face face, Brush faceBrush, Brush holeBrush)
	{
		foreach (KeyValuePair<int, HashSet<Polygon>> item in _faceDict)
		{
			if (item.Key != face.ID)
			{
				continue;
			}
			foreach (Polygon item2 in item.Value)
			{
				item2.Fill = faceBrush;
				item2.Stroke = faceBrush;
			}
			break;
		}
		foreach (KeyValuePair<int, HashSet<Polygon>> item3 in _holeDict)
		{
			if (item3.Key != face.ID)
			{
				continue;
			}
			{
				foreach (Polygon item4 in item3.Value)
				{
					item4.Fill = holeBrush;
					item4.Stroke = faceBrush;
				}
				break;
			}
		}
	}

	private void Modification()
	{
		GetBendFaceByOrder();
	}

	private void GetBendFaceByOrder()
	{
		if (_mouseActiveBend != null && FittingProfilesDict.Count > 0)
		{
			ProfileToUse.Value.Item2 = FittingProfilesDict[_mouseActiveBend].Item2;
			SelectedBends.Add(_doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _mouseActiveBend.Order));
		}
	}

	public void Dispose()
	{
		ImageUnfoldModel = null;
		if (_loadThread != null && (_loadThread?.IsAlive).Value)
		{
			_loadThread.Abort();
		}
		_loadThread = null;
		FittingProfilesDict = null;
		SelectedMachine = null;
		SelectedBends = null;
		FittingProfilesDict = null;
		ToolConfigModel = null;
		Machines = null;
		PreferredProfile = null;
		ImagePunchProfile = null;
		ImageDieProfile = null;
		ImageDefaultPunchProfile = null;
		ImageDefaultDieProfile = null;
		if (ProfileToUse.Value != null)
		{
			ProfileToUse.Value.Item1 = null;
			ProfileToUse.Value.Item2 = null;
		}
	}
}

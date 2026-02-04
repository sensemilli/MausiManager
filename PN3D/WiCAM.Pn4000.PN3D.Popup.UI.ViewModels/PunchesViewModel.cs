using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.ClampingSystem;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using Microsoft.Win32;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class PunchesViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private readonly IPunchHelperDeprecated _punchHelper;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private TabItem _actualTab;

	private CollectionView _punchPartView;

	private CollectionView _punchProfileView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private ICommand _checkPunchProfile;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private PunchGroupViewModel _selectedGroup;

	private HolderProfileViewModel _selectedHolder;

	private AdapterProfileViewModel _selectedAdapter;

	private PunchProfileViewModel _selectedProfile;

	private PunchPartViewModel _selectedPart;

	private SensorDiskViewModel _selectedDisk;

	private IDrawToolProfiles _drawToolProfiles;

	private bool _singleSelected = true;

	private bool _isCopyButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private ChangedConfigType _changed;

	private bool _showProfilesWithoutParts;

	private bool _showAllParts;

	private RelayCommand<object> _cmdMirrorGeometry;

	public Action<ChangedConfigType> DataChanged;

	public TabItem ActualTab
	{
		get
		{
			return this._actualTab;
		}
		set
		{
			this._actualTab = value;
			base.NotifyPropertyChanged("ActualTab");
			this.CheckActualTab();
		}
	}

	public FrameworkElement EditScreen
	{
		get
		{
			return this._editScreen;
		}
		set
		{
			this._editScreen = value;
			base.NotifyPropertyChanged("EditScreen");
		}
	}

	public Visibility EditScreenVisible
	{
		get
		{
			return this._editScreenVisible;
		}
		set
		{
			this._editScreenVisible = value;
			base.NotifyPropertyChanged("EditScreenVisible");
		}
	}

	public Type LastSelectedType
	{
		get
		{
			return this._lastSelectedType;
		}
		set
		{
			this._lastSelectedType = value;
			base.NotifyPropertyChanged("LastSelectedType");
			if (this._lastSelectedType == typeof(PunchPart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(PunchProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
			else if (this._lastSelectedType == typeof(SensorDisk))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedDisk;
			}
		}
	}

	public PunchGroupViewModel SelectedGroup
	{
		get
		{
			return this._selectedGroup;
		}
		set
		{
			this._selectedGroup = value;
			base.NotifyPropertyChanged("SelectedGroup");
			if (this.SelectedProfile != null)
			{
				if (this._selectedGroup != null)
				{
					this.SelectedProfile.GroupID = this._selectedGroup.ID;
					this.SelectedProfile.GroupName = this._selectedGroup.Name;
				}
				else
				{
					this.SelectedProfile.GroupID = -1;
					this.SelectedProfile.GroupName = "";
				}
			}
		}
	}

	public HolderProfileViewModel EmptyHolder { get; set; } = new HolderProfileViewModel(new HolderProfile(-1));

	public HolderProfileViewModel SelectedHolder
	{
		get
		{
			return this._selectedHolder;
		}
		set
		{
			this._selectedHolder = value;
			base.NotifyPropertyChanged("SelectedHolder");
			if (this.SelectedProfile != null)
			{
				if (this._selectedHolder != null)
				{
					this.SelectedProfile.HolderID = this._selectedHolder.ID;
					this.SelectedProfile.HolderType = this._selectedHolder.Name;
				}
				else
				{
					this.SelectedProfile.HolderID = -1;
					this.SelectedProfile.HolderType = "";
				}
			}
		}
	}

	public AdapterProfileViewModel SelectedAdapter
	{
		get
		{
			return this._selectedAdapter;
		}
		set
		{
			this._selectedAdapter = value;
			base.NotifyPropertyChanged("SelectedAdapter");
			if (this.SelectedProfile != null)
			{
				if (this._selectedAdapter != null)
				{
					this.SelectedProfile.AdapterID = this._selectedAdapter.ID;
					this.SelectedProfile.AdapterType = this._selectedAdapter.Name;
				}
				else
				{
					this.SelectedProfile.AdapterID = -1;
					this.SelectedProfile.AdapterType = "";
				}
			}
		}
	}

	public PunchProfileViewModel SelectedProfile
	{
		get
		{
			return this._selectedProfile;
		}
		set
		{
			this._selectedProfile = value;
			base.NotifyPropertyChanged("SelectedProfile");
			if (base.BendMachine.Punches.PunchProfiles.Count > 0)
			{
				this.SelectedGroup = ((this._selectedProfile != null) ? base.ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == this._selectedProfile.GroupID) : null);
				this.SelectedHolder = ((this._selectedProfile != null) ? base.ToolConfigModel.UpperHolderProfiles.FirstOrDefault((HolderProfileViewModel g) => g.ID == this._selectedProfile.HolderID) : null);
				this.SelectedAdapter = ((this._selectedProfile != null) ? base.ToolConfigModel.UpperAdapterProfiles.FirstOrDefault((AdapterProfileViewModel g) => g.ID == this._selectedProfile.AdapterID) : null);
			}
			this.ProfileSelectionChanged();
		}
	}

	public ObservableCollection<object> SelectedProfiles { get; internal set; } = new ObservableCollection<object>();

	public PunchPartViewModel SelectedPart
	{
		get
		{
			return this._selectedPart;
		}
		set
		{
			this._selectedPart = value;
			base.NotifyPropertyChanged("SelectedPart");
			this.PartsSelectionChanged();
		}
	}

	public ObservableCollection<object> SelectedParts { get; internal set; } = new ObservableCollection<object>();

	public SensorDiskViewModel SelectedDisk
	{
		get
		{
			return this._selectedDisk;
		}
		set
		{
			this._selectedDisk = value;
			base.NotifyPropertyChanged("SelectedDisk");
			this.DiskSelectionChanged();
		}
	}

	public ObservableCollection<object> SelectedDisks { get; internal set; } = new ObservableCollection<object>();

	public ObservableCollection<SensorDiskViewModel> PossibleDisks { get; set; } = new ObservableCollection<SensorDiskViewModel>();

	public ICommand SelectPathCommand => this._selectPath ?? (this._selectPath = new RelayCommand(SelectPath_Click));

	public ICommand SelectClick => this._selectClick ?? (this._selectClick = new RelayCommand(SelectType));

	public ICommand KeyDownDelete => this._keyDownDelete ?? (this._keyDownDelete = new RelayCommand(Delete_Click));

	public ICommand CheckDieProfileCommand => this._checkPunchProfile ?? (this._checkPunchProfile = new RelayCommand(CheckPunchGeometry));

	public ObservableCollection<string> ClampingType { get; set; }

	public Brush ProfileBorderBrush
	{
		get
		{
			return this._profileBorderBrush;
		}
		set
		{
			this._profileBorderBrush = value;
			base.NotifyPropertyChanged("ProfileBorderBrush");
		}
	}

	public Brush PartsBorderBrush
	{
		get
		{
			return this._partsBorderBrush;
		}
		set
		{
			this._partsBorderBrush = value;
			base.NotifyPropertyChanged("PartsBorderBrush");
		}
	}

	public bool SingleSelected
	{
		get
		{
			return this._singleSelected;
		}
		set
		{
			this._singleSelected = value;
			base.NotifyPropertyChanged("SingleSelected");
		}
	}

	public bool IsCopyButtonEnabled
	{
		get
		{
			return this._isCopyButtonEnabled;
		}
		set
		{
			this._isCopyButtonEnabled = value;
			base.NotifyPropertyChanged("IsCopyButtonEnabled");
		}
	}

	public bool IsDeleteButtonEnabled
	{
		get
		{
			return this._isDeleteButtonEnabled;
		}
		set
		{
			this._isDeleteButtonEnabled = value;
			base.NotifyPropertyChanged("IsDeleteButtonEnabled");
		}
	}

	public bool IsEditButtonEnabled
	{
		get
		{
			return this._isEditButtonEnabled;
		}
		set
		{
			this._isEditButtonEnabled = value;
			base.NotifyPropertyChanged("IsEditButtonEnabled");
		}
	}

	public bool IsAddButtonEnabled
	{
		get
		{
			return this._isAddButtonEnabled;
		}
		set
		{
			this._isAddButtonEnabled = value;
			base.NotifyPropertyChanged("IsAddButtonEnabled");
		}
	}

	public bool IsOkButtonEnabled
	{
		get
		{
			return this._isOkButtonEnabled;
		}
		set
		{
			this._isOkButtonEnabled = value;
			base.NotifyPropertyChanged("IsOkButtonEnabled");
		}
	}

	public bool IsCancelButtonEnabled
	{
		get
		{
			return this._isCancelButtonEnabled;
		}
		set
		{
			this._isCancelButtonEnabled = value;
			base.NotifyPropertyChanged("IsCancelButtonEnabled");
		}
	}

	public bool IsSaveButtonEnabled
	{
		get
		{
			return this._isSaveButtonEnabled;
		}
		set
		{
			this._isSaveButtonEnabled = value;
			base.NotifyPropertyChanged("IsSaveButtonEnabled");
		}
	}

	public ObservableCollection<string> PunchGroupNames { get; set; }

	public ObservableCollection<GeometryFileDataViewModel> GeometryFiles { get; set; }

	public ObservableCollection<RadMenuItem> ProfileContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> PartContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> DiskContextMenuItems { get; set; }

	public bool ShowProfilesWithoutParts
	{
		get
		{
			return this._showProfilesWithoutParts;
		}
		set
		{
			this._showProfilesWithoutParts = value;
			base.NotifyPropertyChanged("ShowProfilesWithoutParts");
			this.LoadTools();
		}
	}

	public bool ShowAllParts
	{
		get
		{
			return this._showAllParts;
		}
		set
		{
			this._showAllParts = value;
			base.NotifyPropertyChanged("ShowAllParts");
			this.LoadTools();
		}
	}

	public CollectionView PunchProfileView
	{
		get
		{
			return this._punchProfileView;
		}
		set
		{
			this._punchProfileView = value;
			base.NotifyPropertyChanged("PunchProfileView");
		}
	}

	public CollectionView PunchPartView
	{
		get
		{
			return this._punchPartView;
		}
		set
		{
			this._punchPartView = value;
			base.NotifyPropertyChanged("PunchPartView");
		}
	}

	public RelayCommand<object> CmdMirrorGeometry => this._cmdMirrorGeometry ?? (this._cmdMirrorGeometry = new RelayCommand<object>(MirrorGeometry));

	public PunchesViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pathService, IConfigProvider configProvider, IModelFactory modelFactory, IPunchHelperDeprecated punchHelper, IDrawToolProfiles drawToolProfiles)
		: base(globals, mainWindowDataProvider, pathService, modelFactory)
	{
		this._configProvider = configProvider;
		this._punchHelper = punchHelper;
		this._drawToolProfiles = drawToolProfiles;
	}

	public void Init(BendMachine bendMachine, ToolConfigModel toolConfigModel, IToolExpert tools = null)
	{
		base.ToolConfigModel = toolConfigModel;
		base.BendMachine = bendMachine;
		base.Tools = tools;
		this.ClampingType = new ObservableCollection<string>();
		base.ImagePart.Loaded += Image3DOnLoaded;
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		this.InitializeProfileContextMenuItems();
		this.InitializePartContextMenuItems();
		this.InitializeDiskContextMenuItems();
		((Screen3D)base.ImagePart).MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		if (base.BendMachine != null)
		{
			this.LoadTools();
			foreach (ClampingSystemProfile clampingSystemProfile in base.BendMachine.ClampingSystem.ClampingSystemProfiles)
			{
				this.ClampingType.Add(clampingSystemProfile.Name);
			}
		}
		this.PunchGroupNames = new ObservableCollection<string>(base.ToolConfigModel.PunchGroups.Select((PunchGroupViewModel g) => g.Name));
		this.PunchGroupNames.Insert(0, "*");
		string path = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry;
		this.GeometryFiles = (Directory.Exists(path) ? (from f in Directory.GetFiles(path)
			select new GeometryFileDataViewModel
			{
				Name = new FileInfo(f).Name
			}).ToObservableCollection() : null);
		base.SelectedType = "Punches";
	}

	private void InitializeProfileContextMenuItems()
	{
		this.ProfileContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyProfile_Click)
		};
		this.ProfileContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditProfile_Click)
		};
		this.ProfileContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteProfile_Click)
		};
		this.ProfileContextMenuItems.Add(item3);
	}

	private void InitializePartContextMenuItems()
	{
		this.PartContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyPart_Click)
		};
		this.PartContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditPart_Click)
		};
		this.PartContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand((Action)delegate
			{
				this.DeletePart_Click(this.SelectedPart);
			})
		};
		this.PartContextMenuItems.Add(item3);
	}

	private void InitializeDiskContextMenuItems()
	{
		this.DiskContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyDisk_Click)
		};
		this.DiskContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditDisk_Click)
		};
		this.DiskContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand((Action)delegate
			{
				this.DeleteDisk_Click(this.SelectedDisk);
			})
		};
		this.DiskContextMenuItems.Add(item3);
	}

	public void LoadTools()
	{
		this.PunchProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.PunchProfiles);
		this.PunchProfileView.Filter = PunchProfileFilter;
		this.PunchProfileView.Refresh();
		this.PunchPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.PunchParts);
		this.PunchPartView.Filter = PunchPartFilter;
		this.PunchPartView.Refresh();
		this.SelectedProfile = ((this.PunchProfileView.Count > 0) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(0)) : null);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		Parallel.ForEach((IEnumerable<PunchProfileViewModel>)base.ToolConfigModel.PunchProfiles, (Action<PunchProfileViewModel>)delegate(PunchProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		Parallel.ForEach((IEnumerable<PunchPartViewModel>)base.ToolConfigModel.PunchParts, (Action<PunchPartViewModel>)delegate(PunchPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		this.PunchProfileView.Filter = PunchProfileFilter;
		this.PunchProfileView.Refresh();
		this.PunchPartView.Filter = PunchPartFilter;
		this.PunchPartView.Refresh();
		this.SelectedProfile = ((this.PunchProfileView.Count > 0) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(0)) : null);
		this.SelectedPart = ((this.PunchPartView.Count > 0) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(0)) : null);
	}

	public override void UpdateSelectedItemGeometry()
	{
		this.ProfileSelectionChanged();
	}

	public void ProfileSelectionChanged()
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			if (base.ImageProfile is Canvas canvas)
			{
				canvas.Children.Clear();
			}
			base.NotifyPropertyChanged("ImageProfile");
			this.PunchPartView.Filter = PunchPartFilter;
			this.PunchPartView.Refresh();
			this.SelectType("Profiles");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<PunchProfileViewModel>)base.ToolConfigModel.PunchProfiles, (Action<PunchProfileViewModel>)delegate(PunchProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
		{
			this._drawToolProfiles.LoadPunchPreview2D(this.SelectedProfile.PunchProfile, (Canvas)base.ImageProfile, base.BendMachine);
			base.NotifyPropertyChanged("ImageProfile");
		}
		this.PunchPartView.Filter = PunchPartFilter;
		this.PunchPartView.Refresh();
		int num = ((this.PunchPartView.Count <= 0) ? (-1) : 0);
		this.SelectedPart = ((num >= 0 && this.PunchPartView.Count > 0 && num < this.PunchPartView.Count) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(num)) : null);
		this.SelectType("Profiles");
		this.SetEditorEnableRules();
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			if (base.ImagePart is Screen3D screen3D)
			{
				screen3D.ScreenD3D?.RemoveModel(null);
				screen3D.ScreenD3D?.RemoveBillboard(null);
				screen3D.ScreenD3D?.ZoomExtend();
			}
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<PunchPartViewModel>)base.ToolConfigModel.PunchParts, (Action<PunchPartViewModel>)delegate(PunchPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		if (this.SelectedPart.IsAngleMeasurementTool)
		{
			this.PossibleDisks.Add(new SensorDiskViewModel());
			this.PossibleDisks = base.ToolConfigModel.SensorDisks.Where((SensorDiskViewModel d) => d.MeasuringToolTypeID == this.SelectedPart.ToolTypeID).ToObservableCollection();
		}
		else
		{
			this.PossibleDisks.Clear();
		}
		base.NotifyPropertyChanged("PossibleDisks");
		global::WiCAM.Pn4000.BendModel.Model model = null;
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			((Screen3D)base.ImagePart).ScreenD3D?.ZoomExtend();
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		string text;
		if (!string.IsNullOrEmpty(this.SelectedPart.GeometryFile?.Name))
		{
			text = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + this.SelectedPart.GeometryFile.Name;
			if (!File.Exists(text))
			{
				text = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + this.SelectedProfile.GeometryFile.Name;
			}
		}
		else
		{
			text = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + this.SelectedProfile.GeometryFile.Name;
		}
		if (base.ImagePart is Screen3D screen3D2)
		{
			screen3D2.ShowNavigation(show: false);
			screen3D2.SetConfigProviderAndApplySettings(this._configProvider);
		}
		((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
		((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
		if (text.EndsWith(".c3mo"))
		{
			if (this.SelectedProfile != null)
			{
				model = ModelSerializer.Deserialize(text);
				if (model != null)
				{
					model.Transform = Matrix4d.Identity;
					model.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
					model.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
					model.Transform *= Matrix4d.Translation((0.0 - this.SelectedPart.Length) / 2.0, 0.0, (0.0 - this.SelectedProfile.WorkingHeight) / 2.0);
				}
			}
		}
		else if (this.SelectedProfile != null)
		{
			model = CadGeoLoader.LoadCadGeo3D(text, this.SelectedPart.Length, 200.0, toolEdges: true, null, null);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 4.0);
				model.Transform *= Matrix4d.Translation(0.0, 0.0, (0.0 - this.SelectedProfile.WorkingHeight) / 2.0);
			}
		}
		if (this.SelectedProfile != null && !string.IsNullOrEmpty(this.SelectedPart?.SensorDisk?.GeometryFile?.Name))
		{
			string text2 = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + this.SelectedPart.SensorDisk.GeometryFile.Name;
			global::WiCAM.Pn4000.BendModel.Model model2 = null;
			if (text2.EndsWith(".c3mo"))
			{
				model2 = ModelSerializer.Deserialize(text2);
				model2.Parent = model;
				if (model != null)
				{
					model2.Transform = Matrix4d.Identity;
				}
			}
			else
			{
				model2 = CadGeoLoader.LoadCadGeo3D(text2, this.SelectedPart.SensorDisk.DiskThickness, 200.0, toolEdges: true, null, null);
				if (model2 != null)
				{
					model2.Transform *= Matrix4d.Translation(this.SelectedPart.Length * 0.5, 0.0, 0.0);
				}
			}
			if (model2 != null)
			{
				model.SubModels.Add(model2);
			}
		}
		if (model != null)
		{
			((Screen3D)base.ImagePart).ScreenD3D?.AddModel(model, render: false);
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(1.5707963705062866);
			identity *= Matrix4d.RotationX(0.39269909262657166);
			((Screen3D)base.ImagePart).ScreenD3D?.SetViewDirectionByMatrix4d(identity, render: false, delegate
			{
				((Screen3D)base.ImagePart).ScreenD3D.ZoomExtend();
			});
		}
		base.NotifyPropertyChanged("ImagePart");
		this.SelectType("Parts");
		this.SetEditorEnableRules();
	}

	private void DiskSelectionChanged()
	{
		if (this.SelectedDisk == null)
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			((Screen3D)base.ImagePart).ScreenD3D.ZoomExtend();
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Disks");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<SensorDiskViewModel>)base.ToolConfigModel.SensorDisks, (Action<SensorDiskViewModel>)delegate(SensorDiskViewModel item)
		{
			if (item != this.SelectedDisk)
			{
				item.IsSelected = false;
			}
		});
		if (this.SelectedDisk == null)
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			((Screen3D)base.ImagePart).ScreenD3D.ZoomExtend();
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Disks");
			this.SetEditorEnableRules();
			return;
		}
		string text = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + this.SelectedDisk.GeometryFile.Name;
		if (base.ImagePart is Screen3D screen3D)
		{
			screen3D.ShowNavigation(show: false);
			screen3D.SetConfigProviderAndApplySettings(this._configProvider);
			screen3D.ScreenD3D?.RemoveModel(null);
			screen3D.ScreenD3D?.RemoveBillboard(null);
		}
		global::WiCAM.Pn4000.BendModel.Model model;
		if (text.EndsWith(".c3mo"))
		{
			model = ModelSerializer.Deserialize(text);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
				model.Transform *= Matrix4d.Translation((0.0 - this.SelectedDisk.DiskThickness) / 2.0, 0.0, (0.0 - this.SelectedProfile.WorkingHeight) / 2.0);
			}
		}
		else
		{
			model = CadGeoLoader.LoadCadGeo3D(text, this.SelectedDisk.DiskThickness, 200.0, toolEdges: true, null, null);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 4.0);
				model.Transform *= Matrix4d.Translation(0.0, 0.0, 0.0);
			}
		}
		if (model != null)
		{
			((Screen3D)base.ImagePart).ScreenD3D?.AddModel(model, render: false);
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(1.5707963705062866);
			identity *= Matrix4d.RotationX(0.39269909262657166);
			((Screen3D)base.ImagePart).ScreenD3D?.SetViewDirectionByMatrix4d(identity, render: false, delegate
			{
				((Screen3D)base.ImagePart).ScreenD3D.ZoomExtend();
			});
		}
		base.NotifyPropertyChanged("ImagePart");
		this.SelectType("Disks");
		this.SetEditorEnableRules();
	}

	private bool PunchProfileFilter(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.PunchParts.Any((PunchPartViewModel p) => p.PunchProfileID == ((PunchProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool PunchPartFilter(object item)
	{
		if (item == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		if (((PunchPartViewModel)item).PunchProfileID == this.SelectedProfile.ID)
		{
			if (this.ShowAllParts || ((PunchPartViewModel)item).Amount <= 0)
			{
				return this.ShowAllParts;
			}
			return true;
		}
		return false;
	}

	public void AddProfile_Click()
	{
		int id = ((base.ToolConfigModel.PunchProfiles.Count > 0) ? (base.ToolConfigModel.PunchProfiles.Max((PunchProfileViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.PunchProfiles.Add(new PunchProfileViewModel(new PunchProfile(id), null, null, null));
		int num = this.PunchProfileView.Count - 1;
		this.SelectedProfile = ((num >= 0 && this.PunchProfileView.Count > 0 && num < this.PunchProfileView.Count) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(num)) : null);
		this._changed = ChangedConfigType.Punches;
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile != null)
		{
			int id = ((base.ToolConfigModel.PunchParts.Count > 0) ? (base.ToolConfigModel.PunchParts.Max((PunchPartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.PunchParts.Add(new PunchPartViewModel(new PunchPart(id, this.SelectedProfile.ID), null));
			int num = this.PunchPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.PunchPartView.Count > 0 && num < this.PunchPartView.Count) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.Punches;
		}
	}

	public void AddDisk_Click()
	{
		int id = ((base.ToolConfigModel.SensorDisks.Count > 0) ? (base.ToolConfigModel.SensorDisks.Max((SensorDiskViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.SensorDisks.Add(new SensorDiskViewModel(new SensorDisk(id)));
		int num = base.ToolConfigModel.SensorDisks.Count - 1;
		this.SelectedDisk = ((num >= 0 && base.ToolConfigModel.SensorDisks.Count > 0 && num < base.ToolConfigModel.SensorDisks.Count) ? base.ToolConfigModel.SensorDisks[num] : null);
		this._changed = ChangedConfigType.Punches;
	}

	public void CopyPart_Click()
	{
		if (this.SelectedPart != null)
		{
			int iD = ((base.ToolConfigModel.PunchParts.Count > 0) ? (base.ToolConfigModel.PunchParts.Max((PunchPartViewModel p) => p.ID) + 1) : 0);
			PunchPart newPart = this.SelectedPart.PunchPart.Copy();
			newPart.ID = iD;
			SensorDiskViewModel sensorDisk = base.ToolConfigModel.SensorDisks.FirstOrDefault((SensorDiskViewModel d) => d.ID == newPart.SensorDiskID);
			base.ToolConfigModel.PunchParts.Add(new PunchPartViewModel(newPart, sensorDisk));
			int num = this.PunchPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.PunchPartView.Count > 0 && num < this.PunchPartView.Count) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.Punches;
		}
	}

	public void CopyProfile_Click()
	{
		if (this.SelectedProfile == null)
		{
			return;
		}
		int iD = ((base.ToolConfigModel.PunchProfiles.Count > 0) ? (base.ToolConfigModel.PunchProfiles.Max((PunchProfileViewModel p) => p.ID) + 1) : 0);
		PunchProfile newProfile = this.SelectedProfile.PunchProfile.Copy();
		newProfile.ID = iD;
		AdapterProfileViewModel adapter = null;
		if (newProfile.AdapterID >= 0)
		{
			adapter = base.ToolConfigModel.UpperAdapterProfiles.FirstOrDefault((AdapterProfileViewModel a) => a.ID == newProfile.AdapterID);
		}
		HolderProfileViewModel holder = null;
		if (newProfile.HolderID >= 0)
		{
			holder = base.ToolConfigModel.UpperHolderProfiles.FirstOrDefault((HolderProfileViewModel h) => h.ID == newProfile.HolderID);
		}
		PunchGroupViewModel punchGroup = base.ToolConfigModel.PunchGroups.FirstOrDefault((PunchGroupViewModel g) => g.ID == newProfile.GroupID);
		base.ToolConfigModel.PunchProfiles.Add(new PunchProfileViewModel(newProfile, punchGroup, adapter, holder));
		int num = this.PunchProfileView.Count - 1;
		this.SelectedProfile = ((num >= 0 && this.PunchProfileView.Count > 0 && num < this.PunchProfileView.Count) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(num)) : null);
		this._changed = ChangedConfigType.Punches;
	}

	public void CopyDisk_Click()
	{
		if (this.SelectedDisk != null)
		{
			int iD = ((base.ToolConfigModel.SensorDisks.Count > 0) ? (base.ToolConfigModel.SensorDisks.Max((SensorDiskViewModel p) => p.ID) + 1) : 0);
			SensorDisk sensorDisk = this.SelectedDisk.SensorDisk.Copy();
			sensorDisk.ID = iD;
			base.ToolConfigModel.SensorDisks.Add(new SensorDiskViewModel(sensorDisk));
			int num = base.ToolConfigModel.SensorDisks.Count - 1;
			this.SelectedDisk = ((num >= 0 && base.ToolConfigModel.SensorDisks.Count > 0 && num < base.ToolConfigModel.SensorDisks.Count) ? base.ToolConfigModel.SensorDisks[num] : null);
			this._changed = ChangedConfigType.Punches;
		}
	}

	private void SelectType(object param)
	{
		switch ((string)param)
		{
		case "Profiles":
			this.LastSelectedType = typeof(PunchProfile);
			base.LastSelectedObject = this.SelectedProfile;
			break;
		case "Parts":
			this.LastSelectedType = typeof(PunchPart);
			base.LastSelectedObject = this.SelectedPart;
			break;
		case "Disks":
			this.LastSelectedType = typeof(SensorDisk);
			base.LastSelectedObject = this.SelectedDisk;
			break;
		}
	}

	public void Delete_Click(object param)
	{
		if (param == null)
		{
			return;
		}
		switch (param.GetType().ToString().Split('.')
			.Last())
		{
		case "PunchProfileViewModel":
			if (this.SelectedProfiles.Any())
			{
				this.DeleteProfile_Click();
			}
			break;
		case "PunchPartViewModel":
			if (!this.SelectedParts.Any())
			{
				return;
			}
			this.DeletePart_Click(param);
			break;
		case "SensorDiskViewModel":
			if (!this.SelectedDisks.Any())
			{
				return;
			}
			this.DeleteDisk_Click(param);
			break;
		}
		this._changed = ChangedConfigType.Punches;
	}

	private void CheckPunchGeometry()
	{
		string text = this._punchHelper.CheckPunchGeometry(this.SelectedProfile.PunchProfile, base.BendMachine);
		bool num = text != "";
		text = ($"Checking \"{this.SelectedProfile.PunchProfile.Name}\" (Non standard punch profiles might not be checked correctly):\n" + text).Replace("\n", "\n\n");
		if (num)
		{
			base._globals.MessageDisplay.ShowWarningMessage(text);
		}
		else
		{
			base._globals.MessageDisplay.ShowInformationMessage(text + "All correct");
		}
	}

	private void DeleteProfile_Click()
	{
		int num = base.ToolConfigModel.PunchProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count();
		List<PunchProfileViewModel> list = this.SelectedProfiles.Select((object p) => p as PunchProfileViewModel).ToList();
		foreach (PunchProfileViewModel item in list)
		{
			this.DeletePartsFromProfile(item.ID);
		}
		base.ToolConfigModel.PunchProfiles = base.ToolConfigModel.PunchProfiles.Except(list).ToObservableCollection();
		if (num >= base.ToolConfigModel.PunchProfiles.Count)
		{
			num = base.ToolConfigModel.PunchProfiles.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedProfile = null;
		}
		else
		{
			int num2 = this.PunchProfileView.Count - 1;
			this.SelectedProfile = ((num2 >= 0 && this.PunchProfileView.Count > 0 && num2 < this.PunchProfileView.Count) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(num2)) : null);
		}
		this.PunchProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.PunchProfiles);
		this.PunchProfileView.Filter = PunchProfileFilter;
		this.PunchProfileView.Refresh();
	}

	private void DeletePart_Click(object param)
	{
		int num = base.ToolConfigModel.PunchParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count();
		if (num > base.ToolConfigModel.PunchParts.Count)
		{
			num = this.PunchPartView.IndexOf((PunchPartViewModel)param) - 1;
		}
		List<PunchPartViewModel> second = this.SelectedParts.Select((object p) => p as PunchPartViewModel).ToList();
		base.ToolConfigModel.PunchParts = base.ToolConfigModel.PunchParts.Except(second).ToObservableCollection();
		this.SetEditorEnableRules();
		if (num < 0 && this.PunchPartView.Count > 0)
		{
			num = 0;
		}
		this.SelectedPart = ((num >= 0 && this.PunchPartView.Count > 0 && num < this.PunchPartView.Count) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(num)) : null);
		this.PunchPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.PunchParts);
		this.PunchPartView.Filter = PunchPartFilter;
		this.PunchPartView.Refresh();
	}

	private void DeleteDisk_Click(object param)
	{
		int num = base.ToolConfigModel.SensorDisks.IndexOf(this.SelectedDisks.Last()) + 1 - this.SelectedDisks.Count();
		if (num > base.ToolConfigModel.SensorDisks.Count)
		{
			num = base.ToolConfigModel.SensorDisks.IndexOf((SensorDiskViewModel)param) - 1;
		}
		List<SensorDiskViewModel> second = this.SelectedDisks.Select((object p) => p as SensorDiskViewModel).ToList();
		base.ToolConfigModel.SensorDisks = base.ToolConfigModel.SensorDisks.Except(second).ToObservableCollection();
		this.SetEditorEnableRules();
		if (num < 0 && base.ToolConfigModel.SensorDisks.Count > 0)
		{
			num = 0;
		}
		this.SelectedDisk = ((num >= 0 && base.ToolConfigModel.SensorDisks.Count > 0 && num < base.ToolConfigModel.SensorDisks.Count) ? base.ToolConfigModel.SensorDisks[num] : null);
	}

	public void EditProfile_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.PunchProfiles.IndexOf(this.SelectedProfile);
				base.ToolConfigModel.PunchProfiles.Remove(this.SelectedProfile);
				base.ToolConfigModel.PunchProfiles.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as PunchProfileViewModel);
				this.SelectedProfile = base.ToolConfigModel.PunchProfiles[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<PunchProfileViewModel>)base.ToolConfigModel.PunchProfiles).Select((Func<PunchProfileViewModel, ToolItemViewModelBase>)((PunchProfileViewModel i) => i)), selectedItem: this.SelectedProfile, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
		EditScreenView editScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		if (this.EditScreen != null)
		{
			((this.EditScreen as EditScreenView)?.DataContext as EditScreenViewModel)?.Dispose();
			this.EditScreen = null;
		}
		this.EditScreen = editScreen;
		this.EditScreenVisible = Visibility.Visible;
		this.SetEditorEnableRules();
	}

	public void EditPart_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.PunchParts.IndexOf(this.SelectedPart);
				base.ToolConfigModel.PunchParts.Remove(this.SelectedPart);
				base.ToolConfigModel.PunchParts.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as PunchPartViewModel);
				this.SelectedPart = base.ToolConfigModel.PunchParts[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: base.ToolConfigModel.PunchParts.Where((PunchPartViewModel p) => p.PunchProfileID == this.SelectedProfile.ID).Select((Func<PunchPartViewModel, ToolItemViewModelBase>)((PunchPartViewModel i) => i)), selectedItem: this.SelectedPart, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
		this.EditScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		this.EditScreenVisible = Visibility.Visible;
		this.SetEditorEnableRules();
	}

	public void EditDisk_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.SensorDisks.IndexOf(this.SelectedDisk);
				base.ToolConfigModel.SensorDisks.Remove(this.SelectedDisk);
				base.ToolConfigModel.SensorDisks.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as SensorDiskViewModel);
				this.SelectedDisk = base.ToolConfigModel.SensorDisks[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<SensorDiskViewModel>)base.ToolConfigModel.SensorDisks).Select((Func<SensorDiskViewModel, ToolItemViewModelBase>)((SensorDiskViewModel i) => i)), selectedItem: this.SelectedDisk, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
		this.EditScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		this.EditScreenVisible = Visibility.Visible;
		this.SetEditorEnableRules();
	}

	private void DeletePartsFromProfile(int profileId)
	{
		base.ToolConfigModel.PunchParts.Where((PunchPartViewModel p) => p.PunchProfileID == profileId).ToList().ForEach(delegate(PunchPartViewModel p)
		{
			base.ToolConfigModel.PunchParts.Remove(p);
		});
	}

	private void SelectPath_Click(object param)
	{
		if (param != null)
		{
			string text = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry;
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				InitialDirectory = text
			};
			bool? flag = openFileDialog.ShowDialog();
			if (flag.HasValue && flag.Value)
			{
				if (Path.GetDirectoryName(openFileDialog.FileName) + "\\" != text)
				{
					File.Copy(openFileDialog.FileName, text + openFileDialog.SafeFileName, overwrite: true);
				}
				((dynamic)param).GeometryFile = openFileDialog.SafeFileName;
			}
		}
		this.SelectedProfile = this.SelectedProfile;
	}

	public void SetEditorEnableRules()
	{
		if (this.EditScreen != null)
		{
			this.IsAddButtonEnabled = false;
			this.IsCopyButtonEnabled = false;
			this.IsDeleteButtonEnabled = false;
			this.IsEditButtonEnabled = false;
			this.IsOkButtonEnabled = false;
			this.IsCancelButtonEnabled = false;
			this.IsSaveButtonEnabled = false;
			return;
		}
		if (this.LastSelectedType == typeof(PunchProfile))
		{
			ObservableCollection<object> selectedProfiles = this.SelectedProfiles;
			this.SingleSelected = selectedProfiles == null || selectedProfiles.Count <= 1;
			if (this.SelectedProfile == null)
			{
				this.IsCopyButtonEnabled = false;
				this.IsDeleteButtonEnabled = false;
				this.IsEditButtonEnabled = false;
				this.IsAddButtonEnabled = true;
				this.IsOkButtonEnabled = true;
				this.IsCancelButtonEnabled = true;
				this.IsSaveButtonEnabled = true;
				return;
			}
			this.IsCopyButtonEnabled = this.SelectedProfile != null && this.SingleSelected;
			this.IsEditButtonEnabled = this.SelectedProfile != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedProfile != null;
		}
		else if (this.LastSelectedType == typeof(PunchPart))
		{
			ObservableCollection<object> selectedParts = this.SelectedParts;
			this.SingleSelected = selectedParts == null || selectedParts.Count <= 1;
			if (this.SelectedPart == null)
			{
				this.IsCopyButtonEnabled = false;
				this.IsDeleteButtonEnabled = false;
				this.IsEditButtonEnabled = false;
				this.IsAddButtonEnabled = true;
				this.IsOkButtonEnabled = true;
				this.IsCancelButtonEnabled = true;
				this.IsSaveButtonEnabled = true;
				return;
			}
			this.IsCopyButtonEnabled = this.SelectedPart != null && this.SingleSelected;
			this.IsEditButtonEnabled = this.SelectedPart != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedPart != null;
		}
		else if (this.LastSelectedType == typeof(SensorDisk))
		{
			ObservableCollection<object> selectedDisks = this.SelectedDisks;
			this.SingleSelected = selectedDisks == null || selectedDisks.Count <= 1;
			if (this.SelectedDisk == null)
			{
				this.IsCopyButtonEnabled = false;
				this.IsDeleteButtonEnabled = false;
				this.IsEditButtonEnabled = false;
				this.IsAddButtonEnabled = true;
				this.IsOkButtonEnabled = true;
				this.IsCancelButtonEnabled = true;
				this.IsSaveButtonEnabled = true;
				return;
			}
			this.IsCopyButtonEnabled = this.SelectedDisk != null && this.SingleSelected;
			this.IsEditButtonEnabled = this.SelectedDisk != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedDisk != null;
		}
		else
		{
			this.IsCopyButtonEnabled = false;
			this.IsDeleteButtonEnabled = false;
			this.IsEditButtonEnabled = false;
		}
		this.IsAddButtonEnabled = true;
		this.IsOkButtonEnabled = true;
		this.IsCancelButtonEnabled = true;
		this.IsSaveButtonEnabled = true;
	}

	private void CheckActualTab()
	{
		switch (this.ActualTab?.Name)
		{
		case "Profiles":
			this.PunchProfileView.Filter = PunchPartFilter;
			this.PunchProfileView.Refresh();
			this.SelectedProfile = ((this.PunchProfileView.Count > 0) ? ((PunchProfileViewModel)this.PunchProfileView.GetItemAt(0)) : null);
			break;
		case "Parts":
			this.PunchPartView.Filter = PunchPartFilter;
			this.PunchPartView.Refresh();
			this.SelectedPart = ((this.PunchPartView.Count > 0) ? ((PunchPartViewModel)this.PunchPartView.GetItemAt(0)) : null);
			break;
		case "Disks":
			this.SelectedDisk = this.PossibleDisks.FirstOrDefault();
			break;
		}
	}

	public void MirrorGeometry(object param)
	{
		if (!(param is PunchProfileViewModel punchProfileViewModel))
		{
			return;
		}
		string path = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + punchProfileViewModel.PunchProfile.GeometryFile;
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			CadGeoHelper cadGeoHelper = new CadGeoHelper();
			CadGeoInfo cadGeoInfo = cadGeoHelper.ReadCadgeo(path);
			foreach (GeoElementInfo geoElement in cadGeoInfo.GeoElements)
			{
				if (geoElement is GeoLineInfo geoLineInfo)
				{
					geoLineInfo.X0 *= -1.0;
					geoLineInfo.X1 *= -1.0;
				}
				else if (geoElement is GeoArcInfo geoArcInfo)
				{
					geoArcInfo.X0 *= -1.0;
					geoArcInfo.Direction *= -1;
					if (geoArcInfo.BeginAngle <= 180.0)
					{
						geoArcInfo.BeginAngle = 180.0 - geoArcInfo.BeginAngle;
					}
					else
					{
						geoArcInfo.BeginAngle = 360.0 - (geoArcInfo.BeginAngle - 180.0);
					}
					if (geoArcInfo.EndAngle <= 180.0)
					{
						geoArcInfo.EndAngle = 180.0 - geoArcInfo.EndAngle;
					}
					else
					{
						geoArcInfo.EndAngle = 360.0 - (geoArcInfo.EndAngle - 180.0);
					}
				}
			}
			cadGeoHelper.Write(cadGeoInfo, path);
			foreach (PunchPartViewModel punchPart in base.ToolConfigModel.PunchParts)
			{
				if (punchPart.PunchProfileID == punchProfileViewModel.PunchProfile.ID && !string.IsNullOrWhiteSpace(punchPart.GeometryFile.Name))
				{
					path = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry + punchPart.GeometryFile.Name;
					if (File.Exists(path) && path.ToLower().EndsWith(".c3mo"))
					{
						global::WiCAM.Pn4000.BendModel.Model model = ModelSerializer.Deserialize(path);
						model.ModifyVertices(Matrix4d.Scale(1.0, -1.0, 1.0), transformSubModels: true);
						ModelSerializer.Serialize(path, model);
					}
					else
					{
						MessageBox.Show("Error on file '" + path + "'. Geometry can't be mirrored.", "error", MessageBoxButton.OK, MessageBoxImage.Hand);
					}
				}
			}
		}
		catch (Exception)
		{
			MessageBox.Show("Error occurred. Please load backup.", "error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		this.ProfileSelectionChanged();
	}

	public void Dispose()
	{
		if (base.ImageProfile?.GetType() == typeof(Screen3D))
		{
			((Screen3D)base.ImageProfile).Dispose();
		}
		base.ImageProfile = null;
		if (base.ImagePart?.GetType() == typeof(Screen3D))
		{
			((Screen3D)base.ImagePart).Dispose();
		}
		base.ImagePart = null;
	}

	public void Save()
	{
		base.BendMachine.Punches.PunchProfiles = new ObservableCollection<PunchProfile>();
		foreach (PunchProfileViewModel punchProfile in base.ToolConfigModel.PunchProfiles)
		{
			base.BendMachine.Punches.PunchProfiles.Add(punchProfile.PunchProfile);
		}
		base.BendMachine.Punches.PunchParts = new ObservableCollection<PunchPart>();
		foreach (PunchPartViewModel punchPart in base.ToolConfigModel.PunchParts)
		{
			base.BendMachine.Punches.PunchParts.Add(punchPart.PunchPart);
		}
		base.BendMachine.Punches.SensorDisks = new List<SensorDisk>();
		foreach (SensorDiskViewModel item in base.ToolConfigModel.SensorDisks.Where((SensorDiskViewModel d) => d.ID > -1))
		{
			base.BendMachine.Punches.SensorDisks.Add(item.SensorDisk);
		}
		if (base.ToolConfigModel.PunchProfiles.Any((PunchProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Punches;
		}
		if (base.ToolConfigModel.PunchParts.Any((PunchPartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Punches;
		}
		if (base.ToolConfigModel.SensorDisks.Any((SensorDiskViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Punches;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

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
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
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
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
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

public class DiesViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private readonly ITranslator _translator;

	private readonly IDieHelperDeprecated _dieHelper;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private CollectionView _diePartView;

	private CollectionView _dieProfileView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private ICommand _checkDieProfile;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private DieGroupViewModel _selectedGroup;

	private AdapterProfileViewModel _selectedAdapter;

	private HolderProfileViewModel _selectedHolder;

	private DieProfileViewModel _selectedProfile;

	private DiePartViewModel _selectedPart;

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
			if (this._lastSelectedType == typeof(DiePart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(DieProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
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

	public DieProfileViewModel SelectedProfile
	{
		get
		{
			return this._selectedProfile;
		}
		set
		{
			this._selectedProfile = value;
			base.NotifyPropertyChanged("SelectedProfile");
			if (base.ToolConfigModel.DieProfiles.Count > 0)
			{
				this.SelectedGroup = ((this._selectedProfile != null) ? base.ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == this._selectedProfile.GroupID) : null);
				this.SelectedHolder = ((this._selectedProfile != null) ? base.ToolConfigModel.LowerHolderProfiles.FirstOrDefault((HolderProfileViewModel g) => g.ID == this._selectedProfile.HolderID) : null);
				this.SelectedAdapter = ((this._selectedProfile != null) ? base.ToolConfigModel.LowerAdapterProfiles.FirstOrDefault((AdapterProfileViewModel g) => g.ID == this._selectedProfile.AdapterID) : null);
			}
			this.ProfileSelectionChanged();
		}
	}

	public ObservableCollection<object> SelectedProfiles { get; internal set; } = new ObservableCollection<object>();

	public DiePartViewModel SelectedPart
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

	public ObservableCollection<string> ClampingType { get; set; }

	public DieGroupViewModel SelectedGroup
	{
		get
		{
			return this._selectedGroup;
		}
		set
		{
			this._selectedGroup = value;
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
			base.NotifyPropertyChanged("SelectedGroup");
		}
	}

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

	public ICommand SelectPathCommand => this._selectPath ?? (this._selectPath = new RelayCommand(SelectPath_Click));

	public ICommand SelectClick => this._selectClick ?? (this._selectClick = new RelayCommand(SelectType));

	public ICommand KeyDownDelete => this._keyDownDelete ?? (this._keyDownDelete = new RelayCommand(Delete_Click));

	public ICommand CheckDieProfileCommand => this._checkDieProfile ?? (this._checkDieProfile = new RelayCommand(CheckDieGeometry));

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

	public ObservableCollection<string> DieGroupNames { get; set; }

	public ObservableCollection<GeometryFileDataViewModel> GeometryFiles { get; set; }

	public ObservableCollection<RadMenuItem> ProfileContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> PartContextMenuItems { get; set; }

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

	public CollectionView DieProfileView
	{
		get
		{
			return this._dieProfileView;
		}
		set
		{
			this._dieProfileView = value;
			base.NotifyPropertyChanged("DieProfileView");
		}
	}

	public CollectionView DiePartView
	{
		get
		{
			return this._diePartView;
		}
		set
		{
			this._diePartView = value;
			base.NotifyPropertyChanged("DiePartView");
		}
	}

	public List<ComboboxEntry<VWidthTypes>> VWidthTypes { get; } = new List<ComboboxEntry<VWidthTypes>>();

	public RelayCommand<object> CmdMirrorGeometry => this._cmdMirrorGeometry ?? (this._cmdMirrorGeometry = new RelayCommand<object>(MirrorGeometry));

	public override void UpdateSelectedItemGeometry()
	{
		this.ProfileSelectionChanged();
	}

	public DiesViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, ITranslator translator, IModelFactory modelFactory, IDieHelperDeprecated dieHelper, IDrawToolProfiles drawToolProfiles)
		: base(globals, mainWindowDataProvider, pnPathService, modelFactory)
	{
		this._configProvider = configProvider;
		this._translator = translator;
		this._dieHelper = dieHelper;
		this._drawToolProfiles = drawToolProfiles;
	}

	public void Init(BendMachine bendMachine, ToolConfigModel toolModel, IToolExpert tools = null)
	{
		this.EditScreenVisible = Visibility.Collapsed;
		base.ToolConfigModel = toolModel;
		base.BendMachine = bendMachine;
		base.Tools = tools;
		this.ClampingType = new ObservableCollection<string>();
		base.ImagePart.Loaded += Image3DOnLoaded;
		foreach (VWidthTypes value in global::System.Enum.GetValues(typeof(VWidthTypes)))
		{
			if (value != 0)
			{
				this.VWidthTypes.Add(new ComboboxEntry<VWidthTypes>(this._translator.Translate("l_enum.VWidthTypes." + value), value));
			}
		}
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		this.InitializeProfileContextMenuItems();
		this.InitializePartContextMenuItems();
		((Screen3D)base.ImagePart).MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		if (base.BendMachine != null)
		{
			this.LoadTools();
			foreach (ClampingSystemProfile clampingSystemProfile in base.BendMachine.ClampingSystem.ClampingSystemProfiles)
			{
				this.ClampingType.Add(clampingSystemProfile.Name);
			}
		}
		this.DieGroupNames = new ObservableCollection<string>(base.ToolConfigModel.DieGroups.Select((DieGroupViewModel g) => g.Name));
		this.DieGroupNames.Insert(0, "*");
		string path = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry;
		this.GeometryFiles = (Directory.Exists(path) ? (from f in Directory.GetFiles(path)
			select new GeometryFileDataViewModel
			{
				Name = new FileInfo(f).Name
			}).ToObservableCollection() : null);
		base.SelectedType = "Dies";
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

	public void LoadTools()
	{
		this.DieProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.DieProfiles);
		this.DieProfileView.Filter = DieProfileFilter;
		this.DieProfileView.Refresh();
		this.DiePartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.DieParts);
		this.DiePartView.Filter = DiePartFilter;
		this.DiePartView.Refresh();
		this.SelectedProfile = ((this.DieProfileView.Count > 0) ? ((DieProfileViewModel)this.DieProfileView.GetItemAt(0)) : null);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		Parallel.ForEach((IEnumerable<DieProfileViewModel>)base.ToolConfigModel.DieProfiles, (Action<DieProfileViewModel>)delegate(DieProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		Parallel.ForEach((IEnumerable<DiePartViewModel>)base.ToolConfigModel.DieParts, (Action<DiePartViewModel>)delegate(DiePartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		this.DieProfileView.Filter = DieProfileFilter;
		this.DieProfileView.Refresh();
		this.DiePartView.Filter = DiePartFilter;
		this.DiePartView.Refresh();
		this.SelectedProfile = ((this.DieProfileView.Count > 0) ? ((DieProfileViewModel)this.DieProfileView.GetItemAt(0)) : null);
		this.SelectedPart = ((this.DiePartView.Count > 0) ? ((DiePartViewModel)this.DiePartView.GetItemAt(0)) : null);
	}

	private void ProfileSelectionChanged()
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile?.Name))
		{
			((Canvas)base.ImageProfile)?.Children.Clear();
			base.NotifyPropertyChanged("ImageProfile");
			this.DiePartView.Filter = DiePartFilter;
			this.DiePartView.Refresh();
			this.SelectType("Profiles");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<DieProfileViewModel>)base.ToolConfigModel.DieProfiles, (Action<DieProfileViewModel>)delegate(DieProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
		{
			this._drawToolProfiles.LoadDiePreview2D(this.SelectedProfile.DieProfile, (Canvas)base.ImageProfile, base.BendMachine);
			base.NotifyPropertyChanged("ImageProfile");
		}
		this.DiePartView.Filter = DiePartFilter;
		this.DiePartView.Refresh();
		int num = ((this.DiePartView.Count <= 0) ? (-1) : 0);
		this.SelectedPart = ((num >= 0 && this.DiePartView.Count > 0 && num < this.DiePartView.Count) ? ((DiePartViewModel)this.DiePartView.GetItemAt(num)) : null);
		this.SelectType("Profiles");
		this.SetEditorEnableRules();
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<DiePartViewModel>)base.ToolConfigModel.DieParts, (Action<DiePartViewModel>)delegate(DiePartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		string text;
		if (!string.IsNullOrEmpty(this.SelectedPart.GeometryFile.Name))
		{
			text = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + this.SelectedPart.GeometryFile.Name;
			if (!File.Exists(text))
			{
				text = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + this.SelectedProfile.GeometryFile.Name;
			}
		}
		else
		{
			text = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + this.SelectedProfile.GeometryFile.Name;
		}
		if (base.ImagePart is Screen3D screen3D)
		{
			screen3D.ShowNavigation(show: false);
			screen3D.SetConfigProviderAndApplySettings(this._configProvider);
		}
		((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
		((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
		global::WiCAM.Pn4000.BendModel.Model model;
		if (text.EndsWith(".c3mo"))
		{
			model = ModelSerializer.Deserialize(text);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
				model.Transform *= Matrix4d.Translation((0.0 - this.SelectedPart.Length) / 2.0, 0.0, this.SelectedProfile.WorkingHeight / 2.0);
			}
		}
		else
		{
			model = CadGeoLoader.LoadCadGeo3D(text, this.SelectedPart.Length, 200.0, toolEdges: true, null, null);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 4.0);
				model.Transform *= Matrix4d.Translation(0.0, 0.0, this.SelectedProfile.WorkingHeight / 2.0);
			}
		}
		if (!string.IsNullOrEmpty(this.SelectedProfile.FrontHemPart?.GeometryFile?.Name))
		{
			string text2 = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + this.SelectedProfile.FrontHemPart.GeometryFile.Name;
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
				HemPartViewModel hemPartViewModel = base.ToolConfigModel.FrontHemParts.Where((HemPartViewModel h) => h.ProfileID == this.SelectedProfile.FrontHemPart.ID).FirstOrDefault((HemPartViewModel h) => h.Length == this.SelectedPart.Length);
				if (hemPartViewModel != null)
				{
					model2 = CadGeoLoader.LoadCadGeo3D(text2, hemPartViewModel.Length, 200.0, toolEdges: true, null, null);
					if (model2 != null)
					{
						double y = 0.0;
						if (this.SelectedProfile.FrontHemPart.HemID == 32)
						{
							Vector3d max = model2.Shells.First().AABBTree.Root.BoundingBox.Max;
							y = model.Shells.First().AABBTree.Root.BoundingBox.Min.Y - max.Y;
						}
						else if (this.SelectedProfile.FrontHemPart.HemID == 33)
						{
							Vector3d min = model2.Shells.First().AABBTree.Root.BoundingBox.Min;
							y = model.Shells.First().AABBTree.Root.BoundingBox.Max.Y - min.Y;
						}
						model2.Transform *= Matrix4d.Translation(0.0, y, 0.0);
					}
				}
			}
			if (model2 != null)
			{
				model.SubModels.Add(model2);
			}
		}
		if (!string.IsNullOrEmpty(this.SelectedProfile.BackHemPart?.GeometryFile?.Name))
		{
			string text3 = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + this.SelectedProfile.BackHemPart.GeometryFile.Name;
			global::WiCAM.Pn4000.BendModel.Model model3 = null;
			if (text3.EndsWith(".c3mo"))
			{
				model3 = ModelSerializer.Deserialize(text3);
				model3.Parent = model;
				if (model != null)
				{
					model3.Transform = Matrix4d.Identity;
				}
			}
			else
			{
				HemPartViewModel hemPartViewModel2 = base.ToolConfigModel.BackHemParts.Where((HemPartViewModel h) => h.ProfileID == this.SelectedProfile.BackHemPart.ID).FirstOrDefault((HemPartViewModel h) => h.Length == this.SelectedPart.Length);
				if (hemPartViewModel2 != null)
				{
					model3 = CadGeoLoader.LoadCadGeo3D(text3, hemPartViewModel2.Length, 200.0, toolEdges: true, null, null);
					if (model3 != null)
					{
						double y2 = 0.0;
						if (this.SelectedProfile.BackHemPart.HemID == 32)
						{
							Vector3d max2 = model3.Shells.First().AABBTree.Root.BoundingBox.Max;
							y2 = model.Shells.First().AABBTree.Root.BoundingBox.Min.Y - max2.Y;
						}
						else if (this.SelectedProfile.BackHemPart.HemID == 33)
						{
							Vector3d min2 = model3.Shells.First().AABBTree.Root.BoundingBox.Min;
							y2 = model.Shells.First().AABBTree.Root.BoundingBox.Max.Y - min2.Y;
						}
						model3.Transform *= Matrix4d.Translation(0.0, y2, 0.0);
					}
				}
			}
			if (model3 != null)
			{
				model.SubModels.Add(model3);
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

	private bool DieProfileFilter(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.DieParts.Any((DiePartViewModel p) => p.DieProfileID == ((DieProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool DiePartFilter(object item)
	{
		if (item == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		if (((DiePartViewModel)item).DieProfileID == this.SelectedProfile.ID)
		{
			if (this.ShowAllParts || ((DiePartViewModel)item).Amount <= 0)
			{
				return this.ShowAllParts;
			}
			return true;
		}
		return false;
	}

	public void AddProfile_Click()
	{
		VWidthTypes vWidthType = base.ToolConfigModel.DieProfiles.FirstOrDefault()?.DieProfile.VWidthType ?? global::WiCAM.Pn4000.Contracts.Tools.VWidthTypes.Undefined;
		int id = ((base.ToolConfigModel.DieProfiles.Count > 0) ? (base.ToolConfigModel.DieProfiles.Max((DieProfileViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.DieProfiles.Add(new DieProfileViewModel(new DieProfile(id, vWidthType), null, null, null, null, null));
		int num = this.DieProfileView.Count - 1;
		this.SelectedProfile = ((num >= 0 && this.DieProfileView.Count > 0 && num < this.DieProfileView.Count) ? ((DieProfileViewModel)this.DieProfileView.GetItemAt(num)) : null);
		this._changed = ChangedConfigType.Dies;
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile != null)
		{
			int id = ((base.ToolConfigModel.DieParts.Count > 0) ? (base.ToolConfigModel.DieParts.Max((DiePartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.DieParts.Add(new DiePartViewModel(new DiePart(id, this.SelectedProfile.ID)));
			int num = this.DiePartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.DiePartView.Count > 0 && num < this.DiePartView.Count) ? ((DiePartViewModel)this.DiePartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.Dies;
		}
	}

	public void CopyPart_Click()
	{
		if (this.SelectedPart != null)
		{
			int iD = ((base.ToolConfigModel.DieParts.Count > 0) ? (base.ToolConfigModel.DieParts.Max((DiePartViewModel p) => p.ID) + 1) : 0);
			DiePart diePart = this.SelectedPart.DiePart.Copy();
			diePart.ID = iD;
			base.ToolConfigModel.DieParts.Add(new DiePartViewModel(diePart));
			int num = this.DiePartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.DiePartView.Count > 0 && num < this.DiePartView.Count) ? ((DiePartViewModel)this.DiePartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.Dies;
		}
	}

	public void CopyProfile_Click()
	{
		if (this.SelectedProfile == null)
		{
			return;
		}
		int iD = ((base.ToolConfigModel.DieProfiles.Count > 0) ? (base.ToolConfigModel.DieProfiles.Max((DieProfileViewModel p) => p.ID) + 1) : 0);
		DieProfile newProfile = this.SelectedProfile.DieProfile.Copy();
		newProfile.ID = iD;
		AdapterProfileViewModel adapter = null;
		if (newProfile.AdapterID >= 0)
		{
			adapter = base.ToolConfigModel.LowerAdapterProfiles.FirstOrDefault((AdapterProfileViewModel a) => a.ID == newProfile.AdapterID);
		}
		HolderProfileViewModel holder = null;
		if (newProfile.HolderID >= 0)
		{
			holder = base.ToolConfigModel.LowerHolderProfiles.FirstOrDefault((HolderProfileViewModel h) => h.ID == newProfile.HolderID);
		}
		HemProfileViewModel frontHemPart = null;
		if (newProfile.FrontHemPartID >= 0)
		{
			frontHemPart = base.ToolConfigModel.FrontHemProfiles?.FirstOrDefault((HemProfileViewModel h) => h.ID == newProfile.FrontHemPartID);
		}
		HemProfileViewModel backHemPart = null;
		if (newProfile.BackHemPartID >= 0)
		{
			backHemPart = base.ToolConfigModel.BackHemProfiles?.FirstOrDefault((HemProfileViewModel h) => h.ID == newProfile.BackHemPartID);
		}
		DieGroupViewModel dieGroup = base.ToolConfigModel.DieGroups.FirstOrDefault((DieGroupViewModel g) => g.ID == newProfile.GroupID);
		base.ToolConfigModel.DieProfiles.Add(new DieProfileViewModel(newProfile, dieGroup, adapter, holder, frontHemPart, backHemPart));
		int num = this.DieProfileView.Count - 1;
		this.SelectedProfile = ((num >= 0 && this.DieProfileView.Count > 0 && num < this.DieProfileView.Count) ? ((DieProfileViewModel)this.DieProfileView.GetItemAt(num)) : null);
		this._changed = ChangedConfigType.Dies;
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "Profiles"))
		{
			if (text == "Parts")
			{
				this.LastSelectedType = typeof(DiePart);
				base.LastSelectedObject = this.SelectedPart;
			}
		}
		else
		{
			this.LastSelectedType = typeof(DieProfile);
			base.LastSelectedObject = this.SelectedProfile;
		}
	}

	public void Delete_Click(object param)
	{
		if (param == null)
		{
			return;
		}
		string text = param.GetType().ToString().Split('.')
			.Last();
		if (!(text == "DieProfileViewModel"))
		{
			if (text == "DiePartViewModel")
			{
				if (!this.SelectedParts.Any())
				{
					return;
				}
				this.DeletePart_Click(param);
			}
		}
		else if (this.SelectedProfiles.Any())
		{
			this.DeleteProfile_Click();
		}
		this._changed = ChangedConfigType.Dies;
	}

	private void CheckDieGeometry()
	{
		string text = this._dieHelper.CheckDieGeometry(this.SelectedProfile.DieProfile, base.BendMachine);
		bool num = text != "";
		text = ($"Checking \"{this.SelectedProfile.DieProfile.Name}\" (Non standard die profiles might not be checked correctly):\n" + text).Replace("\n", "\n\n");
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
		int num = base.ToolConfigModel.DieProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count;
		List<DieProfileViewModel> list = this.SelectedProfiles.Select((object p) => p as DieProfileViewModel).ToList();
		foreach (DieProfileViewModel item in list)
		{
			this.DeletePartsFromProfile(item.ID);
		}
		base.ToolConfigModel.DieProfiles = base.ToolConfigModel.DieProfiles.Except(list).ToObservableCollection();
		if (num >= base.ToolConfigModel.DieProfiles.Count)
		{
			num = base.ToolConfigModel.DieProfiles.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedProfile = null;
		}
		else
		{
			int num2 = this.DieProfileView.Count - 1;
			this.SelectedProfile = ((num2 >= 0 && this.DieProfileView.Count > 0 && num2 < this.DieProfileView.Count) ? ((DieProfileViewModel)this.DieProfileView.GetItemAt(num2)) : null);
		}
		this.DieProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.DieProfiles);
		this.DieProfileView.Filter = DieProfileFilter;
		this.DieProfileView.Refresh();
	}

	private void DeletePart_Click(object param)
	{
		if (param != null)
		{
			int num = base.ToolConfigModel.DieParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count;
			if (num > base.ToolConfigModel.DieParts.Count)
			{
				num = this.DiePartView.IndexOf((DiePartViewModel)param) - 1;
			}
			List<DiePartViewModel> second = this.SelectedParts.Select((object p) => p as DiePartViewModel).ToList();
			base.ToolConfigModel.DieParts = base.ToolConfigModel.DieParts.Except(second).ToObservableCollection();
			this.SetEditorEnableRules();
			if (num < 0 && this.DiePartView.Count > 0)
			{
				num = 0;
			}
			this.SelectedPart = ((num >= 0 && this.DiePartView.Count > 0 && num < this.DiePartView.Count) ? ((DiePartViewModel)this.DiePartView.GetItemAt(num)) : null);
			this.DiePartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.DieParts);
			this.DiePartView.Filter = DiePartFilter;
			this.DiePartView.Refresh();
		}
	}

	private void DeletePartsFromProfile(int profileId)
	{
		base.ToolConfigModel.DieParts.Where((DiePartViewModel p) => p.DieProfileID == profileId).ToList().ForEach(delegate(DiePartViewModel p)
		{
			base.ToolConfigModel.DieParts.Remove(p);
		});
	}

	private void SelectPath_Click(object param)
	{
		if (param != null)
		{
			string text = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry;
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

	public void MirrorGeometry(object param)
	{
		if (!(param is DieProfileViewModel dieProfileViewModel))
		{
			return;
		}
		string path = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + dieProfileViewModel.DieProfile.GeometryFile;
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
			foreach (DiePartViewModel diePart in base.ToolConfigModel.DieParts)
			{
				if (diePart.DieProfileID == dieProfileViewModel.DieProfile.ID && !string.IsNullOrWhiteSpace(diePart.GeometryFile.Name))
				{
					path = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry + diePart.GeometryFile.Name;
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

	public void EditProfile_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.DieProfiles.IndexOf(this.SelectedProfile);
				base.ToolConfigModel.DieProfiles.Remove(this.SelectedProfile);
				base.ToolConfigModel.DieProfiles.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as DieProfileViewModel);
				this.SelectedProfile = base.ToolConfigModel.DieProfiles[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<DieProfileViewModel>)base.ToolConfigModel.DieProfiles).Select((Func<DieProfileViewModel, ToolItemViewModelBase>)((DieProfileViewModel i) => i)), selectedItem: this.SelectedProfile, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
				int index = base.ToolConfigModel.DieParts.IndexOf(this.SelectedPart);
				base.ToolConfigModel.DieParts.Remove(this.SelectedPart);
				base.ToolConfigModel.DieParts.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as DiePartViewModel);
				this.SelectedPart = base.ToolConfigModel.DieParts[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: base.ToolConfigModel.DieParts.Where((DiePartViewModel p) => p.DieProfileID == this.SelectedProfile.ID).Select((Func<DiePartViewModel, ToolItemViewModelBase>)((DiePartViewModel i) => i)), selectedItem: this.SelectedPart, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
		this.EditScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		this.EditScreenVisible = Visibility.Visible;
		this.SetEditorEnableRules();
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
		if (this.LastSelectedType == typeof(DieProfile))
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
		else if (this.LastSelectedType == typeof(DiePart))
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
		base.BendMachine.Dies.DieProfiles = new ObservableCollection<DieProfile>();
		foreach (DieProfileViewModel dieProfile in base.ToolConfigModel.DieProfiles)
		{
			base.BendMachine.Dies.DieProfiles.Add(dieProfile.DieProfile);
		}
		base.BendMachine.Dies.DieParts = new ObservableCollection<DiePart>();
		foreach (DiePartViewModel diePart in base.ToolConfigModel.DieParts)
		{
			base.BendMachine.Dies.DieParts.Add(diePart.DiePart);
		}
		if (base.ToolConfigModel.DieProfiles.Any((DieProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		if (base.ToolConfigModel.DieParts.Any((DiePartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

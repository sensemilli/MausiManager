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
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.ClampingSystem;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using Microsoft.Win32;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class UpperAdapterViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private CollectionView _adapterPartView;

	private CollectionView _adapterProfileView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private AdapterProfileViewModel _selectedProfile;

	private AdapterPartViewModel _selectedPart;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private bool _singleSelected = true;

	private bool _isCopyButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private CollectionView _itemsView;

	private readonly ILogCenterService _logCenterService;

	private ChangedConfigType _changed;

	private bool _showProfilesWithoutParts;

	private bool _showAllParts;

	public Action<ChangedConfigType> DataChanged;

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
			if (this._lastSelectedType == typeof(AdapterPart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(AdapterProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
		}
	}

	public AdapterProfileViewModel SelectedProfile
	{
		get
		{
			return this._selectedProfile;
		}
		set
		{
			this._selectedProfile = value;
			base.NotifyPropertyChanged("SelectedProfile");
			this.ProfileSelectionChanged();
		}
	}

	public IEnumerable<object> SelectedProfiles { get; internal set; } = new ObservableCollection<object>();

	public AdapterPartViewModel SelectedPart
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

	public IEnumerable<object> SelectedParts { get; internal set; } = new ObservableCollection<object>();

	public ObservableCollection<string> ClampingType { get; set; }

	public ICommand SelectPathCommand => this._selectPath ?? (this._selectPath = new RelayCommand(SelectPath_Click));

	public ICommand SelectClick => this._selectClick ?? (this._selectClick = new RelayCommand(SelectType));

	public ICommand KeyDownDelete => this._keyDownDelete ?? (this._keyDownDelete = new RelayCommand(Delete_Click));

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
			this.LoadAdapters();
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
			this.LoadAdapters();
		}
	}

	public CollectionView AdapterProfileView
	{
		get
		{
			return this._adapterProfileView;
		}
		set
		{
			this._adapterProfileView = value;
			base.NotifyPropertyChanged("AdapterProfileView");
		}
	}

	public CollectionView AdapterPartView
	{
		get
		{
			return this._adapterPartView;
		}
		set
		{
			this._adapterPartView = value;
			base.NotifyPropertyChanged("AdapterPartView");
		}
	}

	public UpperAdapterViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory, IDrawToolProfiles drawToolProfiles)
		: base(globals, mainWindowDataProvider, pnPathService, modelFactory)
	{
		this._configProvider = configProvider;
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
		((Screen3D)base.ImagePart).MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		if (base.BendMachine != null)
		{
			this.LoadAdapters();
			foreach (ClampingSystemProfile clampingSystemProfile in base.BendMachine.ClampingSystem.ClampingSystemProfiles)
			{
				this.ClampingType.Add(clampingSystemProfile.Name);
			}
		}
		string path = base.BendMachine.MachinePath + base.BendMachine.UpperToolsGeometry;
		this.GeometryFiles = (Directory.Exists(path) ? (from f in Directory.GetFiles(path)
			select new GeometryFileDataViewModel
			{
				Name = new FileInfo(f).Name
			}).ToObservableCollection() : null);
		base.SelectedType = "Adapters";
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

	public void LoadAdapters()
	{
		this.AdapterProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperAdapterProfiles);
		this.AdapterProfileView.Filter = AdapterProfileFilter;
		this.AdapterProfileView.Refresh();
		this.AdapterPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperAdapterParts);
		this.AdapterPartView.Filter = AdapterPartFilter;
		this.AdapterPartView.Refresh();
		this.SelectedProfile = ((this.AdapterProfileView.Count > 0) ? ((AdapterProfileViewModel)this.AdapterProfileView.GetItemAt(0)) : null);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		Parallel.ForEach((IEnumerable<AdapterProfileViewModel>)base.ToolConfigModel.UpperAdapterProfiles, (Action<AdapterProfileViewModel>)delegate(AdapterProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		Parallel.ForEach((IEnumerable<AdapterPartViewModel>)base.ToolConfigModel.UpperAdapterParts, (Action<AdapterPartViewModel>)delegate(AdapterPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		this.AdapterProfileView.Filter = AdapterProfileFilter;
		this.AdapterProfileView.Refresh();
		this.AdapterPartView.Filter = AdapterPartFilter;
		this.AdapterPartView.Refresh();
		this.SelectedProfile = ((this.AdapterProfileView.Count > 0) ? ((AdapterProfileViewModel)this.AdapterProfileView.GetItemAt(0)) : null);
		this.SelectedPart = ((this.AdapterPartView.Count > 0) ? ((AdapterPartViewModel)this.AdapterPartView.GetItemAt(0)) : null);
	}

	private void ProfileSelectionChanged()
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			if (base.ImageProfile != null)
			{
				((Canvas)base.ImageProfile).Children.Clear();
				base.NotifyPropertyChanged("ImageProfile");
			}
			this.AdapterPartView.Filter = AdapterPartFilter;
			this.AdapterPartView.Refresh();
			this.SelectType("Profiles");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<AdapterProfileViewModel>)base.ToolConfigModel.UpperAdapterProfiles, (Action<AdapterProfileViewModel>)delegate(AdapterProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
		{
			this._drawToolProfiles.LoadAdapterPreview2D(this.SelectedProfile.AdapterProfile, (Canvas)base.ImageProfile, base.BendMachine);
			base.NotifyPropertyChanged("ImageProfile");
		}
		this.AdapterPartView.Filter = AdapterPartFilter;
		this.AdapterPartView.Refresh();
		int num = ((this.AdapterPartView.Count <= 0) ? (-1) : 0);
		this.SelectedPart = ((num >= 0 && this.AdapterPartView.Count > 0 && num < this.AdapterPartView.Count) ? ((AdapterPartViewModel)this.AdapterPartView.GetItemAt(num)) : null);
		this.SelectType("Profiles");
		this.SetEditorEnableRules();
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedPart == null || this.SelectedPart == null || string.IsNullOrEmpty(this.SelectedProfile?.GeometryFile?.Name))
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<AdapterPartViewModel>)base.ToolConfigModel.UpperAdapterParts, (Action<AdapterPartViewModel>)delegate(AdapterPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		if (this.SelectedPart == null || this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		string text;
		if (!string.IsNullOrEmpty(this.SelectedPart.GeometryFile.Name))
		{
			text = base.BendMachine.MachinePath + base.BendMachine.AdapterGeometry + "\\" + this.SelectedPart.GeometryFile.Name;
			if (!File.Exists(text))
			{
				text = base.BendMachine.MachinePath + base.BendMachine.AdapterGeometry + "\\" + this.SelectedProfile.GeometryFile.Name;
			}
		}
		else
		{
			text = base.BendMachine.MachinePath + base.BendMachine.AdapterGeometry + "\\" + this.SelectedProfile.GeometryFile.Name;
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

	private bool AdapterProfileFilter(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.UpperAdapterParts.Any((AdapterPartViewModel p) => p.AdapterProfileID == ((AdapterProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool AdapterPartFilter(object item)
	{
		if (item == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		if (((AdapterPartViewModel)item).AdapterProfileID == this.SelectedProfile.ID)
		{
			if (this.ShowAllParts || ((AdapterPartViewModel)item).Amount <= 0)
			{
				return this.ShowAllParts;
			}
			return true;
		}
		return false;
	}

	public void AddProfile_Click()
	{
		int id = ((base.ToolConfigModel.UpperAdapterProfiles.Count > 0) ? (base.ToolConfigModel.UpperAdapterProfiles.Max((AdapterProfileViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.UpperAdapterProfiles.Add(new AdapterProfileViewModel(new AdapterProfile(id)));
		this.SelectedProfile = base.ToolConfigModel.UpperAdapterProfiles.LastOrDefault();
		this._changed = ChangedConfigType.UpperAdapter;
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile != null)
		{
			int id = ((base.ToolConfigModel.UpperAdapterParts.Count > 0) ? (base.ToolConfigModel.UpperAdapterParts.Max((AdapterPartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.UpperAdapterParts.Add(new AdapterPartViewModel(new AdapterPart(id, this.SelectedProfile.ID)));
			int num = this.AdapterPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.AdapterPartView.Count > 0 && num < this.AdapterPartView.Count) ? ((AdapterPartViewModel)this.AdapterPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperAdapter;
		}
	}

	public void CopyPart_Click()
	{
		if (this.SelectedPart != null)
		{
			int iD = ((base.ToolConfigModel.UpperAdapterParts.Count > 0) ? (base.ToolConfigModel.UpperAdapterParts.Max((AdapterPartViewModel p) => p.ID) + 1) : 0);
			AdapterPart adapterPart = this.SelectedPart.AdapterPart.Copy();
			adapterPart.ID = iD;
			base.ToolConfigModel.UpperAdapterParts.Add(new AdapterPartViewModel(adapterPart));
			int num = this.AdapterPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.AdapterPartView.Count > 0 && num < this.AdapterPartView.Count) ? ((AdapterPartViewModel)this.AdapterPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperAdapter;
		}
	}

	public void CopyProfile_Click()
	{
		if (this.SelectedProfile != null)
		{
			int iD = ((base.ToolConfigModel.DieProfiles.Count > 0) ? (base.ToolConfigModel.DieProfiles.Max((DieProfileViewModel p) => p.ID) + 1) : 0);
			AdapterProfile adapterProfile = this.SelectedProfile.AdapterProfile.Copy();
			adapterProfile.ID = iD;
			base.ToolConfigModel.UpperAdapterProfiles.Add(new AdapterProfileViewModel(adapterProfile));
			int num = this.AdapterProfileView.Count - 1;
			this.SelectedProfile = ((num >= 0 && this.AdapterProfileView.Count > 0 && num < this.AdapterProfileView.Count) ? ((AdapterProfileViewModel)this.AdapterProfileView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperAdapter;
		}
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "Profiles"))
		{
			if (text == "Parts")
			{
				this.LastSelectedType = typeof(AdapterPart);
				base.LastSelectedObject = this.SelectedPart;
			}
		}
		else
		{
			this.LastSelectedType = typeof(AdapterProfile);
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
		if (!(text == "AdapterProfileViewModel"))
		{
			if (text == "AdapterPartViewModel")
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
		this._changed = ChangedConfigType.UpperAdapter;
	}

	private void DeleteProfile_Click()
	{
		int num = base.ToolConfigModel.UpperAdapterProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count();
		List<AdapterProfileViewModel> list = this.SelectedProfiles.Select((object p) => p as AdapterProfileViewModel).ToList();
		foreach (AdapterProfileViewModel item in list)
		{
			this.DeletePartsFromProfile(item.ID);
		}
		base.ToolConfigModel.UpperAdapterProfiles = base.ToolConfigModel.UpperAdapterProfiles.Except(list).ToObservableCollection();
		if (num >= base.ToolConfigModel.UpperAdapterProfiles.Count)
		{
			num = base.ToolConfigModel.UpperAdapterProfiles.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedProfile = null;
			return;
		}
		int num2 = this.AdapterProfileView.Count - 1;
		this.SelectedProfile = ((num2 >= 0 && this.AdapterProfileView.Count > 0 && num2 < this.AdapterProfileView.Count) ? ((AdapterProfileViewModel)this.AdapterProfileView.GetItemAt(num2)) : null);
	}

	private void DeletePart_Click(object param)
	{
		int num = base.ToolConfigModel.UpperAdapterParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count();
		if (num > base.ToolConfigModel.UpperAdapterParts.Count)
		{
			num = this.AdapterPartView.IndexOf((AdapterPartViewModel)param) - 1;
		}
		List<AdapterPartViewModel> second = this.SelectedParts.Select((object p) => p as AdapterPartViewModel).ToList();
		base.ToolConfigModel.UpperAdapterParts = base.ToolConfigModel.UpperAdapterParts.Except(second).ToObservableCollection();
		this.SetEditorEnableRules();
		if (num < 0 && this.AdapterPartView.Count > 0)
		{
			num = 0;
		}
		int num2 = this.AdapterPartView.Count - 1;
		this.SelectedPart = ((num2 >= 0 && this.AdapterPartView.Count > 0 && num2 < this.AdapterPartView.Count) ? ((AdapterPartViewModel)this.AdapterPartView.GetItemAt(num2)) : null);
		this.AdapterPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperAdapterParts);
		this.AdapterPartView.Filter = AdapterPartFilter;
		this.AdapterPartView.Refresh();
	}

	public override void UpdateSelectedItemGeometry()
	{
		this.ProfileSelectionChanged();
	}

	private void DeletePartsFromProfile(int profileId)
	{
		base.ToolConfigModel.UpperAdapterParts.Where((AdapterPartViewModel p) => p.AdapterProfileID == profileId).ToList().ForEach(delegate(AdapterPartViewModel p)
		{
			base.ToolConfigModel.UpperAdapterParts.Remove(p);
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

	public void EditProfile_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.UpperAdapterProfiles.IndexOf(this.SelectedProfile);
				base.ToolConfigModel.UpperAdapterProfiles.Remove(this.SelectedProfile);
				base.ToolConfigModel.UpperAdapterProfiles.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as AdapterProfileViewModel);
				this.SelectedProfile = base.ToolConfigModel.UpperAdapterProfiles[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<AdapterProfileViewModel>)base.ToolConfigModel.UpperAdapterProfiles).Select((Func<AdapterProfileViewModel, ToolItemViewModelBase>)((AdapterProfileViewModel i) => i)), selectedItem: this.SelectedProfile, isUpper: true, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
				int index = base.ToolConfigModel.UpperAdapterParts.IndexOf(this.SelectedPart);
				base.ToolConfigModel.UpperAdapterParts.Remove(this.SelectedPart);
				base.ToolConfigModel.UpperAdapterParts.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as AdapterPartViewModel);
				this.SelectedPart = base.ToolConfigModel.UpperAdapterParts[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: base.ToolConfigModel.UpperAdapterParts.Where((AdapterPartViewModel p) => p.AdapterProfileID == this.SelectedProfile.ID).Select((Func<AdapterPartViewModel, ToolItemViewModelBase>)((AdapterPartViewModel i) => i)), selectedItem: this.SelectedPart, isUpper: true, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
		if (this.LastSelectedType == typeof(AdapterProfile))
		{
			this.SingleSelected = this.SelectedProfiles.Count() < 2;
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
			this.IsEditButtonEnabled = this.SelectedPart != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedProfile != null;
		}
		else if (this.LastSelectedType == typeof(AdapterPart))
		{
			this.SingleSelected = this.SelectedParts.Count() < 2;
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
			this.IsEditButtonEnabled = this.SelectedPart != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedProfile != null;
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
		base.BendMachine.Adapter.UpperAdapterProfiles = new ObservableCollection<AdapterProfile>();
		foreach (AdapterProfileViewModel upperAdapterProfile in base.ToolConfigModel.UpperAdapterProfiles)
		{
			base.BendMachine.Adapter.UpperAdapterProfiles.Add(upperAdapterProfile.AdapterProfile);
		}
		base.BendMachine.Adapter.UpperAdapterParts = new ObservableCollection<AdapterPart>();
		foreach (AdapterPartViewModel upperAdapterPart in base.ToolConfigModel.UpperAdapterParts)
		{
			base.BendMachine.Adapter.UpperAdapterParts.Add(upperAdapterPart.AdapterPart);
		}
		if (base.ToolConfigModel.UpperAdapterProfiles.Any((AdapterProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.UpperAdapter;
		}
		if (base.ToolConfigModel.UpperAdapterParts.Any((AdapterPartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.UpperAdapter;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

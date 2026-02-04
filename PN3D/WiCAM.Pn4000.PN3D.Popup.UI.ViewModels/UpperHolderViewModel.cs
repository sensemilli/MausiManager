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
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.ClampingSystem;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
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

public class UpperHolderViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private CollectionView _holderPartView;

	private CollectionView _holderProfileView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private HolderProfileViewModel _selectedProfile;

	private HolderPartViewModel _selectedPart;

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
			if (this._lastSelectedType == typeof(HolderPart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(HolderProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
		}
	}

	public HolderProfileViewModel SelectedProfile
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

	public HolderPartViewModel SelectedPart
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
			this.LoadHolders();
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
			this.LoadHolders();
		}
	}

	public CollectionView HolderProfileView
	{
		get
		{
			return this._holderProfileView;
		}
		set
		{
			this._holderProfileView = value;
			base.NotifyPropertyChanged("HolderProfileView");
		}
	}

	public CollectionView HolderPartView
	{
		get
		{
			return this._holderPartView;
		}
		set
		{
			this._holderPartView = value;
			base.NotifyPropertyChanged("HolderPartView");
		}
	}

	public UpperHolderViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory, IDrawToolProfiles drawToolProfiles)
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
			this.LoadHolders();
			foreach (ClampingSystemProfile clampingSystemProfile in base.BendMachine.ClampingSystem.ClampingSystemProfiles)
			{
				this.ClampingType.Add(clampingSystemProfile.Name);
			}
		}
		string path = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry;
		this.GeometryFiles = (Directory.Exists(path) ? (from f in Directory.GetFiles(path)
			select new GeometryFileDataViewModel
			{
				Name = new FileInfo(f).Name
			}).ToObservableCollection() : null);
		base.SelectedType = "UpperHolders";
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

	public void LoadHolders()
	{
		this.HolderProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperHolderProfiles);
		this.HolderProfileView.Filter = HolderProfileFilter;
		this.HolderProfileView.Refresh();
		this.HolderPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperHolderParts);
		this.HolderPartView.Filter = HolderPartFilter;
		this.HolderPartView.Refresh();
		this.SelectedProfile = ((this.HolderProfileView.Count > 0) ? ((HolderProfileViewModel)this.HolderProfileView.GetItemAt(0)) : null);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		Parallel.ForEach((IEnumerable<HolderProfileViewModel>)base.ToolConfigModel.UpperHolderProfiles, (Action<HolderProfileViewModel>)delegate(HolderProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		Parallel.ForEach((IEnumerable<HolderPartViewModel>)base.ToolConfigModel.UpperHolderParts, (Action<HolderPartViewModel>)delegate(HolderPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		this.HolderProfileView.Filter = HolderProfileFilter;
		this.HolderProfileView.Refresh();
		this.HolderPartView.Filter = HolderPartFilter;
		this.HolderPartView.Refresh();
		this.SelectedProfile = ((this.HolderProfileView.Count > 0) ? ((HolderProfileViewModel)this.HolderProfileView.GetItemAt(0)) : null);
		this.SelectedPart = ((this.HolderPartView.Count > 0) ? ((HolderPartViewModel)this.HolderPartView.GetItemAt(0)) : null);
	}

	private void ProfileSelectionChanged()
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Canvas)base.ImageProfile).Children.Clear();
			base.NotifyPropertyChanged("ImageProfile");
			this.HolderPartView.Filter = HolderPartFilter;
			this.HolderPartView.Refresh();
			this.SelectType("Profiles");
			this.SetEditorEnableRules();
			return;
		}
		Parallel.ForEach((IEnumerable<HolderProfileViewModel>)base.ToolConfigModel.UpperHolderProfiles, (Action<HolderProfileViewModel>)delegate(HolderProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
		{
			this._drawToolProfiles.LoadHolderPreview2D(this.SelectedProfile.HolderProfile, (Canvas)base.ImageProfile, base.BendMachine);
			base.NotifyPropertyChanged("ImageProfile");
		}
		this.HolderPartView.Filter = HolderPartFilter;
		this.HolderPartView.Refresh();
		int num = ((this.HolderPartView.Count <= 0) ? (-1) : 0);
		this.SelectedPart = ((num >= 0 && this.HolderPartView.Count > 0 && num < this.HolderPartView.Count) ? ((HolderPartViewModel)this.HolderPartView.GetItemAt(num)) : null);
		this.SelectType("Profiles");
		this.SetEditorEnableRules();
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedPart != null || this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SetEditorEnableRules();
			this.SelectType("Parts");
			return;
		}
		Parallel.ForEach((IEnumerable<HolderPartViewModel>)base.ToolConfigModel.UpperHolderParts, (Action<HolderPartViewModel>)delegate(HolderPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		if (this.SelectedPart != null || this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
			((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			base.NotifyPropertyChanged("ImagePart");
			this.SetEditorEnableRules();
			this.SelectType("Parts");
			return;
		}
		string text;
		if (!string.IsNullOrEmpty(this.SelectedPart.GeometryFile.Name))
		{
			text = base.BendMachine.MachinePath + base.BendMachine.HolderGeometry + "\\" + this.SelectedPart.GeometryFile.Name;
			if (!File.Exists(text))
			{
				text = base.BendMachine.MachinePath + base.BendMachine.HolderGeometry + "\\" + this.SelectedProfile.GeometryFile.Name;
			}
		}
		else
		{
			text = base.BendMachine.MachinePath + base.BendMachine.HolderGeometry + "\\" + this.SelectedProfile.GeometryFile.Name;
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

	private bool HolderProfileFilter(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.UpperHolderParts.Any((HolderPartViewModel p) => p.HolderProfileID == ((HolderProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool HolderPartFilter(object item)
	{
		if (item == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		if (((HolderPartViewModel)item).HolderProfileID == this.SelectedProfile.ID)
		{
			if (this.ShowAllParts || ((HolderPartViewModel)item).Amount <= 0)
			{
				return this.ShowAllParts;
			}
			return true;
		}
		return false;
	}

	public void AddProfile_Click()
	{
		int id = ((base.ToolConfigModel.UpperHolderProfiles.Count > 0) ? (base.ToolConfigModel.UpperHolderProfiles.Max((HolderProfileViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.UpperHolderProfiles.Add(new HolderProfileViewModel(new HolderProfile(id)));
		int num = this.HolderProfileView.Count - 1;
		this.SelectedProfile = ((num >= 0 && this.HolderProfileView.Count > 0 && num < this.HolderProfileView.Count) ? ((HolderProfileViewModel)this.HolderProfileView.GetItemAt(num)) : null);
		this._changed = ChangedConfigType.UpperHolder;
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile != null)
		{
			int id = ((base.ToolConfigModel.UpperHolderParts.Count > 0) ? (base.ToolConfigModel.UpperHolderParts.Max((HolderPartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.UpperHolderParts.Add(new HolderPartViewModel(new HolderPart(id, this.SelectedProfile.ID)));
			int num = this.HolderPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.HolderPartView.Count > 0 && num < this.HolderPartView.Count) ? ((HolderPartViewModel)this.HolderPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperHolder;
		}
	}

	public void CopyPart_Click()
	{
		if (this.SelectedPart != null)
		{
			int iD = ((base.ToolConfigModel.UpperHolderParts.Count > 0) ? (base.ToolConfigModel.UpperHolderParts.Max((HolderPartViewModel p) => p.ID) + 1) : 0);
			HolderPart holderPart = this.SelectedPart.HolderPart.Copy();
			holderPart.ID = iD;
			base.ToolConfigModel.UpperHolderParts.Add(new HolderPartViewModel(holderPart));
			int num = this.HolderPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.HolderPartView.Count > 0 && num < this.HolderPartView.Count) ? ((HolderPartViewModel)this.HolderPartView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperHolder;
		}
	}

	public void CopyProfile_Click()
	{
		if (this.SelectedProfile != null)
		{
			int iD = ((base.ToolConfigModel.DieProfiles.Count > 0) ? (base.ToolConfigModel.DieProfiles.Max((DieProfileViewModel p) => p.ID) + 1) : 0);
			HolderProfile holderProfile = this.SelectedProfile.HolderProfile.Copy();
			holderProfile.ID = iD;
			base.ToolConfigModel.UpperHolderProfiles.Add(new HolderProfileViewModel(holderProfile));
			int num = this.HolderProfileView.Count - 1;
			this.SelectedProfile = ((num >= 0 && this.HolderProfileView.Count > 0 && num < this.HolderProfileView.Count) ? ((HolderProfileViewModel)this.HolderProfileView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.UpperHolder;
		}
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "Profiles"))
		{
			if (text == "Parts")
			{
				this.LastSelectedType = typeof(HolderPart);
				base.LastSelectedObject = this.SelectedPart;
			}
		}
		else
		{
			this.LastSelectedType = typeof(HolderProfile);
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
		if (!(text == "HolderProfileViewModel"))
		{
			if (text == "HolderPartViewModel")
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
		this._changed = ChangedConfigType.UpperHolder;
	}

	private void DeleteProfile_Click()
	{
		int num = base.ToolConfigModel.UpperHolderProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count();
		List<HolderProfileViewModel> list = this.SelectedProfiles.Select((object p) => p as HolderProfileViewModel).ToList();
		foreach (HolderProfileViewModel item in list)
		{
			this.DeletePartsFromProfile(item.ID);
		}
		base.ToolConfigModel.UpperHolderProfiles = base.ToolConfigModel.UpperHolderProfiles.Except(list).ToObservableCollection();
		if (num >= base.ToolConfigModel.UpperHolderProfiles.Count)
		{
			num = base.ToolConfigModel.UpperHolderProfiles.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedProfile = null;
			return;
		}
		int num2 = this.HolderProfileView.Count - 1;
		this.SelectedProfile = ((num2 >= 0 && this.HolderProfileView.Count > 0 && num2 < this.HolderProfileView.Count) ? ((HolderProfileViewModel)this.HolderProfileView.GetItemAt(num2)) : null);
	}

	private void DeletePart_Click(object param)
	{
		int num = base.ToolConfigModel.UpperHolderParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count();
		if (num > base.ToolConfigModel.UpperHolderParts.Count)
		{
			num = this.HolderPartView.IndexOf((HolderPartViewModel)param) - 1;
		}
		List<HolderPartViewModel> second = this.SelectedParts.Select((object p) => p as HolderPartViewModel).ToList();
		base.ToolConfigModel.UpperHolderParts = base.ToolConfigModel.UpperHolderParts.Except(second).ToObservableCollection();
		this.SetEditorEnableRules();
		if (num < 0 && this.HolderPartView.Count > 0)
		{
			num = 0;
		}
		int num2 = this.HolderPartView.Count - 1;
		this.SelectedPart = ((num2 >= 0 && this.HolderPartView.Count > 0 && num2 < this.HolderPartView.Count) ? ((HolderPartViewModel)this.HolderPartView.GetItemAt(num2)) : null);
		this.HolderPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.UpperHolderParts);
		this.HolderPartView.Filter = HolderPartFilter;
		this.HolderPartView.Refresh();
	}

	private void DeletePartsFromProfile(int profileId)
	{
		base.ToolConfigModel.UpperHolderParts.Where((HolderPartViewModel p) => p.HolderProfileID == profileId).ToList().ForEach(delegate(HolderPartViewModel p)
		{
			base.ToolConfigModel.UpperHolderParts.Remove(p);
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
				int index = base.ToolConfigModel.UpperHolderProfiles.IndexOf(this.SelectedProfile);
				base.ToolConfigModel.UpperHolderProfiles.Remove(this.SelectedProfile);
				base.ToolConfigModel.UpperHolderProfiles.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HolderProfileViewModel);
				this.SelectedProfile = base.ToolConfigModel.UpperHolderProfiles[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<HolderProfileViewModel>)base.ToolConfigModel.UpperHolderProfiles).Select((Func<HolderProfileViewModel, ToolItemViewModelBase>)((HolderProfileViewModel i) => i)), selectedItem: this.SelectedProfile, isUpper: true, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
				int index = base.ToolConfigModel.UpperHolderParts.IndexOf(this.SelectedPart);
				base.ToolConfigModel.UpperHolderParts.Remove(this.SelectedPart);
				base.ToolConfigModel.UpperHolderParts.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HolderPartViewModel);
				this.SelectedPart = base.ToolConfigModel.UpperHolderParts[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: base.ToolConfigModel.UpperHolderParts.Where((HolderPartViewModel p) => p.HolderProfileID == this.SelectedProfile.ID).Select((Func<HolderPartViewModel, ToolItemViewModelBase>)((HolderPartViewModel i) => i)), selectedItem: this.SelectedPart, isUpper: true, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
		if (this.LastSelectedType == typeof(HolderProfile))
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
		else if (this.LastSelectedType == typeof(HolderPart))
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
		base.BendMachine.Holder.UpperHolderProfiles = new ObservableCollection<HolderProfile>();
		foreach (HolderProfileViewModel upperHolderProfile in base.ToolConfigModel.UpperHolderProfiles)
		{
			base.BendMachine.Holder.UpperHolderProfiles.Add(upperHolderProfile.HolderProfile);
		}
		base.BendMachine.Holder.UpperHolderParts = new ObservableCollection<HolderPart>();
		foreach (HolderPartViewModel upperHolderPart in base.ToolConfigModel.UpperHolderParts)
		{
			base.BendMachine.Holder.UpperHolderParts.Add(upperHolderPart.HolderPart);
		}
		if (base.ToolConfigModel.UpperHolderProfiles.Any((HolderProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.UpperHolder;
		}
		if (base.ToolConfigModel.UpperHolderParts.Any((HolderPartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.UpperHolder;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

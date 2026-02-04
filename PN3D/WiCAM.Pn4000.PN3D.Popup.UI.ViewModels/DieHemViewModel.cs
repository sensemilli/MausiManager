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
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
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

public class DieHemViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private TabItem _actualProfileTab;

	private TabItem _actualPartTab;

	private int _selectedPartTabId;

	private CollectionView _frontHemProfileView;

	private CollectionView _backHemProfileView;

	private CollectionView _frontHemPartView;

	private CollectionView _backHemPartView;

	private ICommand _selectPath;

	private ICommand _selectClick;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _profileBorderBrush;

	private Brush _partsBorderBrush;

	private HemProfileViewModel _selectedProfile;

	private HemPartViewModel _selectedPart;

	private readonly IDrawToolProfiles _drawToolProfiles;

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

	public TabItem ActualProfileTab
	{
		get
		{
			return this._actualProfileTab;
		}
		set
		{
			this._actualProfileTab = value;
			if (this.ActualProfileTab.Name == "FrontHemProfiles")
			{
				this.SelectedPartTabId = 0;
				this.SelectedProfile = base.ToolConfigModel.FrontHemProfiles.FirstOrDefault();
			}
			else
			{
				this.SelectedPartTabId = 1;
				this.SelectedProfile = base.ToolConfigModel.BackHemProfiles.FirstOrDefault();
			}
			base.NotifyPropertyChanged("ActualProfileTab");
		}
	}

	public int SelectedPartTabId
	{
		get
		{
			return this._selectedPartTabId;
		}
		set
		{
			this._selectedPartTabId = value;
			base.NotifyPropertyChanged("SelectedPartTabId");
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
			if (this._lastSelectedType == typeof(HemPart))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.PartsBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPart;
			}
			else if (this._lastSelectedType == typeof(HemProfile))
			{
				this.ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.PartsBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedProfile;
			}
		}
	}

	public HemProfileViewModel SelectedProfile
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

	public ObservableCollection<object> SelectedProfiles { get; internal set; } = new ObservableCollection<object>();

	public HemPartViewModel SelectedPart
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

	public ObservableCollection<RadMenuItem> FrontProfileContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> BackProfileContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> FrontPartContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> BackPartContextMenuItems { get; set; }

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

	public CollectionView BackHemProfileView
	{
		get
		{
			return this._backHemProfileView;
		}
		set
		{
			this._backHemProfileView = value;
			base.NotifyPropertyChanged("BackHemProfileView");
		}
	}

	public CollectionView FrontHemProfileView
	{
		get
		{
			return this._frontHemProfileView;
		}
		set
		{
			this._frontHemProfileView = value;
			base.NotifyPropertyChanged("FrontHemProfileView");
		}
	}

	public CollectionView BackHemPartView
	{
		get
		{
			return this._backHemPartView;
		}
		set
		{
			this._backHemPartView = value;
			base.NotifyPropertyChanged("BackHemPartView");
		}
	}

	public CollectionView FrontHemPartView
	{
		get
		{
			return this._frontHemPartView;
		}
		set
		{
			this._frontHemPartView = value;
			base.NotifyPropertyChanged("FrontHemPartView");
		}
	}

	public DieHemViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory, IDrawToolProfiles drawToolProfiles)
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
		base.ImagePart.Loaded += Image3DOnLoaded;
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		this.InitializeFrontProfileContextMenuItems();
		this.InitializeBackProfileContextMenuItems();
		this.InitializeFrontPartContextMenuItems();
		this.InitializeBackPartContextMenuItems();
		((Screen3D)base.ImagePart).MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		if (base.BendMachine != null)
		{
			this.LoadTools();
		}
		string path = base.BendMachine.MachinePath + base.BendMachine.LowerToolsGeometry;
		this.GeometryFiles = (Directory.Exists(path) ? (from f in Directory.GetFiles(path)
			select new GeometryFileDataViewModel
			{
				Name = new FileInfo(f).Name
			}).ToObservableCollection() : null);
		base.SelectedType = "Hems";
	}

	private void InitializeFrontProfileContextMenuItems()
	{
		this.FrontProfileContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyProfile_Click)
		};
		this.FrontProfileContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditProfile_Click)
		};
		this.FrontProfileContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteProfile_Click)
		};
		this.FrontProfileContextMenuItems.Add(item3);
	}

	private void InitializeBackProfileContextMenuItems()
	{
		this.BackProfileContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyProfile_Click)
		};
		this.BackProfileContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditProfile_Click)
		};
		this.BackProfileContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteProfile_Click)
		};
		this.BackProfileContextMenuItems.Add(item3);
	}

	private void InitializeFrontPartContextMenuItems()
	{
		this.FrontPartContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyPart_Click)
		};
		this.FrontPartContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditPart_Click)
		};
		this.FrontPartContextMenuItems.Add(item2);
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
		this.FrontPartContextMenuItems.Add(item3);
	}

	private void InitializeBackPartContextMenuItems()
	{
		this.BackPartContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyPart_Click)
		};
		this.BackPartContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditPart_Click)
		};
		this.BackPartContextMenuItems.Add(item2);
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
		this.BackPartContextMenuItems.Add(item3);
	}

	public void LoadTools()
	{
		this.FrontHemProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.FrontHemProfiles);
		this.FrontHemProfileView.Filter = HemProfileFilterFront;
		this.FrontHemProfileView.Refresh();
		this.BackHemProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.BackHemProfiles);
		this.BackHemProfileView.Filter = HemProfileFilterBack;
		this.BackHemProfileView.Refresh();
		this.FrontHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.FrontHemParts);
		this.FrontHemPartView.Filter = HemPartFilter;
		this.FrontHemPartView.Refresh();
		this.BackHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.BackHemParts);
		this.BackHemPartView.Filter = HemPartFilter;
		this.BackHemPartView.Refresh();
		if (this.ActualProfileTab != null)
		{
			if (this.ActualProfileTab.Name == "FrontHemProfiles")
			{
				this.SelectedProfile = ((this.FrontHemProfileView.Count > 0) ? ((HemProfileViewModel)this.FrontHemProfileView.GetItemAt(0)) : null);
			}
			else
			{
				this.SelectedProfile = ((this.BackHemProfileView.Count > 0) ? ((HemProfileViewModel)this.BackHemProfileView.GetItemAt(0)) : null);
			}
		}
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.ActualProfileTab == null)
		{
			return;
		}
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			Parallel.ForEach((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.FrontHemProfiles, (Action<HemProfileViewModel>)delegate(HemProfileViewModel item)
			{
				if (item != this.SelectedProfile)
				{
					item.IsSelected = false;
				}
			});
			Parallel.ForEach((IEnumerable<HemPartViewModel>)base.ToolConfigModel.FrontHemParts, (Action<HemPartViewModel>)delegate(HemPartViewModel item)
			{
				if (item != this.SelectedPart)
				{
					item.IsSelected = false;
				}
			});
			this.FrontHemProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.FrontHemProfiles);
			this.FrontHemProfileView.Filter = HemProfileFilterFront;
			this.FrontHemProfileView.Refresh();
			this.FrontHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.FrontHemParts);
			this.FrontHemPartView.Filter = HemPartFilter;
			this.FrontHemPartView.Refresh();
			this.SelectedProfile = ((this.FrontHemProfileView.Count > 0) ? ((HemProfileViewModel)this.FrontHemProfileView.GetItemAt(0)) : null);
			return;
		}
		Parallel.ForEach((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.BackHemProfiles, (Action<HemProfileViewModel>)delegate(HemProfileViewModel item)
		{
			if (item != this.SelectedProfile)
			{
				item.IsSelected = false;
			}
		});
		Parallel.ForEach((IEnumerable<HemPartViewModel>)base.ToolConfigModel.BackHemParts, (Action<HemPartViewModel>)delegate(HemPartViewModel item)
		{
			if (item != this.SelectedPart)
			{
				item.IsSelected = false;
			}
		});
		this.BackHemProfileView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.BackHemProfiles);
		this.BackHemProfileView.Filter = HemProfileFilterBack;
		this.BackHemProfileView.Refresh();
		this.BackHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.BackHemParts);
		this.BackHemPartView.Filter = HemPartFilter;
		this.BackHemPartView.Refresh();
		this.SelectedProfile = ((this.BackHemProfileView.Count > 0) ? ((HemProfileViewModel)this.BackHemProfileView.GetItemAt(0)) : null);
	}

	private void ProfileSelectionChanged()
	{
		if (this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile?.Name))
		{
			if (base.ImageProfile != null)
			{
				((Canvas)base.ImageProfile).Children.Clear();
			}
			base.NotifyPropertyChanged("ImageProfile");
			if (this.ActualProfileTab.Name == "FrontHemProfiles")
			{
				this.FrontHemPartView.Filter = HemPartFilter;
				this.FrontHemPartView.Refresh();
			}
			else
			{
				this.BackHemPartView.Filter = HemPartFilter;
				this.BackHemPartView.Refresh();
			}
			this.SelectType("Profiles");
			this.SetEditorEnableRules();
			return;
		}
		if (this.ActualProfileTab != null && this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			Parallel.ForEach((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.FrontHemProfiles, (Action<HemProfileViewModel>)delegate(HemProfileViewModel item)
			{
				if (item != this.SelectedProfile)
				{
					item.IsSelected = false;
				}
			});
			if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
			{
				this._drawToolProfiles.LoadDiePreview2D(this.SelectedProfile.HemProfile, (Canvas)base.ImageProfile, base.BendMachine);
				base.NotifyPropertyChanged("ImageProfile");
			}
			this.FrontHemPartView.Filter = HemPartFilter;
			this.FrontHemPartView.Refresh();
			int num = ((this.FrontHemPartView.Count <= 0) ? (-1) : 0);
			this.SelectedPart = ((num >= 0 && this.FrontHemPartView.Count > 0 && num < this.FrontHemPartView.Count) ? ((HemPartViewModel)this.FrontHemPartView.GetItemAt(num)) : null);
		}
		else
		{
			Parallel.ForEach((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.BackHemProfiles, (Action<HemProfileViewModel>)delegate(HemProfileViewModel item)
			{
				if (item != this.SelectedProfile)
				{
					item.IsSelected = false;
				}
			});
			if (!this.SelectedProfile.GeometryFile.Name.EndsWith(".n3d"))
			{
				this._drawToolProfiles.LoadDiePreview2D(this.SelectedProfile.HemProfile, (Canvas)base.ImageProfile, base.BendMachine);
				base.NotifyPropertyChanged("ImageProfile");
			}
			this.BackHemPartView.Filter = HemPartFilter;
			this.BackHemPartView.Refresh();
			int num2 = ((this.BackHemPartView.Count <= 0) ? (-1) : 0);
			this.SelectedPart = ((num2 >= 0 && this.BackHemPartView.Count > 0 && num2 < this.BackHemPartView.Count) ? ((HemPartViewModel)this.BackHemPartView.GetItemAt(num2)) : null);
		}
		this.SelectType("Profiles");
		this.SetEditorEnableRules();
	}

	private void PartsSelectionChanged()
	{
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
		{
			if (base.ImagePart != null)
			{
				((Screen3D)base.ImagePart).ScreenD3D?.RemoveModel(null);
				((Screen3D)base.ImagePart).ScreenD3D?.RemoveBillboard(null);
			}
			base.NotifyPropertyChanged("ImagePart");
			this.SelectType("Parts");
			this.SetEditorEnableRules();
			return;
		}
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			Parallel.ForEach((IEnumerable<HemPartViewModel>)base.ToolConfigModel.FrontHemParts, (Action<HemPartViewModel>)delegate(HemPartViewModel item)
			{
				if (item != this.SelectedPart)
				{
					item.IsSelected = false;
				}
			});
		}
		else
		{
			Parallel.ForEach((IEnumerable<HemPartViewModel>)base.ToolConfigModel.BackHemParts, (Action<HemPartViewModel>)delegate(HemPartViewModel item)
			{
				if (item != this.SelectedPart)
				{
					item.IsSelected = false;
				}
			});
		}
		if (this.SelectedPart == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.GeometryFile.Name))
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

	private bool HemProfileFilterFront(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.FrontHemParts.Any((HemPartViewModel p) => p.ProfileID == ((HemProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool HemProfileFilterBack(object item)
	{
		if (item == null)
		{
			return false;
		}
		if (!this.ShowProfilesWithoutParts)
		{
			return base.ToolConfigModel.BackHemParts.Any((HemPartViewModel p) => p.ProfileID == ((HemProfileViewModel)item).ID && ((!this.ShowAllParts && p.Amount > 0 && p.Implemented) || this.ShowAllParts));
		}
		return true;
	}

	private bool HemPartFilter(object item)
	{
		if (item == null || this.SelectedProfile == null || string.IsNullOrEmpty(this.SelectedProfile.ID.ToString()))
		{
			return false;
		}
		if (((HemPartViewModel)item).ProfileID == this.SelectedProfile.ID)
		{
			if (this.ShowAllParts || ((HemPartViewModel)item).Amount <= 0)
			{
				return this.ShowAllParts;
			}
			return true;
		}
		return false;
	}

	public void AddProfile_Click()
	{
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int id = ((base.ToolConfigModel.FrontHemProfiles.Count > 0) ? (base.ToolConfigModel.FrontHemProfiles.Max((HemProfileViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.FrontHemProfiles.Add(new HemProfileViewModel(new HemProfile(id)));
			int num = this.FrontHemProfileView.Count - 1;
			this.SelectedProfile = ((num >= 0 && this.FrontHemProfileView.Count > 0 && num < this.FrontHemProfileView.Count) ? ((HemProfileViewModel)this.FrontHemProfileView.GetItemAt(num)) : null);
			this._changed = ChangedConfigType.Dies;
		}
		else
		{
			int id2 = ((base.ToolConfigModel.BackHemProfiles.Count > 0) ? (base.ToolConfigModel.BackHemProfiles.Max((HemProfileViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.BackHemProfiles.Add(new HemProfileViewModel(new HemProfile(id2)));
			int num2 = this.BackHemProfileView.Count - 1;
			this.SelectedProfile = ((num2 >= 0 && this.BackHemProfileView.Count > 0 && num2 < this.BackHemProfileView.Count) ? ((HemProfileViewModel)this.BackHemProfileView.GetItemAt(num2)) : null);
			this._changed = ChangedConfigType.Dies;
		}
	}

	public void AddPart_Click()
	{
		if (this.SelectedProfile == null)
		{
			return;
		}
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int id = ((base.ToolConfigModel.FrontHemParts.Count > 0) ? (base.ToolConfigModel.FrontHemParts.Max((HemPartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.FrontHemParts.Add(new HemPartViewModel(new HemPart(id, this.SelectedProfile.ID)));
			int num = this.FrontHemPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.FrontHemPartView.Count > 0 && num < this.FrontHemPartView.Count) ? ((HemPartViewModel)this.FrontHemPartView.GetItemAt(num)) : null);
		}
		else
		{
			int id2 = ((base.ToolConfigModel.BackHemParts.Count > 0) ? (base.ToolConfigModel.BackHemParts.Max((HemPartViewModel p) => p.ID) + 1) : 0);
			base.ToolConfigModel.BackHemParts.Add(new HemPartViewModel(new HemPart(id2, this.SelectedProfile.ID)));
			int num2 = this.BackHemPartView.Count - 1;
			this.SelectedPart = ((num2 >= 0 && this.BackHemPartView.Count > 0 && num2 < this.BackHemPartView.Count) ? ((HemPartViewModel)this.BackHemPartView.GetItemAt(num2)) : null);
		}
		this._changed = ChangedConfigType.Dies;
	}

	public void CopyPart_Click()
	{
		if (this.SelectedPart == null)
		{
			return;
		}
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int iD = ((base.ToolConfigModel.FrontHemParts.Count > 0) ? (base.ToolConfigModel.FrontHemParts.Max((HemPartViewModel p) => p.ID) + 1) : 0);
			HemPart hemPart = this.SelectedPart.HemPart.Copy();
			hemPart.ID = iD;
			base.ToolConfigModel.FrontHemParts.Add(new HemPartViewModel(hemPart));
			int num = this.FrontHemPartView.Count - 1;
			this.SelectedPart = ((num >= 0 && this.FrontHemPartView.Count > 0 && num < this.FrontHemPartView.Count) ? ((HemPartViewModel)this.FrontHemPartView.GetItemAt(num)) : null);
		}
		else
		{
			int iD2 = ((base.ToolConfigModel.BackHemParts.Count > 0) ? (base.ToolConfigModel.BackHemParts.Max((HemPartViewModel p) => p.ID) + 1) : 0);
			HemPart hemPart2 = this.SelectedPart.HemPart.Copy();
			hemPart2.ID = iD2;
			base.ToolConfigModel.BackHemParts.Add(new HemPartViewModel(hemPart2));
			int num2 = this.BackHemPartView.Count - 1;
			this.SelectedPart = ((num2 >= 0 && this.BackHemPartView.Count > 0 && num2 < this.BackHemPartView.Count) ? ((HemPartViewModel)this.BackHemPartView.GetItemAt(num2)) : null);
		}
		this._changed = ChangedConfigType.Dies;
	}

	public void CopyProfile_Click()
	{
		if (this.SelectedProfile == null)
		{
			return;
		}
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int iD = ((base.ToolConfigModel.FrontHemProfiles.Count > 0) ? (base.ToolConfigModel.FrontHemProfiles.Max((HemProfileViewModel p) => p.ID) + 1) : 0);
			HemProfile hemProfile = this.SelectedProfile.HemProfile.Copy();
			hemProfile.ID = iD;
			base.ToolConfigModel.FrontHemProfiles.Add(new HemProfileViewModel(hemProfile));
			int num = this.FrontHemProfileView.Count - 1;
			this.SelectedProfile = ((num >= 0 && this.FrontHemProfileView.Count > 0 && num < this.FrontHemProfileView.Count) ? ((HemProfileViewModel)this.FrontHemProfileView.GetItemAt(num)) : null);
		}
		else
		{
			int iD2 = ((base.ToolConfigModel.BackHemProfiles.Count > 0) ? (base.ToolConfigModel.BackHemProfiles.Max((HemProfileViewModel p) => p.ID) + 1) : 0);
			HemProfile hemProfile2 = this.SelectedProfile.HemProfile.Copy();
			hemProfile2.ID = iD2;
			base.ToolConfigModel.BackHemProfiles.Add(new HemProfileViewModel(hemProfile2));
			int num2 = this.BackHemProfileView.Count - 1;
			this.SelectedProfile = ((num2 >= 0 && this.BackHemProfileView.Count > 0 && num2 < this.BackHemProfileView.Count) ? ((HemProfileViewModel)this.BackHemProfileView.GetItemAt(num2)) : null);
		}
		this._changed = ChangedConfigType.Dies;
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "Profiles"))
		{
			if (text == "Parts")
			{
				this.LastSelectedType = typeof(HemPart);
				base.LastSelectedObject = this.SelectedPart;
			}
		}
		else
		{
			this.LastSelectedType = typeof(HemProfile);
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
		if (!(text == "HemProfileViewModel"))
		{
			if (text == "HemPartViewModel")
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

	private void DeleteProfile_Click()
	{
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int num = base.ToolConfigModel.FrontHemProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count();
			List<HemProfileViewModel> list = this.SelectedProfiles.Select((object p) => p as HemProfileViewModel).ToList();
			foreach (HemProfileViewModel item in list)
			{
				this.DeletePartsFromProfile(item.ID);
			}
			base.ToolConfigModel.FrontHemProfiles = base.ToolConfigModel.FrontHemProfiles.Except(list).ToObservableCollection();
			if (num >= base.ToolConfigModel.FrontHemProfiles.Count)
			{
				num = base.ToolConfigModel.FrontHemProfiles.Count - 1;
			}
			this.SetEditorEnableRules();
			if (num < 0)
			{
				this.SelectedProfile = null;
				return;
			}
			int num2 = this.FrontHemProfileView.Count - 1;
			this.SelectedProfile = ((num2 >= 0 && this.FrontHemProfileView.Count > 0 && num2 < this.FrontHemProfileView.Count) ? ((HemProfileViewModel)this.FrontHemProfileView.GetItemAt(num2)) : null);
			return;
		}
		int num3 = base.ToolConfigModel.BackHemProfiles.IndexOf(this.SelectedProfiles.Last()) + 1 - this.SelectedProfiles.Count();
		List<HemProfileViewModel> list2 = this.SelectedProfiles.Select((object p) => p as HemProfileViewModel).ToList();
		foreach (HemProfileViewModel item2 in list2)
		{
			this.DeletePartsFromProfile(item2.ID);
		}
		base.ToolConfigModel.BackHemProfiles = base.ToolConfigModel.BackHemProfiles.Except(list2).ToObservableCollection();
		if (num3 >= base.ToolConfigModel.BackHemProfiles.Count)
		{
			num3 = base.ToolConfigModel.BackHemProfiles.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num3 < 0)
		{
			this.SelectedProfile = null;
			return;
		}
		int num4 = this.BackHemProfileView.Count - 1;
		this.SelectedProfile = ((num4 >= 0 && this.BackHemProfileView.Count > 0 && num4 < this.BackHemProfileView.Count) ? ((HemProfileViewModel)this.BackHemProfileView.GetItemAt(num4)) : null);
	}

	private void DeletePart_Click(object param)
	{
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			int num = base.ToolConfigModel.FrontHemParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count();
			if (num > base.ToolConfigModel.FrontHemParts.Count)
			{
				num = this.FrontHemPartView.IndexOf((HemPartViewModel)param) - 1;
			}
			List<HemPartViewModel> second = this.SelectedParts.Select((object p) => p as HemPartViewModel).ToList();
			base.ToolConfigModel.FrontHemParts = base.ToolConfigModel.FrontHemParts.Except(second).ToObservableCollection();
			this.SetEditorEnableRules();
			if (num < 0 && this.FrontHemPartView.Count > 0)
			{
				num = 0;
			}
			this.SelectedPart = ((num >= 0 && this.FrontHemPartView.Count > 0 && num < this.FrontHemPartView.Count) ? ((HemPartViewModel)this.FrontHemPartView.GetItemAt(num)) : null);
			this.FrontHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.FrontHemParts);
			this.FrontHemPartView.Filter = HemPartFilter;
			this.FrontHemPartView.Refresh();
		}
		else
		{
			int num2 = base.ToolConfigModel.BackHemParts.IndexOf(this.SelectedParts.Last()) + 1 - this.SelectedParts.Count();
			if (num2 > base.ToolConfigModel.BackHemParts.Count)
			{
				num2 = this.BackHemPartView.IndexOf((HemPartViewModel)param) - 1;
			}
			List<HemPartViewModel> second2 = this.SelectedParts.Select((object p) => p as HemPartViewModel).ToList();
			base.ToolConfigModel.BackHemParts = base.ToolConfigModel.BackHemParts.Except(second2).ToObservableCollection();
			this.SetEditorEnableRules();
			if (num2 < 0 && this.BackHemPartView.Count > 0)
			{
				num2 = 0;
			}
			this.SelectedPart = ((num2 >= 0 && this.BackHemPartView.Count > 0 && num2 < this.BackHemPartView.Count) ? ((HemPartViewModel)this.BackHemPartView.GetItemAt(num2)) : null);
			this.BackHemPartView = (CollectionView)CollectionViewSource.GetDefaultView(base.ToolConfigModel.BackHemParts);
			this.BackHemPartView.Filter = HemPartFilter;
			this.BackHemPartView.Refresh();
		}
	}

	private void DeletePartsFromProfile(int profileId)
	{
		if (this.ActualProfileTab.Name == "FrontHemProfiles")
		{
			base.ToolConfigModel.FrontHemParts.Where((HemPartViewModel p) => p.ProfileID == profileId).ToList().ForEach(delegate(HemPartViewModel p)
			{
				base.ToolConfigModel.FrontHemParts.Remove(p);
			});
		}
		else
		{
			base.ToolConfigModel.BackHemParts.Where((HemPartViewModel p) => p.ProfileID == profileId).ToList().ForEach(delegate(HemPartViewModel p)
			{
				base.ToolConfigModel.BackHemParts.Remove(p);
			});
		}
	}

	public void EditProfile_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				if (this.ActualProfileTab.Name == "FrontHemProfiles")
				{
					int index = base.ToolConfigModel.FrontHemProfiles.IndexOf(this.SelectedProfile);
					base.ToolConfigModel.FrontHemProfiles.Remove(this.SelectedProfile);
					base.ToolConfigModel.FrontHemProfiles.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HemProfileViewModel);
					this.SelectedProfile = base.ToolConfigModel.FrontHemProfiles[index];
				}
				else
				{
					int index2 = base.ToolConfigModel.BackHemProfiles.IndexOf(this.SelectedProfile);
					base.ToolConfigModel.BackHemProfiles.Remove(this.SelectedProfile);
					base.ToolConfigModel.BackHemProfiles.Insert(index2, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HemProfileViewModel);
					this.SelectedProfile = base.ToolConfigModel.BackHemProfiles[index2];
				}
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: (this.ActualProfileTab.Name == "FrontHemProfiles") ? ((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.FrontHemProfiles).Select((Func<HemProfileViewModel, ToolItemViewModelBase>)((HemProfileViewModel i) => i)) : ((IEnumerable<HemProfileViewModel>)base.ToolConfigModel.BackHemProfiles).Select((Func<HemProfileViewModel, ToolItemViewModelBase>)((HemProfileViewModel i) => i)), selectedItem: this.SelectedProfile, isUpper: this.ActualProfileTab.Name == "FrontHemProfiles", toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
				if (this.ActualProfileTab.Name == "FrontHemProfiles")
				{
					int index = base.ToolConfigModel.FrontHemParts.IndexOf(this.SelectedPart);
					base.ToolConfigModel.FrontHemParts.Remove(this.SelectedPart);
					base.ToolConfigModel.FrontHemParts.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HemPartViewModel);
					this.SelectedPart = base.ToolConfigModel.FrontHemParts[index];
				}
				else
				{
					int index2 = base.ToolConfigModel.BackHemParts.IndexOf(this.SelectedPart);
					base.ToolConfigModel.BackHemParts.Remove(this.SelectedPart);
					base.ToolConfigModel.BackHemParts.Insert(index2, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as HemPartViewModel);
					this.SelectedPart = base.ToolConfigModel.BackHemParts[index2];
				}
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: (this.ActualProfileTab.Name == "FrontHemProfiles") ? base.ToolConfigModel.FrontHemParts.Where((HemPartViewModel p) => p.ProfileID == this.SelectedProfile.ID).Select((Func<HemPartViewModel, ToolItemViewModelBase>)((HemPartViewModel i) => i)) : base.ToolConfigModel.BackHemParts.Where((HemPartViewModel p) => p.ProfileID == this.SelectedProfile.ID).Select((Func<HemPartViewModel, ToolItemViewModelBase>)((HemPartViewModel i) => i)), selectedItem: this.SelectedPart, isUpper: this.ActualProfileTab.Name == "FrontHemProfiles", toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
		this.EditScreen = new EditScreenView
		{
			DataContext = dataContext
		};
		this.EditScreenVisible = Visibility.Visible;
		this.SetEditorEnableRules();
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
		if (this.LastSelectedType == typeof(HemProfile))
		{
			ObservableCollection<object> selectedProfiles = this.SelectedProfiles;
			this.SingleSelected = selectedProfiles == null || selectedProfiles.Count() <= 1;
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
		else if (this.LastSelectedType == typeof(HemPart))
		{
			ObservableCollection<object> selectedParts = this.SelectedParts;
			this.SingleSelected = selectedParts == null || selectedParts.Count() <= 1;
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
		base.BendMachine.Dies.HemProfiles = new ObservableCollection<HemProfile>();
		foreach (HemProfileViewModel frontHemProfile in base.ToolConfigModel.FrontHemProfiles)
		{
			base.BendMachine.Dies.HemProfiles.Add(frontHemProfile.HemProfile);
		}
		foreach (HemProfileViewModel backHemProfile in base.ToolConfigModel.BackHemProfiles)
		{
			base.BendMachine.Dies.HemProfiles.Add(backHemProfile.HemProfile);
		}
		base.BendMachine.Dies.HemParts = new ObservableCollection<HemPart>();
		foreach (HemPartViewModel frontHemPart in base.ToolConfigModel.FrontHemParts)
		{
			base.BendMachine.Dies.HemParts.Add(frontHemPart.HemPart);
		}
		foreach (HemPartViewModel backHemPart in base.ToolConfigModel.BackHemParts)
		{
			base.BendMachine.Dies.HemParts.Add(backHemPart.HemPart);
		}
		if (base.ToolConfigModel.FrontHemProfiles.Any((HemProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		if (base.ToolConfigModel.FrontHemParts.Any((HemPartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		if (base.ToolConfigModel.BackHemProfiles.Any((HemProfileViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		if (base.ToolConfigModel.BackHemParts.Any((HemPartViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.Dies;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

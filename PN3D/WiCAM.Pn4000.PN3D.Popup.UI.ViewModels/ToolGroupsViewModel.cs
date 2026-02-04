using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ToolGroupsViewModel : ToolViewModelBase
{
	private readonly IConfigProvider _configProvider;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private FrameworkElement _editScreen;

	private Visibility _editScreenVisible;

	private PunchGroupViewModel _selectedPunchGroup;

	private DieGroupViewModel _selectedDieGroup;

	private ICommand _selectClick;

	private ICommand _punchesOrderChangedCommand;

	private ICommand _diesOrderChangedCommand;

	private ICommand _keyDownDelete;

	private Type _lastSelectedType;

	private Brush _punchBorderBrush;

	private Brush _dieBorderBrush;

	private PunchProfileViewModel _selectedPrimaryToolPunch;

	private DieProfileViewModel _selectedPrimaryToolDie;

	private bool _singleSelected = true;

	private bool _isCopyButtonEnabled;

	private bool _isEditButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isOkButtonEnabled;

	private bool _isCancelButtonEnabled;

	private bool _isSaveButtonEnabled;

	private FrameworkElement _imagePart;

	private ChangedConfigType _changed;

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

	public ICommand SelectClick => this._selectClick ?? (this._selectClick = new RelayCommand(SelectType));

	public ICommand PunchesOrderChangedCommand => this._punchesOrderChangedCommand ?? (this._punchesOrderChangedCommand = new RelayCommand((Action<object>)delegate
	{
		this.RefreshPunchPriorities();
	}));

	public ICommand DiesOrderChangedCommand => this._diesOrderChangedCommand ?? (this._diesOrderChangedCommand = new RelayCommand((Action<object>)delegate
	{
		this.RefreshDiePriorities();
	}));

	public ICommand KeyDownDelete => this._keyDownDelete ?? (this._keyDownDelete = new RelayCommand(Delete_Click));

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
			if (this._lastSelectedType == typeof(DieGroup))
			{
				this.PunchBorderBrush = new SolidColorBrush(Colors.Transparent);
				this.DieBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				base.LastSelectedObject = this.SelectedPunchGroup;
			}
			else if (this._lastSelectedType == typeof(PunchGroup))
			{
				this.PunchBorderBrush = new SolidColorBrush(Colors.DarkBlue);
				this.DieBorderBrush = new SolidColorBrush(Colors.Transparent);
				base.LastSelectedObject = this.SelectedDieGroup;
			}
		}
	}

	public PunchGroupViewModel SelectedPunchGroup
	{
		get
		{
			return this._selectedPunchGroup;
		}
		set
		{
			this._selectedPunchGroup = value;
			base.NotifyPropertyChanged("SelectedPunchGroup");
			this.SelectType("PunchGroups");
			if (base.BendMachine.Punches.PunchGroups.Count > 0)
			{
				this.RefreshPunchPreviews();
				Parallel.ForEach((IEnumerable<PunchGroupViewModel>)base.ToolConfigModel.PunchGroups, (Action<PunchGroupViewModel>)delegate(PunchGroupViewModel item)
				{
					if (item != this.SelectedPunchGroup)
					{
						item.IsSelected = false;
					}
				});
			}
			PunchGroupViewModel selectedPunchGroup = this.SelectedPunchGroup;
			if (selectedPunchGroup != null && selectedPunchGroup.PrimaryToolId >= 0)
			{
				this.SelectedPrimaryToolPunch = base.ToolConfigModel.PunchProfiles.FirstOrDefault((PunchProfileViewModel p) => p.ID == this.SelectedPunchGroup.PrimaryToolId);
			}
			this.SetEditorEnableRules();
		}
	}

	public DieGroupViewModel SelectedDieGroup
	{
		get
		{
			return this._selectedDieGroup;
		}
		set
		{
			this._selectedDieGroup = value;
			base.NotifyPropertyChanged("SelectedDieGroup");
			this.SelectType("DieGroups");
			if (base.BendMachine.Dies.DieGroups.Count > 0)
			{
				this.RefreshDiePreviews();
				Parallel.ForEach((IEnumerable<DieGroupViewModel>)base.ToolConfigModel.DieGroups, (Action<DieGroupViewModel>)delegate(DieGroupViewModel item)
				{
					if (item != this.SelectedDieGroup)
					{
						item.IsSelected = false;
					}
				});
			}
			DieGroupViewModel selectedDieGroup = this.SelectedDieGroup;
			if (selectedDieGroup != null && selectedDieGroup.PrimaryToolId >= 0)
			{
				this.SelectedPrimaryToolDie = base.ToolConfigModel.DieProfiles.FirstOrDefault((DieProfileViewModel p) => p.ID == this.SelectedDieGroup.PrimaryToolId);
			}
			this.SetEditorEnableRules();
		}
	}

	public ObservableCollection<object> SelectedPunchGroups { get; internal set; }

	public ObservableCollection<object> SelectedDieGroups { get; internal set; }

	public Brush PunchBorderBrush
	{
		get
		{
			return this._punchBorderBrush;
		}
		set
		{
			this._punchBorderBrush = value;
			base.NotifyPropertyChanged("PunchBorderBrush");
		}
	}

	public Brush DieBorderBrush
	{
		get
		{
			return this._dieBorderBrush;
		}
		set
		{
			this._dieBorderBrush = value;
			base.NotifyPropertyChanged("DieBorderBrush");
		}
	}

	public PunchProfileViewModel SelectedPrimaryToolPunch
	{
		get
		{
			return this._selectedPrimaryToolPunch;
		}
		set
		{
			this._selectedPrimaryToolPunch = value;
			base.NotifyPropertyChanged("SelectedPrimaryToolPunch");
			if (this.SelectedPrimaryToolPunch != null)
			{
				this.SelectedPunchGroup.PrimaryToolId = this.SelectedPrimaryToolPunch.ID;
				this.SelectedPunchGroup.PrimaryToolName = this.SelectedPrimaryToolPunch.Name;
			}
		}
	}

	public DieProfileViewModel SelectedPrimaryToolDie
	{
		get
		{
			return this._selectedPrimaryToolDie;
		}
		set
		{
			this._selectedPrimaryToolDie = value;
			base.NotifyPropertyChanged("SelectedPrimaryToolDie");
			if (this.SelectedPrimaryToolDie != null)
			{
				this.SelectedDieGroup.PrimaryToolId = this.SelectedPrimaryToolDie.ID;
				this.SelectedDieGroup.PrimaryToolName = this.SelectedPrimaryToolDie.Name;
			}
		}
	}

	public ObservableCollection<PunchGroupToolViewModel> PunchGroupTools { get; set; }

	public ObservableCollection<DieGroupToolViewModel> DieGroupTools { get; set; }

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

	public new FrameworkElement ImagePart
	{
		get
		{
			return this._imagePart;
		}
		set
		{
			this._imagePart = value;
			base.NotifyPropertyChanged("ImagePart");
		}
	}

	public ObservableCollection<RadMenuItem> PunchGroupContextMenuItems { get; set; }

	public ObservableCollection<RadMenuItem> DieGroupContextMenuItems { get; set; }

	public ToolGroupsViewModel(IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IPnPathService pnPathService, IConfigProvider configProvider, IModelFactory modelFactory, IDrawToolProfiles drawToolProfiles)
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
		this.ImagePart = new Canvas
		{
			Height = 1.0,
			Width = 1.0
		};
		this.ImagePart.Loaded += Image3DOnLoaded;
		this.PunchGroupTools = new ObservableCollection<PunchGroupToolViewModel>();
		this.DieGroupTools = new ObservableCollection<DieGroupToolViewModel>();
		this.InitializePunchGroupContextMenuItems();
		this.InitializeDieGroupContextMenuItems();
		if (base.ToolConfigModel.DieGroups.Count > 0)
		{
			this.SelectedDieGroup = null;
			DieGroupViewModel selectedDieGroup = this.SelectedDieGroup;
			if (selectedDieGroup != null && selectedDieGroup.PrimaryToolId >= 0)
			{
				this.SelectedPrimaryToolDie = base.ToolConfigModel.DieProfiles.FirstOrDefault((DieProfileViewModel p) => p.ID == this.SelectedDieGroup.PrimaryToolId);
			}
		}
		this.SelectedDieGroup = base.ToolConfigModel.DieGroups.FirstOrDefault();
		if (base.ToolConfigModel.PunchGroups.Count > 0)
		{
			this.SelectedPunchGroup = null;
			PunchGroupViewModel selectedPunchGroup = this.SelectedPunchGroup;
			if (selectedPunchGroup != null && selectedPunchGroup.PrimaryToolId >= 0)
			{
				this.SelectedPrimaryToolPunch = base.ToolConfigModel.PunchProfiles.FirstOrDefault((PunchProfileViewModel p) => p.ID == this.SelectedPunchGroup.PrimaryToolId);
			}
		}
		this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups.FirstOrDefault();
	}

	private void InitializePunchGroupContextMenuItems()
	{
		this.PunchGroupContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyPunchGroup_Click)
		};
		this.PunchGroupContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditPunchGroup_Click)
		};
		this.PunchGroupContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeletePunchGroup_Click)
		};
		this.PunchGroupContextMenuItems.Add(item3);
	}

	private void InitializeDieGroupContextMenuItems()
	{
		this.DieGroupContextMenuItems = new ObservableCollection<RadMenuItem>();
		RadMenuItem item = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button8_Copy"),
			Icon = new RadGlyph
			{
				Glyph = "\ue65d"
			},
			Command = new RelayCommand(CopyDieGroup_Click)
		};
		this.DieGroupContextMenuItems.Add(item);
		RadMenuItem item2 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button1_Edit"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10b"
			},
			Command = new RelayCommand(EditDieGroup_Click)
		};
		this.DieGroupContextMenuItems.Add(item2);
		RadMenuItem item3 = new RadMenuItem
		{
			Header = Application.Current.FindResource("l_popup.Button0_Delete"),
			Icon = new RadGlyph
			{
				Glyph = "\ue10c"
			},
			Command = new RelayCommand(DeleteDieGroup_Click)
		};
		this.DieGroupContextMenuItems.Add(item3);
	}

	private void Image3DOnLoaded(object sender, RoutedEventArgs e)
	{
		this.SelectedDieGroup = base.ToolConfigModel.DieGroups.FirstOrDefault();
		this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups.FirstOrDefault();
	}

	public void Refresh()
	{
		foreach (PunchGroupViewModel group in base.ToolConfigModel.PunchGroups)
		{
			IEnumerable<PunchProfileViewModel> enumerable = base.ToolConfigModel.PunchProfiles.Where((PunchProfileViewModel p) => p.GroupID == group.ID);
			foreach (PunchProfileViewModel item in enumerable)
			{
				item.GroupName = group.Name;
			}
			PunchProfileViewModel punchProfileViewModel = enumerable.OrderBy((PunchProfileViewModel p) => p.Priority).FirstOrDefault();
			if (punchProfileViewModel != null)
			{
				group.PrimaryToolId = punchProfileViewModel.ID;
				group.PrimaryToolName = punchProfileViewModel.Name;
			}
			else
			{
				group.PrimaryToolId = -1;
				group.PrimaryToolName = null;
			}
		}
		foreach (DieGroupViewModel group2 in base.ToolConfigModel.DieGroups)
		{
			IEnumerable<DieProfileViewModel> enumerable2 = base.ToolConfigModel.DieProfiles.Where((DieProfileViewModel p) => p.GroupID == group2.ID);
			foreach (DieProfileViewModel item2 in enumerable2)
			{
				item2.GroupName = group2.Name;
			}
			DieProfileViewModel dieProfileViewModel = enumerable2.OrderBy((DieProfileViewModel p) => p.Priority).FirstOrDefault();
			if (dieProfileViewModel != null)
			{
				group2.PrimaryToolId = dieProfileViewModel.ID;
				group2.PrimaryToolName = dieProfileViewModel.Name;
			}
			else
			{
				group2.PrimaryToolId = -1;
				group2.PrimaryToolName = null;
			}
		}
		this.Save();
	}

	private void RefreshPunchPreviews()
	{
		this.PunchGroupTools.Clear();
		foreach (PunchProfileViewModel item in base.ToolConfigModel.PunchProfiles.Where((PunchProfileViewModel p) => p.GroupID == this.SelectedPunchGroup?.ID))
		{
			this.PunchGroupTools.Add(new PunchGroupToolViewModel(item, base.BendMachine, this._drawToolProfiles));
		}
		this.PunchGroupTools = this.PunchGroupTools.OrderBy((PunchGroupToolViewModel p) => p.Profile.Priority).ToObservableCollection();
		base.NotifyPropertyChanged("PunchGroupTools");
	}

	private void RefreshDiePreviews()
	{
		this.DieGroupTools.Clear();
		foreach (DieProfileViewModel item in base.ToolConfigModel.DieProfiles.Where((DieProfileViewModel p) => p.GroupID == this.SelectedDieGroup?.ID))
		{
			this.DieGroupTools.Add(new DieGroupToolViewModel(item, base.BendMachine, this._drawToolProfiles));
		}
		this.DieGroupTools = this.DieGroupTools.OrderBy((DieGroupToolViewModel p) => p.Profile.Priority).ToObservableCollection();
		base.NotifyPropertyChanged("DieGroupTools");
	}

	private void RefreshPunchPriorities()
	{
		for (int i = 0; i < this.PunchGroupTools.Count; i++)
		{
			this.PunchGroupTools[i].Profile.Priority = i + 1;
		}
		PunchGroupToolViewModel punchGroupToolViewModel = this.PunchGroupTools.FirstOrDefault();
		if (punchGroupToolViewModel == null)
		{
			this.SelectedPunchGroup.PrimaryToolId = -1;
			this.SelectedPunchGroup.PrimaryToolName = null;
		}
		else
		{
			this.SelectedPunchGroup.PrimaryToolId = punchGroupToolViewModel.Profile.ID;
			this.SelectedPunchGroup.PrimaryToolName = punchGroupToolViewModel.Profile.Name;
		}
	}

	private void RefreshDiePriorities()
	{
		for (int i = 0; i < this.DieGroupTools.Count; i++)
		{
			this.DieGroupTools[i].Profile.Priority = i + 1;
		}
		DieGroupToolViewModel dieGroupToolViewModel = this.DieGroupTools.FirstOrDefault();
		if (dieGroupToolViewModel == null)
		{
			this.SelectedDieGroup.PrimaryToolId = -1;
			this.SelectedDieGroup.PrimaryToolName = null;
		}
		else
		{
			this.SelectedDieGroup.PrimaryToolId = dieGroupToolViewModel.Profile.ID;
			this.SelectedDieGroup.PrimaryToolName = dieGroupToolViewModel.Profile.Name;
		}
	}

	private void SelectType(object param)
	{
		string text = (string)param;
		if (!(text == "PunchGroups"))
		{
			if (text == "DieGroups")
			{
				this.LastSelectedType = typeof(DieGroup);
				base.LastSelectedObject = this.SelectedDieGroup;
			}
		}
		else
		{
			this.LastSelectedType = typeof(PunchGroup);
			base.LastSelectedObject = this.SelectedPunchGroup;
		}
	}

	public void AddPunchGroup_Click()
	{
		int id = ((base.ToolConfigModel.PunchGroups.Count > 0) ? (base.ToolConfigModel.PunchGroups.Max((PunchGroupViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.PunchGroups.Add(new PunchGroupViewModel(new PunchGroup(id)));
		this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups.LastOrDefault();
		this._changed = ChangedConfigType.PunchGroup;
	}

	public void AddDieGroup_Click()
	{
		int id = ((base.ToolConfigModel.DieGroups.Count > 0) ? (base.ToolConfigModel.DieGroups.Max((DieGroupViewModel p) => p.ID) + 1) : 0);
		base.ToolConfigModel.DieGroups.Add(new DieGroupViewModel(new DieGroup(id)));
		this.SelectedDieGroup = base.ToolConfigModel.DieGroups.LastOrDefault();
		this._changed = ChangedConfigType.DieGroup;
	}

	public void CopyPunchGroup_Click()
	{
		if (this.SelectedPunchGroup != null)
		{
			int iD = ((base.ToolConfigModel.PunchGroups.Count > 0) ? (base.ToolConfigModel.PunchGroups.Max((PunchGroupViewModel p) => p.ID) + 1) : 0);
			PunchGroup punchGroup = this.SelectedPunchGroup.PunchGroup.Copy();
			punchGroup.ID = iD;
			base.ToolConfigModel.PunchGroups.Add(new PunchGroupViewModel(punchGroup));
			this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups.LastOrDefault();
			this._changed = ChangedConfigType.PunchGroup;
		}
	}

	public void CopyDieGroup_Click()
	{
		if (this.SelectedDieGroup != null)
		{
			int iD = ((base.ToolConfigModel.DieGroups.Count > 0) ? (base.ToolConfigModel.DieGroups.Max((DieGroupViewModel p) => p.ID) + 1) : 0);
			DieGroup dieGroup = this.SelectedDieGroup.DieGroup.Copy();
			dieGroup.ID = iD;
			base.ToolConfigModel.DieGroups.Add(new DieGroupViewModel(dieGroup));
			this.SelectedDieGroup = base.ToolConfigModel.DieGroups.LastOrDefault();
			this._changed = ChangedConfigType.DieGroup;
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
		if (!(text == "PunchGroupViewModel"))
		{
			if (text == "DieGroupViewModel" && this.SelectedDieGroups.Any())
			{
				this.DeleteDieGroup_Click();
			}
		}
		else if (this.SelectedPunchGroups.Any())
		{
			this.DeletePunchGroup_Click();
		}
	}

	private void DeletePunchGroup_Click()
	{
		int num = base.ToolConfigModel.PunchGroups.IndexOf((PunchGroupViewModel)this.SelectedPunchGroups.Last()) + 1 - this.SelectedPunchGroups.Count();
		base.ToolConfigModel.PunchGroups = base.ToolConfigModel.PunchGroups.Except(this.SelectedPunchGroups.Select((object p) => p as PunchGroupViewModel)).ToObservableCollection();
		if (num >= base.ToolConfigModel.PunchGroups.Count)
		{
			num = base.ToolConfigModel.PunchGroups.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedPunchGroup = null;
		}
		else
		{
			this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups[num];
		}
		this._changed = ChangedConfigType.PunchGroup;
	}

	private void DeleteDieGroup_Click()
	{
		int num = base.ToolConfigModel.DieGroups.IndexOf((DieGroupViewModel)this.SelectedDieGroups.Last()) + 1 - this.SelectedDieGroups.Count();
		base.ToolConfigModel.DieGroups = base.ToolConfigModel.DieGroups.Except(this.SelectedDieGroups.Select((object d) => d as DieGroupViewModel)).ToObservableCollection();
		if (num >= base.ToolConfigModel.DieGroups.Count)
		{
			num = base.ToolConfigModel.DieGroups.Count - 1;
		}
		this.SetEditorEnableRules();
		if (num < 0)
		{
			this.SelectedDieGroup = null;
		}
		else
		{
			this.SelectedDieGroup = base.ToolConfigModel.DieGroups[num];
		}
		this._changed = ChangedConfigType.DieGroup;
	}

	public void EditPunchGroup_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.PunchGroups.IndexOf(this.SelectedPunchGroup);
				base.ToolConfigModel.PunchGroups.Remove(this.SelectedPunchGroup);
				base.ToolConfigModel.PunchGroups.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as PunchGroupViewModel);
				this.SelectedPunchGroup = base.ToolConfigModel.PunchGroups[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<PunchGroupViewModel>)base.ToolConfigModel.PunchGroups).Select((Func<PunchGroupViewModel, ToolItemViewModelBase>)((PunchGroupViewModel i) => i)), selectedItem: this.SelectedPunchGroup, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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

	public void EditDieGroup_Click()
	{
		EditScreenViewModel dataContext = new EditScreenViewModel(closeAction: delegate(bool isOk, bool close)
		{
			if (isOk)
			{
				int index = base.ToolConfigModel.DieGroups.IndexOf(this.SelectedDieGroup);
				base.ToolConfigModel.DieGroups.Remove(this.SelectedDieGroup);
				base.ToolConfigModel.DieGroups.Insert(index, ((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as DieGroupViewModel);
				this.SelectedDieGroup = base.ToolConfigModel.DieGroups[index];
			}
			if (close)
			{
				((this.EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
				this.EditScreen = null;
				this.SetEditorEnableRules();
			}
		}, items: ((IEnumerable<DieGroupViewModel>)base.ToolConfigModel.DieGroups).Select((Func<DieGroupViewModel, ToolItemViewModelBase>)((DieGroupViewModel i) => i)), selectedItem: this.SelectedDieGroup, isUpper: false, toolConfigModel: base.ToolConfigModel, bendMachine: base.BendMachine, configProvider: this._configProvider);
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
		if (this.LastSelectedType == typeof(DieGroup))
		{
			ObservableCollection<object> selectedDieGroups = this.SelectedDieGroups;
			this.SingleSelected = selectedDieGroups == null || selectedDieGroups.Count() <= 1;
			if (this.SelectedDieGroup == null)
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
			this.IsCopyButtonEnabled = this.SelectedDieGroup != null && this.SingleSelected;
			this.IsEditButtonEnabled = this.SelectedDieGroup != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedDieGroup != null;
		}
		else if (this.LastSelectedType == typeof(PunchGroup))
		{
			ObservableCollection<object> selectedPunchGroups = this.SelectedPunchGroups;
			this.SingleSelected = selectedPunchGroups == null || selectedPunchGroups.Count() <= 1;
			if (this.SelectedPunchGroup == null)
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
			this.IsCopyButtonEnabled = this.SelectedPunchGroup != null && this.SingleSelected;
			this.IsEditButtonEnabled = this.SelectedPunchGroup != null && this.SingleSelected;
			this.IsDeleteButtonEnabled = this.SelectedPunchGroup != null;
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
		this.ImagePart = null;
	}

	private void Save()
	{
		base.BendMachine.Punches.PunchGroups = new ObservableCollection<PunchGroup>();
		foreach (PunchGroupViewModel punchGroup in base.ToolConfigModel.PunchGroups)
		{
			base.BendMachine.Punches.PunchGroups.Add(punchGroup.PunchGroup);
		}
		base.BendMachine.Dies.DieGroups = new ObservableCollection<DieGroup>();
		foreach (DieGroupViewModel dieGroup in base.ToolConfigModel.DieGroups)
		{
			base.BendMachine.Dies.DieGroups.Add(dieGroup.DieGroup);
		}
		if (base.ToolConfigModel.PunchGroups.Any((PunchGroupViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.PunchGroup;
		}
		if (base.ToolConfigModel.DieGroups.Any((DieGroupViewModel p) => p.IsChanged))
		{
			this._changed = ChangedConfigType.DieGroup;
		}
		this.DataChanged?.Invoke(this._changed);
	}
}

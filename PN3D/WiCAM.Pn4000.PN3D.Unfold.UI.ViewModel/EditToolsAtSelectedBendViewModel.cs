using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Unfold.UI.Model;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class EditToolsAtSelectedBendViewModel : ScreenControlBaseViewModel, ISubViewModel
{
	private ICombinedBendDescriptorInternal _selectedCommonBend;

	private EditToolsAtSelectedBendModel _model;

	private Screen3D _screen3d;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IGlobals _globals;

	private readonly IMainWindowBlock _mainWindowBlock;

	private IBendMachineSimulation _machine;

	private FrameworkElement _imagePunch;

	private FrameworkElement _imageDie;

	private PunchGroupViewModel _selectedPunchGroup;

	private DieGroupViewModel _selectedDieGroup;

	private PunchProfileViewModel _selectedPunchProfile;

	private DieProfileViewModel _selectedDieProfile;

	private PreferredProfileViewModel _selectedPreferredProfile;

	private ToolConfigModel _toolBase;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private PunchGroupToolViewModel _selectedPunchGroupViewModel;

	private DieGroupToolViewModel _selectedDieGroupViewModel;

	private ObservableCollection<PreferredProfileViewModel> _preferredProfiles;

	private ICommand _okCommand;

	private int _originalPunchGroupId = -1;

	private int _originalDieGroupId = -1;

	private int _originalPunchProfileId = -1;

	private int _originalDieProfileId = -1;

	private int _originalPreferredProfileId = -1;

	private bool _punchGroupChanged;

	private bool _dieGroupChanged;

	private bool _punchProfileChanged;

	private bool _dieProfileChanged;

	private bool _preferredProfileChanged;

	private bool _applyToAllBends;

	private bool _applyToAllStepBends;

	private bool _anyChanged
	{
		get
		{
			if (!this._preferredProfileChanged && !this._punchGroupChanged && !this._dieGroupChanged && !this._punchProfileChanged)
			{
				return this._dieProfileChanged;
			}
			return true;
		}
	}

	public ICombinedBendDescriptorInternal SelectedCommonBend
	{
		get
		{
			return this._selectedCommonBend;
		}
		set
		{
			this._selectedCommonBend = value;
			base.NotifyPropertyChanged("SelectedCommonBend");
		}
	}

	public FrameworkElement ImagePunch
	{
		get
		{
			return this._imagePunch;
		}
		set
		{
			this._imagePunch = value;
			base.NotifyPropertyChanged("ImagePunch");
		}
	}

	public FrameworkElement ImageDie
	{
		get
		{
			return this._imageDie;
		}
		set
		{
			this._imageDie = value;
			base.NotifyPropertyChanged("ImageDie");
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
			this.RefreshPunchPreviews();
			this._punchGroupChanged = this._selectedPunchGroup?.ID != this._originalPunchGroupId;
			this.SelectedPunchProfile = this.ToolBase.PunchProfiles.FirstOrDefault((PunchProfileViewModel p) => p.ID == this._selectedPunchGroup?.PrimaryToolId);
			if (this._selectedPunchGroup?.ID != this.SelectedPreferredProfile?.PunchGroupId)
			{
				this.SelectedPreferredProfile = this.ToolBase.PreferredProfiles.FirstOrDefault((PreferredProfileViewModel pp) => pp.PunchGroupId == this.SelectedPunchGroup?.ID && pp.DieGroupId == this.SelectedDieGroup?.ID);
			}
			this.SelectedPunchGroupViewModel = this.PunchGroupTools.FirstOrDefault((PunchGroupToolViewModel p) => p.Profile.ID == this.SelectedPunchProfile?.ID);
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
			this.RefreshDiePreviews();
			this._dieGroupChanged = this._selectedDieGroup?.ID != this._originalDieGroupId;
			this.SelectedDieProfile = this.ToolBase.DieProfiles.FirstOrDefault((DieProfileViewModel p) => p.ID == this._selectedDieGroup?.PrimaryToolId);
			if (this._selectedDieGroup?.ID != this.SelectedPreferredProfile?.DieGroupId)
			{
				this.SelectedPreferredProfile = this.ToolBase.PreferredProfiles.FirstOrDefault((PreferredProfileViewModel pp) => pp.PunchGroupId == this.SelectedPunchGroup?.ID && pp.DieGroupId == this.SelectedDieGroup?.ID);
			}
			this.SelectedDieGroupViewModel = this.DieGroupTools.FirstOrDefault((DieGroupToolViewModel p) => p.Profile.ID == this.SelectedDieProfile?.ID);
		}
	}

	public PunchProfileViewModel SelectedPunchProfile
	{
		get
		{
			return this._selectedPunchProfile;
		}
		set
		{
			this._selectedPunchProfile = value;
			base.NotifyPropertyChanged("SelectedPunchProfile");
			this.DrawPunch();
			this._punchProfileChanged = this._selectedPunchProfile?.ID != this._originalPunchProfileId;
		}
	}

	public DieProfileViewModel SelectedDieProfile
	{
		get
		{
			return this._selectedDieProfile;
		}
		set
		{
			this._selectedDieProfile = value;
			base.NotifyPropertyChanged("SelectedDieProfile");
			this.DrawDie();
			this._dieProfileChanged = this._selectedDieProfile?.ID != this._originalDieProfileId;
		}
	}

	public PreferredProfileViewModel SelectedPreferredProfile
	{
		get
		{
			return this._selectedPreferredProfile;
		}
		set
		{
			this._selectedPreferredProfile = value;
			base.NotifyPropertyChanged("SelectedPreferredProfile");
			this._preferredProfileChanged = this._selectedPreferredProfile?.Id != this._originalPreferredProfileId;
			if (this._selectedPreferredProfile != null)
			{
				this.SelectedDieGroup = this.ToolBase.DieGroups.FirstOrDefault((DieGroupViewModel dg) => dg.ID == this._selectedPreferredProfile.DieGroupId);
				this.SelectedPunchGroup = this.ToolBase.PunchGroups.FirstOrDefault((PunchGroupViewModel pg) => pg.ID == this._selectedPreferredProfile.PunchGroupId);
			}
		}
	}

	public ToolConfigModel ToolBase
	{
		get
		{
			return this._toolBase;
		}
		set
		{
			this._toolBase = value;
			base.NotifyPropertyChanged("ToolBase");
		}
	}

	public PunchGroupToolViewModel SelectedPunchGroupViewModel
	{
		get
		{
			return this._selectedPunchGroupViewModel;
		}
		set
		{
			this._selectedPunchGroupViewModel = value;
			base.NotifyPropertyChanged("SelectedPunchGroupViewModel");
			this.SelectedPunchProfile = this.ToolBase.PunchProfiles.FirstOrDefault((PunchProfileViewModel p) => p.ID == this._selectedPunchGroupViewModel?.Profile?.ID);
		}
	}

	public DieGroupToolViewModel SelectedDieGroupViewModel
	{
		get
		{
			return this._selectedDieGroupViewModel;
		}
		set
		{
			this._selectedDieGroupViewModel = value;
			base.NotifyPropertyChanged("SelectedDieGroupViewModel");
			this.SelectedDieProfile = this.ToolBase.DieProfiles.FirstOrDefault((DieProfileViewModel p) => p.ID == this._selectedDieGroupViewModel?.Profile?.ID);
		}
	}

	public ObservableCollection<PreferredProfileViewModel> PreferredProfiles
	{
		get
		{
			return this._preferredProfiles;
		}
		set
		{
			this._preferredProfiles = value;
			base.NotifyPropertyChanged("PreferredProfiles");
		}
	}

	public ObservableCollection<PunchGroupToolViewModel> PunchGroupTools { get; set; }

	public ObservableCollection<DieGroupToolViewModel> DieGroupTools { get; set; }

	public Visibility ApplyToStepBendVisibility
	{
		get
		{
			if (!this.SelectedCommonBend.Enumerable.Any((IBendDescriptor x) => x.BendParams.IsStepBend))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public ICommand OKCommand => this._okCommand ?? (this._okCommand = new RelayCommand(OK));

	public bool ApplyToAllBends
	{
		get
		{
			return this._applyToAllBends;
		}
		set
		{
			this._applyToAllBends = value;
			base.NotifyPropertyChanged("ApplyToAllBends");
		}
	}

	public bool ApplyToAllStepBends
	{
		get
		{
			return this._applyToAllStepBends;
		}
		set
		{
			this._applyToAllStepBends = value;
			base.NotifyPropertyChanged("ApplyToAllStepBends");
		}
	}

	public event Action<ISubViewModel, Triangle, global::WiCAM.Pn4000.BendModel.Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action RequestRepaint;

	public EditToolsAtSelectedBendViewModel(EditToolsAtSelectedBendModel model, IGlobals globals, IMainWindowBlock windowBlock, Screen3D screen3d, global::WiCAM.Pn4000.BendModel.Model bendModel, Vector3d anchorPoint, IShortcutSettingsCommon shortcutSettingsCommon)
		: base(screen3d, bendModel, anchorPoint)
	{
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

	private PreferredProfileViewModel CheckForPreferredProfile(PunchGroupViewModel punchGroup, DieGroupViewModel dieGroup)
	{
		return this.ToolBase.PreferredProfiles.FirstOrDefault((PreferredProfileViewModel p) => p.PunchGroupId == punchGroup?.ID && p.DieGroupId == dieGroup?.ID);
	}

	private void RefreshPunchPreviews()
	{
		this.PunchGroupTools.Clear();
		foreach (PunchProfileViewModel item in from p in this.ToolBase.PunchProfiles
			where p.Implemented && p.GroupID == this.SelectedPunchGroup?.ID
			select p into i
			orderby i.Priority
			select i)
		{
			this.PunchGroupTools.Add(new PunchGroupToolViewModel(item, this._machine.BendMachine, this._drawToolProfiles, 50.0, 50.0));
		}
		this.PunchGroupTools = this.PunchGroupTools.OrderBy((PunchGroupToolViewModel p) => p.Profile.Priority).ToObservableCollection();
		base.NotifyPropertyChanged("PunchGroupTools");
	}

	private void RefreshDiePreviews()
	{
		this.DieGroupTools.Clear();
		foreach (DieProfileViewModel item in from p in this.ToolBase.DieProfiles
			where p.Implemented && p.GroupID == this.SelectedDieGroup?.ID
			select p into i
			orderby i.Priority
			select i)
		{
			this.DieGroupTools.Add(new DieGroupToolViewModel(item, this._machine.BendMachine, this._drawToolProfiles, 50.0, 50.0));
		}
		this.DieGroupTools = this.DieGroupTools.OrderBy((DieGroupToolViewModel p) => p.Profile.Priority).ToObservableCollection();
		base.NotifyPropertyChanged("DieGroupTools");
	}

	private void DrawPunch()
	{
		if (this.SelectedPunchProfile != null)
		{
			this._drawToolProfiles.LoadPunchPreview2D(this.SelectedPunchProfile.PunchProfile, (Canvas)this.ImagePunch, this._machine.BendMachine);
		}
		else
		{
			((Canvas)this.ImagePunch).Children.Clear();
		}
		base.NotifyPropertyChanged("ImagePunch");
	}

	private void DrawDie()
	{
		if (this.SelectedDieProfile != null)
		{
			this._drawToolProfiles.LoadDiePreview2D(this.SelectedDieProfile.DieProfile, (Canvas)this.ImageDie, this._machine.BendMachine);
		}
		else
		{
			((Canvas)this.ImageDie).Children.Clear();
		}
		base.NotifyPropertyChanged("ImageDie");
	}

	private void ImagePunchOnLoaded(object sender, RoutedEventArgs e)
	{
		this.DrawPunch();
	}

	private void ImageDieOnLoaded(object sender, RoutedEventArgs e)
	{
		this.DrawDie();
	}

	public void SetActive(bool active)
	{
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
		if (!e.Handled && this._shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			this.Close();
			e.Handle();
		}
	}

	public new bool Close()
	{
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		base.Close();
		return true;
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
	}

	public void MouseEnterCommand()
	{
		base.Opacity = base.OpacityMax;
	}

	public void MouseLeaveCommand()
	{
		base.Opacity = base.OpacityMin;
	}

	private void OK(object param)
	{
		this.Close();
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}
}

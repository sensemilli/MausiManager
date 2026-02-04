using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.GuiWpf.TabBend.OpacityControl;

public class OpacityControlViewModel : ViewModelBase
{
	public class ProfileSelection : ViewModelBase
	{
		private ICommand _loadProfileCommand;

		private bool _selected;

		private readonly int _index;

		private readonly IProfileOpacitySelector _profileOpacitySelector;

		public bool Checked
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
				NotifyPropertyChanged("Checked");
			}
		}

		public ICommand SelectThisProfileCommand => _loadProfileCommand ?? (_loadProfileCommand = new RelayCommand((Action<object>)delegate
		{
			_profileOpacitySelector?.LoadOpacityProfileFromConfig(_index);
		}));

		public ProfileSelection(IProfileOpacitySelector profileOpacitySelector, int n)
		{
			_profileOpacitySelector = profileOpacitySelector;
			_index = n;
			Checked = false;
		}
	}

	public class OpacityFilter : ViewModelBase
	{
		private double opacity;

		private bool overwritten;

		private readonly OpacityControlViewModel transparencyControlViewModel;

		public string Tag { get; set; }

		public double Opacity
		{
			get
			{
				return opacity;
			}
			set
			{
				opacity = value;
				NotifyPropertyChanged("Opacity");
				if (!transparencyControlViewModel.DontSave)
				{
					transparencyControlViewModel.UpdateCurrentProfile(AffectedRoles, opacity);
				}
			}
		}

		public List<PartRole> AffectedRoles { get; set; }

		public OpacityFilter(string tag, OpacityControlViewModel transparencyControlViewModel, params PartRole[] affectedRoles)
		{
			Tag = tag;
			this.transparencyControlViewModel = transparencyControlViewModel;
			AffectedRoles = affectedRoles.ToList();
		}
	}

	private ICommand _addProfileCommand;

	private ICommand _removeProfileCommand;

	private ICommand _saveCameraStateCommand;

	private ICommand _eraseCameraStateCommand;

	private int _currentProfile;

	private int _numberOfProfiles;

	private IProfileOpacitySelector _profileOpacitySelector;

	private readonly ITranslator _translator;

	private Visibility _profileLocked;

	private bool _expanderExpanded;

	private Action<List<PartRole>, double> _updateAction;

	public ObservableCollection<OpacityFilter> OpacityFilters { get; set; }

	public ObservableCollection<ProfileSelection> ProfileSelections { get; set; }

	public int CurrentProfile
	{
		get
		{
			return _currentProfile;
		}
		set
		{
			_currentProfile = Math.Clamp(value, 0, ProfileSelections.Count - 1);
			for (int i = 0; i < ProfileSelections.Count; i++)
			{
				ProfileSelections[i].Checked = _currentProfile == i;
			}
		}
	}

	public int NumberOfProfiles
	{
		get
		{
			return _numberOfProfiles;
		}
		set
		{
			_numberOfProfiles = Math.Clamp(value, 1, 16);
			while (_numberOfProfiles != ProfileSelections.Count)
			{
				if (ProfileSelections.Count < _numberOfProfiles)
				{
					ProfileSelections.Add(new ProfileSelection(_profileOpacitySelector, ProfileSelections.Count));
				}
				else
				{
					ProfileSelections.Remove(ProfileSelections.Last());
				}
			}
		}
	}

	public Visibility ProfileControlVisibility
	{
		get
		{
			return _profileLocked;
		}
		set
		{
			_profileLocked = value;
			NotifyPropertyChanged("ProfileControlVisibility");
		}
	}

	public bool ExpanderExpanded
	{
		get
		{
			if (!_expanderExpanded)
			{
				return ForceExpand;
			}
			return true;
		}
		set
		{
			_expanderExpanded = value;
			NotifyPropertyChanged("ExpanderExpanded");
		}
	}

	public bool ForceExpand { get; set; }

	public bool DontSave { get; set; }

	public ICommand AddProfileCommand => _addProfileCommand ?? (_addProfileCommand = new RelayCommand(AddProfile, CanAddProfile));

	public ICommand RemoveProfileCommand => _removeProfileCommand ?? (_removeProfileCommand = new RelayCommand(RemoveProfile, CanRemoveProfile));

	public ICommand SaveCameraStateCommand => _saveCameraStateCommand ?? (_saveCameraStateCommand = new RelayCommand(SaveCameraState, CanSaveCameraState));

	public ICommand EraseCameraStateCommand => _eraseCameraStateCommand ?? (_eraseCameraStateCommand = new RelayCommand(EraseSavedCameraState, CanEraseSavedCameraState));

	public OpacityControlViewModel(IProfileOpacitySelector profileOpacitySelector, ITranslator translator)
	{
		_profileOpacitySelector = profileOpacitySelector;
		_translator = translator;
		OpacityFilters = new ObservableCollection<OpacityFilter>();
		OpacityFilters.Add(CreateFilter("All", (PartRole[])System.Enum.GetValues(typeof(PartRole))));
		OpacityFilters.Add(CreateFilter("BendModel", PartRole.BendModel));
		OpacityFilters.Add(CreateFilter("Frame", PartRole.MainFrame, PartRole.Beam, PartRole.BeamLeft, PartRole.BeamRight, PartRole.LTS_Table, PartRole.UTS_Beam));
		OpacityFilters.Add(CreateFilter("LowerTools", PartRole.Lower_Tool));
		OpacityFilters.Add(CreateFilter("UpperTools", PartRole.Upper_Tool));
		OpacityFilters.Add(CreateFilter("Finger", PartRole.LeftFinger, PartRole.RightFinger, PartRole.LeftFingerSupport, PartRole.RightFingerSupport, PartRole.TotalFingerSupport, PartRole.FingerConfigModel, PartRole.ToolConfigModel));
		List<PartRole> list = new List<PartRole>();
		for (int i = 27; i <= 43; i++)
		{
			list.Add((PartRole)i);
		}
		OpacityFilters.Add(CreateFilter("LiftingAid", list.ToArray()));
		OpacityFilters.Add(CreateFilter("AngleMeasurement", PartRole.AngleMeasurement));
		OpacityFilters.Add(CreateFilter("Unassigned", default(PartRole)));
		List<PartRole> list2 = ((PartRole[])System.Enum.GetValues(typeof(PartRole))).Where((PartRole x) => OpacityFilters.Count((OpacityFilter y) => y.AffectedRoles.Any((PartRole z) => z == x)) < 2).ToList();
		OpacityFilters.Add(CreateFilter("Other", list2.ToArray()));
		ProfileSelections = new ObservableCollection<ProfileSelection>();
	}

	public void Init(Action<List<PartRole>, double> updateAction, bool profileLocked, IProfileOpacitySelector profileOpacitySelector)
	{
		_updateAction = updateAction;
		ProfileControlVisibility = (profileLocked ? Visibility.Collapsed : Visibility.Visible);
		_profileOpacitySelector = profileOpacitySelector;
		if (profileOpacitySelector != null)
		{
			profileOpacitySelector.OpacityProfileChanged += ProfileChanged;
		}
	}

	private OpacityFilter CreateFilter(string tag, params PartRole[] affectedRoles)
	{
		return new OpacityFilter(_translator.Translate("OpacityControl.Type" + tag), this, affectedRoles);
	}

	public void ProfileChanged(int n, int numberOfProfiles, Dictionary<PartRole, double> overrides)
	{
		DontSave = true;
		NumberOfProfiles = numberOfProfiles;
		CurrentProfile = n;
		foreach (OpacityFilter opacityFilter in OpacityFilters)
		{
			List<double> list = opacityFilter.AffectedRoles.Select((PartRole x) => overrides[x]).Distinct().ToList();
			if (list.Count == 1)
			{
				opacityFilter.Opacity = list[0];
			}
		}
		DontSave = false;
	}

	public void AddProfile()
	{
		_profileOpacitySelector?.AddAProfile();
	}

	public bool CanAddProfile()
	{
		return ProfileSelections.Count < 16;
	}

	public void RemoveProfile()
	{
		_profileOpacitySelector?.RemoveCurrentProfile();
	}

	public bool CanRemoveProfile()
	{
		return ProfileSelections.Count > 1;
	}

	public void UpdateCurrentProfile(List<PartRole> affectedRoles, double opacity)
	{
		_updateAction(affectedRoles, opacity);
	}

	public void SaveCameraState()
	{
		_profileOpacitySelector?.SaveCameraState();
	}

	public bool CanSaveCameraState()
	{
		return _profileOpacitySelector?.CanSaveCameraState() ?? false;
	}

	public void EraseSavedCameraState()
	{
		_profileOpacitySelector?.EraseSavedCameraState();
	}

	public bool CanEraseSavedCameraState()
	{
		return _profileOpacitySelector?.CanEraseSavedCameraState() ?? false;
	}
}

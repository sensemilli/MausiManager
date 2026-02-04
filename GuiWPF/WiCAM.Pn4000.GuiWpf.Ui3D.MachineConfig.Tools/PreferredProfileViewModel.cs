using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.MachineComponents;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class PreferredProfileViewModel : ToolItemViewModelBase
{
	private readonly PreferredProfilesViewModel _baseVm;

	private int _material3DGroupID;

	private RadObservableCollection<AlternativeToolsetViewModel>? _alternativeToolsVm;

	public IPreferredProfile? PreferredProfile { get; private set; }

	public bool IsSelected { get; set; }

	public double Thickness { get; set; }

	public double ThicknessUi
	{
		get
		{
			return _baseVm.UnitConverter.Length.ToUi(Thickness, 2);
		}
		set
		{
			Thickness = _baseVm.UnitConverter.Length.FromUi(value);
		}
	}

	public int Material3DGroupID
	{
		get
		{
			return _material3DGroupID;
		}
		set
		{
			if (_material3DGroupID != value)
			{
				_material3DGroupID = value;
				NotifyPropertyChanged("Material3DGroupID");
			}
		}
	}

	public double MinRadius { get; set; }

	public double MinRadiusUi
	{
		get
		{
			return _baseVm.UnitConverter.Length.ToUi(MinRadius, 2);
		}
		set
		{
			MinRadius = _baseVm.UnitConverter.Length.FromUi(value);
		}
	}

	public double MaxRadius { get; set; }

	public double MaxRadiusUi
	{
		get
		{
			return _baseVm.UnitConverter.Length.ToUi(MaxRadius, 2);
		}
		set
		{
			MaxRadius = _baseVm.UnitConverter.Length.FromUi(value);
		}
	}

	public double MinAngle { get; set; }

	public double MinAngleUi
	{
		get
		{
			return _baseVm.UnitConverter.Angle.ToUi(MinAngle, 4);
		}
		set
		{
			MinAngle = _baseVm.UnitConverter.Angle.FromUi(value);
		}
	}

	public double MaxAngle { get; set; }

	public double MaxAngleUi
	{
		get
		{
			return _baseVm.UnitConverter.Angle.ToUi(MaxAngle, 4);
		}
		set
		{
			MaxAngle = _baseVm.UnitConverter.Angle.FromUi(value);
		}
	}

	public List<IPreferredProfileToolSet> AlternativeTools { get; private set; } = new List<IPreferredProfileToolSet>();

	public RadObservableCollection<AlternativeToolsetViewModel> AlternativeToolsVm => _alternativeToolsVm ?? (_alternativeToolsVm = new RadObservableCollection<AlternativeToolsetViewModel>(AlternativeTools.Select((IPreferredProfileToolSet x) => new AlternativeToolsetViewModel(x, _baseVm))));

	public PreferredProfileViewModel(PreferredProfilesViewModel baseVm, IPreferredProfile preferredProfile, Material3DGroupViewModel materialGroup3D)
	{
		_baseVm = baseVm;
		LoadFromPreferredProfile(preferredProfile);
	}

	public PreferredProfileViewModel(PreferredProfilesViewModel baseVm)
	{
		_baseVm = baseVm;
	}

	public bool CanSave()
	{
		if (PreferredProfile != null || _alternativeToolsVm != null)
		{
			RadObservableCollection<AlternativeToolsetViewModel>? alternativeToolsVm = _alternativeToolsVm;
			if (alternativeToolsVm == null || alternativeToolsVm.Count >= 1)
			{
				RadObservableCollection<AlternativeToolsetViewModel>? alternativeToolsVm2 = _alternativeToolsVm;
				if (alternativeToolsVm2 != null && !alternativeToolsVm2.All((AlternativeToolsetViewModel x) => x.CanSave()))
				{
					_baseVm.MachineVm.MessageLogGlobal.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.SaveErrorPreferredProfileInvalidEntries", Material3DGroupID, ThicknessUi);
					return false;
				}
				return true;
			}
		}
		_baseVm.MachineVm.MessageLogGlobal.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.SaveErrorPreferredProfileNoEntries", Material3DGroupID, ThicknessUi);
		return false;
	}

	public IPreferredProfile SaveToPreferredProfile()
	{
		if (PreferredProfile == null)
		{
			IPreferredProfile preferredProfile2 = (PreferredProfile = new PreferredProfile());
		}
		PreferredProfile.Thickness = Thickness;
		PreferredProfile.MaterialGroupID = Material3DGroupID;
		PreferredProfile.MinRadius = MinRadius;
		PreferredProfile.MaxRadius = MaxRadius;
		PreferredProfile.MinAngle = MinAngle;
		PreferredProfile.MaxAngle = MaxAngle;
		if (_alternativeToolsVm != null)
		{
			int priority = 1;
			PreferredProfile.SetAllAlternativeProfiles(AlternativeToolsVm.Select((AlternativeToolsetViewModel x) => x.CreatePreferredProfileToolSet(priority++, PreferredProfile)));
		}
		return PreferredProfile;
	}

	private void LoadFromPreferredProfile(IPreferredProfile profile)
	{
		PreferredProfile = profile;
		Thickness = PreferredProfile.Thickness;
		Material3DGroupID = PreferredProfile.MaterialGroupID;
		MinRadius = PreferredProfile.MinRadius;
		MaxRadius = PreferredProfile.MaxRadius;
		MinAngle = PreferredProfile.MinAngle;
		MaxAngle = PreferredProfile.MaxAngle;
		AlternativeTools = PreferredProfile.AlternativeTools.ToList();
	}

	public PreferredProfileViewModel Dupplicate()
	{
		PreferredProfileViewModel preferredProfileViewModel = new PreferredProfileViewModel(_baseVm);
		preferredProfileViewModel._material3DGroupID = _material3DGroupID;
		preferredProfileViewModel.Thickness = Thickness;
		preferredProfileViewModel.MinRadius = MinRadius;
		preferredProfileViewModel.MaxRadius = MaxRadius;
		preferredProfileViewModel.MinAngle = MinAngle;
		preferredProfileViewModel.MaxAngle = MaxAngle;
		preferredProfileViewModel.AlternativeToolsVm.AddRange(AlternativeToolsVm.Select((AlternativeToolsetViewModel x) => x.Dupplicate()));
		return preferredProfileViewModel;
	}
}

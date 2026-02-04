using System.Windows.Input;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class PunchGroupToolViewModel : GroupToolViewModel
{
	private PunchProfileViewModel _punchProfile;

	private ICommand _increasePriorityCommand;

	private ICommand _decreasePriorityCommand;

	private bool _isSelected;

	public bool IsSelected
	{
		get
		{
			return this._isSelected;
		}
		set
		{
			this._isSelected = value;
			base.NotifyPropertyChanged("IsSelected");
		}
	}

	public PunchProfileViewModel Profile
	{
		get
		{
			return this._punchProfile;
		}
		set
		{
			this._punchProfile = value;
			base.NotifyPropertyChanged("Profile");
		}
	}

	public ICommand IncreasePriorityCommand => this._increasePriorityCommand ?? (this._increasePriorityCommand = new RelayCommand(delegate
	{
		this.IncreasePriority();
	}));

	public ICommand DecreasePriorityCommand => this._decreasePriorityCommand ?? (this._decreasePriorityCommand = new RelayCommand(delegate
	{
		this.DecreasePriority();
	}));

	public PunchGroupToolViewModel(PunchProfileViewModel punchProfile, BendMachine bendMachine, IDrawToolProfiles drawToolProfiles, double width = 92.0, double height = 93.0)
		: base(width, height)
	{
		this.Profile = punchProfile;
		drawToolProfiles.LoadPunchPreview2D(this.Profile.PunchProfile, base.ImageProfile, bendMachine, showWorkingHeight: false);
	}

	private void IncreasePriority()
	{
		if (this.Profile.Priority < 5)
		{
			this.Profile.Priority++;
		}
	}

	private void DecreasePriority()
	{
		if (this.Profile.Priority > 1)
		{
			this.Profile.Priority--;
		}
	}
}

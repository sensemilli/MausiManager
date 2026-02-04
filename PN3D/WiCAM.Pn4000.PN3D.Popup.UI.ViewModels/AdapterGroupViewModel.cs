using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class AdapterGroupViewModel : GroupToolViewModel
{
	private AdapterProfileViewModel _adapterProfile;

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

	public AdapterProfileViewModel Profile
	{
		get
		{
			return this._adapterProfile;
		}
		set
		{
			this._adapterProfile = value;
			base.NotifyPropertyChanged("Profile");
		}
	}

	public AdapterGroupViewModel(AdapterProfileViewModel adapterProfile, BendMachine bendMachine, IDrawToolProfiles drawToolProfiles, double width = 92.0, double height = 93.0)
		: base(width, height)
	{
		this.Profile = adapterProfile;
		drawToolProfiles.LoadAdapterPreview2D(this.Profile.AdapterProfile, base.ImageProfile, bendMachine, showWorkingHeight: false);
	}
}

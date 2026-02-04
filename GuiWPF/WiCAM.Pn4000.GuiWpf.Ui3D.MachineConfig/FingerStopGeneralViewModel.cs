using System;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class FingerStopGeneralViewModel : ViewModelBase
{
	private FingerStopSettingsViewModel _fingerStopSettings;

	public Action<ChangedConfigType> DataChanged;

	public FingerStopSettingsViewModel FingerStopSettings
	{
		get
		{
			return _fingerStopSettings;
		}
		set
		{
			_fingerStopSettings = value;
			NotifyPropertyChanged("FingerStopSettings");
		}
	}

	public FingerStopGeneralViewModel(IFingerStopSettings settings)
	{
		FingerStopSettings = new FingerStopSettingsViewModel(settings);
	}

	public void Save(IFingerStopSettings saveTarget)
	{
		FingerStopSettings.Save(saveTarget);
		DataChanged?.Invoke(FingerStopSettings.Changed);
	}
}

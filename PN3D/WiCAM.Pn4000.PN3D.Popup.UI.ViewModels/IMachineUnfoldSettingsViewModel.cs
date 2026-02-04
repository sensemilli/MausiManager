using System;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public interface IMachineUnfoldSettingsViewModel
{
	public interface ISettingsUnfold
	{
		bool? IgnoreBendTable { get; set; }

		double? DefaultKFactor { get; set; }
	}

	ISettingsUnfold Settings { get; }

	void Init(IBendMachine machine, Action<ChangedConfigType> dataChangedAction);

	void Dispose();

	void Save();
}

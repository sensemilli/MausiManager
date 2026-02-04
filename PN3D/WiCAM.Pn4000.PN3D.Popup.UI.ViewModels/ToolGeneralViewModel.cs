using System;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ToolGeneralViewModel : ViewModelBase
{
	private ToolSettingsViewModel _toolSettings;

	public Action<ChangedConfigType> DataChanged;

	public ToolSettingsViewModel ToolSettings
	{
		get
		{
			return this._toolSettings;
		}
		set
		{
			this._toolSettings = value;
			base.NotifyPropertyChanged("ToolSettings");
		}
	}

	public ToolGeneralViewModel(BendMachine bendMachine)
	{
		this.ToolSettings = new ToolSettingsViewModel(bendMachine?.ToolSettings);
	}

	public void Save()
	{
		this.DataChanged?.Invoke(this.ToolSettings.Changed);
	}
}

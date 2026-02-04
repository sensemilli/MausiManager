using System;
using PPInterface;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

internal class MachineUnfoldSettingsViewModel : IMachineUnfoldSettingsViewModel
{
	public class SettingsUnfold : IMachineUnfoldSettingsViewModel.ISettingsUnfold
	{
		[TranslatedDisplayName("l_popup.MachineSettingUnfold.IgnoreBendTable")]
		[TranslatedDescription("l_popup.MachineSettingUnfold.IgnoreBendTableTt")]
		public bool? IgnoreBendTable { get; set; }

		[TranslatedDisplayName("l_popup.MachineSettingUnfold.DefaultKFactor")]
		[TranslatedDescription("l_popup.MachineSettingUnfold.DefaultKFactorTt")]
		public double? DefaultKFactor { get; set; }
	}

	private ChangedConfigType _changed;

	public Action<ChangedConfigType> DataChanged;

	public IBendMachine Machine { get; private set; }

	private Action<ChangedConfigType> _dataChanged { get; set; }

	public IMachineUnfoldSettingsViewModel.ISettingsUnfold Settings { get; } = new SettingsUnfold();

	public void Init(IBendMachine machine, Action<ChangedConfigType> dataChangedAction)
	{
		this.Machine = machine;
		this.Settings.DefaultKFactor = this.Machine?.UnfoldConfig?.DefaultKFactor;
		this.Settings.IgnoreBendTable = this.Machine?.UnfoldConfig?.IgnoreBendTable;
		this._dataChanged = dataChangedAction;
		this._changed = ChangedConfigType.NoChanges;
	}

	public void Dispose()
	{
	}

	public void Save()
	{
		this.Machine.UnfoldConfig.DefaultKFactor = this.Settings.DefaultKFactor;
		this.Machine.UnfoldConfig.IgnoreBendTable = this.Settings.IgnoreBendTable;
		this._dataChanged?.Invoke(this._changed);
	}
}

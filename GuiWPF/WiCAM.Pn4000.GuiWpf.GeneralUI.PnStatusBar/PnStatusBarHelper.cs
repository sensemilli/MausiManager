using System.Collections.ObjectModel;
using System.Windows.Controls;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar;

public class PnStatusBarHelper : ViewModelBase, IPnStatusBarHelper
{
	private IFactorio _factorio;

	public ObservableCollection<ContentControl> Slot { get; set; }

	public PnStatusBarHelper(IFactorio factorio)
	{
		_factorio = factorio;
		Slot = new ObservableCollection<ContentControl>();
		for (int i = 0; i < 9; i++)
		{
			Slot.Add(null);
		}
	}

	public void SetSlotControl<T>(int slot)
	{
		T val = _factorio.Resolve<T>();
		ContentControl contentControl = Slot[slot];
		if (contentControl != null && contentControl.DataContext is IPnStatusViewModel pnStatusViewModel)
		{
			pnStatusViewModel.SetActive(isActive: false);
		}
		Slot[slot] = val as ContentControl;
		contentControl = Slot[slot];
		if (contentControl != null && contentControl.DataContext is IPnStatusViewModel pnStatusViewModel2)
		{
			pnStatusViewModel2.SetActive(isActive: true);
		}
	}

	public void RemoveSlotControl(int slot)
	{
		ContentControl contentControl = Slot[slot];
		if (contentControl != null && contentControl.DataContext is IPnStatusViewModel pnStatusViewModel)
		{
			pnStatusViewModel.SetActive(isActive: false);
		}
		Slot[slot] = null;
	}
}

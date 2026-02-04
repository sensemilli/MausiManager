using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot8Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot8Control
{
	public PnStatusBarPKernelSlot8Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 8)
	{
	}
}

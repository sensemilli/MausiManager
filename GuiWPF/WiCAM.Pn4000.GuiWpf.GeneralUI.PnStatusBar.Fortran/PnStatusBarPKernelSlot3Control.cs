using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot3Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot3Control
{
	public PnStatusBarPKernelSlot3Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 3)
	{
	}
}

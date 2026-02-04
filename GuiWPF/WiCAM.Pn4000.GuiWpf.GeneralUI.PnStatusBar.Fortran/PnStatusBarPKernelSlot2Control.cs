using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot2Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot2Control
{
	public PnStatusBarPKernelSlot2Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 2)
	{
	}
}

using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot5Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot5Control
{
	public PnStatusBarPKernelSlot5Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 5)
	{
	}
}

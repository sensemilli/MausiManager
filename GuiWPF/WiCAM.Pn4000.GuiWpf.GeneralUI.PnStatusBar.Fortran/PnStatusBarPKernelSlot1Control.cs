using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot1Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot1Control
{
	public PnStatusBarPKernelSlot1Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 1)
	{
	}
}

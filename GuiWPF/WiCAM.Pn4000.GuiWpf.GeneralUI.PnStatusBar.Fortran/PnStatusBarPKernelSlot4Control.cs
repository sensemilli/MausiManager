using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PnStatusBarPKernelSlot4Control : PnStatusBarPKernelControl, IPnStatusBarPKernelSlot4Control
{
	public PnStatusBarPKernelSlot4Control(IPKernelStatusBarViewModel viewModel)
		: base(viewModel, 4)
	{
	}
}

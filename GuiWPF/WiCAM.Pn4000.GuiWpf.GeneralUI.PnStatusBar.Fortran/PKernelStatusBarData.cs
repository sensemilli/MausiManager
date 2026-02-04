using System.Collections.Generic;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PKernelStatusBarData : IPKernelStatusBarData
{
	public bool isVisible { get; set; }

	public string MainStatus { get; set; } = string.Empty;

	public List<string> SubStatusList { get; set; } = new List<string>();
}

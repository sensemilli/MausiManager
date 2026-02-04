using System.Windows.Shell;
using WiCAM.Pn4000.GuiContracts;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar;

internal class MainWindowTaskbarItemInfo : IMainWindowTaskbarItemInfo
{
	public TaskbarItemInfo TaskbarItemInfo { get; } = new TaskbarItemInfo();
}

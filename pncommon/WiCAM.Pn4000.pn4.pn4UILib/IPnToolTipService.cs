using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WiCAM.Pn4000.Contracts.PnCommands;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public interface IPnToolTipService
{
	void SetTooltip(FrameworkElement element, IPnCommand pnCommand);

	void SetTooltip(FrameworkElement element, string l1, string l2, string dectription, ImageSource img);

	void UpdateTooltipWithPreview(FrameworkElement element, ImageSource preview);

	ToolTip GetToolTip(IPnCommand pnCommand);
}

using System.Windows;
using System.Windows.Controls;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Ribbon;

public class NewPnRibbonBase : UserControl
{
	public DataTemplate TabEntityTemplate { get; set; }

	public DataTemplate SimpleButtonTemplate { get; set; }

	public DataTemplate GroupBoxTemplate { get; set; }
}

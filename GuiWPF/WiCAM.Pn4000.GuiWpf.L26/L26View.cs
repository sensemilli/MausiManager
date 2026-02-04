using System.Windows;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.L26;

public partial class L26View : Window, IComponentConnector
{
	public L26View(L26ViewModel model)
	{
		base.DataContext = model;
		InitializeComponent();
	}
}

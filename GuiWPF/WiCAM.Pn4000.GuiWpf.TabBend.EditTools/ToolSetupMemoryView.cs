using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public partial class ToolSetupMemoryView : UserControl, IComponentConnector
{
	private ToolSetupMemoryViewModel _vm;

	public ToolSetupMemoryView(ToolSetupMemoryViewModel vm)
	{
		_vm = vm;
		base.DataContext = vm;
		InitializeComponent();
	}
}

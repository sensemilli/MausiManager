using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public partial class PopupBendParameterView : UserControl, IComponentConnector
{
	public PopupBendParameterView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		((PopupBendParameterViewModel)base.DataContext).SetEditorEnableRules();
	}
}

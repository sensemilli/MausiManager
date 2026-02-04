using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public partial class BendDataBaseView : UserControl, IComponentConnector
{
	public BendDataBaseView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		((BendDataBaseViewModel)base.DataContext)?.SetEditorEnableRules();
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as BendDataBaseViewModel).SelectedItems = elementViewContainer.GridView.SelectedItems;
		}
	}
}

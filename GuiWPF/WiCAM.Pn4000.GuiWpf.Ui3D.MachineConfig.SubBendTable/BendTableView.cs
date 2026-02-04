using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public partial class BendTableView : UserControl, IComponentConnector
{
	private BendTableViewModel _vm;

	public BendTableView()
	{
		InitializeComponent();
	}

	private void DataControl_OnSelectionChanged(object? sender, SelectionChangeEventArgs e)
	{
	}

	private void GridViewDataControl_OnDeleting(object? sender, GridViewDeletingEventArgs e)
	{
		if (sender is RadGridView { ItemsSource: RadObservableCollection<BendTableEntry> itemsSource })
		{
			itemsSource.RemoveRange(e.Items.Select((object x) => x as BendTableEntry).ToArray());
			((CancelRoutedEventArgs)e).Cancel = true;
		}
	}

	private void GridViewDataControl_OnAddingNewDataItem(object? sender, GridViewAddingNewEventArgs e)
	{
		e.NewObject = _vm.CreateBendTableEntry();
	}

	private void BendTableView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_vm = e.NewValue as BendTableViewModel;
	}
}

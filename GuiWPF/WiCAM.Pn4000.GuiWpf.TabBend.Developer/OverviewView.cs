using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal partial class OverviewView : UserControl, IComponentConnector
{
	public OverviewView()
	{
		InitializeComponent();
	}

	private void RadTreeView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is RadTreeView { DataContext: OverviewViewModel dataContext } radTreeView)
		{
			dataContext.SelectedItem = radTreeView.SelectedItem;
		}
	}
}

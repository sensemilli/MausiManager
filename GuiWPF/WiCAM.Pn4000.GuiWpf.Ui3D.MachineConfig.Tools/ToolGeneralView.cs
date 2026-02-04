using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public partial class ToolGeneralView : UserControl, IComponentConnector
{
	public ToolGeneralView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGeneralViewModel).SelectedToolLists = ToolLists.SelectedItems.Cast<ToolListViewModel>().ToObservableCollection();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGeneralViewModel).SelectedToolLists = ToolLists.SelectedItems.Cast<ToolListViewModel>().ToObservableCollection();
		}
	}
}

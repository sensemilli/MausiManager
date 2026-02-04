using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public partial class MachineToolsUpperAdaptersView : UserControl, IComponentConnector
{
	public MachineToolsUpperAdaptersView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		LbDieParts.FilterDescriptors.Clear();
		if (base.DataContext != null)
		{
			(base.DataContext as UpperAdaptersViewModel).SelectedProfiles = LbDieProfiles.SelectedItems.Select((object x) => (UpperAdapterViewModel)x).ToObservableCollection();
			((UpperAdaptersViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void Selector_OnSelectionChangedParts(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as UpperAdaptersViewModel).SelectedParts = LbDieParts.SelectedItems.Select((object x) => (LowerToolPieceViewModel)x).ToObservableCollection();
			((UpperAdaptersViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as UpperAdaptersViewModel).SelectedProfiles = LbDieProfiles.SelectedItems.Select((object x) => (UpperAdapterViewModel)x).ToObservableCollection();
			(base.DataContext as UpperAdaptersViewModel).SelectedParts = LbDieParts.SelectedItems.Select((object x) => (LowerToolPieceViewModel)x).ToObservableCollection();
		}
	}

	private void ProfileContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			UpperAdaptersViewModel upperAdaptersViewModel = base.DataContext as UpperAdaptersViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				upperAdaptersViewModel.SelectedProfile = gridViewRow?.DataContext as UpperAdapterViewModel;
			}
		}
	}

	private void PartContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			UpperAdaptersViewModel upperAdaptersViewModel = base.DataContext as UpperAdaptersViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				upperAdaptersViewModel.SelectedPart = gridViewRow?.DataContext as LowerToolPieceViewModel;
			}
		}
	}
}

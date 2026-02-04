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

public partial class MachineToolsUpperToolsView : UserControl, IComponentConnector
{
	public MachineToolsUpperToolsView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (((UpperToolsViewModel)base.DataContext)?.SelectedProfile != null)
		{
			LbProfiles.ScrollIntoView(((UpperToolsViewModel)base.DataContext)?.SelectedProfile);
		}
		LbParts.FilterDescriptors.Clear();
		if (base.DataContext != null)
		{
			(base.DataContext as UpperToolsViewModel).SelectedProfiles = LbProfiles.SelectedItems.Select((object x) => (UpperToolViewModel)x).ToObservableCollection();
			((UpperToolsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void Selector_OnSelectionChangedParts(object sender, SelectionChangeEventArgs e)
	{
		if (((UpperToolsViewModel)base.DataContext)?.SelectedPart != null)
		{
			LbParts.ScrollIntoView(((UpperToolsViewModel)base.DataContext)?.SelectedPart);
		}
		if (base.DataContext != null)
		{
			(base.DataContext as UpperToolsViewModel).SelectedParts = LbParts.SelectedItems.Select((object x) => (UpperToolPieceViewModel)x).ToObservableCollection();
			((UpperToolsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as UpperToolsViewModel).SelectedProfiles = LbProfiles.SelectedItems.Select((object x) => (UpperToolViewModel)x).ToObservableCollection();
			(base.DataContext as UpperToolsViewModel).SelectedParts = LbParts.SelectedItems.Select((object x) => (UpperToolPieceViewModel)x).ToObservableCollection();
		}
	}

	private void ProfileContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			UpperToolsViewModel upperToolsViewModel = base.DataContext as UpperToolsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				upperToolsViewModel.SelectedProfile = gridViewRow?.DataContext as UpperToolViewModel;
			}
		}
	}

	private void PartContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			UpperToolsViewModel upperToolsViewModel = base.DataContext as UpperToolsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				upperToolsViewModel.SelectedPart = gridViewRow?.DataContext as UpperToolPieceViewModel;
			}
		}
	}

	private void DiskContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			_ = base.DataContext;
			if ((sender as RadContextMenu)?.GetClickedElement<GridViewRow>() == null)
			{
				e.Handled = true;
			}
		}
	}
}

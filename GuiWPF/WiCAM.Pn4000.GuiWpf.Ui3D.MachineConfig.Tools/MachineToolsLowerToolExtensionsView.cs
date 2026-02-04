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

public partial class MachineToolsLowerToolExtensionsView : UserControl, IComponentConnector
{
	public MachineToolsLowerToolExtensionsView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		LbDieParts.FilterDescriptors.Clear();
		if (base.DataContext != null)
		{
			(base.DataContext as LowerToolExtensionsViewModel).SelectedProfiles = LbDieProfiles.SelectedItems.Select((object x) => (LowerToolExtensionViewModel)x).ToObservableCollection();
			((LowerToolExtensionsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void Selector_OnSelectionChangedParts(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as LowerToolExtensionsViewModel).SelectedParts = LbDieParts.SelectedItems.Select((object x) => (LowerToolExtensionPieceViewModel)x).ToObservableCollection();
			((LowerToolExtensionsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as LowerToolExtensionsViewModel).SelectedProfiles = LbDieProfiles.SelectedItems.Select((object x) => (LowerToolExtensionViewModel)x).ToObservableCollection();
			(base.DataContext as LowerToolExtensionsViewModel).SelectedParts = LbDieParts.SelectedItems.Select((object x) => (LowerToolExtensionPieceViewModel)x).ToObservableCollection();
		}
	}

	private void ProfileContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			LowerToolExtensionsViewModel lowerToolExtensionsViewModel = base.DataContext as LowerToolExtensionsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				lowerToolExtensionsViewModel.SelectedProfile = gridViewRow?.DataContext as LowerToolExtensionViewModel;
			}
		}
	}

	private void PartContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			LowerToolExtensionsViewModel lowerToolExtensionsViewModel = base.DataContext as LowerToolExtensionsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				lowerToolExtensionsViewModel.SelectedPart = gridViewRow?.DataContext as LowerToolExtensionPieceViewModel;
			}
		}
	}
}

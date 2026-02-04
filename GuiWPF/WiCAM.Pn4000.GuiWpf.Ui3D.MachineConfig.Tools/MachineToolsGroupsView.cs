using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public partial class MachineToolsGroupsView : UserControl, IComponentConnector
{
	public MachineToolsGroupsView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGroupsViewModel).SelectedPunchGroups = LbPunchGroups.SelectedItems;
			(base.DataContext as ToolGroupsViewModel).SelectedDieGroups = LbDieGroups.SelectedItems;
			((ToolGroupsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGroupsViewModel).SelectedPunchGroups = LbPunchGroups.SelectedItems;
			(base.DataContext as ToolGroupsViewModel).SelectedDieGroups = LbDieGroups.SelectedItems;
		}
	}

	private void PunchGroupContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			ToolGroupsViewModel toolGroupsViewModel = base.DataContext as ToolGroupsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				toolGroupsViewModel.SelectedPunchGroup = gridViewRow?.DataContext as UpperToolGroupViewModel;
			}
		}
	}

	private void DieGroupContextMenu_Opened(object sender, RoutedEventArgs e)
	{
		if (base.DataContext != null)
		{
			ToolGroupsViewModel toolGroupsViewModel = base.DataContext as ToolGroupsViewModel;
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				e.Handled = true;
			}
			else
			{
				toolGroupsViewModel.SelectedDieGroup = gridViewRow?.DataContext as LowerToolGroupViewModel;
			}
		}
	}


}

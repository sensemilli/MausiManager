using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Views;

public partial class ToolGroupsView : UserControl, IComponentConnector
{
	public ToolGroupsView()
	{
		this.InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGroupsViewModel).SelectedPunchGroups = this.LbPunchGroups.SelectedItems;
			(base.DataContext as ToolGroupsViewModel).SelectedDieGroups = this.LbDieGroups.SelectedItems;
			((ToolGroupsViewModel)base.DataContext).SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext != null)
		{
			(base.DataContext as ToolGroupsViewModel).SelectedPunchGroups = this.LbPunchGroups.SelectedItems;
			(base.DataContext as ToolGroupsViewModel).SelectedDieGroups = this.LbDieGroups.SelectedItems;
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
				toolGroupsViewModel.SelectedPunchGroup = gridViewRow?.DataContext as PunchGroupViewModel;
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
				toolGroupsViewModel.SelectedDieGroup = gridViewRow?.DataContext as DieGroupViewModel;
			}
		}
	}


}

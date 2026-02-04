using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public partial class PreferredProfilesView : UserControl, IComponentConnector
{
	public PreferredProfilesView()
	{
		InitializeComponent();
	}

	private void SetDefaultFilter()
	{
		double thickness = (base.DataContext as PreferredProfilesViewModel).Thickness;
		int? material3DGroupID = (base.DataContext as PreferredProfilesViewModel).Material3DGroupID;
		if (material3DGroupID.HasValue)
		{
			Telerik.Windows.Controls.GridViewColumn gridViewColumn = LbProfiles.GridView.Columns["ThicknessUi"];
			Telerik.Windows.Controls.GridViewColumn gridViewColumn2 = LbProfiles.GridView.Columns["Material3DGroupID"];
			IColumnFilterDescriptor columnFilterDescriptor = gridViewColumn.ColumnFilterDescriptor;
			IColumnFilterDescriptor columnFilterDescriptor2 = gridViewColumn2.ColumnFilterDescriptor;
			LbProfiles.GridView.FilterDescriptors.SuspendNotifications();
			columnFilterDescriptor.FieldFilter.Filter1.Operator = FilterOperator.IsEqualTo;
			columnFilterDescriptor.FieldFilter.Filter1.Value = thickness;
			columnFilterDescriptor2.FieldFilter.Filter1.Operator = FilterOperator.IsEqualTo;
			columnFilterDescriptor2.FieldFilter.Filter1.Value = material3DGroupID.Value;
			columnFilterDescriptor2.FieldFilter.LogicalOperator = FilterCompositionLogicalOperator.Or;
			columnFilterDescriptor2.FieldFilter.Filter2.Operator = FilterOperator.IsEqualTo;
			columnFilterDescriptor2.FieldFilter.Filter2.Value = -1;
			LbProfiles.GridView.FilterDescriptors.ResumeNotifications();
		}
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (base.DataContext is PreferredProfilesViewModel preferredProfilesViewModel)
		{
			preferredProfilesViewModel.SelectedItems = LbProfiles.GridView.SelectedItems;
			preferredProfilesViewModel.SetEditorEnableRules();
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.DataContext is PreferredProfilesViewModel preferredProfilesViewModel)
		{
			preferredProfilesViewModel.SelectedItems = LbProfiles.GridView.SelectedItems;
			SetDefaultFilter();
		}
	}

	private void RadContextMenu_Opening(object sender, RadRoutedEventArgs e)
	{
		if (base.DataContext is PreferredProfilesViewModel preferredProfilesViewModel)
		{
			GridViewRow gridViewRow = (sender as RadContextMenu)?.GetClickedElement<GridViewRow>();
			if (gridViewRow == null)
			{
				((RoutedEventArgs)(object)e).Handled = true;
			}
			else
			{
				preferredProfilesViewModel.SelectedProfile = gridViewRow?.DataContext as PreferredProfileViewModel;
			}
		}
	}

	private void GridViewDataControl_OnAddingToolSet(object? sender, GridViewAddingNewEventArgs e)
	{
		if (base.DataContext is PreferredProfilesViewModel baseVm)
		{
			e.NewObject = new AlternativeToolsetViewModel(baseVm);
		}
	}

	private void GridViewDataControl_OnAddingPreferredProfile(object? sender, GridViewAddingNewEventArgs e)
	{
		if (base.DataContext is PreferredProfilesViewModel baseVm)
		{
			e.NewObject = new PreferredProfileViewModel(baseVm);
		}
	}
}

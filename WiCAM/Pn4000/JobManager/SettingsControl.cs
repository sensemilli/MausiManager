using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public partial class SettingsControl : UserControl, IComponentConnector
{
	public SettingsControl()
	{
		InitializeComponent();
	}

	private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		DataGrid dataGrid = sender as DataGrid;
		IInputElement inputElement = dataGrid.InputHitTest(e.GetPosition(dataGrid));
		if (inputElement == null)
		{
			return;
		}
		DataGridRow dataGridRow = WpfVisualHelper.FindVisualParent<DataGridRow>(inputElement as UIElement);
		if (dataGridRow == null)
		{
			return;
		}
		try
		{
			ColumnConfigurationInfo columnConfigurationInfo = (ColumnConfigurationInfo)dataGridRow.Item;
			DataGridCell dataGridCell = WpfVisualHelper.FindVisualParent<DataGridCell>(inputElement as UIElement);
			if (dataGridCell != null && dataGridCell.Column.DisplayIndex <= 0)
			{
				if (!columnConfigurationInfo.IsDefault)
				{
					columnConfigurationInfo.IsSelected = !columnConfigurationInfo.IsSelected;
				}
				else
				{
					e.Handled = true;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}
}

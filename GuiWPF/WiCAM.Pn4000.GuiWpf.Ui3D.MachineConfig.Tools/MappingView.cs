using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public partial class MappingView : UserControl, IComponentConnector
{
	public MappingView()
	{
		InitializeComponent();
	}

	private void ToolMappings_OnPastingCellClipboardContent(object? sender, GridViewCellClipboardEventArgs e)
	{
		GridViewCellInfo cell = e.Cell;
		if (!(base.DataContext is MappingViewModel mappingViewModel) || !(cell.Item is MappingViewModel.MappingVm mapping))
		{
			return;
		}
		string text = e.Value?.ToString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (cell.Column == MultiToolIdColumn && int.TryParse(text, out var result))
			{
				mappingViewModel.SetMultiToolById(mapping, result);
			}
			if (cell.Column == MultiToolNameColumn)
			{
				mappingViewModel.SetMultiToolByName(mapping, text);
			}
			if (cell.Column == ToolIdColumn && int.TryParse(text, out var result2))
			{
				mappingViewModel.SetToolById(mapping, result2);
			}
			if (cell.Column == ToolNameColumn)
			{
				mappingViewModel.SetToolByName(mapping, text);
			}
		}
	}

	private void MountIdMappings_OnPastingCellClipboardContent(object? sender, GridViewCellClipboardEventArgs e)
	{
		GridViewCellInfo cell = e.Cell;
		if (base.DataContext is MappingViewModel mappingViewModel && cell.Item is MappingViewModel.MappingVm mapping)
		{
			string text = e.Value?.ToString();
			if (!string.IsNullOrWhiteSpace(text) && cell.Column == MountIdColumn && int.TryParse(text, out var result))
			{
				mappingViewModel.SetMountById(mapping, result);
			}
		}
	}
}

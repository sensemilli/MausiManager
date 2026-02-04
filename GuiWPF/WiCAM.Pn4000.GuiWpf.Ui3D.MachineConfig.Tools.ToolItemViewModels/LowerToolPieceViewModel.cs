using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class LowerToolPieceViewModel : ToolPieceViewModel
{
	public LowerToolPieceViewModel(MachineToolsViewModel machineToolsViewModel, ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length = 0.0, int amount = 0)
		: base(machineToolsViewModel, currentToolList, multiTool, fileName, length, amount)
	{
	}

	public LowerToolPieceViewModel(ToolListViewModel currentToolList, MultiToolViewModel multiTool, IEnumerable<AliasPieceViewModel> aliasPieces, int totalAmount, IToolPieceProfile part, MachineToolsViewModel machineToolsViewModel)
		: base(machineToolsViewModel, currentToolList, multiTool, aliasPieces, totalAmount, part)
	{
	}

	public new IToolPieceProfile Save(IToolPieceProfile toolPieceProfile)
	{
		if (toolPieceProfile == null)
		{
			toolPieceProfile = base._toolStorage.CreateDiePiece(base.MultiTool.MultiToolProfile);
		}
		base.Save(toolPieceProfile);
		return toolPieceProfile;
	}
}

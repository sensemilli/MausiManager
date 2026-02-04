using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.GuiWpf.Services;

public interface IModelStyleProvider
{
	Color ToolPieceFaceColor { get; }

	Color ToolPieceEdgeColor { get; }

	Color ToolPieceHighlightFaceColor { get; }

	Color ToolPieceHighlightEdgeColor { get; }

	float ToolPieceHighlightEdgeWidth { get; }

	Color ToolPieceFaceColorError { get; }

	Color ToolPieceEdgeColorError { get; }

	Color ToolPieceHighlightFaceColorError { get; }

	Color ToolPieceHighlightEdgeColorError { get; }
}

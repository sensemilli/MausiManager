using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal interface IEditToolsAddPieceVisualizer
{
	bool IsActive { get; }

	void ColorModelParts(IPaintTool painter);

	void StartAddingPieces(bool addLeft);

	void StartAddingAdapters();

	void StartAddingExtensions(IToolProfile adapter);

	void AddPiece(IToolPieceProfile profile, bool addLeft);

	void AddAdapter(IAdapterProfile profile);

	void AddExtension(IDieFoldExtentionProfile profile);

	void Stop();
}

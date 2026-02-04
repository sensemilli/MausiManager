using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.GuiWpf.Services;

internal class ModelStyleProvider : IModelStyleProvider
{
	private readonly IConfigProvider _configProvider;

	public Color ToolPieceFaceColor { get; private set; }

	public Color ToolPieceEdgeColor { get; } = Color.Black;

	public Color ToolPieceHighlightFaceColor { get; private set; }

	public Color ToolPieceHighlightEdgeColor { get; private set; }

	public float ToolPieceHighlightEdgeWidth => 5f;

	public Color ToolPieceFaceColorError { get; private set; }

	public Color ToolPieceEdgeColorError { get; } = Color.Black;

	public Color ToolPieceHighlightFaceColorError { get; private set; }

	public Color ToolPieceHighlightEdgeColorError { get; private set; }

	public ModelStyleProvider(IConfigProvider configProvider)
	{
		_configProvider = configProvider;
		Refresh();
	}

	private void Refresh()
	{
		ModelColors3DConfig modelColors3DConfig = _configProvider.InjectOrCreate<ModelColors3DConfig>();
		ToolPieceFaceColor = Convert(modelColors3DConfig.ToolFaceColor);
		ToolPieceHighlightFaceColor = Convert(modelColors3DConfig.ToolHighlightFaceColor);
		ToolPieceHighlightEdgeColor = Convert(modelColors3DConfig.ToolHighlightEdgeColor);
		ToolPieceFaceColorError = Convert(modelColors3DConfig.ToolFaceColorError);
		ToolPieceHighlightFaceColorError = Convert(modelColors3DConfig.ToolHighlightFaceColorError);
		ToolPieceHighlightEdgeColorError = Convert(modelColors3DConfig.ToolHighlightEdgeColorError);
	}

	private static Color Convert(CfgColor color)
	{
		return new Color(color.R, color.G, color.B, color.A);
	}
}

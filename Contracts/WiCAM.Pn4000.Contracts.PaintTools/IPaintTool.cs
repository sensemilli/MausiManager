using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.PaintTools;

public interface IPaintTool
{
	void FrameStart();

	void FrameApply()
	{
		this.FrameApply(out HashSet<Model> _, out HashSet<Shell> _);
	}

	void FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);

	void SetFaceColorInShell(Face face, Color? highlightColor);

	void SetEdgeColorInShell(FaceHalfEdge edge, Color? highlightColor, float? width = null);

	void SetFaceColorAlpha(Face? face, Model model, float? alpha);

	void SetFaceColor(Face? face, Model model, Color? color);

	void SetEdgeColor(FaceHalfEdge? edge, Model model, Color? color, float? width = null);

	void SetModelVisibility(Model? model, bool visible, bool applyToSubModels = false);

	void SetSubModelsVisibility(Model? model, bool visible);

	void SetModelOpacity(Model? model, double? opacity, bool applyToSubModels = false);

	void SetModelFaceColor(Model? model, Color? color, bool applyToSubModels = false);

	void SetModelEdgeColor(Model? model, Color? color, float? lineWidth = null, bool applyToSubModels = false);

	void SetModelColors(Model? model, Color? colorFace, Color? colorEdge, float? lineWidth, bool applyToSubModels = false);
}

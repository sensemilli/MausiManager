using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.PN3D.CAD;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold;

public interface IModelForPnStdPreparation
{
	Cad2DDatabase Db2D { get; }

	bool ErrorDetected { get; }

	void Apply2D(IDoc3d doc, Model model, Face faceSelected, Model faceModelSelected, bool bendLines, bool removeProjectionHoles, bool includeZeroBorders);
}

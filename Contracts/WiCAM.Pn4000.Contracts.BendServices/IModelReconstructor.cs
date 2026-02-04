using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.BendServices;

public interface IModelReconstructor
{
	Model AnalyzePartReconstructive(Shell shell, Model shellModel, double thickness, out int usedFirstFaceGroupId, out int usedFirstFaceGroupSide, Face startFace);
}

using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ITransformState
{
	void SetBend(FaceGroup fg, double kFactor, double step, bool modifyVertices, bool modifyTransforms);

	void SetBend(FaceGroup fg, double unfoldBendLength, double kFactor, double step, bool modifyVertices, bool modifyTransforms);

	void SetPartTransform(Matrix4d partRootTransform, bool modifyTransforms = true);

	void ApplyModifications();

	void ApplyAxisModifications();

	void ApplyUnfoldModifications();

	void ApplyBends(ITransformState source);

	void SetAxisMotions(IBendMachineGeometry? machineGeometry);

	void SetStateTo(ITransformState destination);

	ITransformState Copy();
}

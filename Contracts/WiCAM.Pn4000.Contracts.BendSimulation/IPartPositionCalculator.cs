using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IPartPositionCalculator
{
	Matrix4d GetPartPositionInToolStationDefault(double springStep, Vector3d posWorld, FaceGroup fgBend, Model fgModel, bool isReversedGeometry, double workingHeightDie, Matrix4d partTransform, double lowerToolSystemWorkingHeight, double springHeight);

	Matrix4d GetBasicHemTransform(FaceGroup g, Model gModel, Vector3d offsetBend, double offset, double stepRescaled, bool isReversedGeometry, bool originLowerLeg);
}

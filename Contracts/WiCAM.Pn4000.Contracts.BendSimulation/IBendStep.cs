using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IBendStep : ISimulationStep
{
	double CalculateHeightOffsetByAngle(double angle);

	double CalculateHeightOffsetAtStep(double step);

	Vector3d CalculateLegBox();
}

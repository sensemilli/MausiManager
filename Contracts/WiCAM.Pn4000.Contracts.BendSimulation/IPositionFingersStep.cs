using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IPositionFingersStep : ISimulationStep
{
	void CalculateMotionsLeftFinger(Vector3d? stopPoint);

	void CalculateMotionsRightFinger(Vector3d? stopPoint);

	void ExecuteRightFinger(double step);

	void ExecuteLeftFinger(double step);

	new void Execute(double step, bool doCollisionChecks);
}

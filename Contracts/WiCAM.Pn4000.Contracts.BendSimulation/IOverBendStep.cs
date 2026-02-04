namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IOverBendStep : ISimulationStep
{
	double CalculateHeightOffset(double step, out double radiusOut, out double bendAngle);

	double CalculateOverbendAngleAndSetSpringBackFactor();
}

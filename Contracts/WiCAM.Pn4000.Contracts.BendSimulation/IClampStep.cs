namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IClampStep : ISimulationStep
{
	double CalculateClampAngle();

	double CalculateHeightOffsetAtStep(double step);

	double CalculateBoxHeight(double depth);
}

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IRelaxStep : ISimulationStep, IStepMinToolGap
{
	double CalculateHeightOffsetAtStep(double stepRescaled);
}

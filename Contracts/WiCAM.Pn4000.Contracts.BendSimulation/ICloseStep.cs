namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ICloseStep : ISimulationStep
{
	double CalculateMutePointOffset();
}

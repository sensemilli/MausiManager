namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationAngleMeasurementInfo
{
	bool UseAngleMeasurement { get; }

	double? AngleMeasurementPositionRel { get; }
}

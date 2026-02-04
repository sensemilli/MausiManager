namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationStepFilter
{
	bool PositionFingerStep { get; }

	bool LiftingAid { get; }

	bool AngleMeasurementStep { get; }

	bool PositionPartStep { get; }

	bool CloseStep { get; }

	bool TouchStep { get; }

	bool ClampStep { get; }

	bool RetractFingersStep { get; }

	bool OverbendStep { get; }

	bool RelaxStep { get; }

	bool OpenStep { get; }

	bool RemovePartStep { get; }

	bool SheetHandling { get; }
}

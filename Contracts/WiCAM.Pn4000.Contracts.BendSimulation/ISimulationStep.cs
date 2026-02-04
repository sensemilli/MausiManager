using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationStep
{
	ITransformState InitialTransformState { get; }

	bool IsGeometryModifier { get; }

	double StepIncrement { get; }

	int StepCountValidation { get; }

	double StepDurationSpeedIndependent { get; }

	bool IsSheetMetalHandlingStep { get; }

	ISimulationBendInfo BendInfo { get; }

	void Execute(double step);

	void Execute(double step, bool doCollisionChecks);

	void Undo();

	Matrix4d GetPartPositionInToolStation(double springStep);

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBend();

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetDirectNeighbours();
}

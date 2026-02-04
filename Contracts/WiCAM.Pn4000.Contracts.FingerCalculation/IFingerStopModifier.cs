using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.FingerCalculation;

public interface IFingerStopModifier
{
	IFingerStopPointInternal CreateFingerStopPoint(Vector3d p, PartRole fingerRole, IFingerStopCombination fingerStopCombination, Model part);

	void SnapFinger(PartRole fingerRole, ISimulationThread simulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos, bool snapLeft = true, bool snapRight = true);

	void SnapFingerUp(PartRole fingerRole, ISimulationThread simulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);

	void SnapFingerLeft(PartRole fingerRole, ISimulationThread simulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);

	void SnapFingerRight(PartRole fingerRole, ISimulationThread simulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);

	void ApplyPositions(PartRole primaryFinger, Vector3d left, Vector3d right, bool snapLeft, bool snapRight, ISimulationThread simulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);

	StopCombinationType EvaluateHitFaces(IFingerStop finger, Model part);

	void UpdateStopFaceCombinations(IPnBndDoc doc, IFingerStopPointInternal posLeft, IFingerStopPointInternal posRight);
}

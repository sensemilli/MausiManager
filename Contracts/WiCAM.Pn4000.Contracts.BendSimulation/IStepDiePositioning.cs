using WiCAM.Pn4000.BendModel.Base.Motions;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IStepDiePositioning : IBendingStep, ISimulationStep
{
	double GetIAxisOffset();

	void GetIPosLowerBeamOnPart(out MotionLinearAxis axisI, out double pos);
}

using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.BendPP;

public interface IAxisTranslator
{
	IBendMachine Machine { get; init; }

	(double r, double x, double z) LeftFingerWorldToPpCoords(Vector3d world, ISimulationBendInfo info);

	(double r, double x, double z) RightFingerWorldToPpCoords(Vector3d world, ISimulationBendInfo info);

	(double r, double x, double z) LeftFingerWorldToMachineUiCoords(Vector3d world, ISimulationBendInfo info);

	(double r, double x, double z) RightFingerWorldToMachineUiCoords(Vector3d world, ISimulationBendInfo info);

	Vector3d LeftFingerMachineUiToWorldCoords(double r, double x, double z, ISimulationBendInfo info);

	Vector3d RightFingerMachineUiToWorldCoords(double r, double x, double z, ISimulationBendInfo info);
}

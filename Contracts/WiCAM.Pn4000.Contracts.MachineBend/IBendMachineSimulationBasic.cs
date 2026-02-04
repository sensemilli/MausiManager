using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendMachineSimulationBasic
{
	IBendmachineDepricated BendMachine { get; }

	void RestorePrefferedTools();

	void CalculateBendSteps(bool calculateFingerPos, bool backToStart = true, bool toolConfigActive = false);

	void CheckConfig(IMessageDisplay messageDisplay);
}

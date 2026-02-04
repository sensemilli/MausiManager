using WiCAM.Pn4000.Contracts.PnCommands;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine;

public interface IUiBendMachineService
{
	void BendMachineConfig(IPnCommandArg arg);

	F2exeReturnCode SelectBendMachineFunc(IPnCommandArg arg);
}

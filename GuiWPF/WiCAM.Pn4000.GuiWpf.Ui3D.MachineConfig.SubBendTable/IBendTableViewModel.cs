using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public interface IBendTableViewModel
{
	void Init(IBendMachine bendMachine);

	void Save(IBendMachine? bendMachine);

	bool CanSave();
}

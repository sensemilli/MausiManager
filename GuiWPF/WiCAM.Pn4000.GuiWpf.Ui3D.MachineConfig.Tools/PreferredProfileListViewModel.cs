using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class PreferredProfileListViewModel
{
	public IPreferredProfileList? List { get; set; }

	public required string Description { get; set; }
}

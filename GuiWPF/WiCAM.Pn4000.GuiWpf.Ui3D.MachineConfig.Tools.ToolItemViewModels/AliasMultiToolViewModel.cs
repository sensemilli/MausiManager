using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class AliasMultiToolViewModel
{
	private readonly MachineToolsViewModel _machineVm;

	public IAliasMultiToolProfile? Profile { get; private set; }

	public int Id { get; private set; }

	public bool Deleted { get; set; }

	public AliasMultiToolViewModel(IAliasMultiToolProfile profile, MachineToolsViewModel machineVm)
	{
		_machineVm = machineVm;
		Profile = profile;
		Id = Profile.ID;
	}

	public AliasMultiToolViewModel(MachineToolsViewModel machineVm)
	{
		_machineVm = machineVm;
		Id = _machineVm.GenerateFakeId();
	}

	public void Save(IGlobalToolStorage storage)
	{
		if (Deleted)
		{
			if (Profile != null)
			{
				storage.DeleteAliasMultiToolProfile(Profile.ID);
			}
			return;
		}
		if (Profile == null)
		{
			IAliasMultiToolProfile aliasMultiToolProfile2 = (Profile = storage.CreateAliasMultiToolProfile());
		}
		Id = Profile.ID;
	}
}

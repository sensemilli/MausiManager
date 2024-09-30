using System.Collections.Generic;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager.Classes;

public interface ISettingsMachineReader
{
	List<MachineViewInfo> ReadMachines(List<int> allowedMachines);
}

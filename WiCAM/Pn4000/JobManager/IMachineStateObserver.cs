using System.Collections.Generic;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public interface IMachineStateObserver
{
	void Filter(IEnumerable<MachineViewInfo> machines);
}

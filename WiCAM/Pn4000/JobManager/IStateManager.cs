using System.Collections.Generic;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public interface IStateManager : IService
{
	void AttachImageObserver(IPreviewObserver manager);

	void AttachJobFilterObserver(IFilter filter);

	void AttachMachineObserver(IMachineStateObserver observer);

	void AttachPartFilterObserver(IFilterParts filter);

	void NotifyMachinesChanged(IEnumerable<MachineViewInfo> machines);

	void NotifyPreviewChanged(string path);
}

using System.Collections.Generic;
using WiCAM.Pn4000.Machine;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public class StateManager : IStateManager, IService
{
	private IJobManagerSettings _settings;

	private readonly List<IMachineStateObserver> _machineObservers = new List<IMachineStateObserver>();

	private readonly List<IPreviewObserver> _previewObservers = new List<IPreviewObserver>();

	private readonly List<IFilter> _jobFilterObservers = new List<IFilter>();

	private readonly List<IFilterParts> _partFilterObservers = new List<IFilterParts>();

	private readonly List<IFilterPlates> _plateFilterObservers = new List<IFilterPlates>();

	public StateManager(IJobManagerSettings settings)
	{
		_settings = settings;
	}

	public void AttachJobFilterObserver(IFilter filter)
	{
		_jobFilterObservers.Add(filter);
	}

	public void NotifyJobFilterChanged(List<FilterInfo> filters)
	{
		foreach (IFilter jobFilterObserver in _jobFilterObservers)
		{
			jobFilterObserver.FilterJobs(filters);
		}
	}

	public void AttachPlateFilterObserver(IFilterPlates filter)
	{
		_plateFilterObservers.Add(filter);
	}

	public void NotifyPlateFilterChanged(List<FilterInfo> filters)
	{
		foreach (IFilterPlates plateFilterObserver in _plateFilterObservers)
		{
			plateFilterObserver.FilterPlates(filters);
		}
	}

	public void AttachPartFilterObserver(IFilterParts filter)
	{
		_partFilterObservers.Add(filter);
	}

	public void NotifyPartFilterChanged(List<FilterInfo> filters)
	{
		foreach (IFilterParts partFilterObserver in _partFilterObservers)
		{
			partFilterObserver.FilterParts(filters);
		}
	}

	public void AttachImageObserver(IPreviewObserver manager)
	{
		_previewObservers.Add(manager);
	}

	public void NotifyPreviewChanged(string path)
	{
		foreach (IPreviewObserver previewObserver in _previewObservers)
		{
			previewObserver.PreviewChanged(path);
		}
	}

	public void NotifyMachinesChanged(IEnumerable<MachineViewInfo> machines)
	{
		foreach (IMachineStateObserver machineObserver in _machineObservers)
		{
			machineObserver.Filter(machines);
		}
	}

	public void AttachMachineObserver(IMachineStateObserver observer)
	{
		_machineObservers.Add(observer);
	}
}

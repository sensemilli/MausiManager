using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager;

public class MachinesControlViewModel : ViewModelBase, IViewModel
{
	private IJobManagerServiceProvider _provider;

	private IJobManagerSettings _settings;

	private IStateManager _stateManager;

	public IView View { get; private set; }

	public ObservableCollection<MachineViewInfo> Machines { get; set; }

	public void Initialize(IView view, IJobManagerServiceProvider provider)
	{
		View = view;
		_provider = provider;
		_settings = _provider.FindService<IJobManagerSettings>();
		Machines = new ObservableCollection<MachineViewInfo>();
		MachineViewInfo.MachineClickedHandler = MachineClicked;
		InitializeMachines(_settings.Machines);
	}

	public void AttachStateManager(IStateManager manager)
	{
		_stateManager = manager;
	}

	private void InitializeMachines(IEnumerable<MachineViewInfo> machines)
	{
		Machines.Clear();
		if (EnumerableHelper.IsNullOrEmpty(machines))
		{
			return;
		}
		if (machines.ToList().Count > 1)
		{
			Machines.Add(new MachineViewInfo
			{
				Machine = new MachineInfo(0)
				{
					Producer = "All"
				}
			});
		}
		foreach (MachineViewInfo machine in machines)
		{
			Machines.Add(machine);
		}
		MachineClicked(Machines[0]);
	}

	private void MachineClicked(MachineViewInfo machine)
	{
		machine.IsSelected = !machine.IsSelected;
		if (machine.Machine.Number == 0)
		{
			foreach (MachineViewInfo machine2 in Machines)
			{
				machine2.IsSelected = machine.IsSelected;
			}
		}
		else
		{
			bool isSelected = true;
			MachineViewInfo machineViewInfo = null;
			foreach (MachineViewInfo machine3 in Machines)
			{
				if (machine3.Number == 0)
				{
					machineViewInfo = machine3;
				}
				else if (!machine3.IsSelected)
				{
					isSelected = false;
					break;
				}
			}
			if (machineViewInfo != null)
			{
				machineViewInfo.IsSelected = isSelected;
			}
		}
		if (_stateManager != null)
		{
			_stateManager.NotifyMachinesChanged(_settings.Machines);
		}
	}
}

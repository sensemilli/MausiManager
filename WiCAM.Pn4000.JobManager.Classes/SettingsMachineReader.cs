using System.Collections.Generic;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.JobManager.Classes;

internal class SettingsMachineReader : ISettingsMachineReader
{
	public List<MachineViewInfo> ReadMachines(List<int> allowedMachines)
	{
		Logger.Info("ReadMachines");
		List<MachineInfo> list = new List<MachineInfo>();
		if (EnumerableHelper.IsNullOrEmpty(allowedMachines))
		{
			list = PnMachineHelper.ReadNamesOnly();
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				foreach (MachineInfo item in list)
				{
					allowedMachines.Add(item.Number);
				}
			}
		}
		else
		{
			foreach (int allowedMachine in allowedMachines)
			{
				MachineInfo machineInfo = PnMachineHelper.ReadOneMachine(allowedMachine);
				if (machineInfo.Producer != null)
				{
					list.Add(machineInfo);
				}
			}
		}
		List<MachineViewInfo> list2 = new List<MachineViewInfo>();
		if (!EnumerableHelper.IsNullOrEmpty(list))
		{
			Logger.Info("Found {0} machines", list.Count);
			foreach (MachineInfo item2 in list)
			{
				FindImage(item2);
				list2.Add(MachineViewInfo.FromMachine(item2));
			}
		}
		return list2;
	}

	private void FindImage(MachineInfo machine)
	{
		string searchString = "name=";
		string text = IOHelper.FileReadAllText(PnPathBuilder.PathInPnDriveWithFormat("u\\pn\\machine\\machine_{0:D4}\\bin\\startpp.bat", machine.Number));
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string line = StringHelper.GetLine(text, searchString, 0);
		if (string.IsNullOrEmpty(line))
		{
			return;
		}
		string text2 = StringHelper.GetValue(line).Trim();
		if (!string.IsNullOrEmpty(text2))
		{
			string text3 = PnPathBuilder.PathInPnDriveWithFormat("u\\pn\\pprun\\{0}.jpg", text2);
			if (IOHelper.FileExists(text3))
			{
				machine.Image32 = text3;
			}
		}
	}
}

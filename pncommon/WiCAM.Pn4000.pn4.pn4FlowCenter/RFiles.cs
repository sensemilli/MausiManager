using System.Collections.Generic;
using System.IO;
using pncommon.WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class RFiles
{
	public static List<RFileRecord> ReadFlatFileAndClearDouble(string module_name, string name, IPnPathService pnPathService)
	{
		string path = pnPathService.RFile(module_name, name);
		string[] array;
		try
		{
			array = File.ReadAllLines(path);
		}
		catch
		{
			return null;
		}
		List<RFileRecord> list = new List<RFileRecord>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RFileRecord rFileRecord = RFileRecord.FromString(array2[i]);
			if (rFileRecord == null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (flag)
				{
					break;
				}
				RFileRecord rFileRecord2 = list[j];
				flag = rFileRecord2.FunctionName == rFileRecord.FunctionName && rFileRecord2.FunctionGroup == rFileRecord.FunctionGroup;
			}
			if (!flag)
			{
				list.Add(rFileRecord);
			}
		}
		return list;
	}

	public static List<IRFileRecord> ReadFile(string module_name, string name, IPnPathService pnPathService)
	{
		string path = pnPathService.RFile(module_name, name);
		List<IRFileRecord> list = new List<IRFileRecord>();
		if (!File.Exists(path))
		{
			return list;
		}
		string[] array;
		try
		{
			array = File.ReadAllLines(path);
		}
		catch
		{
			return list;
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RFileRecord rFileRecord = RFileRecord.FromString(array2[i]);
			if (rFileRecord != null)
			{
				list.Add(rFileRecord);
			}
		}
		foreach (IRFileRecord item in list)
		{
			if (item.FunctionGroup == 100)
			{
				item.SubRecords = RFiles.ReadFile(module_name, item.FunctionName, pnPathService);
			}
		}
		return list;
	}
}

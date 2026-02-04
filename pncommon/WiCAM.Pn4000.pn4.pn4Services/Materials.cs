using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Unfold.BendTable;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class Materials : global::BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Unfold.BendTable.Materials
{
	public Materials(IPnPathService pathService)
		: base(pathService)
	{
	}

	public void Init(string pnDrive, SpringbackConfig springbackConfig, Action<Exception> onError = null)
	{
		base.Init(pnDrive, onError);
		try
		{
			Dictionary<int, (int Id, string Desc, List<(double Ratio, double Springback)> Values)> values = springbackConfig.Values.ToDictionary<(int, string, List<(double, double)>), int>(((int Id, string Desc, List<(double Ratio, double Springback)> Values) x) => x.Id);
			Dictionary<int, (int, string, List<(double, double)>)> dictionary = springbackConfig.MaterialAssignment.ToDictionary(((int MatId, int SpringbackGroup) x) => x.MatId, ((int MatId, int SpringbackGroup) x) => values[x.SpringbackGroup]);
			foreach (IMaterialArt material in base.MaterialList)
			{
				if (dictionary.TryGetValue(material.Number, out var value))
				{
					material.SpringbackTable = value.Item3;
				}
			}
		}
		catch (Exception)
		{
			onError?.Invoke(new Exception("SpringbackConfig is corrupt"));
		}
	}
}

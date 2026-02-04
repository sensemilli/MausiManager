using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class FingerStopCombinationViewModel
{
	public IFingerStopCombination Combination { get; set; }

	public int ID { get; set; }

	public IFingerStop FingerStop { get; }

	public IBendMachine BendMachine { get; }

	public string Img
	{
		get
		{
			IStopCombinations stopCombinations = ((FingerStop.FingerModel.PartRole == PartRole.LeftFinger) ? BendMachine.StopCombinationsLeftFinger : BendMachine.StopCombinationsRightFinger);
			return Path.Combine(BendMachine.MachinePath, stopCombinations.GetData(Combination)?.GetIconPath());
		}
	}

	public FingerStopCombinationViewModel(IBendMachine machineConfig, IFingerStop selectedFinger, IFingerStopCombination item1)
	{
		BendMachine = machineConfig;
		FingerStop = selectedFinger;
		Combination = item1;
	}

	protected virtual Pair<List<string>, int> GetStopCombinationId(List<Pair<List<string>, int>> list, IFingerStopCombination combination)
	{
		if (combination != null)
		{
			foreach (Pair<List<string>, int> item in list)
			{
				if (combination.FaceNames.TrueForAll(item.Item1.Contains))
				{
					return item;
				}
			}
		}
		return new Pair<List<string>, int>(new List<string>(), -1);
	}

	public override int GetHashCode()
	{
		return Combination.GetHashCode();
	}
}

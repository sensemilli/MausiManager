using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IStopCombinations
{
	IEnumerable<string> FaceNames { get; }

	List<IFingerStopCombinationData> Combinations { get; }

	IFingerStopCombinationData? GetData(IFingerStopCombination? combination);

	IFingerStopCombinationData? GetData(StopCombinationType combination);
}

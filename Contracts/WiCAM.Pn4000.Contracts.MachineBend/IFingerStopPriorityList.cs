using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopPriorityList
{
	List<Pair<IFingerStopCombination, IFingerStopPriorityList>> List { get; set; }
}

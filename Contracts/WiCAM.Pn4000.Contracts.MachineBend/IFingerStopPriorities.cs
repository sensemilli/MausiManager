using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopPriorities
{
	List<PartRole> FingerOrder { get; set; }

	IFingerStopPriorityList Priorities0Corners { get; set; }

	IFingerStopPriorityList Priorities1CornersLeft { get; set; }

	IFingerStopPriorityList Priorities1CornersRight { get; set; }

	IFingerStopPriorityList Priorities2Corners { get; set; }

	List<IFingerStopCombination> CreateDefault();
}

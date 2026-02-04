using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.BendOrderCaclulation;

public interface IBendOrderCondition
{
	BendOrderConditionPriorities Priority { get; set; }

	double SubPriority { get; set; }

	bool? IsValid(List<int> orderedBends);

	string Log(Dictionary<int, int> bendIdxToOrder);
}

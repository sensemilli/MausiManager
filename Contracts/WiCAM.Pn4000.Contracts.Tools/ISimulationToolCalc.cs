using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISimulationToolCalc
{
	int? NextToolCalcPosition(bool stopExtendedCalc, HashSet<int>? interestingBendNo = null);
}

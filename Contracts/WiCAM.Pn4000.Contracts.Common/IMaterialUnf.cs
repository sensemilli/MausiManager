using System;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IMaterialUnf
{
	int Number { get; set; }

	string Name { get; set; }

	double Density { get; set; }

	double YieldStrength { get; set; }

	double TensileStrength { get; set; }

	double HeatCapacity { get; set; }

	double WorkHardeningExponent { get; set; }

	double EModul { get; set; }

	DateTime Modified { get; set; }
}

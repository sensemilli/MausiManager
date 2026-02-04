using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IMaterialArt
{
	int Number { get; set; }

	string Name { get; set; }

	double Density { get; set; }

	int Rotation { get; set; }

	int ReferenceNumber { get; set; }

	string AlternativeMaterialNumbers { get; set; }

	int MaterialGroupId { get; set; }

	string Description { get; set; }

	string Remark01 { get; set; }

	string Remark02 { get; set; }

	string Remark03 { get; set; }

	string Remark04 { get; set; }

	string Remark05 { get; set; }

	double ThicknessMin { get; set; }

	double ThicknessMax { get; set; }

	double YieldStrength { get; set; }

	double TensileStrength { get; set; }

	double HeatCapacity { get; set; }

	double WorkHardeningExponent { get; set; }

	double EModul { get; set; }

	int MaterialGroupForBendDeduction { get; set; }

	DateTime Modified { get; set; }

	List<(double Ratio, double Springback)> SpringbackTable { get; set; }
}

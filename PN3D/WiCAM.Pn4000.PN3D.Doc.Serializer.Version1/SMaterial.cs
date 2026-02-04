using System;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SMaterial
{
	public int Number { get; set; }

	public string Name { get; set; }

	public double Density { get; set; }

	public int Rotation { get; set; }

	public int ReferenceNumber { get; set; }

	public string AlternativeMaterialNumbers { get; set; }

	public int Material3dGroupId { get; set; }

	public int MaterialGroupId { get; set; }

	public string Description { get; set; }

	public string Remark01 { get; set; }

	public string Remark02 { get; set; }

	public string Remark03 { get; set; }

	public string Remark04 { get; set; }

	public string Remark05 { get; set; }

	public double ThicknessMin { get; set; }

	public double ThicknessMax { get; set; } = 9999.0;

	public double YieldStrength { get; set; } = 190.0;

	public double TensileStrength { get; set; } = 310.0;

	public double HeatCapacity { get; set; } = 450.0;

	public double WorkHardeningExponent { get; set; } = 0.22;

	public double EModul { get; set; } = 20000.0;

	public DateTime Modified { get; set; }
}

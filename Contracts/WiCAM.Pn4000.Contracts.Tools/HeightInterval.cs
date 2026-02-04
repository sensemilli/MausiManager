using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public class HeightInterval
{
	public bool Slope { get; set; }

	public double ZMin { get; set; }

	public double ZMax { get; set; }

	public List<(double XStart, double XEnd)> Solids { get; set; } = new List<(double, double)>();

	public HeightInterval()
	{
	}

	public HeightInterval(double zMin, double zMax, double xStart, double xEnd)
	{
		this.ZMin = zMin;
		this.ZMax = zMax;
		this.Solids.Add((xStart, xEnd));
	}
}

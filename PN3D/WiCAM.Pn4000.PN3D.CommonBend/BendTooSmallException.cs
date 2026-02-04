using System;

namespace WiCAM.Pn4000.PN3D.CommonBend;

internal class BendTooSmallException : Exception
{
	public double BendLength { get; }

	public double MaxOverlength { get; }

	public double MinToolLength { get; }

	public BendTooSmallException(double bendLength, double maxOverlength, double minToolLength)
	{
		this.BendLength = bendLength;
		this.MaxOverlength = maxOverlength;
		this.MinToolLength = minToolLength;
	}
}

using System;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.PN3D.BendSimulation;

public class MinMax
{
	public double Minx { get; set; } = double.MaxValue;

	public double Miny { get; set; } = double.MaxValue;

	public double Minz { get; set; } = double.MaxValue;

	public double Maxx { get; set; } = double.MinValue;

	public double Maxy { get; set; } = double.MinValue;

	public double Maxz { get; set; } = double.MinValue;

	public Vector3d Min
	{
		get
		{
			return new Vector3d(this.Minx, this.Miny, this.Minz);
		}
		set
		{
			this.Minx = value.X;
			this.Miny = value.Y;
			this.Minz = value.Z;
		}
	}

	public Vector3d Max
	{
		get
		{
			return new Vector3d(this.Maxx, this.Maxy, this.Maxz);
		}
		set
		{
			this.Maxx = value.X;
			this.Maxy = value.Y;
			this.Maxz = value.Z;
		}
	}

	public Vector3d Middle => new Vector3d(this.Minx + this.LenX() / 2.0, this.Miny + this.LenY() / 2.0, this.Minz + this.LenZ() / 2.0);

	public void CheckPoint(Vector3d p1)
	{
		if (p1.X != double.MinValue && p1.X != double.MaxValue)
		{
			if (p1.X < this.Minx)
			{
				this.Minx = p1.X;
			}
			if (p1.Y < this.Miny)
			{
				this.Miny = p1.Y;
			}
			if (p1.Z < this.Minz)
			{
				this.Minz = p1.Z;
			}
			if (p1.X > this.Maxx)
			{
				this.Maxx = p1.X;
			}
			if (p1.Y > this.Maxy)
			{
				this.Maxy = p1.Y;
			}
			if (p1.Z > this.Maxz)
			{
				this.Maxz = p1.Z;
			}
		}
	}

	public double LongestSideLength()
	{
		double num = this.LenX();
		if (this.LenY() > num)
		{
			num = this.LenY();
		}
		if (this.LenZ() > num)
		{
			num = this.LenZ();
		}
		return num;
	}

	public void CheckBoundary(MinMax t)
	{
		this.CheckPoint(new Vector3d(t.Minx, t.Miny, t.Minz));
		this.CheckPoint(new Vector3d(t.Maxx, t.Maxy, t.Maxz));
	}

	public double LenX()
	{
		return Math.Abs(this.Maxx - this.Minx);
	}

	public double LenY()
	{
		return Math.Abs(this.Maxy - this.Miny);
	}

	public double LenZ()
	{
		return Math.Abs(this.Maxz - this.Minz);
	}

	public bool SameSizeAs(MinMax compareMinMax, double tolerance = 0.1)
	{
		return (Math.Abs(this.LenX() - compareMinMax.LenX()) < tolerance) & (Math.Abs(this.LenY() - compareMinMax.LenY()) < tolerance) & (Math.Abs(this.LenZ() - compareMinMax.LenZ()) < tolerance);
	}
}

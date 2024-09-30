using System;
using System.Windows.Media.Media3D;

namespace WiCAM.Pn4000.Archive.Browser.Classes.Geo3D
{
	internal class SmallMinMax
	{
		public double minX = double.MaxValue;

		public double minY = double.MaxValue;

		public double minZ = double.MaxValue;

		public double maxX = double.MinValue;

		public double maxY = double.MinValue;

		public double maxZ = double.MinValue;

		public SmallMinMax()
		{
		}

		public void CheckPoint(Point3D p1)
		{
			if (p1.X < this.minX)
			{
				this.minX = p1.X;
			}
			if (p1.Y < this.minY)
			{
				this.minY = p1.Y;
			}
			if (p1.Z < this.minZ)
			{
				this.minZ = p1.Z;
			}
			if (p1.X > this.maxX)
			{
				this.maxX = p1.X;
			}
			if (p1.Y > this.maxY)
			{
				this.maxY = p1.Y;
			}
			if (p1.Z > this.maxZ)
			{
				this.maxZ = p1.Z;
			}
		}

		internal void CheckTriangle(Triangle3P t)
		{
			this.CheckPoint(t.p1);
			this.CheckPoint(t.p2);
			this.CheckPoint(t.p3);
		}
	}
}
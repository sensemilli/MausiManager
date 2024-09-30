using System;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Classes.Geo3D
{
	public class Triangle3P
	{
		public Point3D p1
		{
			get;
			set;
		}

		public Point3D p2
		{
			get;
			set;
		}

		public Point3D p3
		{
			get;
			set;
		}

		public Triangle3P()
		{
		}

		private static Point3D Create3DPoint(string v1, string v2, string v3)
		{
			return new Point3D(Triangle3P.ToDouble(v1), Triangle3P.ToDouble(v2), Triangle3P.ToDouble(v3));
		}

		public static Triangle3P FromString(string input)
		{
			string[] strArrays = input.Split(new char[] { WiCAM.Pn4000.Common.CS.CharBlank });
			return new Triangle3P()
			{
				p1 = Triangle3P.Create3DPoint(strArrays[0], strArrays[1], strArrays[2]),
				p2 = Triangle3P.Create3DPoint(strArrays[3], strArrays[4], strArrays[5]),
				p3 = Triangle3P.Create3DPoint(strArrays[6], strArrays[7], strArrays[8])
			};
		}

		private static double ToDouble(string input)
		{
			return StringHelper.ToDouble(input);
		}
	}
}
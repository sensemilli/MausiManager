using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace WiCAM.Pn4000.Archive.Browser.Classes.Geo3D
{
	public class PartInfo
	{
		public int ID;

		public int count;

		public string Name;

		public List<Triangle3P> Triangles = new List<Triangle3P>();

		public GeometryModel3D Model3D;

		public Geo3DInfo Item;

		public PartInfo()
		{
		}
	}
}
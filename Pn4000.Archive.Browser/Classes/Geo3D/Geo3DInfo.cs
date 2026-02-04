using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Classes.Geo3D
{
	public class Geo3DInfo
	{
		public List<PartInfo> Parts;

		private SmallMinMax ModelBoundary;

		private double Scale;

		public Model3DGroup @group;

        public ModelVisual3D modelVisual3D { get; private set; }

        private GeometryModel3D GeometryModel;

		public double y_rot;

		public double z_rot;

		public double SizeX
		{
			get;
			private set;
		}

		public double SizeY
		{
			get;
			private set;
		}

		public double SizeZ
		{
			get;
			private set;
		}

		public Geo3DInfo()
		{
		}

		private void CalculateMinMax()
		{
			if (this.ModelBoundary.minX != double.MaxValue && this.ModelBoundary.minY != double.MaxValue && this.ModelBoundary.minZ != double.MaxValue && this.ModelBoundary.maxX != double.MinValue && this.ModelBoundary.maxY != double.MinValue && this.ModelBoundary.maxZ != double.MinValue)
			{
				this.SizeX = Math.Abs(this.ModelBoundary.maxX - this.ModelBoundary.minX);
				this.SizeY = Math.Abs(this.ModelBoundary.maxY - this.ModelBoundary.minY);
				this.SizeZ = Math.Abs(this.ModelBoundary.maxZ - this.ModelBoundary.minZ);
			}
		}

		private Matrix3D CalculateRotationMatrix(double x, double y, double z)
		{
			Matrix3D matrix3D = new Matrix3D();
			matrix3D.Rotate(new Quaternion(new Vector3D(1, 0, 0), x));
			matrix3D.Rotate(new Quaternion(new Vector3D(0, 1, 0) * matrix3D, y));
			matrix3D.Rotate(new Quaternion(new Vector3D(0, 0, 1) * matrix3D, z));
			return matrix3D;
		}

		private ModelVisual3D GenerateModelGeometry(byte transp)
		{
			this.CalculateMinMax();
			double sizeX = 1 / this.SizeX;
			double sizeY = 1 / this.SizeY;
			double sizeZ = 1 / this.SizeZ;
			this.Scale = sizeX;
			if (sizeY < this.Scale)
			{
				this.Scale = sizeY;
			}
			if (sizeZ < this.Scale)
			{
				this.Scale = sizeZ;
			}
			double modelBoundary = this.ModelBoundary.minX * this.Scale;
			double num = this.ModelBoundary.minY * this.Scale;
			double modelBoundary1 = this.ModelBoundary.minZ * this.Scale;
			double num1 = this.ModelBoundary.maxX * this.Scale;
			double modelBoundary2 = this.ModelBoundary.maxY * this.Scale;
			double num2 = this.ModelBoundary.maxZ * this.Scale;
			double num3 = -(modelBoundary + (num1 - modelBoundary) / 2);
			double num4 = -(num + (modelBoundary2 - num) / 2);
			double num5 = -(modelBoundary1 + (num2 - modelBoundary1) / 2);
			this.@group = new Model3DGroup();
			 modelVisual3D = new ModelVisual3D();
			foreach (PartInfo part in this.Parts)
			{
				Color orangeRed = Colors.OrangeRed;
				orangeRed.A = transp;
				this.GeometryModel = new GeometryModel3D()
				{
					Material = new DiffuseMaterial(new SolidColorBrush(orangeRed))
				};
				MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
				int num6 = 0;
				foreach (Triangle3P triangle in part.Triangles)
				{
					this.ScaleTriangle(meshGeometry3D, triangle.p1, this.Scale, num3, num4, num5);
					this.ScaleTriangle(meshGeometry3D, triangle.p2, this.Scale, num3, num4, num5);
					this.ScaleTriangle(meshGeometry3D, triangle.p3, this.Scale, num3, num4, num5);
					meshGeometry3D.TriangleIndices.Add(num6);
					meshGeometry3D.TriangleIndices.Add(num6 + 1);
					meshGeometry3D.TriangleIndices.Add(num6 + 2);
					num6 += 3;
				}
				this.GeometryModel.Geometry = meshGeometry3D;
				this.@group.Children.Add(this.GeometryModel);
				part.Model3D = this.GeometryModel;
			}
			this.@group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-10, -10, -10)));
			this.@group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(10, 10, 10)));
			this.@group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(10, 10, -10)));
			this.@group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-10, -10, 10)));
			modelVisual3D.Content = this.@group;
			return modelVisual3D;
		}

		public void LoadFromP3D(string path)
		{
			this.ModelBoundary = new SmallMinMax();
			this.Parts = new List<PartInfo>();
			PartInfo partInfo = new PartInfo();
			this.Parts.Add(partInfo);
			bool flag = false;
			string[] strArrays = IOHelper.FileReadAllLines(path);
			if (!EnumerableHelper.IsNullOrEmpty(strArrays))
			{
				string[] strArrays1 = strArrays;
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					string str = strArrays1[i];
					if (str.Length > 0 && str[0] == '#')
					{
						flag = str.Contains("EntryModel3D.PnBndFileFace");
					}
					if (flag && str.Length > 15 && str.Substring(0, 13) == "Tessellation|")
					{
						string[] strArrays2 = str.Substring(13).Split(new char[] { WiCAM.Pn4000.Common.CS.CharSemikolon });
						for (int j = 0; j < (int)strArrays2.Length; j++)
						{
							string str1 = strArrays2[j];
							if (str1 != string.Empty)
							{
								Triangle3P triangle3P = Triangle3P.FromString(str1);
								partInfo.Triangles.Add(triangle3P);
								this.ModelBoundary.CheckTriangle(triangle3P);
							}
						}
					}
				}
			}
		}

		public void MakeRotation()
		{
			MatrixTransform3D matrixTransform3D = new MatrixTransform3D(this.CalculateRotationMatrix(0, this.y_rot, this.z_rot));
			foreach (object child in this.@group.Children)
			{
				if (!(child is GeometryModel3D))
				{
					continue;
				}
				(child as GeometryModel3D).Transform = matrixTransform3D;
			}
		}

		private Point3D ScaleTriangle(MeshGeometry3D mesh, Point3D p, double scale, double dx, double dy, double dz)
		{
			Point3D point3D = new Point3D(p.X * scale + dx, p.Y * scale + dy, p.Z * scale + dz);
			mesh.Positions.Add(point3D);
			return point3D;
		}

		public void SetModel3DOutput(Viewport3D myViewport3D)
		{
			this.SetModel3DOutput(myViewport3D, 150);
		}

		public void SetModel3DOutput(Viewport3D myViewport3D, byte transparency)
		{
			myViewport3D.Children.Clear();
			myViewport3D.Children.Add(this.GenerateModelGeometry(transparency));
			myViewport3D.Camera = new OrthographicCamera(new Point3D(-1, 0, 0), new Vector3D(2, 0, 0), new Vector3D(0, 0, 1), 1.5);
			this.y_rot = -30;
			this.z_rot = 135;
			this.MakeRotation();
		}
	}
}
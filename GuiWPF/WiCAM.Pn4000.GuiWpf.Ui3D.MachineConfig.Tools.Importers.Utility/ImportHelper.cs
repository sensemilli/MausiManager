using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DxfToCadGeo;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.HealCadGeo;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Contracts.Factorys;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;

public class ImportHelper
{
	private readonly IModelFactory _modelFactory;

	public ImportHelper(IModelFactory modelFactory)
	{
		_modelFactory = modelFactory;
	}

	public double SpecialConvertToDouble(string s)
	{
		return SpecialTryConvertToDouble(s).GetValueOrDefault();
	}

	public double? SpecialTryConvertToDouble(string s)
	{
		s = s.Replace("'", "").Trim().Replace(',', '.');
		if (!double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return null;
		}
		return result;
	}

	public void FixStlOrientation(Model model, double RotateX, double RotateZ)
	{
		Matrix4d matrix4d = Matrix4d.RotationX(RotateX);
		Matrix4d matrix4d2 = Matrix4d.RotationZ(RotateZ);
		Matrix4d matrix4d3 = Matrix4d.Scale(1.0, -1.0, 1.0);
		Matrix4d transform = matrix4d * matrix4d2 * matrix4d3;
		model.ModifyVertices(transform, transformSubModels: false);
	}

	public void CalculateShell0Edges(Model model, double angle)
	{
		double num = angle / 180.0 * Math.PI;
		foreach (Triangle item in model.Shells[0].Faces.SelectMany((Face f) => f.Mesh))
		{
			TriangleHalfEdge[] array = new TriangleHalfEdge[3] { item.E0, item.E1, item.E2 };
			foreach (TriangleHalfEdge triangleHalfEdge in array)
			{
				if (triangleHalfEdge.CounterEdge == null || triangleHalfEdge.CounterEdge.Triangle.CalculatedTriangleNormal.UnsignedAngle(item.CalculatedTriangleNormal) >= num)
				{
					FaceHalfEdge faceHalfEdge = new FaceHalfEdge(item.Face, EdgeType.Line);
					faceHalfEdge.IsHole = true;
					faceHalfEdge.AddVertex(triangleHalfEdge.V0);
					faceHalfEdge.AddVertex(triangleHalfEdge.V1);
					faceHalfEdge.Color = new Color(0.8f, 0.8f, 0.8f, 1f);
					item.Face.HoleEdgesCw.Add(new List<FaceHalfEdge> { faceHalfEdge });
				}
			}
		}
	}

	public ICadGeo Get2DGeometry(string filePath)
	{
		string text = Path.GetExtension(filePath).ToLower();
		if (!(text == ".dxf"))
		{
			if (text == ".smx")
			{
				ICadGeo cadGeo = SmxLoader.LoadAsCadGeo(filePath);
				HealCadGeo_.Heal(cadGeo);
				return cadGeo;
			}
			return null;
		}
		ICadGeo cadGeo2 = DxfToCadGeo.Convert(filePath, _modelFactory);
		HealCadGeo_.Heal(cadGeo2);
		return cadGeo2;
	}

	public Dictionary<string, string> GetProperties(object obj)
	{
		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		PropertyInfo[] array = properties;
		foreach (PropertyInfo obj2 in array)
		{
			string name = obj2.Name;
			object value = obj2.GetValue(obj, null);
			dictionary.Add(name, (value == null) ? string.Empty : value.ToString());
		}
		return dictionary;
	}

	public string MakeValidFileName(string fileName)
	{
		return Path.GetInvalidFileNameChars().Aggregate(fileName, (string current, char c) => current.Replace(c, '_'));
	}
}

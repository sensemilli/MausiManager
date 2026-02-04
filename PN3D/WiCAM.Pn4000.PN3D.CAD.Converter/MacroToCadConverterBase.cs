using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class MacroToCadConverterBase
{
	protected enum OrientationTypes
	{
		Top = 0,
		Bottom = 1,
		Side = 2,
		OtherTop = 3,
		OtherBottom = 4
	}

	protected static OrientationTypes GetOrientation(Face face, Matrix4d worldMatrix)
	{
		return MacroToCadConverterBase.GetOrientation(face.Mesh.First().TriangleNormal, worldMatrix);
	}

	protected static OrientationTypes GetOrientation(Vector3d normal, Matrix4d worldMatrix)
	{
		return MacroToCadConverterBase.GetOrientation(worldMatrix.TransformNormal(normal));
	}

	protected static OrientationTypes GetOrientation(Vector3d normalInWorldSpace)
	{
		double num = 0.0015;
		if (normalInWorldSpace.Z > 1.0 - num)
		{
			return OrientationTypes.Top;
		}
		if (normalInWorldSpace.Z < -1.0 + num)
		{
			return OrientationTypes.Bottom;
		}
		if (Math.Abs(normalInWorldSpace.Z) < num)
		{
			return OrientationTypes.Side;
		}
		if (normalInWorldSpace.Z > 0.0)
		{
			return OrientationTypes.OtherTop;
		}
		return OrientationTypes.OtherBottom;
	}

	private static bool CheckForCircle(FaceHalfEdge edge, Matrix4d transform, out CircleCheck2D circle)
	{
		circle = new CircleCheck2D(default(Vector3d), default(Vector3d), default(Vector3d));
		if (edge.Vertices.Count < 3)
		{
			return false;
		}
		Vector3d pos = edge.Vertices[0].Pos;
		Vector3d pos2 = edge.Vertices[edge.Vertices.Count / 3].Pos;
		Vector3d pos3 = edge.Vertices[edge.Vertices.Count - 1].Pos;
		pos = transform.Transform(pos);
		pos2 = transform.Transform(pos2);
		pos3 = transform.Transform(pos3);
		circle = new CircleCheck2D(pos, pos2, pos3);
		if (!circle.IsCircle || circle.R > 10000.0 || !circle.CheckAllPointsOnCircle(edge, transform))
		{
			return false;
		}
		circle.CalclulateStartAngEndAngDir(pos, pos2, pos3);
		return true;
	}

	private static CadGeoElement GetLine(FaceHalfEdge edge, Matrix4d worldMatrix, int color)
	{
		Vector3d v = edge.StartVertex.Pos;
		Vector3d v2 = edge.EndVertex.Pos;
		worldMatrix.TransformInPlace(ref v);
		worldMatrix.TransformInPlace(ref v2);
		return new CadGeoLine
		{
			Color = color,
			Type = 1,
			StartPoint = new Vector2d(v.X, v.Y),
			EndPoint = new Vector2d(v2.X, v2.Y)
		};
	}

	protected static CadGeoElement GetLine(Vector3d a, Vector3d b, Matrix4d worldMatrix, int color)
	{
		Vector3d v = new Vector3d(a.X, a.Y, a.Z);
		Vector3d v2 = new Vector3d(b.X, b.Y, b.Z);
		worldMatrix.TransformInPlace(ref v);
		worldMatrix.TransformInPlace(ref v2);
		return new CadGeoLine
		{
			Color = color,
			Type = 1,
			StartPoint = new Vector2d(v.X, v.Y),
			EndPoint = new Vector2d(v2.X, v2.Y)
		};
	}

	private static CadGeoElement GetCadGeoCircle(CircleCheck2D circle, int color)
	{
		return new CadGeoCircle
		{
			Color = color,
			Type = 2,
			Center = new Vector2d(circle.Center.X, circle.Center.Y),
			Direction = circle.Direction,
			StartAngle = circle.StartAngle,
			EndAngle = circle.EndAngle,
			Radius = circle.R
		};
	}

	protected static CadGeoElement GetCadGeoCircleByDiameter(Vector3d middlePoint, double startAngle, double endAngle, double radius, int color)
	{
		return new CadGeoCircle
		{
			Color = color,
			Type = 2,
			Center = new Vector2d(Math.Round(middlePoint.X, 5), Math.Round(middlePoint.Y, 5)),
			Radius = radius,
			StartAngle = startAngle,
			EndAngle = endAngle,
			Direction = -1
		};
	}

	protected static IEnumerable<CadGeoElement> GetCadGeoElement(FaceHalfEdge edge, Matrix4d worldMatrix, int color)
	{
		if (edge.EdgeType == EdgeType.Line)
		{
			return new HashSet<CadGeoElement> { MacroToCadConverterBase.GetLine(edge, worldMatrix, color) };
		}
		return new HashSet<CadGeoElement>(MacroToCadConverterBase.GetContour(edge.Vertices.Select(delegate(Vertex v)
		{
			Vector3d v2 = v.Pos;
			worldMatrix.TransformInPlace(ref v2);
			return new Vector2d(v2.X, v2.Y);
		}).ToList(), color, isOpenContour: true));
	}

	protected static List<CadGeoElement> GetContour(List<Vector2d> points, int color, bool isOpenContour, Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap = null)
	{
		ContourConverter contourConverter = new ContourConverter();
		return contourConverter.ToCadGeoContour(contourConverter.CreateContour(points, isOpenContour, simplify: true, removeShortSegments: true, contourCircleMap), color);
	}
}

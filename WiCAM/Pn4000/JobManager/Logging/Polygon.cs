using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ClipperLib;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.TypeConverter;

namespace WiCAM.Pn4000.BendModel.Base;

[TypeConverter(typeof(BendTypeConverter))]
public class Polygon
{
	public List<Vector2d> Vertices { get; set; } = new List<Vector2d>();

	public Vector2d Centroid
	{
		get
		{
			if (Vertices.Count > 0)
			{
				return Vertices.Aggregate((Vector2d result, Vector2d v) => result + v) / Vertices.Count;
			}
			return default(Vector2d);
		}
	}

	public int WindingNumber
	{
		get
		{
			double num = 0.0;
			if (!TryGetPointInPolygon(out var point))
			{
				return 0;
			}
			List<double> list = new List<double>();
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vector2d vector2d = Vertices[i] - point;
				Vector2d v = Vertices[(i + 1) % Vertices.Count] - point;
				double num2 = vector2d.SignedAngle(v);
				num += num2;
				list.Add(num2);
			}
			return (int)Math.Round(num / (Math.PI * 2.0));
		}
	}

	private bool TryGetPointInPolygon(out Vector2d point)
	{
		point = Vector2d.Zero;
		Vector2d centroid = Centroid;
		if (IsPointInPolygon(centroid, out var intersections, out var onBoundary) && !onBoundary)
		{
			point = centroid;
			return true;
		}
		intersections.Sort();
		intersections = intersections.MakeUnique().ToList();
		List<Vector2d> intersections2;
		if (intersections.Count > 1)
		{
			for (int i = 0; i < intersections.Count; i++)
			{
				Vector2d vector2d = intersections[i];
				Vector2d vector2d2 = intersections[(i + 1) % intersections.Count];
				if (!(vector2d == vector2d2))
				{
					Vector2d vector2d3 = 0.5 * (vector2d + vector2d2);
					if (IsPointInPolygon(vector2d3, out intersections2, out var onBoundary2) && !onBoundary2)
					{
						point = vector2d3;
						return true;
					}
				}
			}
		}
		intersections.Clear();
		for (int j = 0; j < Vertices.Count; j++)
		{
			Vector2d vector2d4 = Vertices[j];
			Vector2d vector2d5 = Vertices[(j + 1) % Vertices.Count];
			if (vector2d4.X != vector2d5.X)
			{
				double num = Math.Min(vector2d4.X, vector2d5.X);
				double num2 = Math.Max(vector2d4.X, vector2d5.X);
				if (num < centroid.X && num2 > centroid.X)
				{
					double num3 = (centroid.X - vector2d4.X) / (vector2d5.X - vector2d4.X);
					Vector2d vector2d6 = vector2d5 - vector2d4;
					intersections.Add(vector2d4 + num3 * vector2d6);
				}
				else if (vector2d4.X == centroid.X)
				{
					intersections.Add(vector2d4);
				}
				else if (vector2d5.X == centroid.X)
				{
					intersections.Add(vector2d5);
				}
			}
			else
			{
				double num4 = Math.Min(vector2d4.Y, vector2d5.Y);
				double num5 = Math.Max(vector2d4.Y, vector2d5.Y);
				if (vector2d4.X == centroid.X && centroid.Y >= num4 && centroid.Y <= num5)
				{
					intersections.Add(centroid);
				}
			}
		}
		intersections.Sort();
		intersections = intersections.MakeUnique().ToList();
		if (intersections.Count > 1)
		{
			for (int k = 0; k < intersections.Count; k++)
			{
				Vector2d vector2d7 = intersections[k];
				Vector2d vector2d8 = intersections[(k + 1) % intersections.Count];
				if (!(vector2d7 == vector2d8))
				{
					Vector2d vector2d9 = 0.5 * (vector2d7 + vector2d8);
					if (IsPointInPolygon(vector2d9, out intersections2, out var onBoundary3) && !onBoundary3)
					{
						point = vector2d9;
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsPointInPolygon(Vector2d p, out List<Vector2d> intersections, double distThreshold = 1E-06)
	{
		bool onBoundary;
		return IsPointInPolygon(p, out intersections, out onBoundary, distThreshold);
	}

	public bool IsPointInPolygon(Vector2d p, out List<Vector2d> intersections, out bool onBoundary, double distThreshold = 1E-06)
	{
		onBoundary = false;
		intersections = new List<Vector2d>();
		List<Pair<Vector2d, int>> list = new List<Pair<Vector2d, int>>();
		int num = -1;
		for (int i = 0; i < Vertices.Count; i++)
		{
			Vector2d vector2d = Vertices[i];
			Vector2d vector2d2 = Vertices[(i + 1) % Vertices.Count];
			Vector2d vector2d3 = vector2d2 - vector2d;
			double num2 = Math.Min(vector2d.X, vector2d2.X);
			double num3 = Math.Max(vector2d.X, vector2d2.X);
			double num4 = Math.Min(vector2d.Y, vector2d2.Y);
			double num5 = Math.Max(vector2d.Y, vector2d2.Y);
			if (num4 != num5)
			{
				Vector2d? vector2d4 = null;
				if (num4 < p.Y && p.Y < num5)
				{
					double num6 = (p.Y - vector2d.Y) / (vector2d2.Y - vector2d.Y);
					vector2d4 = vector2d + num6 * vector2d3;
				}
				else if (vector2d.Y == p.Y)
				{
					vector2d4 = vector2d;
				}
				else if (vector2d2.Y == p.Y)
				{
					vector2d4 = vector2d2;
				}
				if (vector2d4.HasValue)
				{
					Vector2d vector2d5 = p - vector2d4.Value;
					if (vector2d5.Length < distThreshold)
					{
						intersections.Add(vector2d4.Value);
						onBoundary = true;
					}
					int num7 = Math.Sign(vector2d3.X * vector2d5.Y - vector2d3.Y * vector2d5.X);
					if (list.Count == 0 || (num7 != list.Last().Item2 && Math.Sign(p.X - intersections.Last().X) == Math.Sign(p.X - vector2d4.Value.X)) || (num7 == list.Last().Item2 && Math.Sign(p.X - intersections.Last().X) != Math.Sign(p.X - vector2d4.Value.X)))
					{
						if (num == -1)
						{
							num = i;
						}
						list.Add(new Pair<Vector2d, int>(vector2d4.Value, num7));
						intersections.Add(vector2d4.Value);
					}
				}
				else if (num4 == p.Y && p.X >= num2 && p.X <= num3)
				{
					onBoundary = true;
					intersections.Add(new Vector2d(num2, num4));
					intersections.Add(new Vector2d(num3, num4));
				}
			}
			else if (num4 == p.Y && p.X >= num2 && p.X <= num3)
			{
				onBoundary = true;
				intersections.Add(p);
			}
		}
		if (list.Count > 1 && (list.First().Item2 == list.Last().Item2 || Math.Sign(p.X - intersections.Last().X) != Math.Sign(p.X - intersections.First().X)) && (list.First().Item2 != list.Last().Item2 || Math.Sign(p.X - intersections.Last().X) == Math.Sign(p.X - intersections.First().X)))
		{
			list.RemoveAt(list.Count - 1);
		}
		return (list.Where((Pair<Vector2d, int> pair) => pair.Item1.X < p.X).Aggregate(0, (int s, Pair<Vector2d, int> pair) => s + pair.Item2) != 0) | onBoundary;
	}

	public void RoundVertices(int digits)
	{
		for (int i = 0; i < Vertices.Count; i++)
		{
			Vector2d value = Vertices[i];
			value.X = Math.Round(value.X, digits);
			value.Y = Math.Round(value.Y, digits);
			Vertices[i] = value;
		}
	}

	public List<(double start, double end, SpacialRelation relation)> Intersect(Line2D line, double parallelDisThreshold = 0.0001)
	{
		List<(double, double, SpacialRelation)> list = new List<(double, double, SpacialRelation)>();
		List<double> list2 = new List<double>();
		List<(double, double, int)?> list3 = new List<(double, double, int)?>();
		for (int i = 0; i < Vertices.Count; i++)
		{
			Vector2d vector2d = Vertices[i];
			Vector2d vector2d2 = Vertices[(i + 1) % Vertices.Count];
			Line2D line2D = new Line2D(vector2d, vector2d2);
			double num = line.ParameterOfClosestPointOnAxis(line2D.P0);
			double num2 = line.ParameterOfClosestPointOnAxis(line2D.P1);
			if (line.Direction.Normalized.IsParallel(line2D.Direction.Normalized, out var direction))
			{
				if (Math.Abs(line.DistanceToPoint(line2D.P0)) < parallelDisThreshold)
				{
					if (num > num2)
					{
						double num3 = num2;
						num2 = num;
						num = num3;
					}
					if (num >= 0.0 && num2 <= 1.0)
					{
						list3.Add((num, num2, direction));
					}
					else if (num < 0.0 && num2 <= 1.0)
					{
						list3.Add((0.0, num2, direction));
					}
					else if (num >= 0.0 && num2 > 1.0)
					{
						list3.Add((num, 1.0, direction));
					}
					else if (num < 0.0 && num2 > 1.0)
					{
						list3.Add((0.0, 1.0, direction));
					}
				}
			}
			else
			{
				if (!line.Intersect(line2D, out var isec, out var t, out var u))
				{
					continue;
				}
				if (((t >= 0.0 && t <= 1.0) || (line.P0 - isec).Length < 1E-06 || (line.P1 - isec).Length < 1E-06) && ((u >= 0.0 && u <= 1.0) || (vector2d - isec).Length < 1E-06 || (vector2d2 - isec).Length < 1E-06))
				{
					list2.Add(t);
				}
				else if (t >= 0.0 && t <= 1.0)
				{
					if ((vector2d - line.EvalParam(num)).Length < parallelDisThreshold || (vector2d2 - line.EvalParam(num)).Length < parallelDisThreshold)
					{
						list2.Add(num);
					}
					else if ((vector2d - line.EvalParam(num2)).Length < parallelDisThreshold || (vector2d2 - line.EvalParam(num2)).Length < parallelDisThreshold)
					{
						list2.Add(num2);
					}
				}
			}
		}
		List<Vector2d> intersections;
		if (list2.Any())
		{
			list2.Sort();
			if (Math.Abs(list2[0]) < 1E-06)
			{
				list2[0] = 0.0;
			}
			else
			{
				list2.Insert(0, 0.0);
			}
			if (Math.Abs(list2[list2.Count - 1] - 1.0) < 1E-06)
			{
				list2[list2.Count - 1] = 1.0;
			}
			else
			{
				list2.Add(1.0);
			}
			for (int j = 0; j < list2.Count - 1; j++)
			{
				double t2 = list2[j];
				double num4 = list2[j + 1];
				(double, double, int)? tuple = list3.FirstOrDefault<(double, double, int)?>(((double t0, double t1, int dir)? ol) => Math.Abs(ol.Value.t0 - t2) < 1E-06);
				if (tuple.HasValue)
				{
					list.Add((t2, num4, (tuple.Value.Item3 == 1) ? SpacialRelation.OnBorderSameSide : SpacialRelation.OnBorderOtherSide));
					continue;
				}
				Vector2d p = line.P0 + 0.5 * (t2 + num4) * line.Direction;
				if (IsPointInPolygon(p, out intersections))
				{
					list.Add((t2, num4, SpacialRelation.Inside));
				}
				else
				{
					list.Add((t2, num4, SpacialRelation.Outside));
				}
			}
		}
		else if (IsPointInPolygon(line.P0, out intersections))
		{
			list.Add((0.0, 1.0, SpacialRelation.Inside));
		}
		return list;
	}

	public bool Intersect(Polygon other)
	{
		return Intersect(other, checkThinInsideOther: true);
	}

	public bool Intersect(Polygon other, bool checkThinInsideOther)
	{
		List<Vector2d> intersections;
		foreach (Vector2d vertex in other.Vertices)
		{
			if (IsPointInPolygon(vertex, out intersections))
			{
				return true;
			}
		}
		Vector2d p = Vertices.First();
		if (checkThinInsideOther)
		{
			return other.IsPointInPolygon(p, out intersections);
		}
		return false;
	}

	public static Polygon ConvexHull(List<Vector2d> points)
	{
		return new Polygon
		{
			Vertices = ConvexHull2D.MakeHull(points).ToList()
		};
	}

	public bool Includes(Polygon other)
	{
		List<Vector2d> intersections;
		return other.Vertices.All((Vector2d p) => IsPointInPolygon(p, out intersections));
	}

	public string PrintVertices()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Vector2d vertex in Vertices)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 2, stringBuilder2);
			handler.AppendFormatted(vertex.X);
			handler.AppendLiteral("\t");
			handler.AppendFormatted(vertex.Y);
			stringBuilder2.AppendLine(ref handler);
		}
		return stringBuilder.ToString();
	}

	public IEnumerable<Polygon> OffsetPolygon(double offset)
	{
		int SizeFaktor = 10000;
		ClipperOffset clipperOffset = new ClipperOffset();
		clipperOffset.AddPath(Vertices.Select((Vector2d p) => new IntPoint(p.X * (double)SizeFaktor, p.Y * (double)SizeFaktor)).ToList(), JoinType.jtSquare, EndType.etClosedPolygon);
		List<List<IntPoint>> solution = new List<List<IntPoint>>();
		clipperOffset.Execute(ref solution, offset);
		foreach (List<IntPoint> item in solution)
		{
			yield return new Polygon
			{
				Vertices = item.Select((IntPoint p) => new Vector2d((double)p.X / (double)SizeFaktor, (double)p.Y / (double)SizeFaktor)).ToList()
			};
		}
	}
}

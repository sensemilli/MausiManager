using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.TubeInfos;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.CAD.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD;

public class Cad2DDatabase : ICad2DDatabase
{
	private readonly string _modelName;

	private HashSet<HashSet<CadGeoElement>> _innerElements;

	private HashSet<CadGeoElement> _outerLines;

	private readonly HashSet<CadTxtText> _texts;

	private readonly IDoc3d _doc;

	private readonly IConfigProvider _config;

	private readonly IPnPathService _pathService;

	public MinMax Boundary { get; private set; }

	public IEnumerable<HashSet<CadGeoElement>> InnerElements()
	{
		return this._innerElements;
	}

	public IEnumerable<CadGeoElement> OuterElements()
	{
		return this._outerLines;
	}

	public IEnumerable<CadTxtText> TextElements()
	{
		return this._texts;
	}

	public Cad2DDatabase(IDoc3d doc, IConfigProvider config, string modelName)
	{
		this._doc = doc;
		this._config = config;
		this._pathService = this._doc.Factorio.Resolve<IPnPathService>();
		this._innerElements = new HashSet<HashSet<CadGeoElement>>();
		this._outerLines = new HashSet<CadGeoElement>();
		this._texts = new HashSet<CadTxtText>();
		this._modelName = modelName;
	}

	public void CalculateMinMax()
	{
		this.Boundary = new MinMax();
		foreach (CadGeoElement outerLine in this._outerLines)
		{
			if (outerLine.Type == 1)
			{
				if (outerLine is CadGeoLine cadGeoLine)
				{
					this.Boundary.CheckPoint(new Vector3d(cadGeoLine.StartPoint.X, cadGeoLine.StartPoint.Y, 0.0));
					this.Boundary.CheckPoint(new Vector3d(cadGeoLine.EndPoint.X, cadGeoLine.EndPoint.Y, 0.0));
				}
				continue;
			}
			double[] array = new double[2];
			if (outerLine is CadGeoCircle { Radius: var radius } cadGeoCircle)
			{
				double num;
				double num2;
				if (cadGeoCircle.Direction == 1)
				{
					num = cadGeoCircle.StartAngle * Math.PI / 180.0;
					num2 = cadGeoCircle.EndAngle * Math.PI / 180.0;
				}
				else
				{
					num = cadGeoCircle.EndAngle * Math.PI / 180.0;
					num2 = cadGeoCircle.StartAngle * Math.PI / 180.0;
				}
				if (Math.Abs(num - num2) < 1E-05)
				{
					num = 0.0;
					num2 = Math.PI * 2.0;
				}
				double num3 = ((!(num > num2)) ? (num2 - num) : (Math.PI * 2.0 - num + num2));
				if (Math.Abs(num3) < 1E-05)
				{
					num3 = Math.PI * 2.0;
				}
				for (double num4 = num; num4 <= num + num3; num4 += num3 / 10.0)
				{
					array[0] = radius * Math.Cos(num4) + cadGeoCircle.Center.X;
					array[1] = radius * Math.Sin(num4) + cadGeoCircle.Center.Y;
					this.Boundary.CheckPoint(new Vector3d(array[0], array[1], 0.0));
				}
			}
		}
	}

	public bool AddCircle(SimpleHole simpleHole, Matrix4d worldMatrix)
	{
		if (!worldMatrix.TransformNormal(simpleHole.Orientation).IsParallel(Vector3d.UnitZ))
		{
			return false;
		}
		Vector3d v = simpleHole.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		int baseColorPn = this._config.InjectOrCreate<Macro3DConfig>().BaseColorPn;
		this.AddInnerLine(new HashSet<CadGeoElement>
		{
			new CadGeoCircle
			{
				Color = baseColorPn,
				Type = 2,
				Center = new Vector2d(Math.Round(v.X, 5), Math.Round(v.Y, 5)),
				Radius = simpleHole.Radius,
				StartAngle = 0.0,
				EndAngle = 360.0,
				Direction = -1
			}
		});
		return true;
	}

	public bool AddTwoSidedCounterSink(TwoSidedCounterSink twoSidedCounterSink, Matrix4d worldMatrix)
	{
		return TwoSideCountersinkToCadConverter.AddCadGeoElements(twoSidedCounterSink, worldMatrix, this, this._config);
	}

	public bool AddStepDrilling(StepDrilling stepDrilling, Matrix4d worldMatrix)
	{
		return StepDrillingToCadConverter.AddCadGeoElements(stepDrilling, worldMatrix, this, this._config);
	}

	public bool AddCounterSink(CounterSink counterSink, Matrix4d worldMatrix)
	{
		return CounterSinkToCadConverter.AddCadGeoElements(counterSink, worldMatrix, this, this._config, this._pathService);
	}

	public bool AddEmbossedCounterSink(EmbossedCounterSink embossedCounterSink, Matrix4d worldMatrix)
	{
		return EmbossedCounterSinkToCadConverter.AddCadGeoElements(embossedCounterSink, worldMatrix, this, this._config);
	}

	public bool AddLouver(Louver louver, Matrix4d worldMatrix)
	{
		return LouverToCadConverter.AddCadGeoElements(louver, worldMatrix, this, this._config);
	}

	public bool AddBorder(Border border, Matrix4d worldMatrix)
	{
		return BorderToCadConverter.AddCadGeoElements(border, worldMatrix, this, this._config);
	}

	public bool AddChamfer(Chamfer chamfer, Matrix4d worldMatrix, ref int counter, bool showChamferLine)
	{
		return ChamferToCadConverter.AddCadGeoElements(chamfer, worldMatrix, this, this._config, ref counter, showChamferLine);
	}

	public bool AddPressNut(PressNut pressNut, Matrix4d worldMatrix)
	{
		return PressNutToCadConverter.AddCadGeoElements(pressNut, worldMatrix, this, this._config);
	}

	public bool AddBolt(Bolt bolt, Matrix4d worldMatrix)
	{
		return BoltToCadConverter.AddCadGeoElements(bolt, worldMatrix, this, this._config);
	}

	public bool AddThread(Thread thread, Matrix4d worldMatrix)
	{
		return ThreadToCadConverter.AddCadGeoElements(thread, worldMatrix, this, this._config);
	}

	public bool AddBridge(BridgeLance bridge, Matrix4d worldMatrix)
	{
		return BridgeLanceToCadConverter.AddCadGeoElements(bridge, worldMatrix, this, this._config);
	}

	public bool AddLance(Lance lance, Matrix4d worldMatrix)
	{
		return LanceToCadConverter.AddCadGeoElements(lance, worldMatrix, this, this._config);
	}

	public bool AddDeepening(Deepening deepening, Matrix4d worldMatrix, Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap)
	{
		return DeepeningToCadConverter.AddCadGeoElements(deepening, worldMatrix, this, this._config, contourCircleMap);
	}

	public bool AddEmbossmentStamp(EmbossmentStamp eStamp, Matrix4d worldMatrix)
	{
		return EmbossmentStampToCadConverter.AddCadGeoElements(eStamp, worldMatrix, this, this._config);
	}

	public bool AddBlindHole(BlindHole blindHole, Matrix4d worldMatrix, Face topFace)
	{
		return BlindHoleToCadConverter.AddCadGeoElements(blindHole, worldMatrix, this, this._config, topFace);
	}

	public bool AddConicBlindHole(ConicBlindHole conicBlindHole, Matrix4d worldMatrix, Face topFace)
	{
		return ConicBlindHoleToCadConverter.AddCadGeoElements(conicBlindHole, worldMatrix, this, this._config, topFace);
	}

	public bool AddSphericalBlindHole(SphericalBlindHole sphericalBlindHole, Matrix4d worldMatrix, Face topFace)
	{
		return SphericalBlindHoleToCadConverter.AddCadGeoElements(sphericalBlindHole, worldMatrix, this, this._config, topFace);
	}

	public bool AddEmbossedCircle(EmbossedCircle embossed, Matrix4d worldMatrix)
	{
		return EmbossedCircleToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedLine(EmbossedLine embossed, Matrix4d worldMatrix)
	{
		return EmbossedLineToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedSquare(EmbossedSquare embossed, Matrix4d worldMatrix)
	{
		return EmbossedSquareToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedRectangle(EmbossedRectangle embossed, Matrix4d worldMatrix)
	{
		return EmbossedRectangleToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedSquareRounded(EmbossedSquareRounded embossed, Matrix4d worldMatrix)
	{
		return EmbossedSquareRoundedToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedRectangleRounded(EmbossedRectangleRounded embossed, Matrix4d worldMatrix)
	{
		return EmbossedRectangleRoundedToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddEmbossedFreeform(EmbossedFreeform embossed, Matrix4d worldMatrix)
	{
		return EmbossedFreeformToCadConverter.AddCadGeoElements(embossed, worldMatrix, this, this._config);
	}

	public bool AddManufacturingData(ManufacturingMacro mm, Matrix4d worldMatrix)
	{
		return ManufacturingMacroToCadConverter.AddCadGeoElements(mm, worldMatrix, this, this._config);
	}

	public void AddText(CadTxtText cadTxtText)
	{
		this._texts.Add(cadTxtText);
	}

	public void AddInnerLine(HashSet<CadGeoElement> element)
	{
		this._innerElements.Add(element);
	}

	private void AddOuterLine(CadGeoElement line)
	{
		this._outerLines.Add(line);
	}

	public void AddInnerElements(IEnumerable<HashSet<CadGeoElement>> elements)
	{
		foreach (HashSet<CadGeoElement> element in elements)
		{
			this._innerElements.Add(element);
		}
	}

	public void AddInnerElements(IEnumerable<CadGeoElement> elements)
	{
		this._innerElements.Add(new HashSet<CadGeoElement>(elements));
	}

	public void AddOuterElements(IEnumerable<CadGeoElement> elements)
	{
		foreach (CadGeoElement element in elements)
		{
			this._outerLines.Add(element);
		}
	}

	public void AddTextElements(IEnumerable<CadTxtText> elements)
	{
		foreach (CadTxtText element in elements)
		{
			this._texts.Add(element);
		}
	}

	private static void AddLine(CadGeoLine element, TextWriter file)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Color.ToString(), 6));
		stringBuilder.Append("     0     0     1     0     0");
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Type.ToString(), 6));
		stringBuilder.Append(Cad2DDatabase.AddGaps("0", 6));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.StartPoint.X.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.StartPoint.Y.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.EndPoint.X.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.EndPoint.Y.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps("0.00000", 14));
		file.WriteLine(stringBuilder.ToString());
	}

	private static void AddCircle(CadGeoCircle element, TextWriter file)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Color.ToString(), 6));
		stringBuilder.Append("     0     0     1     0     0");
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Type.ToString(), 6));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Direction.ToString(), 6));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Center.X.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Center.Y.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.Diameter.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.StartAngle.ToString(".00000", CultureInfo.InvariantCulture), 14));
		stringBuilder.Append(Cad2DDatabase.AddGaps(element.EndAngle.ToString(".00000", CultureInfo.InvariantCulture), 14));
		file.WriteLine(stringBuilder.ToString());
	}

	private static string AddGaps(string line, int spaces)
	{
		if (line.Length >= spaces)
		{
			return line;
		}
		string text = null;
		int num = spaces - line.Length;
		for (int i = 0; i < num; i++)
		{
			text += " ";
		}
		line = text + line;
		return line;
	}

	public void SimpleAddEdge(Line2D edge, int cl, bool isHole)
	{
		CadGeoLine cadGeoLine = new CadGeoLine
		{
			Color = cl,
			Type = 1,
			StartPoint = new Vector2d(Math.Round(edge.P0.X, 5), Math.Round(edge.P0.Y, 5)),
			EndPoint = new Vector2d(Math.Round(edge.P1.X, 5), Math.Round(edge.P1.Y, 5))
		};
		if (isHole)
		{
			this.AddInnerLine(new HashSet<CadGeoElement> { cadGeoLine });
		}
		else
		{
			this.AddOuterLine(cadGeoLine);
		}
	}

	public void SaveCadTxt(string homeDrive, string homePath)
	{
		string path = homeDrive + homePath + "\\CADTXT";
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CadTxtText text in this._texts)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
			handler.AppendLiteral("T  2 ");
			handler.AppendFormatted(text.Text);
			stringBuilder2.AppendLine(ref handler);
			StringBuilder stringBuilder3 = new StringBuilder();
			stringBuilder3.Append(Cad2DDatabase.AddGaps(text.Position.X.ToString(".0000", CultureInfo.InvariantCulture), 12));
			stringBuilder3.Append(Cad2DDatabase.AddGaps(text.Position.Y.ToString(".0000", CultureInfo.InvariantCulture), 12));
			stringBuilder3.Append(Cad2DDatabase.AddGaps(text.Angle.ToString(".0000", CultureInfo.InvariantCulture), 12));
			stringBuilder3.Append(Cad2DDatabase.AddGaps(text.Height.ToString(".0000", CultureInfo.InvariantCulture), 12));
			stringBuilder3.Append(Cad2DDatabase.AddGaps(text.Color.ToString(), 5));
			stringBuilder.AppendLine(stringBuilder3.ToString());
		}
		File.WriteAllText(path, stringBuilder.ToString(), CurrentEncoding.SystemEncoding);
	}

	public void SaveCadGeo(double preRotation)
	{
		using StreamWriter streamWriter = new StreamWriter(Environment.GetEnvironmentVariable("PNHOMEDRIVE") + Environment.GetEnvironmentVariable("PNHOMEPATH") + "\\CADGEO", append: false, CurrentEncoding.SystemEncoding);
		streamWriter.WriteLine(" V  3.0");
		if (!this._doc.UnfoldModel3D.PartInfo.PartType.HasFlag(PartType.Tube) || (this._doc.UnfoldModel3D.PartInfo.PartType.HasFlag(PartType.Tube) && this._doc.UnfoldModel3D.PartInfo.TubeType != TubeType.RoundTube && this._doc.UnfoldModel3D.PartInfo.TubeType != TubeType.RectangularTube && this._doc.UnfoldModel3D.PartInfo.TubeType != TubeType.SquareTube))
		{
			for (int i = 1; i < 38; i++)
			{
				streamWriter.WriteLine(' ');
			}
		}
		else
		{
			for (int j = 1; j < 34; j++)
			{
				streamWriter.WriteLine(' ');
			}
			switch (this._doc.UnfoldModel3D.PartInfo.TubeType)
			{
			case TubeType.RoundTube:
				streamWriter.WriteLine(TubeType.RoundTube.ToString() + ";");
				streamWriter.WriteLine(' ');
				streamWriter.WriteLine((((RoundTubeInfo)this._doc.UnfoldModel3D.PartInfo.TubeInfo).Radius * 2.0).ToString(".00", CultureInfo.InvariantCulture) + ";");
				streamWriter.WriteLine(' ');
				break;
			case TubeType.RectangularTube:
			{
				streamWriter.WriteLine(TubeType.RectangularTube.ToString() + ";");
				streamWriter.WriteLine(' ');
				RectangularTubeInfo rectangularTubeInfo = (RectangularTubeInfo)this._doc.UnfoldModel3D.PartInfo.TubeInfo;
				streamWriter.WriteLine(rectangularTubeInfo.Width.ToString(".00", CultureInfo.InvariantCulture) + ";" + rectangularTubeInfo.Height.ToString(".00", CultureInfo.InvariantCulture) + ";" + rectangularTubeInfo.CornerRadius.ToString(".00", CultureInfo.InvariantCulture) + ";");
				streamWriter.WriteLine(' ');
				break;
			}
			case TubeType.SquareTube:
			{
				streamWriter.WriteLine(TubeType.RectangularTube.ToString() + ";");
				streamWriter.WriteLine(' ');
				SquareTubeInfo squareTubeInfo = (SquareTubeInfo)this._doc.UnfoldModel3D.PartInfo.TubeInfo;
				streamWriter.WriteLine(squareTubeInfo.Width.ToString(".00", CultureInfo.InvariantCulture) + ";" + squareTubeInfo.Width.ToString(".00", CultureInfo.InvariantCulture) + ";" + squareTubeInfo.CornerRadius.ToString(".00", CultureInfo.InvariantCulture) + ";");
				streamWriter.WriteLine(' ');
				break;
			}
			}
		}
		streamWriter.WriteLine(Cad2DDatabase.AddGaps(preRotation.ToString(".00", CultureInfo.InvariantCulture), 7));
		streamWriter.WriteLine("");
		streamWriter.WriteLine(this._modelName);
		foreach (CadGeoElement outerLine in this._outerLines)
		{
			if (outerLine.GetType() == typeof(CadGeoLine))
			{
				Cad2DDatabase.AddLine(outerLine as CadGeoLine, streamWriter);
			}
			else
			{
				Cad2DDatabase.AddCircle(outerLine as CadGeoCircle, streamWriter);
			}
		}
		foreach (HashSet<CadGeoElement> innerElement in this._innerElements)
		{
			foreach (CadGeoElement item in innerElement)
			{
				if (item.GetType() == typeof(CadGeoLine))
				{
					Cad2DDatabase.AddLine(item as CadGeoLine, streamWriter);
				}
				else
				{
					Cad2DDatabase.AddCircle(item as CadGeoCircle, streamWriter);
				}
			}
		}
	}

	public void SimplifyGeometry()
	{
		this._outerLines = Cad2DDatabase.SimplifyGeometry(this._outerLines);
		HashSet<HashSet<CadGeoElement>> hashSet = new HashSet<HashSet<CadGeoElement>>();
		foreach (HashSet<CadGeoElement> innerElement in this._innerElements)
		{
			HashSet<CadGeoElement> hashSet2 = Cad2DDatabase.SimplifyGeometry(innerElement);
			if (hashSet2.Count > 0)
			{
				hashSet.Add(hashSet2);
			}
		}
		this._innerElements = hashSet;
	}

	public static HashSet<CadGeoElement> SimplifyGeometry(HashSet<CadGeoElement> elements, double minLineLength = 1E-06, double maxDisplacement = 0.005)
	{
		HashSet<CadGeoElement> hashSet = Cad2DDatabase.CombineCircles(elements);
		foreach (CadGeoElement item in Cad2DDatabase.CombineLines(elements, maxDisplacement))
		{
			if (!(item is CadGeoLine cadGeoLine) || !(cadGeoLine.Length < minLineLength))
			{
				hashSet.Add(item);
			}
		}
		return new HashSet<CadGeoElement>(hashSet);
	}

	private static HashSet<CadGeoElement> CombineCircles(IEnumerable<CadGeoElement> elements)
	{
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>(elements.Where((CadGeoElement x) => x.GetType() == typeof(CadGeoCircle)));
		HashSet<CadGeoElement> hashSet2 = new HashSet<CadGeoElement>();
		HashSet<CadGeoCircle> hashSet3 = new HashSet<CadGeoCircle>();
		foreach (CadGeoCircle circle in hashSet)
		{
			if (hashSet3.Contains(circle))
			{
				continue;
			}
			if ((Math.Abs(circle.StartAngle) < 0.011 && Math.Abs(circle.EndAngle - 360.0) < 0.011) || (Math.Abs(circle.StartAngle - 360.0) < 0.011 && Math.Abs(circle.EndAngle) < 0.011))
			{
				hashSet2.Add(circle);
				hashSet3.Add(circle);
				foreach (CadGeoCircle item in hashSet.Where((CadGeoElement x) => ((x as CadGeoCircle).Center - circle.Center).Length < 0.011 && Math.Abs((x as CadGeoCircle).Radius - circle.Radius) < 0.011 && ((Math.Abs(circle.StartAngle) < 0.011 && Math.Abs(circle.EndAngle - 360.0) < 0.011) || (Math.Abs(circle.StartAngle - 360.0) < 0.011 && Math.Abs(circle.EndAngle) < 0.011)) && x.Color == circle.Color).ToList())
				{
					hashSet3.Add(item);
				}
				continue;
			}
			List<CadGeoElement> list = hashSet.Where((CadGeoElement x) => ((x as CadGeoCircle).Center - circle.Center).Length < 0.011 && Math.Abs((x as CadGeoCircle).Radius - circle.Radius) < 0.011 && x.Color == circle.Color).ToList();
			foreach (CadGeoElement item2 in list)
			{
				CadGeoCircle cadGeoCircle = item2 as CadGeoCircle;
				if (cadGeoCircle.Direction != -1)
				{
					double startAngle = cadGeoCircle.StartAngle;
					cadGeoCircle.StartAngle = cadGeoCircle.EndAngle;
					cadGeoCircle.EndAngle = startAngle;
					cadGeoCircle.Direction = -1;
				}
			}
			while (list.Count > 0)
			{
				CadGeoCircle cadGeoCircle2 = list.First() as CadGeoCircle;
				list.Remove(cadGeoCircle2);
				if (hashSet3.Contains(cadGeoCircle2))
				{
					continue;
				}
				hashSet3.Add(cadGeoCircle2);
				CadGeoCircle newCircle = new CadGeoCircle
				{
					Color = cadGeoCircle2.Color,
					Center = cadGeoCircle2.Center,
					Radius = cadGeoCircle2.Radius,
					StartAngle = cadGeoCircle2.StartAngle,
					EndAngle = cadGeoCircle2.EndAngle,
					Type = cadGeoCircle2.Type,
					Direction = cadGeoCircle2.Direction
				};
				for (CadGeoElement cadGeoElement = list.FirstOrDefault((CadGeoElement x) => Math.Abs(((x as CadGeoCircle).StartAngle - newCircle.EndAngle) % 360.0) < 0.15 || Math.Abs(((x as CadGeoCircle).StartAngle - newCircle.EndAngle) % 360.0 - 360.0) < 0.15); cadGeoElement != null; cadGeoElement = list.FirstOrDefault((CadGeoElement x) => Math.Abs(((x as CadGeoCircle).StartAngle - newCircle.EndAngle) % 360.0) < 0.15 || Math.Abs(((x as CadGeoCircle).StartAngle - newCircle.EndAngle) % 360.0 - 360.0) < 0.15))
				{
					newCircle.EndAngle = (cadGeoElement as CadGeoCircle).EndAngle;
					list.Remove(cadGeoElement);
					hashSet3.Add((CadGeoCircle)cadGeoElement);
				}
				for (CadGeoElement cadGeoElement2 = list.FirstOrDefault((CadGeoElement x) => Math.Abs(((x as CadGeoCircle).EndAngle - newCircle.StartAngle) % 360.0) < 0.15 || Math.Abs(((x as CadGeoCircle).EndAngle - newCircle.StartAngle) % 360.0 - 360.0) < 0.15); cadGeoElement2 != null; cadGeoElement2 = list.FirstOrDefault((CadGeoElement x) => Math.Abs(((x as CadGeoCircle).EndAngle - newCircle.StartAngle) % 360.0) < 0.15 || Math.Abs(((x as CadGeoCircle).EndAngle - newCircle.StartAngle) % 360.0 - 360.0) < 0.15))
				{
					newCircle.StartAngle = (cadGeoElement2 as CadGeoCircle).StartAngle;
					list.Remove(cadGeoElement2);
					hashSet3.Add((CadGeoCircle)cadGeoElement2);
				}
				hashSet2.Add(newCircle);
			}
		}
		hashSet = new HashSet<CadGeoElement>(hashSet2);
		hashSet3 = new HashSet<CadGeoCircle>();
		HashSet<CadGeoElement> hashSet4 = new HashSet<CadGeoElement>();
		foreach (CadGeoCircle item3 in hashSet)
		{
			if (hashSet3.Contains(item3))
			{
				continue;
			}
			hashSet4.Add(item3);
			hashSet3.Add(item3);
			foreach (CadGeoCircle item4 in hashSet)
			{
				if (!hashSet3.Contains(item4) && (item4.Center - item3.Center).Length < 0.011 && Math.Abs(item4.Radius - item3.Radius) < 0.011 && item4.Color == item3.Color && Math.Abs(item4.EndAngle - item3.EndAngle) < 0.011 && Math.Abs(item4.StartAngle - item3.StartAngle) < 0.011)
				{
					hashSet3.Add(item4);
				}
			}
		}
		return hashSet4;
	}

	private static IEnumerable<CadGeoElement> CombineLines(IEnumerable<CadGeoElement> elements, double tolerance = 0.005)
	{
		CadGeoElement[] array = elements.Where((CadGeoElement x) => x.GetType() == typeof(CadGeoLine)).ToArray();
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		HashSet<CadGeoLine> hashSet2 = new HashSet<CadGeoLine>();
		CadGeoElement[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			CadGeoLine cadGeoLine = (CadGeoLine)array2[i];
			if (hashSet2.Contains(cadGeoLine))
			{
				continue;
			}
			hashSet2.Add(cadGeoLine);
			CadGeoLine cadGeoLine2 = cadGeoLine;
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int j = 0; j < array.Length; j++)
				{
					CadGeoLine cadGeoLine3 = array[j] as CadGeoLine;
					if (hashSet2.Contains(cadGeoLine3) || cadGeoLine3.Color != cadGeoLine2.Color || Math.Max(cadGeoLine2.StartPoint.X, cadGeoLine2.EndPoint.X) + tolerance < Math.Min(cadGeoLine3.StartPoint.X, cadGeoLine3.EndPoint.X) || Math.Max(cadGeoLine2.StartPoint.Y, cadGeoLine2.EndPoint.Y) + tolerance < Math.Min(cadGeoLine3.StartPoint.Y, cadGeoLine3.EndPoint.Y) || Math.Max(cadGeoLine3.StartPoint.X, cadGeoLine3.EndPoint.X) + tolerance < Math.Min(cadGeoLine2.StartPoint.X, cadGeoLine2.EndPoint.X) || Math.Max(cadGeoLine3.StartPoint.Y, cadGeoLine3.EndPoint.Y) + tolerance < Math.Min(cadGeoLine2.StartPoint.Y, cadGeoLine2.EndPoint.Y))
					{
						continue;
					}
					List<Vector2d> source = new List<Vector2d> { cadGeoLine3.EndPoint, cadGeoLine2.StartPoint, cadGeoLine3.StartPoint, cadGeoLine2.EndPoint };
					CadGeoLine cadGeoLine4 = cadGeoLine2;
					CadGeoLine cadGeoLine5 = cadGeoLine3;
					if ((cadGeoLine3.EndPoint - cadGeoLine3.StartPoint).LengthSquared > (cadGeoLine2.EndPoint - cadGeoLine2.StartPoint).LengthSquared)
					{
						cadGeoLine4 = cadGeoLine3;
						cadGeoLine5 = cadGeoLine2;
					}
					Vector2d direction = cadGeoLine4.Direction;
					double num = cadGeoLine4.StartPoint.Dot(direction);
					double num2 = cadGeoLine4.EndPoint.Dot(direction);
					double num3 = cadGeoLine5.StartPoint.Dot(direction);
					double num4 = cadGeoLine5.EndPoint.Dot(direction);
					if ((!(num3 < num) || !(num4 < num)) && (!(num3 > num2) || !(num4 > num2)))
					{
						List<Vector2d> list = source.OrderBy(((Vector2d)direction).Dot).ToList();
						Vector2d normalized = (list.Last() - list.First()).Normalized;
						Vector2d v = new Vector2d(normalized.Y, 0.0 - normalized.X);
						double num5 = 0.0 - v.Dot(list[0]);
						if (!(Math.Abs(list[1].Dot(v) + num5) > tolerance) && !(Math.Abs(list[2].Dot(v) + num5) > tolerance))
						{
							flag = true;
							cadGeoLine2.StartPoint = list.First();
							cadGeoLine2.EndPoint = list.Last();
							hashSet2.Add(cadGeoLine3);
						}
					}
				}
			}
			hashSet.Add(cadGeoLine2);
		}
		return hashSet;
	}

	private static double PerpendicularDotProduct(Vector2d vector1, Vector2d vector2, Vector2d point)
	{
		return (vector1.X - point.X) * (vector2.Y - point.Y) - (vector1.Y - point.Y) * (vector2.X - point.X);
	}

	private static bool IsPointOnLineViaPdp(Vector2d vector1, Vector2d vector2, Vector2d point)
	{
		return Math.Abs(Cad2DDatabase.PerpendicularDotProduct(vector1, vector2, point)) < 0.001;
	}

	private static bool IsPointOnLineSegment(Vector2d vector1, Vector2d vector2, Vector2d point)
	{
		if ((!(vector1.X <= point.X) || !(point.X <= vector2.X)) && (!(vector2.X <= point.X) || !(point.X <= vector1.X)))
		{
			return false;
		}
		if ((!(vector1.Y <= point.Y) || !(point.Y <= vector2.Y)) && (!(vector2.Y <= point.Y) || !(point.Y <= vector1.Y)))
		{
			return false;
		}
		return Cad2DDatabase.IsPointOnLineViaPdp(vector1, vector2, point);
	}

	public void Transfer(double dx, double dy)
	{
		foreach (CadGeoElement outerLine in this._outerLines)
		{
			if (outerLine.Type == 1 && outerLine is CadGeoLine cadGeoLine)
			{
				cadGeoLine.StartPoint = new Vector2d(cadGeoLine.StartPoint.X + dx, cadGeoLine.StartPoint.Y + dy);
				cadGeoLine.EndPoint = new Vector2d(cadGeoLine.EndPoint.X + dx, cadGeoLine.EndPoint.Y + dy);
			}
			else if (outerLine.Type == 2 && outerLine is CadGeoCircle cadGeoCircle)
			{
				cadGeoCircle.Center = new Vector2d(cadGeoCircle.Center.X + dx, cadGeoCircle.Center.Y + dy);
			}
		}
		foreach (HashSet<CadGeoElement> innerElement in this._innerElements)
		{
			foreach (CadGeoElement item in innerElement)
			{
				if (item.Type == 1 && item is CadGeoLine cadGeoLine2)
				{
					cadGeoLine2.StartPoint = new Vector2d(cadGeoLine2.StartPoint.X + dx, cadGeoLine2.StartPoint.Y + dy);
					cadGeoLine2.EndPoint = new Vector2d(cadGeoLine2.EndPoint.X + dx, cadGeoLine2.EndPoint.Y + dy);
				}
				else if (item.Type == 2 && item is CadGeoCircle cadGeoCircle2)
				{
					cadGeoCircle2.Center = new Vector2d(cadGeoCircle2.Center.X + dx, cadGeoCircle2.Center.Y + dy);
				}
			}
		}
		foreach (CadTxtText text in this._texts)
		{
			text.Position = new Vector2d(text.Position.X + dx, text.Position.Y + dy);
		}
	}

	public void Rotate(Vector2d rotationPoint, double angle)
	{
		foreach (CadGeoElement outerLine in this._outerLines)
		{
			Cad2DDatabase.RotateElement(outerLine, rotationPoint, angle);
		}
		foreach (HashSet<CadGeoElement> innerElement in this._innerElements)
		{
			foreach (CadGeoElement item in innerElement)
			{
				Cad2DDatabase.RotateElement(item, rotationPoint, angle);
			}
		}
		foreach (CadTxtText text in this._texts)
		{
			text.Position = Cad2DDatabase.RotatePoint(text.Position, rotationPoint, angle);
			text.Angle += angle * 180.0 / Math.PI;
			if (text.Angle < 0.0)
			{
				text.Angle = 360.0 + text.Angle;
			}
		}
	}

	private static void RotateElement(CadGeoElement element, Vector2d rotationPoint, double angle)
	{
		if (element.Type == 1 && element is CadGeoLine cadGeoLine)
		{
			cadGeoLine.StartPoint = Cad2DDatabase.RotatePoint(cadGeoLine.StartPoint, rotationPoint, angle);
			cadGeoLine.EndPoint = Cad2DDatabase.RotatePoint(cadGeoLine.EndPoint, rotationPoint, angle);
		}
		else if (element.Type == 2 && element is CadGeoCircle cadGeoCircle)
		{
			cadGeoCircle.Center = Cad2DDatabase.RotatePoint(cadGeoCircle.Center, rotationPoint, angle);
			double num = angle * 180.0 / Math.PI;
			cadGeoCircle.StartAngle += num;
			if (cadGeoCircle.StartAngle < 0.0)
			{
				cadGeoCircle.StartAngle = 360.0 + cadGeoCircle.StartAngle;
			}
			if (cadGeoCircle.StartAngle > 360.0)
			{
				cadGeoCircle.StartAngle -= 360.0;
			}
			cadGeoCircle.EndAngle += num;
			if (cadGeoCircle.EndAngle < 0.0)
			{
				cadGeoCircle.EndAngle = 360.0 + cadGeoCircle.EndAngle;
			}
			if (cadGeoCircle.EndAngle > 360.0)
			{
				cadGeoCircle.EndAngle -= 360.0;
			}
		}
	}

	private static Vector2d RotatePoint(Vector2d pointToRotate, Vector2d centerPoint, double angle)
	{
		double num = Math.Cos(angle);
		double num2 = Math.Sin(angle);
		Vector2d result = default(Vector2d);
		result.X = num * (pointToRotate.X - centerPoint.X) - num2 * (pointToRotate.Y - centerPoint.Y) + centerPoint.X;
		result.Y = num2 * (pointToRotate.X - centerPoint.X) + num * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y;
		return result;
	}
}

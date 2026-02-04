using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;
using WiCAM.Pn4000.PN3D.CAD;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Unfold;

public class ModelForPnStdPreparation : IModelForPnStdPreparation
{
	private class BendZoneEdge
	{
		public double Radius { get; set; }

		public double BendingAllowance { get; set; }

		public double KFactor { get; set; }

		public BendTableReturnValues KFactorAlgorithm { get; set; }

		public ToolSelectionType ToolSelectionAlgorithm { get; set; }

		public double AngleRad { get; set; }

		public Line2D BendingLine { get; set; }

		public FaceGroup BendingZone { get; set; }

		public ICombinedBendDescriptor CombinedBendDescriptor { get; set; }
	}

	private readonly IPrefabricatedPartsManager _prefabricatedPartsManager;

	private List<BendZoneEdge> _bendingZones;

	protected IDoc3d _doc;

	protected readonly IConfigProvider _configProvider;

	protected readonly IMaterialManager _materials;

	protected readonly IAnalyzeConfigProvider _analyzeConfigProvider;

	public Cad2DDatabase Db2D { get; private set; }

	public bool ErrorDetected { get; private set; }

	public ModelForPnStdPreparation(IPrefabricatedPartsManager prefabricatedPartsManager, IConfigProvider configProvider, IMaterialManager materials, IAnalyzeConfigProvider analyzeConfigProvider)
	{
		this._prefabricatedPartsManager = prefabricatedPartsManager;
		this._configProvider = configProvider;
		this._materials = materials;
		this._analyzeConfigProvider = analyzeConfigProvider;
	}

	public void Apply2D(IDoc3d doc, Model model, Face faceSelected, Model faceModelSelected, bool bendLines, bool removeProjectionHoles, bool includeZeroBorders)
	{
		this._doc = doc;
		Face face = faceSelected;
		Model faceModel = faceModelSelected;
		if (face == null)
		{
			(face, faceModel) = model.GetFirstFaceOfGroupModel(doc.VisibleFaceGroupId, doc.VisibleFaceGroupSide);
		}
		if (model.PartInfo.TubeType == TubeType.RoundTube || model.PartInfo.PartType == PartType.RollOffPlate)
		{
			_ = face.Mesh.MaxBy((Triangle t) => t.Area).CalculatedTriangleNormal;
		}
		else
		{
			if (face?.FlatFacePlane == null)
			{
				this.ErrorDetected = true;
				return;
			}
			_ = face.FlatFacePlane.Normal;
		}
		Matrix4d transform = model.Transform;
		Unfold.MoveUnfoldModelToZero(model, face, faceModel);
		Macro3DConfig macro3DConfig = this._configProvider.InjectOrCreate<Macro3DConfig>();
		this._configProvider.InjectOrCreate<PurchasedPartsCfg>();
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig analyzeConfig = this._analyzeConfigProvider.GetAnalyzeConfig();
		this.Db2D = new Cad2DDatabase(doc, this._configProvider, general3DConfig.P3D_UseIdsAs2DPartNames ? doc.EntryModel3D.BodyId.ToString() : doc.DiskFile.Header.ModelName.Replace(".err3d", ".err"));
		Plane topPlane = new Plane(new Vector3d(0.0, 0.0, 0.0), new Vector3d(0.0, 0.0, 1.0));
		this.GenerateOutline(model, topPlane, centerModel: false, removeProjectionHoles, faceSelected != null, includeZeroBorders, out var outlines, out var holes, out var tubeEnds, out var zeroBordersAdded);
		this._doc.ZeroBordersAdded2D = zeroBordersAdded;
		this.CorrectRoundVerticalFaces(model, topPlane, outlines, holes, out var contourCircleMap);
		if (model.PartInfo.PartType == PartType.Tube)
		{
			contourCircleMap.Clear();
		}
		foreach (List<Vector2d> item in outlines)
		{
			this.Db2D.AddOuterElements(ModelForPnStdPreparation.GetContour(item, macro3DConfig.BaseColorPn, model.PartInfo.PartType.HasFlag(PartType.Tube), contourCircleMap));
		}
		foreach (List<Vector2d> item2 in holes)
		{
			this.Db2D.AddInnerElements(ModelForPnStdPreparation.GetContour(item2, macro3DConfig.BaseColorPn, isOpenContour: false, contourCircleMap));
		}
		foreach (List<Vector2d> item3 in tubeEnds)
		{
			Cad2DDatabase db2D = this.Db2D;
			List<CadGeoElement> list = new List<CadGeoElement>();
			CadGeoLine obj = new CadGeoLine
			{
				Color = macro3DConfig.TubeCutColorPn,
				StartPoint = item3[0]
			};
			obj.EndPoint = item3[item3.Count - 1];
			obj.Type = 1;
			list.Add(obj);
			db2D.AddOuterElements(list);
		}
		HashSet<Macro> skippedMacros = new HashSet<Macro>();
		int counter = 0;
		foreach (Model subModel in model.SubModels)
		{
			this.GetMacros(subModel, face, ref skippedMacros, ref counter, analyzeConfig.ShowChamferLine, contourCircleMap);
		}
		if (skippedMacros.Count > 0)
		{
			this._doc.MessageDisplay.ShowTranslatedWarningMessage("Transfer2DWarning.SkippedMacrosWrongOrientation", skippedMacros.Count);
		}
		this.AddAdditionalParts(doc, model, macro3DConfig, topPlane);
		this.AddInvalidMacros(doc, model, macro3DConfig, topPlane);
		if (general3DConfig.P3D_Mark_ReliefIn2D && model == doc.UnfoldModel3D)
		{
			this.AddCutOutMarkers(doc, model, macro3DConfig);
		}
		if (bendLines)
		{
			this.AddBendLines(macro3DConfig, doc.CombinedBendDescriptors, doc.GetModelType(model));
			if (this._bendingZones != null)
			{
				this.AddBendLineTexts();
			}
		}
		this.Orientate2DModel(macro3DConfig, model, this._configProvider.InjectOrCreate<General3DConfig>());
		this.OriginPoint();
		this.BendingTextPreparation();
		model.Transform = transform;
	}

	protected virtual void GenerateOutline(Model model, Plane topPlane, bool centerModel, bool removeProjectionHoles, bool project3d, bool includeZeroBorders, out List<List<Vector2d>> outlines, out List<List<Vector2d>> holes, out List<List<Vector2d>> tubeEnds, out bool zeroBordersAdded)
	{
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		new OutlineDetector().GetOutline(model, topPlane, centerModel, null, removeProjectionHoles, removeMacroHoles: true, project3d, general3DConfig.P3D_Hide_ReliefIn2D ? new List<AuxiliaryShellType> { AuxiliaryShellType.CutOuts } : null, out outlines, out holes, out tubeEnds, out zeroBordersAdded, null, 4, includeZeroBorders);
	}

	private void CorrectRoundVerticalFaces(Model model, Plane topPlane, List<List<Vector2d>> outlines, List<List<Vector2d>> holes, out Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap)
	{
		contourCircleMap = new Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>>();
		IEnumerable<IGrouping<Shell, (Face face, Model model)>> enumerable = from x in model.GetAllFaceModels()
			where x.face.FaceType.HasFlag(FaceType.RoundCylindricalConvex) || x.face.FaceType.HasFlag(FaceType.RoundCylindricalConcave)
			select x into f
			group f by f.face.Shell;
		List<CircleSegment2D> list2 = new List<CircleSegment2D>();
		Matrix4d rot2Ground = topPlane.Normal.CreateRotMatrixToZUpSigned();
		foreach (IGrouping<Shell, (Face, Model)> item3 in enumerable)
		{
			Vector3d normal = item3.Key.GetWorldMatrix(item3.First().Item2).Inverted.TransformNormal(topPlane.Normal);
			List<(Face Face, int Dir)> list3 = item3.Select<(Face, Model), Face>(((Face face, Model model) x) => x.face).Aggregate(new List<(Face, int)>(), delegate(List<(Face Face, int Dir)> list, Face f)
			{
				if (f.CylindricalFaceRotationAxis.Direction.IsParallel(normal, out var direction))
				{
					list.Add((f, direction));
				}
				return list;
			});
			Model item = item3.First().Item2;
			foreach (var item4 in list3)
			{
				Face item2 = item4.Face;
				int num = ((item2.FaceType == FaceType.RoundCylindricalConvex) ? 1 : (-1));
				list2.Add(new CircleSegment2D(center: TriangleEdgeFunctions.CoordOnGround(item2.CylindricalFaceRotationAxis.Origin, item, rot2Ground), start: TriangleEdgeFunctions.CoordOnGround(item2.CylindricalFaceRotationAxis.Origin + item2.CylindricalFaceRotationAxis.MinNormal * item2.CylindricalFaceRotationAxis.Radius * num, item, rot2Ground), end: TriangleEdgeFunctions.CoordOnGround(item2.CylindricalFaceRotationAxis.Origin + item2.CylindricalFaceRotationAxis.MaxNormal * item2.CylindricalFaceRotationAxis.Radius * num, item, rot2Ground), radius: Math.Abs(item2.CylindricalFaceRotationAxis.Radius), isCCW: item4.Dir == 1, mode: CircleSegmentCreationMode.KeepCenterAndRadius));
			}
		}
		AABBTree<Vector2d, CircleSegment2D> aABBTree = new AABBTree<Vector2d, CircleSegment2D>();
		aABBTree.Build(list2, (CircleSegment2D cs) => cs.BoundingBox);
		Dictionary<(List<Vector2d>, CircleSegment2D), List<Vector2d>> dictionary = new Dictionary<(List<Vector2d>, CircleSegment2D), List<Vector2d>>();
		foreach (List<Vector2d> outline in outlines)
		{
			for (int i = 0; i < outline.Count; i++)
			{
				Vector2d p = outline[i];
				foreach (CircleSegment2D item5 in aABBTree.PointQuery(p, 0.015))
				{
					if (item5.IsPointOnSegment(p, 0.015))
					{
						if (!dictionary.TryGetValue((outline, item5), out var value))
						{
							value = new List<Vector2d>();
							dictionary[(outline, item5)] = value;
						}
						value.Add(outline[i]);
					}
				}
			}
		}
		foreach (List<Vector2d> hole in holes)
		{
			for (int j = 0; j < hole.Count; j++)
			{
				Vector2d p2 = hole[j];
				foreach (CircleSegment2D item6 in aABBTree.PointQuery(p2, 0.015))
				{
					if (item6.IsPointOnSegment(p2, 0.015))
					{
						if (!dictionary.TryGetValue((hole, item6), out var value2))
						{
							value2 = new List<Vector2d>();
							dictionary[(hole, item6)] = value2;
						}
						value2.Add(hole[j]);
					}
				}
			}
		}
		foreach (KeyValuePair<(List<Vector2d>, CircleSegment2D), List<Vector2d>> item7 in dictionary)
		{
			if (!contourCircleMap.TryGetValue(item7.Key.Item1, out var value3))
			{
				value3 = new List<Pair<CircleSegment2D, List<Vector2d>>>();
				contourCircleMap.Add(item7.Key.Item1, value3);
			}
			value3.Add(new Pair<CircleSegment2D, List<Vector2d>>(item7.Key.Item2, item7.Value));
		}
	}

	private void AddCutOutMarkers(IDoc3d doc, Model model, Macro3DConfig macro3DConfig)
	{
		IEnumerable<IGrouping<Shell, (Face face, Model model)>> enumerable = from x in model.GetAllFaceModels()
			where x.face.IsTessellated.HasValue
			group x by x.face.Shell;
		List<(Vector2d, int)> list = new List<(Vector2d, int)>();
		foreach (IGrouping<Shell, (Face, Model)> item2 in enumerable)
		{
			Matrix4d worldMatrix = item2.Key.GetWorldMatrix(item2.First().Item2);
			foreach (var item3 in item2)
			{
				Face item = item3.Item1;
				foreach (Triangle item4 in item.Mesh)
				{
					Vector3d vector3d = worldMatrix.Transform(item4.V0.Pos);
					Vector3d vector3d2 = worldMatrix.Transform(item4.V1.Pos);
					Vector3d vector3d3 = worldMatrix.Transform(item4.V2.Pos);
					list.Add((new Vector2d(vector3d.X, vector3d.Y), item.IsTessellated.Value));
					list.Add((new Vector2d(vector3d2.X, vector3d2.Y), item.IsTessellated.Value));
					list.Add((new Vector2d(vector3d3.X, vector3d3.Y), item.IsTessellated.Value));
				}
			}
		}
		foreach (IGrouping<int, (Vector2d, int)> item5 in from x in list
			group x by x.Item1)
		{
			var (faceGroup, model2) = model.GetFaceGroupModelById(Math.Abs(item5.Key));
			if (faceGroup != null && faceGroup.IsBendingZone)
			{
				Matrix4d worldMatrix2 = model2.WorldMatrix;
				Vector3d vector3d4 = worldMatrix2.Transform(faceGroup.ConcaveAxis.Origin);
				Vector3d vector3d5 = worldMatrix2.TransformNormal(faceGroup.ConcaveAxis.Direction);
				Vector3d vector3d6 = vector3d4 + vector3d5;
				Vector3d vector3d7 = vector3d5.Cross(Vector3d.UnitZ);
				Line2D line = new Line2D(new Vector2d(vector3d4.X, vector3d4.Y), new Vector2d(vector3d6.X, vector3d6.Y));
				AABB<Vector2d> aABB = new AABB<Vector2d>(item5.Select<(Vector2d, int), Vector2d>(((Vector2d p, int bend) p) => new Vector2d(line.ParameterOfClosestPointOnAxis(p.p), line.SignedDistanceToPoint(p.p))).ToList());
				List<Vector2d> points = new List<Vector3d>
				{
					vector3d4 + vector3d5 * aABB.Min.X + vector3d7 * aABB.Min.Y - vector3d7 * 0.05 - vector3d5 * 0.05,
					vector3d4 + vector3d5 * aABB.Min.X + vector3d7 * aABB.Max.Y + vector3d7 * 0.05 - vector3d5 * 0.05,
					vector3d4 + vector3d5 * aABB.Max.X + vector3d7 * aABB.Max.Y + vector3d7 * 0.05 + vector3d5 * 0.05,
					vector3d4 + vector3d5 * aABB.Max.X + vector3d7 * aABB.Min.Y - vector3d7 * 0.05 + vector3d5 * 0.05
				}.Select((Vector3d p) => new Vector2d(p.X, p.Y)).ToList();
				this.Db2D.AddOuterElements(ModelForPnStdPreparation.GetContour(points, macro3DConfig.CutOutMarkerColor));
				Vector2d v = line.P0 - line.P1;
				double num = (line.P0 - new Vector2d(line.P0.X + 1.0, line.P0.Y)).SignedAngle(v) * 180.0 / Math.PI;
				if (num >= -90.0 && num < 90.0)
				{
					num += 180.0;
				}
				if (num < 0.0)
				{
					num += 360.0;
				}
				Vector2d vector2d = (aABB.Min + aABB.Max) * 0.5;
				Vector3d vector3d8 = vector3d4 + vector3d5 * vector2d.X + vector3d7 * vector2d.Y;
				CadTxtText cadTxtText = new CadTxtText
				{
					Text = "Freischnitt",
					Position = new Vector2d(vector3d8.X, vector3d8.Y),
					Angle = num,
					Height = 0.5,
					Color = macro3DConfig.CutOutMarkerColor
				};
				this.Db2D.AddText(cadTxtText);
			}
		}
	}

	private bool VerifyOutline(List<List<Vector2d>> outlines, Model model)
	{
		if (outlines == null || outlines.Count < 1 || outlines.Any((List<Vector2d> x) => x.Count == 0))
		{
			return false;
		}
		Matrix4d? matrix4d = model.Parent?.WorldMatrix;
		Matrix4d valueOrDefault = matrix4d.GetValueOrDefault();
		if (!matrix4d.HasValue)
		{
			matrix4d = Matrix4d.Identity;
		}
		Pair<Vector3d, Vector3d> boundary = model.GetBoundary(matrix4d.Value);
		double num = outlines.SelectMany((List<Vector2d> x) => x).Min((Vector2d x) => x.X);
		double num2 = outlines.SelectMany((List<Vector2d> x) => x).Min((Vector2d x) => x.Y);
		double num3 = outlines.SelectMany((List<Vector2d> x) => x).Max((Vector2d x) => x.X);
		double num4 = outlines.SelectMany((List<Vector2d> x) => x).Max((Vector2d x) => x.Y);
		double num5 = 1.0;
		if (Math.Abs(num - boundary.Item1.X) > num5 || Math.Abs(num2 - boundary.Item1.Y) > num5 || Math.Abs(num3 - boundary.Item2.X) > num5 || Math.Abs(num4 - boundary.Item2.Y) > num5)
		{
			return false;
		}
		return true;
	}

	private void GetMacros(Model model, Face topFace, ref HashSet<Macro> skippedMacros, ref int counter, bool showChamferLine, Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap)
	{
		foreach (Shell shell in model.Shells)
		{
			Matrix4d worldMatrix = shell.GetWorldMatrix(model);
			foreach (Macro macro in shell.Macros)
			{
				if (!((macro is TwoSidedCounterSink twoSidedCounterSink) ? this.Db2D.AddTwoSidedCounterSink(twoSidedCounterSink, worldMatrix) : ((macro is StepDrilling stepDrilling) ? this.Db2D.AddStepDrilling(stepDrilling, worldMatrix) : ((macro is CounterSink counterSink) ? this.Db2D.AddCounterSink(counterSink, worldMatrix) : ((macro is EmbossmentStamp eStamp) ? this.Db2D.AddEmbossmentStamp(eStamp, worldMatrix) : ((macro is Deepening deepening) ? this.Db2D.AddDeepening(deepening, worldMatrix, contourCircleMap) : ((macro is SimpleHole simpleHole) ? this.Db2D.AddCircle(simpleHole, worldMatrix) : ((macro is Border border) ? this.Db2D.AddBorder(border, worldMatrix) : ((macro is Chamfer chamfer) ? this.Db2D.AddChamfer(chamfer, worldMatrix, ref counter, showChamferLine) : ((macro is EmbossedCounterSink embossedCounterSink) ? this.Db2D.AddEmbossedCounterSink(embossedCounterSink, worldMatrix) : ((macro is Louver louver) ? this.Db2D.AddLouver(louver, worldMatrix) : ((macro is PressNut pressNut) ? this.Db2D.AddPressNut(pressNut, worldMatrix) : ((macro is Bolt bolt) ? this.Db2D.AddBolt(bolt, worldMatrix) : ((macro is Thread thread) ? this.Db2D.AddThread(thread, worldMatrix) : ((macro is BridgeLance bridge) ? this.Db2D.AddBridge(bridge, worldMatrix) : ((macro is Lance lance) ? this.Db2D.AddLance(lance, worldMatrix) : ((macro is BlindHole blindHole) ? this.Db2D.AddBlindHole(blindHole, worldMatrix, topFace) : ((macro is ConicBlindHole conicBlindHole) ? this.Db2D.AddConicBlindHole(conicBlindHole, worldMatrix, topFace) : ((macro is SphericalBlindHole sphericalBlindHole) ? this.Db2D.AddSphericalBlindHole(sphericalBlindHole, worldMatrix, topFace) : ((macro is EmbossedCircle embossed) ? this.Db2D.AddEmbossedCircle(embossed, worldMatrix) : ((macro is EmbossedLine embossed2) ? this.Db2D.AddEmbossedLine(embossed2, worldMatrix) : ((macro is EmbossedSquare embossed3) ? this.Db2D.AddEmbossedSquare(embossed3, worldMatrix) : ((macro is EmbossedRectangle embossed4) ? this.Db2D.AddEmbossedRectangle(embossed4, worldMatrix) : ((macro is EmbossedSquareRounded embossed5) ? this.Db2D.AddEmbossedSquareRounded(embossed5, worldMatrix) : ((macro is EmbossedRectangleRounded embossed6) ? this.Db2D.AddEmbossedRectangleRounded(embossed6, worldMatrix) : ((macro is EmbossedFreeform embossed7) ? this.Db2D.AddEmbossedFreeform(embossed7, worldMatrix) : ((macro is ManufacturingMacro mm) ? this.Db2D.AddManufacturingData(mm, worldMatrix) : (macro is Dummy))))))))))))))))))))))))))))
				{
					skippedMacros.Add(macro);
				}
			}
		}
		foreach (Model subModel in model.SubModels)
		{
			this.GetMacros(subModel, topFace, ref skippedMacros, ref counter, showChamferLine, contourCircleMap);
		}
	}

	private void AddAdditionalParts(IDoc3d doc, Model model, Macro3DConfig macro3DCfg, Plane topPlane)
	{
		SimulationInstance currentSimulationInstancesAdditionalPart = doc.CurrentSimulationInstancesAdditionalPart;
		if (currentSimulationInstancesAdditionalPart == null)
		{
			return;
		}
		foreach (SimulationInstance.AdditionalGeometry additionalPart in currentSimulationInstancesAdditionalPart.AdditionalParts)
		{
			if (additionalPart.AdditionalGeometryType == SimulationInstance.AdditionalGeometryType.PruchasePart)
			{
				foreach (SimulationInstance.AdditionalGeometryInstance instance2 in additionalPart.Instances)
				{
					(FaceGroup faceGroup, Model model) faceGroupModelById = model.GetFaceGroupModelById(instance2.FaceGroupId);
					FaceGroup item = faceGroupModelById.faceGroup;
					Model item2 = faceGroupModelById.model;
					Matrix4d worldMatrix = item.GetAllFaces().First().Shell.GetWorldMatrix(item2);
					if (this._prefabricatedPartsManager.IgnoreNonHorizontalPlaneConnectedPp)
					{
						Vector3d? vector3d = item?.Side0.FirstOrDefault()?.FlatFacePlane?.Normal;
						if (vector3d.HasValue && !worldMatrix.TransformNormal(vector3d.Value).IsParallel(topPlane.Normal))
						{
							continue;
						}
					}
					Matrix4d transform = instance2.Transformation * worldMatrix;
					additionalPart.Model.Transform = transform;
					Pair<Vector3d, Vector3d> boundary = additionalPart.Model.GetBoundary(Matrix4d.Identity);
					Vector3d vector3d2 = (boundary.Item1 + boundary.Item2) * 0.5;
					int color;
					string text;
					if (vector3d2.Z < 0.0)
					{
						color = macro3DCfg.PurchasedPartsNegColorPn;
						text = "-";
					}
					else
					{
						color = macro3DCfg.PurchasedPartsPosColorPn;
						text = "+";
					}
					new OutlineDetector().GetOutline(additionalPart.Model, topPlane, centerModel: false, null, removeProjectionHoles: false, removeMacroHoles: true, project3d: false, null, out var outlines, out var _, out var _, out var _, null);
					foreach (List<Vector2d> item5 in outlines)
					{
						this.Db2D.AddOuterElements(ModelForPnStdPreparation.GetContour(item5, color));
					}
					string text2 = string.Format(CultureInfo.InvariantCulture, "_PP_" + additionalPart.AssemblyName + "_" + text, default(ReadOnlySpan<object>));
					CadTxtText cadTxtText = new CadTxtText
					{
						Text = text2,
						Angle = 180.0,
						Color = color,
						Height = 1.0,
						Position = new Vector2d(vector3d2.X, vector3d2.Y)
					};
					this.Db2D.AddText(cadTxtText);
				}
				continue;
			}
			List<FaceHalfEdge> list = additionalPart.Model.Shells.First().Faces.SelectMany((Face f) => f.BoundaryEdgesCcw.Concat(f.HoleEdgesCw.SelectMany((List<FaceHalfEdge> h) => h))).ToList();
			foreach (SimulationInstance.AdditionalGeometryInstance instance in additionalPart.Instances)
			{
				(FaceGroup faceGroup, Model model) faceGroupModelById2 = model.GetFaceGroupModelById(instance.FaceGroupId);
				FaceGroup item3 = faceGroupModelById2.faceGroup;
				Model item4 = faceGroupModelById2.model;
				Matrix4d wm = item3.GetAllFaces().First().Shell.GetWorldMatrix(item4);
				if (this._prefabricatedPartsManager.IgnoreNonHorizontalPlaneConnectedPp)
				{
					Vector3d? vector3d3 = item3?.Side0.FirstOrDefault()?.FlatFacePlane?.Normal;
					if (vector3d3.HasValue && !wm.TransformNormal(vector3d3.Value).IsParallel(topPlane.Normal))
					{
						continue;
					}
				}
				foreach (FaceHalfEdge item6 in list)
				{
					List<Vector3d> source = item6.Vertices.Select((Vertex v) => (instance.Transformation * wm).Transform(v.Pos)).ToList();
					List<Vector2d> points = source.Select((Vector3d p) => new Vector2d(p.X, p.Y)).ToList();
					this.Db2D.AddOuterElements(ModelForPnStdPreparation.GetContour(points, (source.First().Z > 0.0) ? macro3DCfg.DeepeningPosColorPn : macro3DCfg.DeepeningNegColorPn, isOpenContour: true));
				}
			}
		}
	}

	private void AddInvalidMacros(IDoc3d doc, Model model, Macro3DConfig macro3DConfig, Plane topPlane)
	{
		int col = macro3DConfig.UnknownMacroPosColorPn;
		HashSet<Face> hashSet = new HashSet<Face>(model.PartInfo.NotConformFaces.Values.SelectMany((List<Face> x) => x));
		Plane plane = new Plane(topPlane.Origin, -1.0 * topPlane.Normal);
		Face[] array = hashSet.ToArray();
		foreach (Face item in array)
		{
			if (!hashSet.Remove(item))
			{
				continue;
			}
			Stack<Face> stack = new Stack<Face>();
			stack.Push(item);
			HashSet<Face> hashSet2 = new HashSet<Face>();
			while (stack.Count > 0)
			{
				hashSet2.Add(stack.Peek());
				foreach (Face allNeighborFace in stack.Pop().GetAllNeighborFaces())
				{
					if (hashSet.Remove(allNeighborFace))
					{
						stack.Push(allNeighborFace);
					}
				}
			}
			OutlineDetector outlineDetector = new OutlineDetector();
			outlineDetector.GetOutline(model, plane, centerModel: false, hashSet2, removeProjectionHoles: false, removeMacroHoles: true, project3d: false, null, out var outlines, out var _, out var tubeEnds, out var zeroBordersAdded, null);
			outlineDetector.GetOutline(model, topPlane, centerModel: false, hashSet2, removeProjectionHoles: false, removeMacroHoles: true, project3d: false, null, out var outlines2, out var holes2, out tubeEnds, out zeroBordersAdded, null);
			double num = GetBbArea(outlines.SelectMany((List<Vector2d> x) => x));
			double num2 = GetBbArea(outlines2.SelectMany((List<Vector2d> x) => x));
			List<List<Vector2d>> source = outlines2;
			List<List<Vector2d>> source2 = holes2;
			if (num > num2)
			{
				Matrix4d matrix4d = plane.Normal.CreateRotMatrixToZUpSigned().Inverted * topPlane.Normal.CreateRotMatrixToZUpSigned();
				Vector3d vecX = matrix4d.Transform(Vector3d.UnitX);
				Vector3d vecY = matrix4d.Transform(Vector3d.UnitY);
				source = outlines.Select((List<Vector2d> x) => x.Select((Vector2d x) => GetVec2D(x.X * vecX + x.Y * vecY)).ToList()).ToList();
				source2 = holes2.Select((List<Vector2d> x) => x.Select((Vector2d x) => GetVec2D(x.X * vecX + x.Y * vecY)).ToList()).ToList();
			}
			this.Db2D.AddOuterElements(source.Select((List<Vector2d> x) => ModelForPnStdPreparation.GetContour(x, col)).Concat(source2.Select((List<Vector2d> x) => ModelForPnStdPreparation.GetContour(x, col))).SelectMany((List<CadGeoElement> x) => x));
		}
		static double GetBbArea(IEnumerable<Vector2d> points)
		{
			if (points.Any())
			{
				double num3 = double.MaxValue;
				double num4 = double.MinValue;
				double num5 = double.MaxValue;
				double num6 = double.MinValue;
				foreach (Vector2d point in points)
				{
					num3 = Math.Min(num3, point.X);
					num4 = Math.Max(num4, point.X);
					num5 = Math.Min(num5, point.Y);
					num6 = Math.Max(num6, point.Y);
				}
				return (num4 - num3) * (num6 - num5);
			}
			return 0.0;
		}
		static Vector2d GetVec2D(Vector3d v3)
		{
			return new Vector2d(v3.X, v3.Y);
		}
	}

	private void AddBendLines(Macro3DConfig macro3DConfig, IEnumerable<ICombinedBendDescriptor> commonBendSequence, UiModelType modelType)
	{
		this._bendingZones = new List<BendZoneEdge>();
		foreach (ICombinedBendDescriptor item4 in commonBendSequence)
		{
			(FaceGroup fg, Model model) tuple = item4.Enumerable.First().BendParams.ModelFaceGroup(modelType);
			FaceGroup item = tuple.fg;
			Matrix4d worldMatrix = tuple.model.WorldMatrix;
			Line line = new Line(worldMatrix.Transform(item.BendMiddlePoint), worldMatrix.TransformNormal(item.ConcaveAxis.Direction));
			List<Triple<double, double, FaceGroup>> list = new List<Triple<double, double, FaceGroup>>();
			foreach (IBendDescriptor item5 in item4.Enumerable)
			{
				(FaceGroup fg, Model model) tuple2 = item5.BendParams.ModelFaceGroup(modelType);
				FaceGroup item2 = tuple2.fg;
				Model item3 = tuple2.model;
				Vector3d unitZLocal = item3.WorldMatrix.Inverted.TransformNormal(Vector3d.UnitZ);
				_ = from t in item2.GetAllFaces().SelectMany((Face f) => f.Mesh)
					where t.CalculatedTriangleNormal.UnsignedAngle(unitZLocal) < 1.5692963267948965
					select t;
				Plane topPlane = new Plane(new Vector3d(0.0, 0.0, 0.0), new Vector3d(0.0, 0.0, 1.0));
				OutlineDetector outlineDetector = new OutlineDetector();
				outlineDetector.GetOutline(item3, topPlane, centerModel: false, item2.GetAllFaces().ToHashSet(), removeProjectionHoles: false, removeMacroHoles: false, project3d: false, null, out var outlines, out var holes, out var _, out var _, null);
				Matrix4d worldMatrix2 = item3.WorldMatrix;
				Line2D line2d = new Line2D(outlineDetector.Rot2Ground.Transform(worldMatrix2.Transform(item2.BendMiddlePoint + item2.ConvexAxis.Direction * item2.ConvexAxis.MinParameter)).XY, outlineDetector.Rot2Ground.Transform(worldMatrix2.Transform(item2.BendMiddlePoint + item2.ConvexAxis.Direction * item2.ConvexAxis.MaxParameter)).XY);
				Line2D line2D = new Line2D(outlineDetector.Rot2Ground.Transform(line.Origin).XY, outlineDetector.Rot2Ground.Transform(line.Origin + line.Direction).XY);
				List<(double start, double end, SpacialRelation relation)> list2 = (from x in outlines.Select((List<Vector2d> vertices) => new Polygon2D
					{
						Vertices = vertices
					}).SelectMany((Polygon2D poly) => poly.Intersect(line2d))
					orderby x.start
					select x).ToList();
				List<(double, double, SpacialRelation)> list3 = (from x in holes.Select((List<Vector2d> vertices) => new Polygon2D
					{
						Vertices = vertices
					}).SelectMany((Polygon2D poly) => poly.Intersect(line2d))
					orderby x.start
					select x).ToList();
				foreach (var item6 in list2)
				{
					if (item6.relation != SpacialRelation.Inside)
					{
						continue;
					}
					var (num, _, _) = item6;
					foreach (var item7 in list3)
					{
						if (num < item7.Item1 && item6.end > item7.Item2)
						{
							list.Add(new Triple<double, double, FaceGroup>(line2D.ParameterOfClosestPointOnAxis(line2d.EvalParam(num)), line2D.ParameterOfClosestPointOnAxis(line2d.EvalParam(item7.Item1)), item2));
							num = item7.Item2;
						}
					}
					list.Add(new Triple<double, double, FaceGroup>(line2D.ParameterOfClosestPointOnAxis(line2d.EvalParam(num)), line2D.ParameterOfClosestPointOnAxis(line2d.EvalParam(item6.end)), item2));
				}
			}
			foreach (Triple<double, double, FaceGroup> item8 in from x in list
				orderby x.Item1
				where Math.Abs(x.Item1 - x.Item2) > 0.01
				select x)
			{
				Vector3d pointOnAxisByParameter = line.GetPointOnAxisByParameter(item8.Item1);
				Vector3d pointOnAxisByParameter2 = line.GetPointOnAxisByParameter(item8.Item2);
				BendZoneEdge bendZoneEdge = new BendZoneEdge
				{
					Radius = item4[0].BendParams.FinalRadius,
					BendingAllowance = item4[0].BendParams.BendingAllowance,
					KFactor = item4[0].BendParams.KFactor,
					KFactorAlgorithm = item4[0].BendParams.KFactorAlgorithm,
					ToolSelectionAlgorithm = item4.ToolSelectionAlgorithm,
					AngleRad = item4[0].BendParams.Angle,
					BendingLine = new Line2D(new Vector2d(pointOnAxisByParameter.X, pointOnAxisByParameter.Y), new Vector2d(pointOnAxisByParameter2.X, pointOnAxisByParameter2.Y)),
					BendingZone = (item8.Item3.ParentGroup ?? item8.Item3),
					CombinedBendDescriptor = item4
				};
				this._bendingZones.Add(bendZoneEdge);
				int cl = ((item4[0].BendParams.Angle > 0.0) ? macro3DConfig.BendPosColorPn : macro3DConfig.BendNegColorPn);
				this.Db2D.SimpleAddEdge(bendZoneEdge.BendingLine, cl, isHole: true);
			}
		}
	}

	private void OriginPoint()
	{
		this.Db2D.CalculateMinMax();
		double num = 0.0 - this.Db2D.Boundary.Minx;
		double num2 = 0.0 - this.Db2D.Boundary.Miny;
		this.Db2D.Transfer(num, num2);
		if (this._bendingZones == null)
		{
			return;
		}
		foreach (BendZoneEdge bendingZone in this._bendingZones)
		{
			Vector2d p = new Vector2d(bendingZone.BendingLine.P0.X + num, bendingZone.BendingLine.P0.Y + num2);
			Vector2d p2 = new Vector2d(bendingZone.BendingLine.P1.X + num, bendingZone.BendingLine.P1.Y + num2);
			bendingZone.BendingLine = new Line2D(p, p2);
		}
	}

	private void BendingTextPreparation()
	{
		double num = this.Db2D.Boundary.LenY();
		AABB<Vector3d> boundingBox = this._doc.EntryModel3D.Shells.First().AABBTree.Root.BoundingBox;
		double value = boundingBox.Max.X - boundingBox.Min.X;
		double value2 = boundingBox.Max.Y - boundingBox.Min.Y;
		double value3 = boundingBox.Max.Z - boundingBox.Min.Z;
		double x2 = 0.0;
		double num2 = 0.0;
		int digits = 3;
		List<string> list = new List<string> { "" };
		list.Add("MATERIAL  = \"" + this._doc.Material.Name + "\"");
		list.Add("3DMATERIAL= \"" + this._materials.GetMaterial3DGroupName(this._doc.Material.MaterialGroupForBendDeduction) + "\"");
		list.Add($"THICKNESS = \"{Math.Round(this._doc.Thickness, digits)}\"");
		list.Add($"3D_SIZE_X = \"{Math.Round(value, digits)}\"");
		list.Add($"3D_SIZE_Y = \"{Math.Round(value2, digits)}\"");
		list.Add($"3D_SIZE_Z = \"{Math.Round(value3, digits)}\"");
		list.Add($"TYPE      = \"{this._doc.EntryModel3D.PartInfo.PartType}\"");
		if (!string.IsNullOrEmpty(this._doc.EntryModel3D.OriginalMaterialName) && this._doc.EntryModel3D.OriginalMaterialName.Trim() != this._doc.Material.Name.Trim())
		{
			list.Add("MATERIAL_IMPORT = \"" + this._doc.EntryModel3D.OriginalMaterialName + "\"");
		}
		string text = this._doc.SavedArchiveNumber.ToString();
		if (string.IsNullOrEmpty(this._doc.SavedFileName))
		{
			text = "";
		}
		list.Add("3D_FILE   = \"" + this._doc.SavedFileName + "\"");
		list.Add("3D_ARCHIV = \"" + text + "\"");
		if (this._doc.AmountInAssembly > 0)
		{
			list.Add($"QUANTITY  = \"{this._doc.AmountInAssembly}\"");
		}
		List<ValidationResult> validationResults = this._doc.ValidationResults;
		if (validationResults == null)
		{
			list.Add("NO VALIDATION CHECK DONE");
		}
		else if (validationResults.Count == 0)
		{
			list.Add("VALIDATION CHECK SUCCESSFUL");
		}
		else
		{
			list.Add($"VALIDATION ERRORS = \"{validationResults.Count((ValidationResult x) => x.IntrinsicErrors != null || x.DistanceErrors != null)}\"");
		}
		double num3 = num * 0.2 / (double)list.Count;
		foreach (string item in list)
		{
			this.Db2D.AddText(new CadTxtText
			{
				Text = item,
				Position = new Vector2d(x2, num2),
				Angle = 0.0,
				Height = num3 * 0.6,
				Color = 1
			});
			num2 -= num3;
		}
	}

	private void AddBendLineTexts()
	{
		int decimals = 4;
		foreach (BendZoneEdge bzi in this._bendingZones)
		{
			List<ICombinedBendDescriptorInternal> source = (from commonBend in this._doc.CombinedBendDescriptors.Reverse()
				where commonBend.Enumerable.Any((IBendDescriptor x) => x.BendParams.UnfoldFaceGroup.ID == bzi.BendingZone.ID)
				select commonBend).ToList();
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = source.FirstOrDefault();
			if (combinedBendDescriptorInternal != null)
			{
				double angle = Math.Round(bzi.AngleRad * 180.0 / Math.PI, decimals);
				double num = Math.Round(bzi.Radius, decimals);
				double num2 = Math.Round(BendDataCalculator.BendDeductionFromBendAllowance(this._doc.Thickness, angle, num, bzi.BendingAllowance), decimals);
				string text = "";
				string text2 = "";
				(IToolProfile? upperTool, IToolProfile? lowerTool, ToolSelectionType tst) bestToolProfiles = this._doc.PreferredProfileStore.GetBestToolProfiles(combinedBendDescriptorInternal);
				IToolProfile item = bestToolProfiles.upperTool;
				IToolProfile item2 = bestToolProfiles.lowerTool;
				text = this.GetPunchName(combinedBendDescriptorInternal);
				text2 = this.GetDieName(combinedBendDescriptorInternal);
				if (string.IsNullOrEmpty(text) && item?.Group is IPunchGroup punchGroup)
				{
					text = punchGroup.PrimaryToolName;
				}
				if (string.IsNullOrEmpty(text2) && item2?.Group is IDieGroup dieGroup)
				{
					text2 = dieGroup.PrimaryToolName;
				}
				Vector2d v = bzi.BendingLine.P0 - bzi.BendingLine.P1;
				double num3 = (bzi.BendingLine.P0 - new Vector2d(bzi.BendingLine.P0.X + 1.0, bzi.BendingLine.P0.Y)).SignedAngle(v) * 180.0 / Math.PI;
				if (num3 >= -90.0 && num3 < 90.0)
				{
					num3 += 180.0;
				}
				if (num3 < 0.0)
				{
					num3 += 360.0;
				}
				Vector2d position = new Vector2d(bzi.BendingLine.P0.X + (bzi.BendingLine.P1.X - bzi.BendingLine.P0.X) / 2.0, bzi.BendingLine.P0.Y + (bzi.BendingLine.P1.Y - bzi.BendingLine.P0.Y) / 2.0);
				List<double> values = source.Select((ICombinedBendDescriptorInternal cbf) => Math.Round(cbf.StopProductAngleSigned * 180.0 / Math.PI, decimals)).ToList();
				List<string> values2 = source.Select((ICombinedBendDescriptorInternal cbf) => this.GetCommonBendCount(cbf, bzi)).ToList();
				CadTxtText cadTxtText = new CadTxtText();
                 text = string.Format(CultureInfo.InvariantCulture,
			    "_BD_{0};{1};{2};{3};{4};{5};{6};{7};",
		      string.Join('_', values),     // Combined angles
		     num,                          // Radius
		     num2,                         // Bend deduction
		      "0",                         // Reserved field
		     "0",                         // Reserved field
		        text,                        // Punch name
		     text2,                       // Die name
		      string.Join('_', values2));  // Combined bend counts

                cadTxtText = new CadTxtText
                {
                    Text = text,
                    Position = position,
                    Angle = num3,
                    Height = 1.0,
                    Color = bzi.AngleRad > 0.0 ? 1 : 3
                };
                CadTxtText cadTxtText2 = cadTxtText;
				if (!double.IsNaN(cadTxtText2.Position.X) && !double.IsNaN(cadTxtText2.Position.Y))
				{
					this.Db2D.AddText(cadTxtText2);
				}
			}
		}
	}

	private string GetPunchName(ICombinedBendDescriptorInternal? cbd)
	{
		if (cbd == null)
		{
			return "";
		}
		return this._doc.ToolsAndBends?.GetBend(cbd)?.PunchProfile?.Name ?? "";
	}

	private string GetDieName(ICombinedBendDescriptorInternal? cbd)
	{
		if (cbd == null)
		{
			return "";
		}
		return this._doc.ToolsAndBends?.GetBend(cbd)?.DieProfile?.Name ?? "";
	}

	private string GetCommonBendCount(ICombinedBendDescriptor commonBendFace, BendZoneEdge bendZoneEdge)
	{
		List<BendZoneEdge> list = this._bendingZones.Where((BendZoneEdge zone) => zone.CombinedBendDescriptor == commonBendFace).ToList();
		if (list.Count == 1)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", commonBendFace.Order + 1);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", commonBendFace.Order + 1, list.IndexOf(bendZoneEdge) + 1);
	}

	private void Orientate2DModel(Macro3DConfig macro3DConfig, Model unfoldModel3D, General3DConfig general3DConfig)
	{
		CadGeoLine cadGeoLine = null;
		double num = 0.0;
		CadGeoLine cadGeoLine2 = null;
		List<Vector2d> list = new List<Vector2d>();
		foreach (CadGeoElement item in this.Db2D.OuterElements())
		{
			if (item.Type == 1)
			{
				CadGeoLine cadGeoLine3 = (CadGeoLine)item;
				list.Add(cadGeoLine3.StartPoint);
				list.Add(cadGeoLine3.EndPoint);
				double lengthSquared = (cadGeoLine3.EndPoint - cadGeoLine3.StartPoint).LengthSquared;
				if (lengthSquared > num)
				{
					num = lengthSquared;
					cadGeoLine = cadGeoLine3;
				}
				if (cadGeoLine2 == null && cadGeoLine3.Color == macro3DConfig.TubeCutColorPn)
				{
					cadGeoLine2 = cadGeoLine3;
				}
			}
			else if (item.Type == 2)
			{
				CadGeoCircle cadGeoCircle = (CadGeoCircle)item;
				Vector3d n = new Vector3d(0.0, 0.0, -1.0);
				Vector2d center = cadGeoCircle.Center;
				double num2 = cadGeoCircle.StartAngle * (Math.PI / 180.0);
				double num3 = cadGeoCircle.EndAngle * (Math.PI / 180.0);
				double radius = cadGeoCircle.Radius;
				double y = center.Y + radius * Math.Sin(num2);
				double x2 = center.X + radius * Math.Cos(num2);
				double y2 = center.Y + radius * Math.Sin(num3);
				double x3 = center.X + radius * Math.Cos(num3);
				Vector2d vector2d = new Vector2d(x2, y);
				Vector2d vector2d2 = new Vector2d(x3, y2);
				Circle3D.SampleCircleSegmentByMaxError(new Vector3d(vector2d.X, vector2d.Y, 0.0), new Vector3d(vector2d2.X, vector2d2.Y, 0.0), new Vector3d(center.X, center.Y, 0.0), n, cadGeoCircle.Direction == -1, 0.01, out var result);
				list.AddRange(result.Select((Vector3d p) => p.XY));
			}
		}
		Deepening deepening = null;
		foreach (Model item2 in unfoldModel3D.GetAllSubModelsWithSelf())
		{
			using (IEnumerator<Macro> enumerator3 = (from macro in item2.Shells.SelectMany((Shell x) => x.Macros)
				where macro is Deepening deepening2 && deepening2.IsSpecialDirectionGrinding
				select macro).GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					deepening = enumerator3.Current as Deepening;
					Vector2d vector2d3 = new Vector2d(list.Min((Vector2d v) => v.X), list.Min((Vector2d v) => v.Y));
					Vector2d vector2d4 = new Vector2d(list.Max((Vector2d v) => v.X), list.Max((Vector2d v) => v.Y));
					Vector2d rotationPoint = (vector2d3 + vector2d4) / 2.0;
					Vector3d vector3d = deepening.Faces.First().Shell.GetWorldMatrix(item2).TransformNormal(deepening.DirectionGrinding);
					double num4 = Math.Atan2(vector3d.Y, vector3d.X);
					this.Db2D.Rotate(rotationPoint, 0.0 - num4);
				}
			}
			if (deepening != null)
			{
				break;
			}
		}
		if (deepening != null)
		{
			return;
		}
		IList<Vector2d> points = ConvexHull2D.MakeHull(list);
		Vector2d center2 = default(Vector2d);
		Vector2d[] u = new Vector2d[2];
		if (general3DConfig.P3D_Apply2DOrientateMinArea)
		{
			ModelForPnStdPreparation.MinAreaRect(points, ref center2, ref u);
		}
		else
		{
			if (cadGeoLine != null)
			{
				Vector2d vector2d5 = cadGeoLine.EndPoint - cadGeoLine.StartPoint;
				if (vector2d5.X > 0.0)
				{
					vector2d5 *= -1.0;
				}
				vector2d5 = (u[1] = vector2d5.Normalized);
				u[0] = new Vector2d(0.0 - vector2d5.Y, vector2d5.X);
			}
			else
			{
				u[0] = new Vector2d(1.0, 0.0);
				u[1] = new Vector2d(0.0, 1.0);
			}
			center2 = new Vector2d(0.0, 0.0);
		}
		Vector2d vector2d6 = u[0];
		double angle = ((!unfoldModel3D.PartInfo.PartType.HasFlag(PartType.Tube) || cadGeoLine2 == null) ? vector2d6.SignedAngle(new Vector2d(0.0, 1.0)) : cadGeoLine2.Direction.SignedAngle(new Vector2d(1.0, 0.0)));
		this.Db2D.Rotate(center2, angle);
	}

	private static double MinAreaRect(IList<Vector2d> points, ref Vector2d center, ref Vector2d[] u)
	{
		double num = double.MaxValue;
		int count = points.Count;
		int i = 0;
		for (int index = count - 1; i < count; index = i, i++)
		{
			Vector2d normalized = (points[i] - points[index]).Normalized;
			Vector2d vector2d = new Vector2d(0.0 - normalized.Y, normalized.X);
			double num2 = double.MaxValue;
			double num3 = double.MaxValue;
			double num4 = double.MinValue;
			double num5 = double.MinValue;
			for (int j = 0; j < count; j++)
			{
				Vector2d vector2d2 = points[j];
				double num6 = vector2d2.Dot(normalized);
				if (num6 < num2)
				{
					num2 = num6;
				}
				if (num6 > num4)
				{
					num4 = num6;
				}
				num6 = vector2d2.Dot(vector2d);
				if (num6 < num3)
				{
					num3 = num6;
				}
				if (num6 > num5)
				{
					num5 = num6;
				}
			}
			double num7 = (num4 - num2) * (num5 - num3);
			Vector2d vector2d3;
			Vector2d vector2d4;
			if (num4 - num2 <= num5 - num3)
			{
				vector2d3 = normalized;
				vector2d4 = vector2d;
			}
			else
			{
				vector2d3 = vector2d;
				vector2d4 = normalized;
			}
			if (Math.Abs(num7 - num) < 1E-06)
			{
				double num8 = u[0].X - vector2d3.X;
				double num9 = u[0].Y - vector2d3.Y;
				if (Math.Abs(num8) > Math.Abs(num9))
				{
					if (num8 < 0.0)
					{
						continue;
					}
				}
				else if (num9 < 0.0)
				{
					continue;
				}
			}
			else if (num7 > num)
			{
				continue;
			}
			num = num7;
			center = points[index] + 0.5 * ((num2 + num4) * normalized + (num3 + num5) * vector2d);
			u[0] = vector2d3;
			u[1] = vector2d4;
		}
		return num;
	}

	private static List<CadGeoElement> GetContour(List<Vector2d> points, int color, bool isOpenContour = false, Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap = null)
	{
		ContourConverter contourConverter = new ContourConverter();
		return contourConverter.ToCadGeoContour(contourConverter.CreateContour(points, isOpenContour, simplify: true, removeShortSegments: true, contourCircleMap), color);
	}
}

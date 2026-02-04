using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Writer;
using WiCAM.Pn4000.BendModel.Writer.Writer;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Unfold;

public class Unfold
{
	public static void MoveUnfoldModelToZero(Model unfoldModel, int faceGroupId, int faceGroupSide)
	{
		if (faceGroupId >= 0)
		{
			(Face face, Model model) firstFaceOfGroupModel = unfoldModel.GetFirstFaceOfGroupModel(faceGroupId, faceGroupSide);
			Unfold.MoveUnfoldModelToZero(unfoldModel, firstFaceOfGroupModel.face, firstFaceOfGroupModel.model);
		}
	}

	public static void MoveUnfoldModelToZero(Model model, Face face, Model faceModel)
	{
		if (face == null)
		{
			return;
		}
		List<FaceHalfEdge> source = (from x in face.BoundaryEdgesCcw
			orderby x.CalculatedLength descending, x.StartVertex.Pos
			select x).ToList();
		FaceHalfEdge faceHalfEdge = source.FirstOrDefault((FaceHalfEdge x) => x.CounterEdge == null && x.EdgeType == EdgeType.Line) ?? source.FirstOrDefault((FaceHalfEdge x) => x.CounterEdge == null && x.EdgeType == EdgeType.Wire) ?? source.FirstOrDefault((FaceHalfEdge x) => x.CounterEdge == null && x.EdgeType == EdgeType.PolyLine) ?? source.FirstOrDefault((FaceHalfEdge x) => x.EdgeType == EdgeType.Line) ?? source.FirstOrDefault((FaceHalfEdge x) => x.EdgeType == EdgeType.Wire) ?? source.FirstOrDefault((FaceHalfEdge x) => x.EdgeType == EdgeType.PolyLine) ?? source.FirstOrDefault((FaceHalfEdge x) => x.CounterEdge == null && x.EdgeType == EdgeType.Circle) ?? source.FirstOrDefault((FaceHalfEdge x) => x.EdgeType == EdgeType.Circle);
		if (faceHalfEdge == null)
		{
			return;
		}
		Vector3d v = (faceHalfEdge.EndVertex.Pos - faceHalfEdge.StartVertex.Pos).Normalized;
		if (!(Math.Abs(v.LengthSquared - 1.0) < 0.001))
		{
			Vector3d ep = faceHalfEdge.EndVertex.Pos;
			v = (ep - faceHalfEdge.Vertices.OrderByDescending((Vertex x) => (x.Pos - ep).LengthSquared).First().Pos).Normalized;
			if (!(Math.Abs(v.LengthSquared - 1.0) < 0.001))
			{
				return;
			}
		}
		Matrix4d worldMatrix = face.Shell.GetWorldMatrix(faceModel);
		Vector3d v2 = face.Mesh.OrderByDescending((Triangle x) => x.Area).First().TriangleNormal;
		worldMatrix.TransformNormalInPlace(ref v2);
		if (!v2.IsParallel(new Vector3d(0.0, 0.0, 1.0), out var direction) || direction < 0)
		{
			worldMatrix.TransformNormalInPlace(ref v);
			Matrix4d matrix4d = Matrix4d.TransformationToUnitCoordinateSystem(v.Cross(v2), v, v2, faceHalfEdge.EndVertex.Pos);
			model.Transform *= matrix4d;
			worldMatrix = face.Shell.GetWorldMatrix(faceModel);
		}
		Vector3d v3 = face.Mesh.OrderByDescending((Triangle x) => x.Area).First().Center;
		worldMatrix.TransformInPlace(ref v3);
		v3.Z -= face.Shell.Thickness / 2.0;
		model.Transform *= Matrix4d.Translation(new Vector3d(0.0, 0.0, 0.0 - v3.Z));
		Pair<Vector3d, Vector3d> boundary = model.GetBoundary(Matrix4d.Identity);
		Vector3d item = boundary.Item1;
		Vector3d item2 = boundary.Item2;
		Vector3d v4 = default(Vector3d) - new Vector3d((item2.X + item.X) / 2.0, (item2.Y + item.Y) / 2.0, 0.0);
		model.Transform *= Matrix4d.Translation(v4);
	}

	public static F2exeReturnCode CheckModelResultForBendZonesAndExtraElements(IDoc3d doc)
	{
		if (doc.UnfoldModel3D == null)
		{
			return F2exeReturnCode.OK_FLAT;
		}
		bool flag = doc.UnfoldModel3D.GetAllSubModelsWithSelf().SelectMany((Model m) => m.Shells).Any((Shell s) => s.RoundFaceGroups.Count > 0);
		bool flag2 = doc.UnfoldModel3D.GetAllSubModelsWithSelf().Any((Model m) => m.PartInfo.NotConformFaces.Count > 0);
		if (!flag && !flag2)
		{
			return F2exeReturnCode.OK_FLAT;
		}
		if (!flag)
		{
			return F2exeReturnCode.OK_NO_BENDINGZONE_EXTRAELEMENTS;
		}
		if (!flag2)
		{
			return F2exeReturnCode.OK;
		}
		return F2exeReturnCode.OK_BENDINGZONE_EXTRAELEMENTS;
	}

	public static void WriteModifiedModelToObj(IDoc3d doc, IGlobals globals)
	{
		if (!globals.ConfigProvider.InjectOrCreate<General3DConfig>().P3D_ObjExportModeModifiedModel || doc.CombinedBendDescriptors.Count == 0)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			foreach (int item in from x in combinedBendDescriptor.Enumerable
				select x.BendParams.ModifiedEntryFaceGroup into x
				select x.ParentGroup?.ID ?? x.ID)
			{
				if (!dictionary.ContainsKey(item))
				{
					dictionary.TryAdd(item, combinedBendDescriptor.Order);
				}
			}
		}
		new ObjWriter().WriteObj(modelColors: globals.ConfigProvider.InjectOrCreate<ModelColors3DConfig>(), model: doc.ModifiedEntryModel3D, visibleFaceGroupId: doc.VisibleFaceGroupId, visibleFaceGroupSide: doc.VisibleFaceGroupSide, commonBendFaceGroupIds: dictionary, filename: Path.Combine(globals.PnPathService.FolderCad3d2Pn, $"{doc.EntryModel3D.PartId}_modified.obj"));
	}

	public static void WriteModifiedModelToGlb(IDoc3d doc, IGlobals globals)
	{
		if (globals.ConfigProvider.InjectOrCreate<General3DConfig>().P3D_GlbExportModeModifiedModel && doc.CombinedBendDescriptors.Count != 0)
		{
			new GltfWriter(doc.ModifiedEntryModel3D).WriteGltf(Path.Combine(globals.PnPathService.FolderCad3d2Pn, $"{doc.EntryModel3D.PartId}_modified.glb"));
		}
	}

	public static double GetRadiusTolerance(double thickness, double radius, General3DConfig pnGeneral3DConfig)
	{
		if (pnGeneral3DConfig == null)
		{
			return 0.0;
		}
		double num = pnGeneral3DConfig.P3D_RadiusToleranceRadius * radius;
		double num2 = pnGeneral3DConfig.P3D_RadiusToleranceThickness * thickness;
		return pnGeneral3DConfig.P3D_RadiusToleranceConstante + num + num2;
	}
}

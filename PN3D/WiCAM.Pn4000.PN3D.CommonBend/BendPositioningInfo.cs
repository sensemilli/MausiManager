using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.CommonBend;

public class BendPositioningInfo : IBendPositioningInfoInternal, IBendPositioningInfo
{
	public int PrimaryFaceGroupId { get; set; }

	public RotationAxis BendConvexAxis { get; set; }

	public List<(double start, double end, int fgId, double fgOffset)> StartEndOffsetsBends { get; set; }

	public List<Pair<double, double>> StartEndOffsetsObstacles { get; set; }

	public bool IsParentLeftOfBendPlane { get; set; }

	public bool IsParentOnSideOfCentroid { get; set; }

	public MachinePartInsertionDirection MachineInsertionDirection { get; set; }

	public bool IsReversedGeometry { get; private set; }

	public bool CalcIsReversedGeometry(MachinePartInsertionDirection dir)
	{
		if ((dir != MachinePartInsertionDirection.PartCentroidBehindMachine || !this.IsParentOnSideOfCentroid || !this.IsParentLeftOfBendPlane) && (dir != MachinePartInsertionDirection.PartCentroidBehindMachine || this.IsParentOnSideOfCentroid || this.IsParentLeftOfBendPlane) && (dir != MachinePartInsertionDirection.PartCentroidInFrontOfMachine || !this.IsParentOnSideOfCentroid || this.IsParentLeftOfBendPlane))
		{
			if (dir == MachinePartInsertionDirection.PartCentroidInFrontOfMachine && !this.IsParentOnSideOfCentroid)
			{
				return this.IsParentLeftOfBendPlane;
			}
			return false;
		}
		return true;
	}

	public void UpdateBendOffsets(List<FaceGroup> bends, Model unfoldModel)
	{
		FaceGroup faceGroup = bends.FirstOrDefault();
		FaceGroupModelMapping fgMapping = new FaceGroupModelMapping(unfoldModel);
		faceGroup = faceGroup.ParentGroup ?? faceGroup;
		this.PrimaryFaceGroupId = faceGroup?.ID ?? (-1);
		this.BendConvexAxis = faceGroup?.ConvexAxis;
		if (faceGroup != null)
		{
			Matrix4d firstWorldMatrixInv = faceGroup.GetModel(fgMapping).WorldMatrix.Inverted;
			double paramFirstBendOffset = this.BendConvexAxis.ParameterOfClosestPointOnAxis(faceGroup.BendMiddlePoint);
			this.StartEndOffsetsBends = (from x in bends.SelectMany((FaceGroup bend) => (!bend.SubGroups.Any()) ? new HashSet<FaceGroup> { bend } : bend.SubGroups).Select(delegate(FaceGroup bend)
				{
					Matrix4d matrix4d = bend.GetModel(fgMapping).WorldMatrix * firstWorldMatrixInv;
					Vector3d v = bend.ConvexAxis.GetPointOnAxisByParameter(bend.ConvexAxis.MinParameter);
					Vector3d v2 = bend.ConvexAxis.GetPointOnAxisByParameter(bend.ConvexAxis.MaxParameter);
					matrix4d.TransformInPlace(ref v);
					matrix4d.TransformInPlace(ref v2);
					double num = this.BendConvexAxis.ParameterOfClosestPointOnAxis(v);
					double num2 = this.BendConvexAxis.ParameterOfClosestPointOnAxis(v2);
					double num3 = this.BendConvexAxis.ParameterOfClosestPointOnAxis(matrix4d.Transform((bend.ParentGroup ?? bend).BendMiddlePoint));
					if (num > num2)
					{
						double num4 = num2;
						num2 = num;
						num = num4;
					}
					return (paramMin: num, paramMax: num2, ID: bend.ID, num3 - paramFirstBendOffset);
				})
				orderby x.paramMin
				select x).ToList();
			Vector3d centroid = unfoldModel.GetCentroid(faceGroup.GetModel(fgMapping).WorldMatrix.Inverted);
			Vector3d v3 = faceGroup.BendMiddlePointNormal * -1.0;
			Plane plane = new Plane(faceGroup.BendMiddlePoint, faceGroup.ConvexAxis.Direction.Cross(v3));
			int num5 = Math.Sign(plane.SignedDistanceToPoint(centroid));
			int num6 = 0;
			Pair<TriangleHalfEdge, TriangleHalfEdge> pair = faceGroup.GetTransitionEdgesToParentModel(fgMapping)?.FirstOrDefault();
			if (pair != null)
			{
				num6 = Math.Sign(plane.SignedDistanceToPoint(pair.Item1.V0.Pos));
			}
			this.IsParentOnSideOfCentroid = num5 == num6;
			this.IsParentLeftOfBendPlane = num6 < 0;
		}
		else
		{
			this.StartEndOffsetsBends = new List<(double, double, int, double)>();
		}
	}

	private void MergeConnectedObstacles(List<Pair<double, double>> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			Pair<double, double> pair = obstacles[i];
			for (int j = i + 1; j < obstacles.Count; j++)
			{
				Pair<double, double> pair2 = obstacles[j];
				if ((pair.Item1 - 1.0 <= pair2.Item1 && pair2.Item1 <= pair.Item2 + 1.0) || (pair.Item1 - 1.0 <= pair2.Item2 && pair2.Item2 <= pair.Item2 + 1.0) || (pair2.Item1 - 1.0 <= pair.Item1 && pair.Item1 <= pair2.Item2 + 1.0) || (pair2.Item1 - 1.0 <= pair.Item2 && pair.Item2 <= pair2.Item2 + 1.0))
				{
					pair.Item2 = Math.Max(pair.Item2, pair2.Item2);
					pair.Item1 = Math.Min(pair.Item1, pair2.Item1);
					obstacles.Remove(pair2);
					j--;
				}
			}
		}
	}
}

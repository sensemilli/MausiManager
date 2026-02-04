using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendPositioningInfo
{
	public int PrimaryFaceGroupId { get; set; }

	public RotationAxis BendConcaveAxis { get; set; }

	public List<Triple<double, double, int>> StartEndOffsetsBends { get; set; }

	public List<SStartEndOffsetBend> StartEndOffsetsBendsNew { get; set; }

	public List<Pair<double, double>> StartEndOffsetsObstacles { get; set; }

	public bool IsParentLeftOfBendPlane { get; set; }

	public bool IsParentOnSideOfCentroid { get; set; }

	public MachinePartInsertionDirection MachineInsertionDirection { get; set; }

	public bool IsReversedGeometry { get; set; }
}

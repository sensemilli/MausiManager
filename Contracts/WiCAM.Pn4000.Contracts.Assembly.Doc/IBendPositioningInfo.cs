using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IBendPositioningInfo
{
	int PrimaryFaceGroupId { get; set; }

	RotationAxis BendConvexAxis { get; set; }

	List<(double start, double end, int fgId, double fgOffset)> StartEndOffsetsBends { get; set; }

	List<Pair<double, double>> StartEndOffsetsObstacles { get; set; }

	bool IsParentLeftOfBendPlane { get; set; }

	bool IsParentOnSideOfCentroid { get; set; }

	MachinePartInsertionDirection MachineInsertionDirection { get; set; }

	bool IsReversedGeometry { get; }
}

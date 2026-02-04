using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBend
{
	public int Order { get; set; }

	public double StartAngle { get; set; }

	public double DestAngle { get; set; }

	public List<(int bendEntryId, int? subBendCount, int? subBendIndex)> FaceGroupIdentifiers { get; set; }

	public List<int> FaceGroupIds { get; set; }

	public List<IRange> BendingZones { get; set; }

	public List<SStartEndOffsetsBends> StartEndOffsetsBends { get; set; }

	public List<SBendState> BendState { get; set; }

	public List<IRange>? CollisionIntervals { get; set; }

	public CombinedBendType BendType { get; set; }
}

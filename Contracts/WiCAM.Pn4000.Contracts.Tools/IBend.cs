using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBend
{
	int Order { get; set; }

	double StartAngle { get; set; }

	double DestAngle { get; set; }

	IEnumerable<(int bendEntryId, int? subBendCount, int? subBendIndex)> FaceGroupIdentifiers { get; set; }

	IEnumerable<int> FaceGroupIds { get; }

	IEnumerable<IRange> BendingZones { get; }

	IEnumerable<IRange>? CollisionIntervals { get; }

	ICombinedBendDescriptor? CombinedBendDescriptor { get; }

	IReadOnlyCollection<IBendState> BendStates { get; }

	CombinedBendType BendType { get; set; }

	List<(double start, double end, int fgId, double fgOffset)> StartEndOffsetsBends { get; }

	void SetCombinedBendDescriptor(ICombinedBendDescriptor? cbd);

	bool TrySetCombinedBendDescriptor(ICombinedBendDescriptor? cbd);

	bool EqualsState(IBend bend);

	IBend Clone();
}

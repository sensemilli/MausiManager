using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendPositioning
{
	IToolCluster? Anchor { get; set; }

	Vector3d Offset { get; set; }

	Vector3d OffsetWorld { get; set; }

	double OffsetWorldX { get; set; }

	IBend Bend { get; }

	int Order => this.Bend.Order;

	IEnumerable<IRange> BendingZonesOrientated { get; }

	bool IsReversedGeometry { get; }

	MachinePartInsertionDirection MachineInsertDirection { get; set; }

	IPunchProfile? PunchProfile { get; set; }

	IDieProfile? DieProfile { get; set; }

	IPunchProfile? PunchProfileByUser { get; set; }

	IDieProfile? DieProfileByUser { get; set; }

	double UpperWorkingHeightAdapters { get; set; }

	double UpperWorkingHeightTotal => this.UpperWorkingHeightAdapters + (this.PunchProfile?.WorkingHeight ?? 0.0);

	Vector3d PositionWorld
	{
		get
		{
			Vector3d offset = this.Offset;
			Vector3d? obj = this.Anchor?.OffsetWorld;
			return (offset + obj) ?? Vector3d.Zero;
		}
	}

	Dictionary<IAcbPunchPiece, AcbActivationResult> AcbStatus { get; set; }
}

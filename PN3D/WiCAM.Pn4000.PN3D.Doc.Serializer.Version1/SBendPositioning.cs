using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendPositioning
{
	public int? AnchorSerializeId { get; set; }

	public SVector3d Offset { get; set; }

	public SBend Bend { get; set; }

	public bool IsReversedGeometry { get; set; }

	public int MachineInsertDirection { get; set; }

	public int? PunchProfileId { get; set; }

	public int? DieProfileId { get; set; }

	public int? PunchProfileByUserId { get; set; }

	public int? DieProfileByUserId { get; set; }

	public double UpperWorkingHeightAdapters { get; set; }

	public List<(int idx, AcbActivationResult status)> AcbSatus { get; set; }
}

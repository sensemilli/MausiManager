using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.TubeInfos;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class DisassemblyPartInfo
{
	public PartType PartType { get; set; }

	public PartType OriginalPartType { get; set; }

	public TubeType TubeType { get; set; }

	public TubeInfo TubeInfo { get; set; }

	public GeometryType GeometryType { get; set; } = GeometryType.Volume;

	public int PurchasedPart { get; set; }

	public bool IgnoreCollision { get; set; }

	public List<SimulationInstance> SimulationInstances { get; set; }

	public List<UserProperty> UserProperties { get; set; } = new List<UserProperty>();
}

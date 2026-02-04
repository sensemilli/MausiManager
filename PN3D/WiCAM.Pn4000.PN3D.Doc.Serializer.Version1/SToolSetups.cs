using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolSetups : SToolCluster, ISToolSetups
{
	public string Desc { get; set; }

	public int MountTypeIdUpperBeam { get; set; }

	public int MountTypeIdLowerBeam { get; set; }

	public int MachineNumber { get; set; }

	public double LowerBeamXStart { get; set; }

	public double LowerBeamXEnd { get; set; }

	public double UpperBeamXStart { get; set; }

	public double UpperBeamXEnd { get; set; }
}

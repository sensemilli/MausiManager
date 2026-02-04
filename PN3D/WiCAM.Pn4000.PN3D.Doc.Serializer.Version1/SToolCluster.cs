using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolCluster
{
	public SVector3d OffsetLocal { get; set; }

	public List<SToolCluster> Children { get; set; }

	public List<SToolSection> Sections { get; set; }

	public int Number { get; set; }

	public int SerializeId { get; set; }
}

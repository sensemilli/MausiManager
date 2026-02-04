using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SAsmDisassemblyPartNode
{
	public int DisassemblyPartId { get; set; }

	public bool HiddenInAssembly { get; set; }

	public bool SuppressedInAssembly { get; set; }

	public List<SAsmDisassemblyPartNode> Children { get; set; }

	public double[] Transform { get; set; }
}

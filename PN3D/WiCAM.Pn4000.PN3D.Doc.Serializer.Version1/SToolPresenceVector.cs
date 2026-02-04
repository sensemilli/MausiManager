using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolPresenceVector
{
	public List<SToolPresence> Presences = new List<SToolPresence>();

	public int CommonBendOrder { get; set; }

	public double RefPointOffsetStart { get; set; }

	public double RefPointOffsetEnd { get; set; }

	public SToolInfo ToolInfo { get; set; }
}

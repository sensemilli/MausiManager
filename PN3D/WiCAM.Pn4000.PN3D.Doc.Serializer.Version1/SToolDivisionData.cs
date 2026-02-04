using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolDivisionData
{
	public SToolPresenceVector ToolDividedPresVector { get; set; } = new SToolPresenceVector();

	public List<SToolStationSection> Sections { get; set; } = new List<SToolStationSection>();
}

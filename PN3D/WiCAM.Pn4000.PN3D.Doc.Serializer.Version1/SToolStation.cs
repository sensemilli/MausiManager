using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolStation
{
	public List<SToolPresenceVectorEntry> BendPresVectors = new List<SToolPresenceVectorEntry>();

	public SToolPresenceVector MergedPresVector;

	public SToolDivisionData ToolDivisionUpperAdapter = new SToolDivisionData();

	public SToolDivisionData ToolDivisionLowerAdapter = new SToolDivisionData();

	public SToolDivisionData ToolDivisionUpper = new SToolDivisionData();

	public SToolDivisionData ToolDivisionLower = new SToolDivisionData();

	public bool ManuallyConfigured { get; set; }

	public double ExtensionLeft { get; set; }

	public double ExtensionRight { get; set; }

	public int Idx { get; set; }
}

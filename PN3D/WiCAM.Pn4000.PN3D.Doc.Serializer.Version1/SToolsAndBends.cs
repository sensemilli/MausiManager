using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolsAndBends : ISToolsAndBends
{
	public List<SToolSetups> ToolSetups { get; set; }

	public List<SBendPositioning> BendPositions { get; set; }
}

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendPart
{
	[XmlAttribute]
	public string Name { get; set; }

	[XmlAttribute]
	public string Description { get; set; }

	[XmlAttribute]
	public string DateCreated { get; set; }

	[XmlAttribute]
	public string LastUpdated { get; set; }

	[XmlAttribute]
	public string CreatedBy { get; set; }

	[XmlAttribute]
	public string UpdatedBy { get; set; }

	[XmlAttribute]
	public double SheetThickness { get; set; }

	[XmlAttribute]
	public double SheetThicknessC { get; set; }

	[XmlAttribute]
	public int BendMethod { get; set; }

	[XmlAttribute]
	public string Info { get; set; }

	[XmlAttribute]
	public string Version { get; set; }

	[XmlAttribute]
	public int ReducedGeometry { get; set; }

	public List<BOSRef> BosRefs { get; set; }

	public List<BendSection> Sections { get; set; }

	public List<BendSurface> Surfaces { get; set; }

	public List<BendLine> Lines { get; set; }

	public List<BendToolSetup> ToolSetups { get; set; }

	public List<BendProcess> Processes { get; set; }
}

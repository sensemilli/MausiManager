using System;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BOSRef
{
	[XmlAttribute]
	public string refname { get; set; }

	[XmlAttribute]
	public string type { get; set; }

	[XmlAttribute]
	public string objname { get; set; }
}

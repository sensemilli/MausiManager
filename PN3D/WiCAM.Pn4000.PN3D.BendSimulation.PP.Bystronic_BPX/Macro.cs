using System;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class Macro
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public int BendType { get; set; }
}

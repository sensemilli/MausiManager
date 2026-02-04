using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendSection
{
	[XmlAttribute]
	public int _ID { get; set; }

	public List<Macro> Macros { get; set; }
}

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendLine
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public double StartX { get; set; }

	[XmlAttribute]
	public double StartY { get; set; }

	[XmlAttribute]
	public double EndX { get; set; }

	[XmlAttribute]
	public double EndY { get; set; }

	[XmlAttribute]
	public double LengthReduction { get; set; }

	public List<BOSRef> BosRefs { get; set; }
}

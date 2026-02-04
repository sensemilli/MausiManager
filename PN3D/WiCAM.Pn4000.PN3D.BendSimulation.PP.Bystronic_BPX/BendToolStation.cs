using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendToolStation
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public double UpperLeftPos { get; set; }

	[XmlAttribute]
	public double UpperLength { get; set; }

	[XmlAttribute]
	public double LowerLeftPos { get; set; }

	[XmlAttribute]
	public double LowerLength { get; set; }

	[XmlAttribute]
	public int CommandDevice { get; set; }

	[XmlAttribute]
	public int TurnUT { get; set; }

	[XmlAttribute]
	public int TurnLT { get; set; }

	[XmlAttribute]
	public int AcceptableSegmentation { get; set; }

	public List<BOSRef> BosRefs { get; set; }

	public List<BendToolSegment> UpperSegments { get; set; }

	public List<BendToolSegment> LowerSegments { get; set; }
}

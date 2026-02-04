using System;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendToolSegment
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public int ToolSegmentType { get; set; }

	[XmlAttribute]
	public double Length { get; set; }

	[XmlAttribute]
	public int HornLength { get; set; }

	[XmlAttribute]
	public int HornHeight { get; set; }
}

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class BendSurface
{
	[XmlAttribute]
	public int _ID { get; set; }

	public List<Curve> Outlines { get; set; }

	public List<Hole> Holes { get; set; }

	public List<Extrusion> Extrusions { get; set; }
}

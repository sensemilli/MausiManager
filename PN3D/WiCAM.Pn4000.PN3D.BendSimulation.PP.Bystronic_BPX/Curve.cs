using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.BystronicConfig.Geometry;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
public class Curve
{
	[XmlAttribute]
	public int _ID { get; set; }

	[XmlAttribute]
	public int ZHeight { get; set; }

	public List<Vertex> Vertices { get; set; }
}

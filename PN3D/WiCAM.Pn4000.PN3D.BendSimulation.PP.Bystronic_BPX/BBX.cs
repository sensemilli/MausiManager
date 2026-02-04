using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "", IsNullable = false)]
public class BBX
{
	[XmlAttribute("version")]
	public double Version { get; set; }

	[XmlAttribute("encoding")]
	public string Encoding { get; set; }

	public BendPart BendPart { get; set; }
}

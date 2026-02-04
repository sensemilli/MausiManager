using System;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.Contracts.Tools;

[Serializable]
public enum VWidthTypes
{
	[XmlEnum("")]
	Undefined = 0,
	[XmlEnum("1")]
	ALvdDelem = 1,
	[XmlEnum("2")]
	BTrumpf = 2
}

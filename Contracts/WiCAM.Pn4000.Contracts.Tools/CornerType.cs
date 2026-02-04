using System;
using System.Xml.Serialization;

namespace WiCAM.Pn4000.Contracts.Tools;

[Serializable]
public enum CornerType
{
	[XmlEnum("")]
	Default = 0,
	[XmlEnum("1")]
	StoneFactor = 1,
	[XmlEnum("2")]
	Tractrix = 2
}

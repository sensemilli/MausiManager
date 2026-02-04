using System.Xml.Serialization;

namespace WiCAM.Pn4000.Contracts.MachineBend.Enums;

public enum ControlerType
{
	[XmlEnum(Name = "TASC 6000")]
	Tasc6000 = 0,
	[XmlEnum(Name = "Touchpoint")]
	Touchpoint = 1,
	[XmlEnum(Name = "Delem")]
	Delem = 2,
	[XmlEnum(Name = "LVD Touch B")]
	LVD = 3,
	[XmlEnum(Name = "Byvision")]
	Byvision = 4,
	[XmlEnum(Name = "P21")]
	P21 = 5,
	[XmlEnum(Name = "Dnc 1200")]
	Dnc1200 = 6,
	[XmlEnum(Name = "Byvision2")]
	Byvision2 = 7,
	Esa = 8,
	StepAutomation = 9,
	CybelecModEva = 10,
	SafanDarley = 11
}

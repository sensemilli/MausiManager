using System;

namespace WiCAM.Pn4000.Contracts.Tools;

[Flags]
public enum SpecialToolTypes
{
	Ordinary = 1,
	HeelLeft = 2,
	HeelRight = 4,
	Acb = 8,
	AcbWireless = 0x10,
	Window = 0x20,
	DieExtension = 0x40
}

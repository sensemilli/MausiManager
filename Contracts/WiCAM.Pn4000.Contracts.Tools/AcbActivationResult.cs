using System;

namespace WiCAM.Pn4000.Contracts.Tools;

[Flags]
public enum AcbActivationResult
{
	None = 0,
	NoDisks = 1,
	Coverd = 2,
	PartlyCoverd = 4,
	NotCoverd = 8,
	ProductAngleToSmall = 0x10,
	ProductAngleToLarge = 0x20,
	InvalidThickness = 0x40,
	InvalidMaterial = 0x80,
	UserActivated = 0x100,
	UserDeactivated = 0x200
}

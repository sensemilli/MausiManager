using System;

namespace WiCAM.Pn4000.Contracts.MachineBend.Enums;

[Flags]
public enum StopCombinationType : ulong
{
	None = 0uL,
	NoValidPosition = 1uL,
	BoundingBoxPosition = 2uL,
	FlatFace0 = 0x10uL,
	FlatFace1 = 0x20uL,
	FlatFace2 = 0x40uL,
	FlatFace3 = 0x80uL,
	SupportFace0 = 0x100uL,
	SupportFace1 = 0x200uL,
	SupportFace2 = 0x400uL,
	SupportFace3 = 0x800uL,
	CylinderFace0 = 0x1000uL,
	CylinderFace1 = 0x2000uL,
	CylinderFace2 = 0x4000uL,
	CylinderFace3 = 0x8000uL,
	SideCylinder0 = 0x10000uL,
	SideCylinder1 = 0x20000uL,
	SideCylinder2 = 0x40000uL,
	SideCylinder3 = 0x80000uL,
	FlatFaceMask = 0xF0uL,
	SupportFaceMask = 0xF00uL,
	CylinderFaceMask = 0xF000uL,
	SideCylinderMask = 0xF0000uL,
	CylinderMask = 0xFF000uL,
	FaceMask = 0xFFFF0uL
}

using System;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

[Flags]
public enum ChangedConfigType : uint
{
	NoChanges = 0u,
	BendTable = 1u,
	PreferredProfiles = 2u,
	PreferredFoldProfiles = 0x80000u,
	Tools = 0xCu,
	Punches = 4u,
	Dies = 8u,
	Adapters = 0x30u,
	UpperAdapter = 0x10u,
	LowerAdapter = 0x20u,
	Holders = 0xC0u,
	UpperHolder = 0x40u,
	LowerHolder = 0x80u,
	ToolGroups = 0x300u,
	PunchGroup = 0x100u,
	DieGroup = 0x200u,
	ClampingSystem = 0xC00u,
	UpperClampingSystem = 0x400u,
	LowerClampingSystem = 0x800u,
	Fingers = 0x3000u,
	LeftFinger = 0x1000u,
	RightFinger = 0x2000u,
	MachineConfig = 0x4000u,
	PostProcessor = 0x8000u,
	MaterialMapping = 0x10000u,
	ToolMapping = 0x20000u,
	BendSequence = 0x40000u
}

using System;

namespace WiCAM.Pn4000.Contracts.Tools;

[Flags]
public enum ToolProfileTypes
{
	Upper = 1,
	Lower = 2,
	Tool = 4,
	Adapter = 8,
	Extension = 0x10,
	Holder = 0x20,
	Fold = 0x40,
	Radius = 0x80,
	Roll = 0x100,
	UpperTool = 5,
	LowerTool = 6,
	UpperFoldTool = 0x45,
	LowerFoldTool = 0x46,
	LowerFoldExtension = 0x56,
	UpperFoldHolder = 0x65,
	LowerFoldHolder = 0x66,
	UpperAdapter = 9,
	LowerAdapter = 0xA
}

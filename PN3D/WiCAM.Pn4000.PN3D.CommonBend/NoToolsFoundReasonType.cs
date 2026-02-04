namespace WiCAM.Pn4000.PN3D.CommonBend;

public enum NoToolsFoundReasonType
{
	NoReason = 0,
	NoMachineSelected = 1,
	NoBendTableEntry = 2,
	NoUpperToolWithRightVAngle = 3,
	NoUpperToolWithRightWorkingHeight = 4,
	NoUpperToolForHemming = 5,
	NoLowerToolWithRightVAngle = 6,
	NoLowerToolWithRightWorkingHeight = 7,
	NoLowerToolForHemming = 8,
	UserForcedProfilesHaveInconsistentWH = 9,
	NoToolCombinationWithCompatibleWH = 10
}

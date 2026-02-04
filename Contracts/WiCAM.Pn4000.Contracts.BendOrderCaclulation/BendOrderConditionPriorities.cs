namespace WiCAM.Pn4000.Contracts.BendOrderCaclulation;

public enum BendOrderConditionPriorities
{
	SplitAngleOrder = 1,
	SelfCollision = 3,
	BigWorkingHeightMachine = 8,
	SplitCombinedBends = 10,
	ToolsCollision = 50,
	HeelsNeeded = 80,
	BigWorkingHeightUpper = 100,
	BigWorkingHeightLower = 99,
	ToolSectionLimit = 150,
	OutToIn = 200
}

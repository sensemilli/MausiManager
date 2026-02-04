namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public enum DocState
{
	Initial = 0,
	EntryModelSet = 1,
	BendDescriptorsCalculated = 2,
	CombinedDescriptorsCalculated = 3,
	UnfoldDone = 4,
	CombinedBendsSorted = 5,
	BendModelCreated = 6,
	MachineLoaded = 7,
	ToolsCalculated = 8,
	FingerCalculated = 9
}

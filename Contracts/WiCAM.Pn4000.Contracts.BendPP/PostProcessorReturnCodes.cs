namespace WiCAM.Pn4000.Contracts.BendPP;

public enum PostProcessorReturnCodes
{
	Successful = 0,
	FailedUnknownReason = 1,
	FailedInvalidFilename = 2,
	FailedOutput = 3,
	ValueOutOfBounds = 4,
	InvalidTool = 5
}

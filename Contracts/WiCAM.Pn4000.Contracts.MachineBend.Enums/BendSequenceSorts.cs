namespace WiCAM.Pn4000.Contracts.MachineBend.Enums;

public enum BendSequenceSorts
{
	OutToInModel = 0,
	InToOutModel = 1,
	LongToShortWithGaps = 2,
	ShortToLongWithGaps = 3,
	OutToInMainFace = 4,
	InToOutMainFace = 5,
	LongToShortWithoutGaps = 6,
	ShortToLongWithoutGaps = 7,
	CommonBendsFirst = 8,
	ParallelsFirstByX = 9,
	ParallelsFirstByY = 10,
	ParallelsLastByX = 11,
	ParallelsLastByY = 12,
	StartShortestBendAndFollowClosestBend = 13,
	BendAngleAscending = 14,
	BendAngleDescending = 15
}

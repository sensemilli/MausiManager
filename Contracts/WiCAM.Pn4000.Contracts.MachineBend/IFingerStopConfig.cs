namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopConfig
{
	IFingerStopSettings Settings { get; set; }

	IStopCombinations CombinationsLeft { get; set; }

	IStopCombinations CombinationsRight { get; set; }

	IFingerStopPriorities Priorities { get; set; }
}

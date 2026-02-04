using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopCombinationData : IFingerStopCombination
{
	FingerStopType FingerType { get; }

	string? CustomIconPath { get; set; }

	string? PpId { get; set; }

	string DefaultIconPath { get; }

	string GetIconPath();
}

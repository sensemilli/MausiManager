using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolListManager
{
	int GetMaxAmount(IToolPieceProfile piece);

	int GetAvailableAmount(IToolPieceProfile piece, IReadOnlyDictionary<IAliasPieceProfile, int> usedAmounts);

	void AddUsedAmount(IToolPieceProfile piece, Dictionary<IAliasPieceProfile, int> usedAmounts, int amount);
}

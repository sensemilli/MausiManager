using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IMigrationToolIdMapping
{
	Dictionary<int, int> PunchProfiles { get; }

	Dictionary<int, int> DieProfiles { get; }

	Dictionary<int, int> UpperAdapterProfiles { get; }

	Dictionary<int, int> LowerAdapterProfiles { get; }

	Dictionary<int, int> PunchGroups { get; }

	Dictionary<int, int> DieGroups { get; }

	Dictionary<int, int> PunchPieces { get; }

	Dictionary<int, int> DiePieces { get; }

	Dictionary<int, int> UpperAdapterPieces { get; }

	Dictionary<int, int> LowerAdapterPieces { get; }
}

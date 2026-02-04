using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendProfile
{
	IReadOnlyCollection<IToolProfile> UpperToolProfiles { get; }

	IReadOnlyCollection<IToolProfile> LowerToolProfiles { get; }
}

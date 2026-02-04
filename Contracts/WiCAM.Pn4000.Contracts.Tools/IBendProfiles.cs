using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendProfiles
{
	IReadOnlyCollection<IBendProfile> Profiles { get; }
}

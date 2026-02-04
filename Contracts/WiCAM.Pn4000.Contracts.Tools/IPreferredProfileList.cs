using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPreferredProfileList
{
	int Id { get; set; }

	List<IPreferredProfile> PreferredProfiles { get; set; }

	string Description { get; set; }
}

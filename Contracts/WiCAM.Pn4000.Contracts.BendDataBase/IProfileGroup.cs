using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IProfileGroup
{
	int ID { get; set; }

	string Name { get; set; }

	IReadOnlyCollection<IToolProfile> ToolProfiles { get; }

	void AddToolProfile(IToolProfile profile);

	void RemoveToolProfile(IToolProfile profile);
}

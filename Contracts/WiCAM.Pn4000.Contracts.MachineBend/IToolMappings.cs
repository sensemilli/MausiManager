using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IToolMappings
{
	Dictionary<int, string> ToolIdMappings { get; }

	Dictionary<int, (string ppName, string desc)> MountTypeIdMappings { get; }
}

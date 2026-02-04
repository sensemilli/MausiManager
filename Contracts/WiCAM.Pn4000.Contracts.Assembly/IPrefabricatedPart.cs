using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Assembly;

public interface IPrefabricatedPart
{
	public enum SearchTypes
	{
		EqualCompare = 0,
		RegexCompare = 1
	}

	string Name { get; }

	int Type { get; }

	bool IsMountedBeforeBending { get; }

	bool IgnoreAtCollision => !this.IsMountedBeforeBending;

	IEnumerable<(string propName, string propValue)> AdditionalProperties { get; }

	SearchTypes NameSearchType { get; }
}

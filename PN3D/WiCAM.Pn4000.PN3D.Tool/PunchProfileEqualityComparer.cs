using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Tool;

internal class PunchProfileEqualityComparer : IEqualityComparer<IPunchProfile>
{
	public bool Equals(IPunchProfile x, IPunchProfile y)
	{
		return x.Name == y.Name;
	}

	public int GetHashCode(IPunchProfile x)
	{
		return x.Name.GetHashCode();
	}
}

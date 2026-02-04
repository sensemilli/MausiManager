using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Tool;

internal class DieProfileEqualityComparer : IEqualityComparer<IDieProfile>
{
	public bool Equals(IDieProfile x, IDieProfile y)
	{
		return x.Name == y.Name;
	}

	public int GetHashCode(IDieProfile x)
	{
		return x.Name.GetHashCode();
	}
}

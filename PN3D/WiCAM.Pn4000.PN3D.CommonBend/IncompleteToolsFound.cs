using System.Collections.Generic;
using System.Linq;

namespace WiCAM.Pn4000.PN3D.CommonBend;

public class IncompleteToolsFound
{
	public List<int> MissingBendUIOrders;

	public string GetNotFoundBendUIOrders()
	{
		return string.Join(", ", this.MissingBendUIOrders.Select((int i) => "#" + i));
	}

	public IncompleteToolsFound(List<int> missingBends)
	{
		this.MissingBendUIOrders = missingBends.Select((int idx) => idx + 1).ToList();
	}
}

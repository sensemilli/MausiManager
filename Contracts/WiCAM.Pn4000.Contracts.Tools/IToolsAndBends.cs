using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolsAndBends
{
	List<IToolSetups> ToolSetups { get; }

	IReadOnlyList<IBendPositioning> BendPositions { get; }

	List<IBendPositioning> BendPositionsList { get; }

	IBendPositioning? GetBend(ICombinedBendDescriptor cbd)
	{
		return this.BendPositions.FirstOrDefault((IBendPositioning b) => b.Order == cbd.Order);
	}
}

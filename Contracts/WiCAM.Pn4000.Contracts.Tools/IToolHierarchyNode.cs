using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolHierarchyNode
{
	List<IToolHierarchyNode> Children { get; }

	IToolSection Section { get; set; }
}

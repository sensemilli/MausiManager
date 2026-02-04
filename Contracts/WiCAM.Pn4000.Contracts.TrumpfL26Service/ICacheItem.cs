using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ICacheItem
{
	string JobName { get; set; }

	string Material { get; set; }

	List<ICachePlate> Plates { get; set; }

	List<ICachePart> Parts { get; set; }

	string NestingInputContent { get; set; }
}

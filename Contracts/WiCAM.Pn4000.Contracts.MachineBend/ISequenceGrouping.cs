using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface ISequenceGrouping
{
	string Description { get; set; }

	int GroupingType { get; set; }

	List<BendSequenceSorts> InnerSortSequences { get; set; }
}

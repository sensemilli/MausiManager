using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendSequenceOrder
{
	Guid Id { get; set; }

	string Description { get; set; }

	List<BendSequenceSorts> Sequences { get; set; }

	bool Enabled { get; set; }

	List<ISequenceGrouping> Groupings { get; set; }
}

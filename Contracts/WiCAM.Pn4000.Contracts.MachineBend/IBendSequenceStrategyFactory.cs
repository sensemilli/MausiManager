using System;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendSequenceStrategyFactory
{
	IBendSequenceOrder CreateNewSequenceOrder(string description, bool enabled, Guid guid);

	ISequenceGrouping CreateGrouping(string description, int groupingType);
}

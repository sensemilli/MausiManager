using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IToolPresenceVector
{
	List<IToolPresence> Presences { get; set; }

	ICombinedBendDescriptorInternal Bend { get; set; }

	double RefPointOffsetStart { get; set; }

	double RefPointOffsetEnd { get; set; }

	IToolInfo ToolInfo { get; set; }

	int PresenceCount { get; }
}

using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopCombination
{
	List<string> FaceNames { get; }

	StopCombinationType Type { get; }

	bool IsClamp { get; }

	bool HasCylinder { get; }

	bool HasSupport { get; }

	bool BoundingBoxPosition { get; }

	bool NoValidPositionFound { get; }
}

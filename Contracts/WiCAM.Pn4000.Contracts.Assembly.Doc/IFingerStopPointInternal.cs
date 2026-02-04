using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.FingerStops;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IFingerStopPointInternal : IFingerStopPoint
{
	PartRole Finger { get; }

	Vector3d BackEdgeCenterPoint { get; set; }

	double RelativeEdgePosition { get; set; }

	bool SavetyDistanceOverToolsUsed { get; set; }

	int Rating { get; set; }

	FingerPosition FingerPosition { get; set; }
}

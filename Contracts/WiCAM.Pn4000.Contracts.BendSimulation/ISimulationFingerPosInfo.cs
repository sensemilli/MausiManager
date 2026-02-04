using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationFingerPosInfo
{
	IFingerStopPoint? LeftFingerStopPoint { get; }

	IFingerStopPoint? RightFingerStopPoint { get; }

	FingerPositioningMode FingerPositioningMode { get; }

	FingerStability FingerStability { get; }

	double? XLeftRetractAuto { get; }

	double? XLeftRetractUser { get; }

	double? XRightRetractAuto { get; }

	double? XRightRetractUser { get; }

	double? RLeftRetractAuto { get; }

	double? RLeftRetractUser { get; }

	double? RRightRetractAuto { get; }

	double? RRightRetractUser { get; }

	double? ZLeftRetractAuto { get; }

	double? ZLeftRetractUser { get; }

	double? ZRightRetractAuto { get; }

	double? ZRightRetractUser { get; }
}

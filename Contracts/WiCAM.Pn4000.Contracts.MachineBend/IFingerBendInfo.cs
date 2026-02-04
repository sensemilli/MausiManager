using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerBendInfo : ISimulationBendInfo
{
	int CombinedBendCount { get; }

	List<IFingerStopPointInternal> StopPointsLeft { get; set; }

	List<IFingerStopPointInternal> StopPointsRight { get; set; }

	IFingerStopPointInternal SelectedStopPointLeft { get; set; }

	IFingerStopPointInternal SelectedStopPointRight { get; set; }

	FingerPositioningMode FingerPositioningMode { get; set; }

	FingerStability FingerStability { get; set; }

	double? XLeftRetractAuto { get; set; }

	double? XRightRetractAuto { get; set; }

	List<Model> LowerTools { get; }

	List<Model> UpperTools { get; }

	List<Model> LowerAdapters { get; }
}

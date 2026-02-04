using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface ICombinedBendDescriptorInternal : ICombinedBendDescriptor
{
	IToolPresenceVector ToolPresenceVector { get; set; }

	new IBendPositioningInfoInternal PositioningInfo { get; }

	new IFingerStopPointInternal? SelectedStopPointLeft { get; set; }

	new IFingerStopPointInternal? SelectedStopPointRight { get; set; }

	List<IFingerStopPointInternal> StopPointsLeft { get; set; }

	List<IFingerStopPointInternal> StopPointsRight { get; set; }

	bool LeftFingerSnap { get; set; }

	bool RightFingerSnap { get; set; }

	bool IsCompatibleBendUnfoldModel(ICombinedBendDescriptorInternal other);

	bool IsCompatibleBendBendModel(ICombinedBendDescriptorInternal other);
}

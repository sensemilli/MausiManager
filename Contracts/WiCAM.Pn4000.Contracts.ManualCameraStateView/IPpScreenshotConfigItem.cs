using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public interface IPpScreenshotConfigItem
{
	Dictionary<MachineParts, double> Opacities { get; }

	ProjectionType ProjectionType { get; set; }

	Matrix4d ViewRotation { get; set; }

	bool AdjustManually { get; set; }
}

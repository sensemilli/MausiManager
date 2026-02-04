using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public interface IPPManualSetupViewModel
{
	ICameraState GetCameraState(double screenWidth, double screenHeight, string title, Action<ISimulationScreen> initScreenAction, IBendMachine machine, ScreenshotType type, bool dontDefault, out ProjectionType? projectionType, out Matrix4d? viewMatrix, out Dictionary<MachineParts, double> partRoleOpacities);
}

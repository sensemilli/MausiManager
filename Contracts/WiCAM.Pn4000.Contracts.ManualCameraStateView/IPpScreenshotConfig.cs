using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public interface IPpScreenshotConfig
{
	string Id { get; }

	string DisplayName { get; }

	Dictionary<ScreenshotType, IPpScreenshotConfigItem> Items { get; }
}

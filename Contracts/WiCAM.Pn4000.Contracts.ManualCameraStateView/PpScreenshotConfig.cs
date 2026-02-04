using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public class PpScreenshotConfig(string id) : IPpScreenshotConfig
{
	public string Id { get; } = id;

	public string DisplayName { get; set; }

	public Dictionary<ScreenshotType, IPpScreenshotConfigItem> Items { get; set; }


}

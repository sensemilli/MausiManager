using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public interface IGlobalPpScreenshotConfig
{
	IPpScreenshotConfigItem GetCustomizeValues(IBendMachine? machine, ScreenshotType type, bool dontDefault = false);

	IPpScreenshotConfigItem GetDefaultCustomizeValues(ScreenshotType type);

	void SaveCustomizeValues(IPpScreenshotConfigItem values, IBendMachine? machine, ScreenshotType type);

	void DeleteCustomizeValues(ScreenshotType type, IBendMachine? machine);

	IPpScreenshotConfig GetGlobalConfig();
}

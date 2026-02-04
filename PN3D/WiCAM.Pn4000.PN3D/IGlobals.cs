using System;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D;

public interface IGlobals
{
	IPKernelFlowGlobalDataService PKernelFlowGlobalData { get; }

	IConfigProvider ConfigProvider { get; }

	IPnPathService PnPathService { get; }

	ILogCenterService logCenterService { get; }

	ILanguageDictionary LanguageDictionary { get; }

	IMessageLogGlobal MessageDisplay { get; }

	IFrontCalls FallbackFrontCalls { get; }

	[Obsolete("use DI")]
	IMaterialManager Materials { get; }

	bool ShowPopups { get; }

	IScreenshotScreen ScreenshotScreen { get; }

	void ShowScreenNavigation();
}

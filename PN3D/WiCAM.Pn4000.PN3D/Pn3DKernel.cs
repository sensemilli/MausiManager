using System;
using System.Windows;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D;

public class Pn3DKernel : IGlobals
{
	private readonly IAutoMode _autoMode;

	private readonly IScreen3DMain _screen3D;

	private readonly ICurrentDocProvider _currentDocProvider;

	public IMessageLogGlobal MessageDisplay => (IMessageLogGlobal)this.FallbackFrontCalls.MessageDisplay;

	public IFrontCalls FallbackFrontCalls { get; set; }

	public bool ShowPopups => this._autoMode.PopupsEnabled;

	public IPKernelFlowGlobalDataService PKernelFlowGlobalData { get; }

	public PN3DRootPipe Pn3DRootPipe { get; private set; }

	public ILanguageDictionary LanguageDictionary { get; set; }

	public IMainWindowDataProvider MainWindowDataProvider { get; set; }

	public Window MainWindow { get; }

	public IPnDebug PnDebug { get; set; }

	public IConfigProvider ConfigProvider { get; set; }

	public IPnPathService PnPathService { get; set; }

	public IMaterialManager Materials { get; set; }

	public ILogCenterService logCenterService { get; set; }

	public IScreenshotScreen ScreenshotScreen { get; }
    //, IScreenshotScreen screenshotScreen
    public Pn3DKernel(IPKernelFlowGlobalDataService pKernelFlow, ILogCenterService logCenter, IPnPathService pathService, IMainWindowDataProvider mainWindow,
		ILanguageDictionary languageDictionary, IConfigProvider configProvider, IAutoMode autoMode, IPnDebug pnDebug, IMaterialManager materialManager, IScreen3DMain screen3D, ICurrentDocProvider currentDocProvider)
	{
		this._autoMode = autoMode;
		//this.ScreenshotScreen = screenshotScreen;
		this.logCenterService = logCenter;
		this.ConfigProvider = configProvider;
		this.PnPathService = pathService;
		this.PKernelFlowGlobalData = pKernelFlow;
		this.MainWindowDataProvider = mainWindow;
		this.MainWindow = mainWindow as Window;
		this.LanguageDictionary = languageDictionary;
		this.PnDebug = pnDebug;
		this._screen3D = screen3D;
		this._currentDocProvider = currentDocProvider;
	}

	public void Init(IFactorio factorio)
	{
		this.Pn3DRootPipe = factorio.Resolve<PN3DRootPipe>();
		this.Materials = factorio.Resolve<IMaterialManager>();
		Console.WriteLine("Pn3DKernel.Init    " + this.PnPathService.PnMasterOrDrive);
        (this.Materials as global::WiCAM.Pn4000.pn4.pn4Services.Materials).Init(this.PnPathService.PnMasterOrDrive, this.ConfigProvider.InjectOrCreate<SpringbackConfig>(), this.logCenterService.CatchRaport);
	}

	public void CheckModelStyle()
	{
		int num = this.MainWindowDataProvider.Ribbon_GetActiveTabID();
		this.MainWindowDataProvider.OnActiveTabChanged();
		if (this._currentDocProvider.CurrentDoc != null && (uint)(num - 897) > 2u)
		{
			this._screen3D.ScreenD3D.RemoveModel(null);
			this._screen3D.ScreenD3D.RemoveBillboard(null);
		}
	}

	public void ShowScreenNavigation()
	{
		this._screen3D.Screen3D.ShowNavigation(show: true);
	}
}

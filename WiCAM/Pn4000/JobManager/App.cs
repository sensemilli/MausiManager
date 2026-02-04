using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Helper;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using ControlzEx.Theming;
using iTextSharp.text.log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Office.Interop.Excel;
using Sense3D.SenseScreen3D.PN3D.Doc;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Config;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.BendDoc;
using WiCAM.Pn4000.BendDoc.Contracts;
using WiCAM.Pn4000.BendDoc.Services;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.BendModel.Services.BendTools.IrregularBendsAnalyzer;
using WiCAM.Pn4000.BendModel.Services.BendTools.PartAnalyzer;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Extensions;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.Contracts.Threading;
using WiCAM.Pn4000.DependencyInjection.Implementations;
using WiCAM.Pn4000.Gmpool;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.GuiWpf;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;
using WiCAM.Pn4000.GuiWpf.PnCommand;
using WiCAM.Pn4000.GuiWpf.Ui3D.DocManagement;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;
using WiCAM.Pn4000.Helpers.Util;
using WiCAM.Pn4000.JobManager.Helpers;
using WiCAM.Pn4000.MachineAndTools.Loaders;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Contracts;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.pn4.pn4UILib.Ribbon;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Screen;
using WiCAM.Pn4000.Screen.Classic2D;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.XEvents;
using WiCAM.PN4000.PnPathService;
using WiCAM.Services.ConfigProviders;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.Loggers;
using WiCAM.Services.Loggers.Contracts;
using Action = System.Action;
using Application = System.Windows.Application;
using IPartAnalyzer = WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers.IPartAnalyzer;
using IScreenshotScreen = WiCAM.Pn4000.Contracts.Screen.IScreenshotScreen;
using TextBox = System.Windows.Controls.TextBox;
using Toolbars = WiCAM.Pn4000.pn4.pn4UILib.Toolbars;
using WeakEventManager = WiCAM.Pn4000.DependencyInjection.Implementations.WeakEventManager;

namespace WiCAM.Pn4000.JobManager;

public partial class App : Application
{
    public static App GetApp { get; private set; }

    public ServiceCollection serviceDescriptors;
	public static ServiceProvider serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
	{
		GetApp = this;
        // Update-Check durchführen
   
        ServiceProviderOptions providerOptions = new ServiceProviderOptions();
		serviceDescriptors = new ServiceCollection();
		serviceDescriptors.AddSingleton<IFactorio, Globals>();
		serviceDescriptors.AddSingleton<IDoc3dFactory, Doc3dFactory>();
		serviceDescriptors.AddSingleton<IPnPathService, PnPathService>();
		serviceDescriptors.AddSingleton<ILogCenterService, LogCenterService>();
		serviceDescriptors.AddSingleton<IConfigProvider, ConfigProvider>();
		serviceDescriptors.AddSingleton<IPKernelFlowGlobalDataService, PKernelFlowGlobalDataService>();
		serviceDescriptors.AddSingleton<ILanguageDictionary, LanguageDictionary>();
		serviceDescriptors.AddSingleton<ITranslator, Translator>();
		serviceDescriptors.AddSingleton<IWeakEventManager, WeakEventManager>();
		serviceDescriptors.AddSingleton<IUnitConverter, WiCAM.Pn4000.PN3D.Converter.UnitConverter>();
        serviceDescriptors.AddScoped<IUnitConverter>(_ => new WiCAM.Pn4000.PN3D.Converter.UnitConverter(new double(), new double()));
		serviceDescriptors.AddSingleton<IMessageLogGlobal, MessageLogGlobal>();
        serviceDescriptors.AddScoped<IWiLogger>(_ => new WiLogger("MausiManager"));
		serviceDescriptors.AddSingleton<IConfigSourceCollection, ConfigSourceCollection>();
		serviceDescriptors.AddSingleton<PnRibbon>();
		serviceDescriptors.AddSingleton<IRibbonMainWindowConnector, RibbonMainWindowConnector>();
		serviceDescriptors.AddSingleton<IPnToolTipService, PnToolTipService>();
		serviceDescriptors.AddSingleton<IPnIconsService, PnIconsService>();
		serviceDescriptors.AddSingleton<IPnColorsService, PnColorsService>();
		serviceDescriptors.AddSingleton<ISubMenuConnector, SubMenuConnector>();
		serviceDescriptors.AddSingleton<Toolbars>();
        serviceDescriptors.AddSingleton<IPnCommandBasics, PnCommandBasics>();
        serviceDescriptors.AddSingletonIntercepted<IPnCommandsBend, PnCommandsBend>();
        serviceDescriptors.AddSingletonIntercepted<IPnCommandsUnfold, PnCommandsUnfold>();
        serviceDescriptors.AddSingletonIntercepted<IPnCommandsScreen, PnCommandsScreen>();
        serviceDescriptors.AddSingletonIntercepted<IPnCommandsOther, PnCommandsOther>();
		serviceDescriptors.AddSingleton<IModelFactory, ModelFactory>();
		serviceDescriptors.AddSingleton<IPnDebug, PnDebug>();
		serviceDescriptors.AddSingleton<IPKernelStatusBarModel, PKernelStatusBarModel>();
		serviceDescriptors.AddSingleton<Pn3DKernel>();
		serviceDescriptors.AddSingleton<UIFeedback>();
		serviceDescriptors.AddSingleton<EventsList>();
		serviceDescriptors.AddSingleton<ICurrentDocProvider, CurrentDocProvider>();
		serviceDescriptors.AddSingleton<IDoc3dFactory, Doc3dFactory>();
		serviceDescriptors.AddSingleton<IDoEvents, DoEventsForms>();
		serviceDescriptors.AddSingleton<IScreen3DMain, Screen3DMain>();
		serviceDescriptors.AddSingleton<IDocManager, DocManager>();
		serviceDescriptors.AddSingleton<IScreenshotScreen, ScreenshotScreeen>();
		serviceDescriptors.AddSingleton<IDocManagerInternal, DocManager>();
		serviceDescriptors.AddSingleton<IMainWindowBlock, MainWindowBlock>();
		serviceDescriptors.AddSingleton<IPnStatusBarHelper, PnStatusBarHelper>();
		serviceDescriptors.AddSingleton<WiCAM.Pn4000.GuiWpf.MainWindowViewModel>();
		serviceDescriptors.AddSingleton<IDocumentManagerViewModel, DocumentManagerViewModel>();
        serviceDescriptors.AddSingletonRegisterBoth<IMainWindowDataProvider, MainWindow>();
        serviceDescriptors.AddSingleton((Func<IServiceProvider, IRibbon>)((IServiceProvider x) => x.GetService<IMainWindowDataProvider>()));
        serviceDescriptors.AddOptions();
		serviceDescriptors.AddPnBndDoc();
		serviceDescriptors.AddPn3d();
        serviceDescriptors.AddSingleton<IMaterialManager, WiCAM.Pn4000.pn4.pn4Services.Materials>();
		serviceDescriptors.AddSingleton<IDockingService, DockingService>();
		serviceDescriptors.AddSingleton<IGlobals, Pn3DKernel>();
		serviceDescriptors.AddSingleton<IDocSerializer, DocSerializer>();
		serviceDescriptors.AddSingleton<ISpatialImport, SpatialImport>();
		serviceDescriptors.AddSingleton<ISpatialLoader, SpatialLoader>();
		serviceDescriptors.AddSingleton<IShowPopupService, ShowPopupService>();
		serviceDescriptors.AddSingleton<IArvLoader, ArvLoader>();
		serviceDescriptors.AddSingleton<IMaterial3dFortran, Material3dFortran>();
        serviceDescriptors.AddScoped<IScopedFactorio, ModelFactory>();
		serviceDescriptors.AddSingleton<IMachineHelper, MachineHelper>();
		serviceDescriptors.AddSingleton<IDieHelperDeprecated, DieHelperDeprecated>();
	//	serviceDescriptors.AddSingleton<IPunchHelperDeprecated, PunchHelperDeprecated>();
		serviceDescriptors.AddSingleton<BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces.IProfilesHelper, ProfilesHelper>();
        //serviceDescriptors.AddKeyedSingleton<IIrregularBendsAnalyzer, IrregularBendsAnalyzer>();
        //serviceDescriptors.AddSingleton<IPartAnalyzer, PartAnalyzer>();
        serviceDescriptors.AddSingleton<WiCAM.Pn4000.BendModel.Services.BendTools.PartAnalyzer.IPartAnalyzer, Sense3D.SenseBendModel.BendModelServices.BendTools.PartAnalyzer.PartAnalyzeer>();
        serviceDescriptors.AddSingleton<IProcessPool, ProcessPool>();
		serviceDescriptors.AddSingleton<ISpatialAssemblyLoader, SpatialAssemblyLoader>();
		serviceDescriptors.AddSingleton<IPN3DDocPipe, PN3DDocPipe>();
		serviceDescriptors.AddSingleton<IPnBndDocImporter, PnBndDocImporter>();
        serviceDescriptors.AddSingleton<IAutoMode, AutoMode>();
		serviceDescriptors.AddSingleton<ExeFlow>();
        serviceDescriptors.AddSingleton<IScreen3DMain, Screen3DMain>();
        serviceDescriptors.AddSingletonRegisterBoth<IClassicScreen2D, ClassicScreen2D>();
        serviceDescriptors.AddSingleton((Func<IServiceProvider, IScreen2D>)((IServiceProvider x) => x.GetService<IClassicScreen2D>()));
        serviceProvider = serviceDescriptors.BuildServiceProvider(providerOptions);


        ResourceDictionary resourceDictionaries; // PNarchive
        base.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		string productName = "JobManager_" + Environment.UserName;
		Logger.ControlLogSize = true;
		Logger.InitializeTracing(string.Empty, productName, AppDomain.CurrentDomain.BaseDirectory);
		if (Environment.CommandLine.Contains("debug", StringComparison.OrdinalIgnoreCase) || Environment.CommandLine.Contains("log", StringComparison.OrdinalIgnoreCase) || Environment.CommandLine.Contains("trace", StringComparison.OrdinalIgnoreCase))
		{
			Logger.ChangeTraceLevel(TraceLevel.Verbose);
		}
		if (!SystemConfiguration.Initialize())
		{
			Logger.Error("System configuration can't be initialised. Application will be terminated!");
			Application.Current.Shutdown(0);
			return;
		}
        //EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDown, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
        //EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocus, new RoutedEventHandler(SelectAllText));
        //EventManager.RegisterClassHandler(typeof(TextBox), Control.MouseDoubleClick, new RoutedEventHandler(SelectAllText));
        ApplicationConfigurationInfo.Instance.MaterialNumber();
        if (!ApplicationConfigurationInfo.Instance.ReadSettings())
        {
            Application.Current.Shutdown(0);
            return;
        }
        if (!AppConfiguration.Instance.Initialize())
        {
            Application.Current.Shutdown(0);
            return;
        }

        base.OnStartup(e);

        // Update-Check durchführen
        var updateManager = new UpdateManager();
        if (updateManager.CheckAndApplyUpdates())
        {
            // Wenn Update durchgeführt wird, Anwendung ordnungsgemäß beenden
            Logger.Info("Programm wird für Update beendet.");

            // Sicherstellen, dass keine weiteren Initialisierungen stattfinden
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Current.Shutdown(0);
            return;
        }

        new Bootstrapper(ServiceFactory.CreateDefaultProvider()).Show();
        switch (AppConfiguration.Instance.ArchiveType)
        {
            case ArchiveFileType.N2D:
            case ArchiveFileType.MPL:
            case ArchiveFileType.M2D:
            case ArchiveFileType.N3D:
                {
                    Console.WriteLine("hello");
                    WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.ArchivesAreReadHandler = new Action(WiCAM.Pn4000.JobManager.MainWindow._instance.ArchivesAreRead);
                    WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = AppArguments.Instance.ArchiveNumber();
                    WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.Initialize(WiCAM.Pn4000.JobManager.MainWindow._instance.ArchiveFilter);
                    resourceDictionaries = StringResourceHelper.Instance.FindDictionary(SystemConfiguration.PnLanguage);
                    return;
                }
        }
        Application.Current.Shutdown(0);
	}

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Console.WriteLine("onstartup");
        AnyCAD.Foundation.GlobalInstance.Initialize();
        // 启用Shape捕捉功能。
        AnyCAD.Foundation.ModelingEngine.EnableSnapShape();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        AnyCAD.Foundation.GlobalInstance.Destroy();
    }


    private void SelectAllText(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is TextBox textBox)
		{
			textBox.SelectAll();
		}
	}

	private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
	{
		DependencyObject dependencyObject = e.OriginalSource as UIElement;
		while (dependencyObject != null && !(dependencyObject is TextBox))
		{
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		if (dependencyObject != null)
		{
			TextBox textBox = (TextBox)dependencyObject;
			if (!textBox.IsKeyboardFocusWithin)
			{
				textBox.Focus();
				e.Handled = true;
			}
		}
	}

	private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		LogException(e.Exception, "TaskScheduler Unhandled exception");
		e.SetObserved();
	}

	private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		LogException(e.Exception, "Current Unhandled exception");
		e.Handled = true;
	}

	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		LogException(e.ExceptionObject as Exception, "CurrentDomain Unhandled exception");
	}

	private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		LogException(e.Exception, "Dispatcher Unhandled exception");
		e.Handled = true;
	}

	private void LogException(Exception e, string text)
	{
		Logger.Error("---------- {0}:", text);
		Logger.Exception(e);
	}

	
	

	[STAThread]
	public static void Main()
	{
		App app = new App();
        Uri resourceLocator = new Uri("/MausiManager;component/WiCAM/Pn4000/JobManager/App.xaml", UriKind.Relative);
        Application.LoadComponent(app, resourceLocator);
        app.InitializeComponent();
		app.Run();

    }
}

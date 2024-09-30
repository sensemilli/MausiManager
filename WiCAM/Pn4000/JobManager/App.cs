using ControlzEx.Theming;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool;

namespace WiCAM.Pn4000.JobManager;

public partial class App : Application
{

	protected override void OnStartup(StartupEventArgs e)
	{
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager.Helpers;

namespace WiCAM.Pn4000.JobManager;

public class MainWindowViewModel1 : ViewModelBase, IDialogViewModel, IPreviewObserver
{
	private IJobManagerServiceProvider _provider;

	private IJobManagerSettings _settings;

	private IJobDataViewModel _jobDataModel;

	private IFilterViewModel _filterViewJobModel;

	private IFilterViewModel _filterViewPlateModel;

	private IFilterViewModel _filterViewPartsModel;

	private MachinesControlViewModel _machinesViewModel;

	private SettingsControlViewModel _settingsControlViewModel;

	private string _WindowName;

	private BitmapImage _selectedImage;

	private Visibility _buttonPlateResetVisibility;

	private int _totalJobs;

	private int _readCounter;

	private int _readJobs;

	private Visibility _statusVisibility;

	private Visibility _printerSettingsButtonVisibility = Visibility.Collapsed;

	private GridLength _column1Width;

	private ICommand _filterDeleteCommand;

	private ICommand _openPrinterSettingsCommand;

	private ICommand _openIniFileCommand;

	private ICommand _openLogFileCommand;

	private ICommand _exitCommand;

	private ICommand _showTemplateCommand;

	public IDialogView View { get; private set; }

	public string WindowName
	{
		get
		{
			return _WindowName;
		}
		set
		{
			_WindowName = value;
			NotifyPropertyChanged("WindowName");
		}
	}

	public BitmapImage SelectedImage
	{
		get
		{
			return _selectedImage;
		}
		set
		{
			_selectedImage = value;
			NotifyPropertyChanged("SelectedImage");
		}
	}

	public Visibility ButtonPlateResetVisibility
	{
		get
		{
			return _buttonPlateResetVisibility;
		}
		set
		{
			_buttonPlateResetVisibility = value;
			NotifyPropertyChanged("ButtonPlateResetVisibility");
		}
	}

	public Visibility IsSettingsVisible
	{
		get
		{
			if (!_settings.IsSettingsVisible)
			{
				return Visibility.Hidden;
			}
			return Visibility.Visible;
		}
		set
		{
			_settings.IsSettingsVisible = value == Visibility.Visible;
			NotifyPropertyChanged(" IsSettingsVisible");
		}
	}

	public bool IsTouchScreen
	{
		get
		{
			return _settings.IsTouchScreen;
		}
		set
		{
			_settings.IsTouchScreen = value;
			_jobDataModel.FontSize = (value ? 25 : 12);
		}
	}

	public int TotalJobs
	{
		get
		{
			return _totalJobs;
		}
		set
		{
			_totalJobs = value;
			NotifyPropertyChanged("TotalJobs");
		}
	}

	public int ReadJobs
	{
		get
		{
			return _readJobs;
		}
		set
		{
			_readJobs = value;
			NotifyPropertyChanged("ReadJobs");
		}
	}

	public Visibility StatusVisibility
	{
		get
		{
			return _statusVisibility;
		}
		set
		{
			_statusVisibility = value;
			NotifyPropertyChanged("StatusVisibility");
		}
	}

	public Visibility PrinterSettingsButtonVisibility
	{
		get
		{
			return _printerSettingsButtonVisibility;
		}
		set
		{
			_printerSettingsButtonVisibility = value;
			NotifyPropertyChanged("PrinterSettingsButtonVisibility");
		}
	}

	public GridLength Column1Width
	{
		get
		{
			return _column1Width;
		}
		set
		{
			_column1Width = value;
			NotifyPropertyChanged("Column1Width");
		}
	}

	public ICommand FilterDeleteCommand
	{
		get
		{
			if (_filterDeleteCommand == null)
			{
				_filterDeleteCommand = new RelayCommand(delegate
				{
					FilterDelete();
				}, (object x) => true);
			}
			return _filterDeleteCommand;
		}
	}

	public ICommand DeleteJobCommand => _jobDataModel.DeleteJobCommand;

	public ICommand DeleteProducedJobsCommand => _jobDataModel.DeleteProducedJobsCommand;

	public ICommand ProduceJobCommand => _jobDataModel.ProduceJobCommand;

	public ICommand ProducePlateCommand => _jobDataModel.ProducePlateCommand;

	public ICommand RejectPartCommand => _jobDataModel.RejectPartCommand;

	public ICommand ReloadJobsCommand => _jobDataModel.ReloadJobsCommand;

	public ICommand SaveProducedJobsCommand => _jobDataModel.SaveProducedJobsCommand;

	public ICommand StornoPlateCommand => _jobDataModel.StornoPlateCommand;

	public ICommand ResetPlateCommand => _jobDataModel.ResetPlateCommand;

	public ICommand SaveSettingsCommand => _settingsControlViewModel.SaveCommand;

	public ICommand PrintPartLabelsCommand => _jobDataModel.PrintPartLabelsCommand;

	public ICommand PrintPlateLabelsCommand => _jobDataModel.PrintPlateLabelsCommand;

	public ICommand PrintJobLabelsCommand => _jobDataModel.PrintJobLabelsCommand;

	public ICommand OpenPrinterSettingsCommand
	{
		get
		{
			if (_openPrinterSettingsCommand == null)
			{
				_openPrinterSettingsCommand = new RelayCommand(delegate
				{
					OpenPrinterSettings();
				}, (object x) => CanOpenPrinterSettings());
			}
			return _openPrinterSettingsCommand;
		}
	}

	public ICommand OpenIniFileCommand
	{
		get
		{
			if (_openIniFileCommand == null)
			{
				_openIniFileCommand = new RelayCommand((Action<object>)delegate
				{
					OpenIniFile();
				});
			}
			return _openIniFileCommand;
		}
	}

	public ICommand OpenLogFileCommand
	{
		get
		{
			if (_openLogFileCommand == null)
			{
				_openLogFileCommand = new RelayCommand((Action<object>)delegate
				{
					OpenLogFile();
				});
			}
			return _openLogFileCommand;
		}
	}

	public ICommand ExitCommand
	{
		get
		{
			if (_exitCommand == null)
			{
				_exitCommand = new RelayCommand(delegate
				{
					Exit();
				}, (object x) => true);
			}
			return _exitCommand;
		}
	}

	public ICommand ShowTemplateCommand
	{
		get
		{
			if (_showTemplateCommand == null)
			{
				_showTemplateCommand = new RelayCommand((Action<object>)delegate
				{
					ShowTemplate();
				});
			}
			return _showTemplateCommand;
		}
	}

	private void FilterDelete()
	{
		_filterViewJobModel.ResetFilters();
		_filterViewPlateModel.ResetFilters();
		_filterViewPartsModel.ResetFilters();
	}

	private void OpenPrinterSettings()
	{
		ProcessHelper.ExecuteProcessVisible(PnPathBuilder.PathInPnDrive("u\\pn\\run\\PnPrinter.exe"), "/config");
	}

	private bool CanOpenPrinterSettings()
	{
		return PrinterSettingsButtonVisibility == Visibility.Visible;
	}

	private void OpenIniFile()
	{
		string path = PnPathBuilder.PathInPnHome(SettingsInfo.IniName);
		OpenFile(path);
	}

	private void OpenLogFile()
	{
		OpenFile(Logger.LogFileName);
	}

	private void Exit()
	{
	//	View.Close();
		//Application.Current.Shutdown(0);
	}

	private void ShowTemplate()
	{
		string path = PnPathBuilder.PathInPnDrive("u\\pn\\gfiles\\JobManagerTemplates.txt");
		OpenFile(path);
	}

	private void OpenFile(string path)
	{
		if (IOHelper.FileExists(path))
		{
			Process process = new Process();
			process.StartInfo.FileName = "notepad.exe";
			process.StartInfo.Arguments = path;
			process.Start();
		}
	}

	public void Initialize(IDialogView view, IJobManagerServiceProvider provider)
	{
		View = view;
		_provider = provider;
		string text = Application.Current.Resources["MainWindow"].ToString();
		string text2 = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		WindowName = text + new string(' ', 20) + "Version : " + text2;
		_settings = _provider.FindService<IJobManagerSettings>();
		Column1Width = new GridLength(_settings.Column1Width, GridUnitType.Pixel);
		ButtonPlateResetVisibility = ((!_settings.IsButtonPlateResetVisible) ? Visibility.Collapsed : Visibility.Visible);
		IStateManager stateManager = _provider.FindService<IStateManager>();
		PrinterSettingsButtonVisibility = (string.IsNullOrWhiteSpace(_settings.PrinterSettingsBatch) ? Visibility.Collapsed : Visibility.Visible);
		if (View is MainWindow mainWindow)
		{
			_jobDataModel = _settings.ModelManager.Register<JobDataControl, JobDataViewModel>(mainWindow.jobDataControl, _provider) as IJobDataViewModel;
			_provider.JobFilter = _jobDataModel;
			_provider.PartFilter = _jobDataModel;
			_provider.PlateFilter = _jobDataModel;
			_filterViewJobModel = _settings.ModelManager.Register<FilterControl, FilterControlViewModel>(mainWindow.filterControlJobs, _provider) as IFilterViewModel;
			_filterViewPlateModel = _settings.ModelManager.Register<FilterControl, FilterPlateControlViewModel>(mainWindow.filterControlPlates, _provider) as IFilterViewModel;
			_filterViewPartsModel = _settings.ModelManager.Register<FilterControl, FilterPartControlViewModel>(mainWindow.filterControlParts, _provider) as IFilterViewModel;
			mainWindow.Loaded += Wnd_Loaded;
			mainWindow.Closing += Wnd_Closing;
			mainWindow.txtBarcodeJob.KeyUp += TxtBarcodeJob_PreviewKeyUp;
			mainWindow.txtBarcodePlate.KeyUp += TxtBarcodePlate_PreviewKeyUp;
			stateManager.AttachMachineObserver(_jobDataModel);
			stateManager.AttachImageObserver(this);
			stateManager.AttachJobFilterObserver(_jobDataModel);
			stateManager.AttachPartFilterObserver(_jobDataModel);
			_machinesViewModel = _settings.ModelManager.Register<MachinesControl, MachinesControlViewModel>(mainWindow.machinesControl, _provider) as MachinesControlViewModel;
			if (_machinesViewModel != null)
			{
				_machinesViewModel.AttachStateManager(stateManager);
				stateManager.NotifyMachinesChanged(_settings.Machines);
			}
			_settingsControlViewModel = new SettingsControlViewModel(_settings, mainWindow.settingsControl.configurableListControl);
			mainWindow.settingsControl.DataContext = _settingsControlViewModel;
		}
		StatusVisibility = Visibility.Visible;
		if (_jobDataModel != null)
		{
			_jobDataModel.FontSize = (IsTouchScreen ? 25 : 12);
		}
	}

	private void ChangeJobsReadProgress(int total)
	{
		if (Application.Current.Dispatcher == null)
		{
			return;
		}
		if (Application.Current.Dispatcher.CheckAccess())
		{
			if (total > 0)
			{
				TotalJobs = total;
				ReadJobs = 0;
				_readCounter = 0;
				return;
			}
			_readCounter++;
			if (_readCounter % 1000 == 0)
			{
				ReadJobs = _readCounter;
			}
			else if (_readCounter == TotalJobs)
			{
				ReadJobs = TotalJobs;
			}
		}
		else
		{
			Application.Current.Dispatcher.BeginInvoke(new Action<int>(ChangeJobsReadProgress), DispatcherPriority.Send, total);
		}
	}

	private void TxtBarcodePlate_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && sender is TextBox textBox)
		{
			if (_jobDataModel.HasToIgnoreEnter)
			{
				_jobDataModel.HasToIgnoreEnter = false;
			}
			else
			{
				_jobDataModel.ProducePlateUsingBarcode(textBox.Text);
			}
			textBox.SelectAll();
		}
	}

	private void TxtBarcodeJob_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && sender is TextBox textBox)
		{
			if (_jobDataModel.HasToIgnoreEnter)
			{
				_jobDataModel.HasToIgnoreEnter = false;
			}
			else
			{
				_jobDataModel.ProduceJobUsingBarcode(textBox.Text);
			}
			textBox.SelectAll();
		}
	}

	private void Wnd_Loaded(object sender, RoutedEventArgs e)
	{
		_settings.ApplyWindowConfiguration(View as Window);
		_jobDataModel.LoadJobs();
	}

	private void Wnd_Closing(object sender, CancelEventArgs e)
	{
		_jobDataModel.SaveSettings();
		_settings.Column1Width = (int)Column1Width.Value;
		_settings.SaveConfiguration(View as Window);
	}

	public void PreviewChanged(string path)
	{
		SelectedImage = null;
		SelectedImage = new ImageFromMemoryHelper().CreateImage(path);
	}

	public bool Show()
	{
		return View.ShowDialog() ?? false;
	}
}

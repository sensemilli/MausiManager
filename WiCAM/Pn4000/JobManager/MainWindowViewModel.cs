using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser.ArchiveN2d;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool;
using WiCAM.Pn4000.JobManager.Helpers;
using WiCAM.Pn4000.Materials;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager
{
	public class MainWindowViewModel : ViewModelBase, WiCAM.Pn4000.JobManager.IDialogViewModel, IPreviewObserver
	{
		private IJobManagerServiceProvider _provider;

		private IJobManagerSettings _settings;

		private IJobDataViewModel _jobDataModel;
        public IAuftragsDataViewModel _auftragsDataModel;
        private IAutoLoopViewModel _autoLoopModel;
        private ICommonCutViewModel _commonCutModel;
		public static MainWindowViewModel Instance { get; private set; }
        public static MainWindowViewModel _MainWindowViewModel;

        private MaterialWindowViewModel _MatWin;
        public static bool _MatWindow;
        public void ChangeFlyoutState()
        {
            MainWindow.mainWindow.SetFlyoutState();
        }

        private ObservableCollection<ArchiveInfo> _archivesCollection = new ObservableCollection<ArchiveInfo>();
        private ObservableCollection<ArchiveInfo> _selectedNodes = new ObservableCollection<ArchiveInfo>();
        public ObservableCollection<NcPartInfo> ArchiveContent
        {
            get
            {
                return NcPartArchivController.Instance.ArchiveContent;
                
            }
            set
            {

            }
        }
        public ObservableCollection<StockMaterialInfo> MaterialsCollection
        {
			get { return _MatWin.MaterialsCollection; }
			set { }
         
        }
        public  FilterHelper<StockMaterialInfo> _filterHelper
        {
            get { return _MatWin._filterHelper; }
            set { }

        }

        private FilterCriteria _filterCriteria;

        public bool IsDatabaseActive
        {
			get { return _MatWin.IsDatabaseActive; }
			set { }
        }
        public StockMaterialInfo SelectedMaterial
        {
            get { return _MatWin.SelectedMaterial; }
            set { _MatWin.SelectedMaterial = value; }
        }
        public bool IsBevorzugtSelected
        {
            get { return _MatWin.IsBevorzugtSelected; }
            set { _MatWin.IsBevorzugtSelected = value; }
        }
        public bool IsZuschnittSelected
        {
            get { return _MatWin.IsZuschnittSelected; }
            set { _MatWin.IsZuschnittSelected = value; }
        }
        public bool IsNormalSelected
        {
            get { return _MatWin.IsNormalSelected; }
            set { _MatWin.IsNormalSelected = value; }
        }
        public bool IsCoilSelected
        {
            get { return _MatWin.IsCoilSelected; }
            set { _MatWin.IsCoilSelected = value; }
        }

        public bool IsRestPlateSelected
        {
            get { return _MatWin.IsRestPlateSelected; }
            set { _MatWin.IsRestPlateSelected = value; }
        }
        public string ItemsTypeText
        {
            get { return _MatWin.ItemsTypeText; }
            set { _MatWin.ItemsTypeText = value; }
        }
        public string StatusText
        {
            get { return _MatWin.StatusText; }
            set { _MatWin.StatusText = value; }
        }
        public Brush DeletedColor
        {
            get { return _MatWin.DeletedColor; }
            set { _MatWin.DeletedColor = value; }
        }
        public bool ShowDeletedItems
        {
            get { return _MatWin.ShowDeletedItems; }
            set { _MatWin.ShowDeletedItems = value; }
        }
        public Dictionary<string, string> FieldNames
        {
            get
            {
                return _MatWin._view.filter.FieldNames;
            }
            set
            {
                _MatWin._view.filter.FieldNames = value;
            }
        }
        public Visibility DateFilterVisibility
        {
            get
            {
                return _filterCriteria.DateFilterVisibility;
            }
            set
            {
                _MatWin._filterCriteria.DateFilterVisibility = value;
                NotifyPropertyChanged("DateFilterVisibility");
            }
        }

        public ICommand ShowHideDebugCommand
        {
            get
            {
                if (this._ShowHideDebugCommand == null)
                {
                    this._ShowHideDebugCommand = new RelayCommand((object x) => this.ShowHideDebug());
                }
                return this._ShowHideDebugCommand;
            }
        }

        private void ShowHideDebug()
        {
            if (!debugIsVisible)
            {
                debugIsVisible = true;
                ConsoleExtension.Show();
            }
            else
            {
                debugIsVisible = false;
                ConsoleExtension.Hide();
            }
        }


        public bool IsFlyoutOpen
        {
            get
            {
                return this._isFlyoutOpen;
            }
            set
            {
                _isFlyoutOpen = value;
            }
        }

        private IFilterViewModel _filterViewJobModel;

		private IFilterViewModel _filterViewPlateModel;

		private IFilterViewModel _filterViewPartsModel;

		private MachinesControlViewModel _machinesViewModel;

		private SettingsControlViewModel _settingsControlViewModel;

		private string _WindowName;

		private BitmapImage _selectedImage;

		private int _totalJobs;

		private int _readCounter;

		private int _readJobs;

		private Visibility _statusVisibility;

		private GridLength _column1Width;
        
		private WiCAM.Pn4000.JobManager.IDialogView JustDecompileGenerated_View_k__BackingField;
        private GridLength _gridFoldersHeight;
        private GridLength _gridFilesHeight;
        GridLength _gridArchiveHeight;
        private ICommand _filterDeleteCommand;

		private ICommand _openIniFileCommand;

		private ICommand _openLogFileCommand;

		private ICommand _exitCommand;

		private ICommand _showTemplateCommand;
        private ICommand _ShowHideDebugCommand;
        private bool debugIsVisible = false;
        private bool _isFlyoutOpen;
        public ICommand RibbonButtonOrdnerZuExel => this._auftragsDataModel.RibbonButtonOrdnerZuExelCommand;
        public ICommand ButtonSelectArchivCommand => this._auftragsDataModel.ButtonSelectArchivCommand;
        public ICommand ButtonReadMaterialCSVCommand => this._auftragsDataModel.ButtonReadMaterialCSVCommand;
        public ICommand ButtonWriteToMaterialDBCommand => this._auftragsDataModel.ButtonWriteToMaterialDBCommand;

        public ICommand ButtonPnArchivBrowserCommand => this._auftragsDataModel.ButtonPnArchivBrowserCommand;
        public ICommand ButtonPnArchivSelectedBrowserCommand => this._auftragsDataModel.ButtonPnArchivBrowserSelectedCommand;
        public ICommand ButtonCreateMaterialPrintExcelCommand => this._auftragsDataModel.ButtonCreateMaterialPrintExcelCommand;
        public ICommand SaveFPRedit => this._autoLoopModel.SaveFPRedit;
        public ICommand FPR996toggleCommand => this._autoLoopModel.FPR996toggleCommand;
        public ICommand FPR997toggleCommand => this._autoLoopModel.FPR997toggleCommand;
        public ICommand FPR998toggleCommand => this._autoLoopModel.FPR998toggleCommand;
        public ICommand FPR999toggleCommand => this._autoLoopModel.FPR999toggleCommand;
        public ICommand LageCommand => this._autoLoopModel.LageCommand;
        public ICommand XrichtenCommand => this._autoLoopModel.XrichtenCommand;
        public ICommand YrichtenCommand => this._autoLoopModel.YrichtenCommand;
        public ICommand TrimmenCommand => this._autoLoopModel.TrimmenCommand;
        public ICommand GleicheDCommand => this._autoLoopModel.GleicheDCommand;
        public ICommand CADdeleteCommand => this._autoLoopModel.CADdeleteCommand;
        public ICommand GravurCommand => this._autoLoopModel.GravurCommand;
        public ICommand StanzenInnenCommand => this._autoLoopModel.StanzenInnenCommand;
        public ICommand KonturAussenCommand => this._autoLoopModel.KonturAussenCommand;
        public ICommand FlaecheAussenCommand => this._autoLoopModel.FlaecheAussenCommand;
        public ICommand StanzenAutoCommand => this._autoLoopModel.StanzenAutoCommand;
        public ICommand FlaecheStanzenCommand => this._autoLoopModel.FlaecheStanzenCommand;
        public ICommand WKZaendernCommand => this._autoLoopModel.WKZaendernCommand;
        public ICommand SortierenCommand => this._autoLoopModel.SortierenCommand;
        public ICommand NCdeleteCommand => this._autoLoopModel.NCdeleteCommand;
        public ICommand ShowLeftFlyoutCommandMain => this._autoLoopModel.ShowLeftFlyoutCommandMain;
        public ICommand MultiShearCommand => this._commonCutModel.MultiShearCommand;
        public ICommand StandartCommand => this._commonCutModel.StandartCommand;
        public ICommand SechsundfuenzigCommand => this._commonCutModel.SechsundfuenzigCommand;
        public ICommand SechsundvierzigCommand => this._commonCutModel.SechsundvierzigCommand;
        public ICommand DreissigCommand => this._commonCutModel.DreissigCommand;

        public ICommand SortXCommand => this._commonCutModel.SortXCommand;

        public ICommand SelectVerticalTrimTool762x5Command => this._commonCutModel.SelectVerticalTrimTool762x5Command;
        public ICommand SelectVerticalTrimTool76x5Command => this._commonCutModel.SelectVerticalTrimTool76x5Command;
        public ICommand SelectVerticalTrimTool56x5Command => this._commonCutModel.SelectVerticalTrimTool56x5Command;
        public ICommand SelectVerticalTrimTool46x5Command => this._commonCutModel.SelectVerticalTrimTool46x5Command;
        public ICommand SelectVerticalTrimTool30x5Command => this._commonCutModel.SelectVerticalTrimTool30x5Command;
        public ICommand SelectVerticalTrimTool15x5Command => this._commonCutModel.SelectVerticalTrimTool15x5Command;

        public ICommand SelectTrimTool762x5Command => this._commonCutModel.SelectTrimTool762x5Command;
        public ICommand SelectTrimTool76x5Command => this._commonCutModel.SelectTrimTool76x5Command;
        public ICommand SelectTrimTool56x5Command => this._commonCutModel.SelectTrimTool56x5Command;
        public ICommand SelectTrimTool46x5Command => this._commonCutModel.SelectTrimTool46x5Command;
        public ICommand SelectTrimTool30x5Command => this._commonCutModel.SelectTrimTool30x5Command;
        public ICommand SechundsiebzigzusechsundsiebzigzweiCommand => this._commonCutModel.SechundsiebzigzusechsundsiebzigzweiCommand;
        public ICommand WLASTCommand => this._commonCutModel.WLASTCommand;
        public ICommand ReadExcel3DCommand => this._auftragsDataModel.ReadExcel3DCommand;
        public ICommand WriteExcel3DCommand => this._auftragsDataModel.WriteExcel3DCommand;
        public ICommand ReadExcel2DCommand => this._auftragsDataModel.ReadExcel2DCommand;
        public ICommand AbgleichCommand => this._auftragsDataModel.AbgleichCommand;
        public ICommand Excel2SwiftCommand => this._auftragsDataModel.Excel2SwiftCommand;

        public ICommand WriteExcel2DCommand => this._auftragsDataModel.WriteExcel2DCommand;
        public ICommand AddAuftragCommand => this._auftragsDataModel.AddAuftragCommand;

        public ICommand ToggleAuftragCommand => this._auftragsDataModel.ToggleAuftragCommand;
        public ICommand ToggleTeileCommand => this._auftragsDataModel.ToggleTeileCommand;
        public ICommand ToggleMaterialCommand => this._auftragsDataModel.ToggleMaterialCommand;


        public ICommand CancelCommand => AddAuftragControlViewModel._AddAuftragControlViewModel.CancelCommand;
        public ICommand CopyCommand => this._MatWin.CopyCommand;
        public ICommand AddCommand => this._MatWin.AddCommand;

        public ICommand EditCommand => this._MatWin.EditCommand;
        public ICommand DeleteCommand => this._MatWin.DeleteCommand;
        public ICommand ShowDeletedCommand => this._MatWin.ShowDeletedCommand;
        public ICommand SettingsCommand => this._MatWin.SettingsCommand;
        public ICommand SaveGmpoolCommand => this._MatWin.SaveGmpoolCommand;
        public ICommand SaveCsvCommand => this._MatWin.SaveCsvCommand;
        public ICommand RestorePlateCommand => this._MatWin.RestorePlateCommand;
        public ICommand OkCommand => this._MatWin.OkCommand;
        public ICommand AddRestPlateCommand => this._MatWin.AddRestPlateCommand;

        public Visibility RestoreButtonVisibility => this._MatWin.RestoreButtonVisibility;
        public Visibility AddRestPlateVisibility => this._MatWin.AddRestPlateVisibility;
        public Visibility AddButtonVisibility => this._MatWin.AddButtonVisibility;

        public FrameworkElement ActiveView
        {
            get
            {
                return (FrameworkElement)_MatWin.GetValue(MaterialWindowViewModel.ActiveViewProperty);
            }
            set
            {
                _MatWin.SetValue(MaterialWindowViewModel.ActiveViewProperty, value);
            }
        }

        public ObservableCollection<ArchiveInfo> ArchivesCollection
        {
            get
            {
                return WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance._model.ArchivesCollection;
            }
            set
            {
                _archivesCollection = value;
                NotifyPropertyChanged("ArchivesCollection");
            }
        }

        public GridLength Column1Width
		{
			get
			{
				return this._column1Width;
			}
			set
			{
				this._column1Width = value;
				base.NotifyPropertyChanged("Column1Width");
			}
		}

        public GridLength GridFoldersHeight
        {
            get
            {
                return _auftragsDataModel.GridFoldersHeight;
            }
            set
            {
                _auftragsDataModel.GridFoldersHeight = value;
                base.NotifyPropertyChanged("_gridFoldersHeight");
            }
        }

        public GridLength GridFilesHeight
        {
            get
            {
                return _auftragsDataModel.GridFilesHeight;
            }
            set
            {
                _auftragsDataModel.GridFilesHeight = value;
                base.NotifyPropertyChanged("_gridFilesHeight");
            }
        }

        public GridLength GridArchiveHeight
        {
            get
            {
                return _auftragsDataModel.GridArchiveHeight;
            }
            set
            {
                _auftragsDataModel.GridArchiveHeight = value;
                base.NotifyPropertyChanged("_gridArchiveHeight");
            }
        }

        public ICommand DeleteJobCommand
		{
			get
			{
				return this._jobDataModel.DeleteJobCommand;
			}
		}

		public ICommand DeleteProducedJobsCommand
		{
			get
			{
				return this._jobDataModel.DeleteProducedJobsCommand;
			}
		}

		public ICommand ExitCommand
		{
			get
			{
				if (this._exitCommand == null)
				{
					this._exitCommand = new RelayCommand((object x) => this.Exit(), (object x) => true);
				}
				return this._exitCommand;
			}
		}

		public ICommand FilterDeleteCommand
		{
			get
			{
				if (this._filterDeleteCommand == null)
				{
					this._filterDeleteCommand = new RelayCommand((object x) => this.FilterDelete(), (object x) => true);
				}
				return this._filterDeleteCommand;
			}
		}

		public Visibility IsSettingsVisible
		{
			get
			{
				if (!this._settings.IsSettingsVisible)
				{
					return Visibility.Hidden;
				}
				return Visibility.Visible;
			}
			set
			{
				this._settings.IsSettingsVisible = value == Visibility.Visible;
				base.NotifyPropertyChanged(" IsSettingsVisible");
			}
		}

		public bool IsTouchScreen
		{
			get
			{
				return this._settings.IsTouchScreen;
			}
			set
			{
				this._settings.IsTouchScreen = value;
				this._jobDataModel.FontSize = (double)((value ? 25 : 12));
			}
		}

		public ICommand OpenIniFileCommand
		{
			get
			{
				if (this._openIniFileCommand == null)
				{
					this._openIniFileCommand = new RelayCommand((object x) => this.OpenIniFile());
				}
				return this._openIniFileCommand;
			}
		}

		public ICommand OpenLogFileCommand
		{
			get
			{
				if (this._openLogFileCommand == null)
				{
					this._openLogFileCommand = new RelayCommand((object x) => this.OpenLogFile());
				}
				return this._openLogFileCommand;
			}
		}

		public ICommand PrintJobLabelsCommand
		{
			get
			{
				return this._jobDataModel.PrintJobLabelsCommand;
			}
		}

		public ICommand PrintPartLabelsCommand
		{
			get
			{
				return this._jobDataModel.PrintPartLabelsCommand;
			}
		}

		public ICommand PrintPlateLabelsCommand
		{
			get
			{
				return this._jobDataModel.PrintPlateLabelsCommand;
			}
		}

		public ICommand ProduceJobCommand
		{
			get
			{
				return this._jobDataModel.ProduceJobCommand;
			}
		}

		public ICommand ProducePlateCommand
		{
			get
			{
				return this._jobDataModel.ProducePlateCommand;
			}
		}

		public int ReadJobs
		{
			get
			{
				return this._readJobs;
			}
			set
			{
				this._readJobs = value;
				base.NotifyPropertyChanged("ReadJobs");
			}
		}

		public ICommand RejectPartCommand
		{
			get
			{
				return this._jobDataModel.RejectPartCommand;
			}
		}

		public ICommand ReloadJobsCommand
		{
			get
			{
				return this._jobDataModel.ReloadJobsCommand;
			}
		}
        public ICommand ReloadWithFinishedJobsCommand => this._jobDataModel.ReloadWithFinishedJobsCommand;


        public ICommand SaveProducedJobsCommand
		{
			get
			{
				return this._jobDataModel.SaveProducedJobsCommand;
			}
		}

		public ICommand SaveSettingsCommand
		{
			get
			{
				return this._settingsControlViewModel.SaveCommand;
			}
		}

		public BitmapImage SelectedImage
		{
			get
			{
				return this._selectedImage;
			}
			set
			{
				this._selectedImage = value;
				base.NotifyPropertyChanged("SelectedImage");
			}
		}

		public ICommand ShowTemplateCommand
		{
			get
			{
				if (this._showTemplateCommand == null)
				{
					this._showTemplateCommand = new RelayCommand((object x) => this.ShowTemplate());
				}
				return this._showTemplateCommand;
			}
		}

		public Visibility StatusVisibility
		{
			get
			{
				return this._statusVisibility;
			}
			set
			{
				this._statusVisibility = value;
				base.NotifyPropertyChanged("StatusVisibility");
			}
		}

		public ICommand StornoPlateCommand
		{
			get
			{
				return this._jobDataModel.StornoPlateCommand;
			}
		}

		public int TotalJobs
		{
			get
			{
				return this._totalJobs;
			}
			set
			{
				this._totalJobs = value;
				base.NotifyPropertyChanged("TotalJobs");
			}
		}

		public WiCAM.Pn4000.JobManager.IDialogView View
		{
			get
			{
				return JustDecompileGenerated_get_View();
			}
			set
			{
				JustDecompileGenerated_set_View(value);
			}
		}

		

        public WiCAM.Pn4000.JobManager.IDialogView JustDecompileGenerated_get_View()
		{
			return this.JustDecompileGenerated_View_k__BackingField;
		}

		private void JustDecompileGenerated_set_View(WiCAM.Pn4000.JobManager.IDialogView value)
		{
			this.JustDecompileGenerated_View_k__BackingField = value;
		}

		public string WindowName
		{
			get
			{
				return this._WindowName;
			}
			set
			{
				this._WindowName = value;
				base.NotifyPropertyChanged("WindowName");
			}
		}

		public MainWindowViewModel()
		{
			_MainWindowViewModel = this;
            IsFlyoutOpen = false;
        }

        private void ChangeJobsReadProgress(int total)
		{
			if (Application.Current.Dispatcher != null)
			{
				if (!Application.Current.Dispatcher.CheckAccess())
				{
					Application.Current.Dispatcher.BeginInvoke(new Action<int>(this.ChangeJobsReadProgress), DispatcherPriority.Send, new object[] { total });
				}
				else
				{
					if (total > 0)
					{
						this.TotalJobs = total;
						this.ReadJobs = 0;
						this._readCounter = 0;
						return;
					}
					this._readCounter++;
					if (this._readCounter % 1000 == 0)
					{
						this.ReadJobs = this._readCounter;
						return;
					}
					if (this._readCounter == this.TotalJobs)
					{
						this.ReadJobs = this.TotalJobs;
						return;
					}
				}
			}
		}

		private void Exit()
		{
			this.View.Close();
			Application.Current.Shutdown(0);
		}

		private void FilterDelete()
		{
			this._filterViewJobModel.ResetFilters();
			this._filterViewPlateModel.ResetFilters();
			this._filterViewPartsModel.ResetFilters();
		}

		public void Initialize(WiCAM.Pn4000.JobManager.IDialogView view, IJobManagerServiceProvider provider)
		{
			this.View = view;
			this._provider = provider;
			string str = Application.Current.Resources["MainWindow"].ToString();
			string str1 = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		//	this.WindowName = string.Concat(str, new string(' ', 20), "Version : ", str1);
			this._settings = this._provider.FindService<IJobManagerSettings>();
			this.Column1Width = new GridLength((double)this._settings.Column1Width, GridUnitType.Pixel);
			IStateManager stateManager = this._provider.FindService<IStateManager>();
			MainWindow mainWindow = this.View as MainWindow;
			if (mainWindow != null)
			{
				this._jobDataModel = this._settings.ModelManager.Register<JobDataControl, JobDataViewModel>(mainWindow.jobDataControl, this._provider) as IJobDataViewModel;
                this._auftragsDataModel = this._settings.ModelManager.Register<AuftragsDataControl, AuftragsDataViewModel>(mainWindow.auftragsDataControl, this._provider) as IAuftragsDataViewModel;
                this._autoLoopModel = this._settings.ModelManager.Register<AutoLoopControl, AutoLoopViewModel>(mainWindow.autoLoopControl, this._provider) as IAutoLoopViewModel;
                this._commonCutModel = this._settings.ModelManager.Register<CommonCutControl, CommonCutViewModel>(mainWindow.commonCutControl, this._provider) as ICommonCutViewModel;
                _MatWin = new MaterialWindowViewModel();

				_MatWin.AreButtonsEnabled = !ApplicationConfigurationInfo.Instance.IsReadOnly();
				_MatWin._grid = mainWindow.gridData;
				this.MaterialsCollection = _MatWin.MaterialsCollection;
				this._filterHelper = _MatWin._filterHelper;
				this._filterCriteria = (new FilterCriteria(WiCAM.Pn4000.Gmpool.CS.GmpoolBrowser, WiCAM.Pn4000.Gmpool.CS.MaterialsFilter)).ReadConfiguration();

				this.IsDatabaseActive = WiCAM.Pn4000.Materials.DataManager.Instance.IsInitialised;
				int num = 1;
				bool flagbool = false;
				if (num == 1)
					flagbool = true;
				bool flag = flagbool;// (bool)num;

				bool flag1 = flag;
				bool flag2 = flag1;

				bool flag3 = flag2;
				bool flag4 = flag3;
				_MatWin.Show();
                this.WindowName = string.Concat("Mausi", new string(' ', 20), "Version : ", "1.2");

                //    this._filter.FilterTextChanged += new RoutedEventHandler(this.FilterTextChanged);
                this._provider.JobFilter = this._jobDataModel;
				this._provider.PartFilter = this._jobDataModel;
				this._provider.PlateFilter = this._jobDataModel;
				this._filterViewJobModel = this._settings.ModelManager.Register<FilterControl, FilterControlViewModel>(mainWindow.filterControlJobs, this._provider) as IFilterViewModel;
				this._filterViewPlateModel = this._settings.ModelManager.Register<FilterControl, FilterPlateControlViewModel>(mainWindow.filterControlPlates, this._provider) as IFilterViewModel;
				this._filterViewPartsModel = this._settings.ModelManager.Register<FilterControl, FilterPartControlViewModel>(mainWindow.filterControlParts, this._provider) as IFilterViewModel;
				mainWindow.Loaded += new RoutedEventHandler(this.Wnd_Loaded);
				mainWindow.Closing += new CancelEventHandler(this.Wnd_Closing);
				mainWindow.txtBarcodeJob.KeyUp += new KeyEventHandler(this.TxtBarcodeJob_PreviewKeyUp);
				mainWindow.txtBarcodePlate.KeyUp += new KeyEventHandler(this.TxtBarcodePlate_PreviewKeyUp);
				stateManager.AttachMachineObserver(this._jobDataModel);
				stateManager.AttachImageObserver(this);
				stateManager.AttachJobFilterObserver(this._jobDataModel);
				stateManager.AttachPartFilterObserver(this._jobDataModel);
				this._machinesViewModel = this._settings.ModelManager.Register<MachinesControl, MachinesControlViewModel>(mainWindow.machinesControl, this._provider) as MachinesControlViewModel;
				if (this._machinesViewModel != null)
				{
					this._machinesViewModel.AttachStateManager(stateManager);
					stateManager.NotifyMachinesChanged(this._settings.Machines);
				}
				this._settingsControlViewModel = new SettingsControlViewModel(this._settings, mainWindow.settingsControl.configurableListControl);
				mainWindow.settingsControl.DataContext = this._settingsControlViewModel;
			}
			this.StatusVisibility = Visibility.Visible;
			if (this._jobDataModel != null)
			{
				this._jobDataModel.FontSize = (double)((this.IsTouchScreen ? 25 : 12));
			}

            ConsoleExtension.Hide();
        }

        private void OpenFile(string path)
		{
			if (IOHelper.FileExists(path))
			{
				Process process = new Process();
				process.StartInfo.FileName = path;
				process.Start();
			}
		}

		private void OpenIniFile()
		{
			this.OpenFile(PnPathBuilder.PathInPnHome(new object[] { SettingsInfo.IniName }));
		}

		private void OpenLogFile()
		{
			this.OpenFile(Logger.LogFileName);
		}

		public void PreviewChanged(string path)
		{
			this.SelectedImage = null;
			this.SelectedImage = (new ImageFromMemoryHelper()).CreateImage(path);
		}

		public bool Show()
		{
			return this.View.ShowDialog().GetValueOrDefault(false);
		}

		private void ShowTemplate()
		{
			this.OpenFile(PnPathBuilder.PathInPnDrive(new object[] { "u\\pn\\gfiles\\JobManagerTemplates.txt" }));
		}

		private void TxtBarcodeJob_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				TextBox textBox = sender as TextBox;
				if (textBox != null)
				{
					if (!this._jobDataModel.HasToIgnoreEnter)
					{
						this._jobDataModel.ProduceJobUsingBarcode(textBox.Text);
					}
					else
					{
						this._jobDataModel.HasToIgnoreEnter = false;
					}
					textBox.SelectAll();
				}
			}
		}

		private void TxtBarcodePlate_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				TextBox textBox = sender as TextBox;
				if (textBox != null)
				{
					if (!this._jobDataModel.HasToIgnoreEnter)
					{
						this._jobDataModel.ProducePlateUsingBarcode(textBox.Text);
					}
					else
					{
						this._jobDataModel.HasToIgnoreEnter = false;
					}
					textBox.SelectAll();
				}
			}
		}

		private void Wnd_Closing(object sender, CancelEventArgs e)
		{
			this._jobDataModel.SaveSettings();
			this._settings.Column1Width = (int)this.Column1Width.Value;
            this._settings.GridFoldersHeight = (int)this.GridFoldersHeight.Value;
            this._settings.GridFilesHeight = (int)this.GridFilesHeight.Value;
            this._settings.GridArchiveHeight = (int)this.GridArchiveHeight.Value;
            this._settings.SaveConfiguration(this.View as Window);
		}

		private void Wnd_Loaded(object sender, RoutedEventArgs e)
		{
			this._settings.ApplyWindowConfiguration(this.View as Window);
			this._jobDataModel.LoadJobs();
		}
	}

    static class ConsoleExtension
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        readonly static IntPtr handle = GetConsoleWindow();
        [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Hide()
        {
            ShowWindow(handle, SW_HIDE); //hide the console
        }
        public static void Show()
        {
            ShowWindow(handle, SW_SHOW); //show the console
        }
    }
}
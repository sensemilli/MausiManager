using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Classes;
using WiCAM.Pn4000.Jobdata.Helpers;
using WiCAM.Pn4000.JobManager.Cora;
using WiCAM.Pn4000.JobManager.Helpers;
using WiCAM.Pn4000.JobManager.LabelPrinter;
using WiCAM.Pn4000.JobManager.ViewModels;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Machine;
using WiCAM.Pn4000.WpfControls;
using Style = System.Windows.Style;

namespace WiCAM.Pn4000.JobManager;

public class JobDataViewModel : ViewModelBase, IJobDataViewModel, IViewModel, IMachineStateObserver, IFilter, IFilterPlates, IFilterParts, IPreviewObserver
{

    private Microsoft.Office.Interop.Excel.Range xlrange;
    private Microsoft.Office.Interop.Excel.Range xlrange2;
    private ICommand _AddToBlechOptCommand;
    private IJobManagerServiceProvider _provider;

	private IJobManagerSettings _settings;

	private IStateManager _stateManager;

	private DataGridManager<JobInfo> _gridJobController;

	private DataGridManager<PlateInfo> _gridPlateController;

	private DataGridManager<PartInfo> _gridPartController;

	private ReadJobStrategyInfo _strategy;

	private List<JobInfo> _originalJobs = new List<JobInfo>();

	private double _fontSize = 12.0;

	private int _jobsAmount;

	private string _jobsPath;

	private GridLength _gridJobHeight;

	private GridLength _gridPlateHeight;

	private int _maxJobsAmount = 100000;

	private int _readJobsAmount;

	private Visibility _progressVisibility = Visibility.Hidden;

	private int _jobsFiltered;

	private ICommand _showSumOfPartsCommand;

	private ICommand _deleteJobCommand;

	private ICommand _deleteProducedJobsCommand;

	private ICommand _saveProducedJobsCommand;

	private ICommand _reloadJobsCommand;

	private ICommand _produceJobCommand;

	private ICommand _producePlateCommand;

	private ICommand _stornoPlateCommand;

	private ICommand _resetPlateCommand;

	private ICommand _rejectPartCommand;

	private ICommand _jobOpenCommand;

	private ICommand _printJobLabelsCommand;

	private ICommand _printPlateLabelsCommand;

	private ICommand _printPartLabelsCommand;

	private bool _isUpdateFromPart;
    private ICommand _reloadWithFinishedJobsCommand;
    private string partsamount;
    private int ii;
    private bool exporting;
    private ICommand _exportCommand;
    private RelayCommand _freigabeLoeschenCommand;

    public IView View { get; private set; }

	public ObservableCollection<JobInfo> Jobs { get; set; } = new ObservableCollection<JobInfo>();


	public ObservableCollection<PlateInfo> Plates { get; set; } = new ObservableCollection<PlateInfo>();


	public ObservableCollection<PartInfo> Parts { get; set; } = new ObservableCollection<PartInfo>();


	public bool HasToIgnoreEnter { get; set; }

	public double FontSize
	{
		get
		{
			return _fontSize;
		}
		set
		{
			_fontSize = value;
			NotifyPropertyChanged("FontSize");
		}
	}

	public int JobsAmount
	{
		get
		{
			return _jobsAmount;
		}
		set
		{
			_jobsAmount = value;
			NotifyPropertyChanged("JobsAmount");
		}
	}

	public string JobsPath
	{
		get
		{
			return _jobsPath;
		}
		set
		{
			_jobsPath = value;
			NotifyPropertyChanged("JobsPath");
		}
	}

	public GridLength GridJobHeight
	{
		get
		{
			return _gridJobHeight;
		}
		set
		{
			_gridJobHeight = value;
			NotifyPropertyChanged("GridJobHeight");
		}
	}

	public GridLength GridPlateHeight
	{
		get
		{
			return _gridPlateHeight;
		}
		set
		{
			_gridPlateHeight = value;
			NotifyPropertyChanged("GridPlateHeight");
		}
	}

	public int MaxJobsAmount
	{
		get
		{
			return _maxJobsAmount;
		}
		set
		{
			_maxJobsAmount = value;
			NotifyPropertyChanged("MaxJobsAmount");
		}
	}

	public int ReadJobsAmount
	{
		get
		{
			return _readJobsAmount;
		}
		set
		{
			_readJobsAmount = value;
			NotifyPropertyChanged("ReadJobsAmount");
		}
	}

	public Visibility ProgressVisibility
	{
		get
		{
			return _progressVisibility;
		}
		set
		{
			_progressVisibility = value;
			NotifyPropertyChanged("ProgressVisibility");
		}
	}

	public int JobsFiltered
	{
		get
		{
			return _jobsFiltered;
		}
		set
		{
			_jobsFiltered = value;
			NotifyPropertyChanged("JobsFiltered");
		}
	}

	public ICommand ShowSumOfPartsCommand
	{
		get
		{
			if (_showSumOfPartsCommand == null)
			{
				_showSumOfPartsCommand = new RelayCommand(delegate(object x)
				{
					ShowSumOfParts(x);
				}, (object x) => CanShowSumOfParts());
			}
			return _showSumOfPartsCommand;
		}
	}

	public ICommand DeleteJobCommand
	{
		get
		{
			if (_deleteJobCommand == null)
			{
				_deleteJobCommand = new RelayCommand(delegate
				{
					DeleteJob();
				}, (object x) => CanDeleteJob());
			}
			return _deleteJobCommand;
		}
	}

	public ICommand DeleteProducedJobsCommand
	{
		get
		{
			if (_deleteProducedJobsCommand == null)
			{
				_deleteProducedJobsCommand = new RelayCommand(delegate
				{
					DeleteProducedJobs();
				}, (object x) => CanDeleteProducedJobs());
			}
			return _deleteProducedJobsCommand;
		}
	}

	public ICommand SaveProducedJobsCommand
	{
		get
		{
			if (_saveProducedJobsCommand == null)
			{
				_saveProducedJobsCommand = new RelayCommand(delegate
				{
					SaveProducedJobs();
				}, (object x) => CanSaveProducedJobs());
			}
			return _saveProducedJobsCommand;
		}
	}

	public ICommand ReloadJobsCommand
	{
		get
		{
			if (_reloadJobsCommand == null)
			{
				_reloadJobsCommand = new RelayCommand(delegate
				{
					ReloadJobs();
				}, (object x) => true);
			}
			return _reloadJobsCommand;
		}
	}

    public ICommand ReloadWithFinishedJobsCommand
    {
        get
        {
            if (this._reloadWithFinishedJobsCommand == null)
                this._reloadWithFinishedJobsCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ReloadWithFinishedJobs()), (Predicate<object>)(x => true));
            return this._reloadWithFinishedJobsCommand;
        }
    }

    public void ReloadWithFinishedJobs()
    {
        Logger.Info("ReloadWithFinishedJobsCommand : {0}", (object)DateTime.Now.ToString("s"));
        this.LoadWithFinishedJobs();
    }

    public void LoadWithFinishedJobs()
    {
        Console.Write("LoadSavedJobs");
        this._settings.JobDataPath = PnPathBuilder.ArDrive + "\\u\\sfa\\jobdata_save";
        JobsPath = this._settings.JobDataPath;

        this.ProgressVisibility = Visibility.Visible;
        Stopwatch sw = Stopwatch.StartNew();
        Task.Run<List<JobInfo>>((Func<List<JobInfo>>)(() => this.ReadJobs(this._settings.JobDataPath))).ContinueWith((Action<Task<List<JobInfo>>>)(t =>
        {
            sw.Stop();
            Logger.Verbose("Read Jobs : {0}", (object)sw.ElapsedMilliseconds);
            this.ShowJobs(t);
        }));
    }


    public ICommand ProduceJobCommand
	{
		get
		{
			if (_produceJobCommand == null)
			{
				_produceJobCommand = new RelayCommand(delegate
				{
					ProduceJob();
				}, (object x) => CanProduceJob());
			}
			return _produceJobCommand;
		}
	}

	public ICommand ProducePlateCommand
	{
		get
		{
			if (_producePlateCommand == null)
			{
				_producePlateCommand = new RelayCommand(delegate
				{
					ProducePlate();
				}, (object x) => CanProducePlate());
			}
			return _producePlateCommand;
		}
	}

	public ICommand StornoPlateCommand
	{
		get
		{
			if (_stornoPlateCommand == null)
			{
				_stornoPlateCommand = new RelayCommand(delegate
				{
					StornoPlate();
				}, (object x) => CanStornoPlate());
			}
			return _stornoPlateCommand;
		}
	}

	public ICommand ResetPlateCommand
	{
		get
		{
			if (_resetPlateCommand == null)
			{
				_resetPlateCommand = new RelayCommand(delegate
				{
					ResetPlate();
				}, (object x) => CanResetPlate());
			}
			return _resetPlateCommand;
		}
	}

	public ICommand RejectPartCommand
	{
		get
		{
			if (_rejectPartCommand == null)
			{
				_rejectPartCommand = new RelayCommand(delegate
				{
					RejectPart();
				}, (object x) => CanRejectPart());
			}
			return _rejectPartCommand;
		}
	}

	public ICommand JobOpenCommand
	{
		get
		{
			if (_jobOpenCommand == null)
			{
				_jobOpenCommand = new RelayCommand((Action<object>)delegate
				{
					JobOpen();
				});
			}
			return _jobOpenCommand;
		}
	}
    public ICommand ExportCommand
    {
        get
        {
            if (_exportCommand == null)
            {
                _exportCommand = new RelayCommand((Action<object>)delegate
                {
                    exporting = true;
                    LoadJobs();
                });
            }
            return _exportCommand;
        }
    }
    public ICommand PrintJobLabelsCommand
	{
		get
		{
			if (_printJobLabelsCommand == null)
			{
				_printJobLabelsCommand = new RelayCommand(delegate
				{
					PrintJobLabels();
				}, (object x) => CanPrintJobLabels());
			}
			return _printJobLabelsCommand;
		}
	}

	public ICommand PrintPlateLabelsCommand
	{
		get
		{
			if (_printPlateLabelsCommand == null)
			{
				_printPlateLabelsCommand = new RelayCommand(delegate
				{
					PrintPlateLabels();
				}, (object x) => CanPrintPlateLabels());
			}
			return _printPlateLabelsCommand;
		}
	}

	public ICommand PrintPartLabelsCommand
	{
		get
		{
			if (_printPartLabelsCommand == null)
			{
				_printPartLabelsCommand = new RelayCommand(delegate
				{
					PrintPartLabels();
				}, (object x) => CanPrintPartLabels());
			}
			return _printPartLabelsCommand;
		}
	}

    public ICommand FreigabeLoeschenCommand
    {
        get
        {
            if (_freigabeLoeschenCommand == null)
            {
                _freigabeLoeschenCommand = new RelayCommand(delegate
                {
                    UnblockDeleteJob();
                }, (object x) => true);
            }
            return _freigabeLoeschenCommand;
        }
    }

    private void UnblockDeleteJob()
    {
		if (!_settings.IsJobDeleteEnabled)
			{
			_settings.IsJobDeleteEnabled = true;
			MessageHelper.Information("Freigabe zum Löschen von Jobs wurde erteilt.");
		}
		else
		{
			_settings.IsJobDeleteEnabled = false;
            MessageHelper.Information("Freigabe zum Löschen von Jobs wurde entfernt.");
        }
        CommandManager.InvalidateRequerySuggested();
    }

    private void ShowSumOfParts(object data)
	{
		if (data is IList listOfParts)
		{
			PartSumWindow partSumWindow = new PartSumWindow();
			partSumWindow.DataContext = new PartSumViewModel(listOfParts);
			partSumWindow.ShowDialog();
		}
	}

	private bool CanShowSumOfParts()
	{
		return true;
	}

	private void DeleteJob()
	{
		Logger.Info("DeleteJob");
		if (!EnumerableHelper.IsNullOrEmpty(_gridJobController.SelectedItems) && MessageHelper.Question(FindStringResource("MsgAskDeleteJobs")) == MessageBoxResult.Yes)
		{
			List<JobInfo> jobs = new List<JobInfo>(_gridJobController.SelectedItems);
			DeleteJobs(jobs);
		}
	}

	private void DeleteJobs(IEnumerable<JobInfo> jobs)
	{
		List<Task<bool>> list = new List<Task<bool>>();
		Plates.Clear();
		Parts.Clear();
		foreach (JobInfo job in jobs)
		{
			Logger.Verbose("     Job:{0}", job.JOB_DATA_1);
			IOHelper.DirectoryDelete(Path.GetDirectoryName(job.Path));
			_originalJobs.Remove(job);
			Task<bool> item = Task.Run(() => DeleteJobNcPrograms(job));
			list.Add(item);
		}
		Task[] tasks = list.ToArray();
		Task.WaitAll(tasks);
		foreach (JobInfo job2 in jobs)
		{
			if (Jobs.ToList().Contains(job2))
			{
				Jobs.Remove(job2);
			}
		}
	}

	private bool CanDeleteJob()
	{
		bool flag = _gridJobController.SelectedItems.Count > 0;
		if (flag)
		{
			flag = _settings.IsJobDeleteEnabled;
		}
		return flag;
	}

    private bool CanDeleteJobSave(bool flag)
    {
        if (flag)
        {
           _settings.IsJobDeleteEnabled = true;
        }
		else
		{
		   _settings.IsJobDeleteEnabled = false;
        }
		return flag;
    }

    private void DeleteProducedJobs()
	{
		Logger.Info("DeleteProducedJobs");
		if (MessageHelper.Question(FindStringResource("MsgAskDeleteProducedJobs")) == MessageBoxResult.Yes)
		{
			List<JobInfo> list = _originalJobs.FindAll((JobInfo x) => x.Status == 3 || x.Status == 1);
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				DeleteJobs(list);
			}
		}
	}

	private bool DeleteJobNcPrograms(JobInfo job)
	{
		Logger.Info("DeleteJobNcPrograms : job = {0}", job.JOB_DATA_1);
		int jOB_MACHINE_NO = job.JOB_MACHINE_NO;
		if (!string.IsNullOrEmpty(_settings.PlateDeleteBatchPath))
		{
			PlateBatchActionHelper plateBatchActionHelper = new PlateBatchActionHelper(_provider);
			foreach (PlateInfo plate in job.Plates)
			{
				plateBatchActionHelper.PlateDeleteBatch(job.JOB_DATA_1, plate.PLATE_HEADER_TXT_1, plate.Path, jOB_MACHINE_NO);
			}
		}
		return true;
	}

	private bool CanDeleteProducedJobs()
	{
		bool result = false;
		if (!EnumerableHelper.IsNullOrEmpty(_originalJobs.FindAll((JobInfo x) => x.Status == 3 || x.Status == 1)))
		{
			result = _settings.IsJobDeleteProducedEnabled;
		}
		return result;
	}

	private void SaveProducedJobs()
	{
		Logger.Info("SaveProducedJobs");
		if (MessageHelper.Question(FindStringResource("MsgAskSaveProducedJobs")) != MessageBoxResult.Yes)
		{
			return;
		}
		ControlJobSavePath();
		if (string.IsNullOrEmpty(_settings.JobDataSavePath))
		{
			return;
		}
		Logger.Info("   destination={0}", _settings.JobDataSavePath);
		List<JobInfo> list = _originalJobs.FindAll((JobInfo x) => x.Status == 3 || x.Status == 1);
		if (EnumerableHelper.IsNullOrEmpty(list))
		{
			return;
		}
		Plates.Clear();
		Parts.Clear();
		List<Task<bool>> list2 = new List<Task<bool>>();
		foreach (JobInfo job in list)
		{
			SaveJob(job, _settings.JobDataSavePath);
			Task<bool> item = Task.Run(() => SaveJobNcPrograms(job));
			list2.Add(item);
		}
		Task[] tasks = list2.ToArray();
		Task.WaitAll(tasks);
	}

	private bool SaveJobNcPrograms(JobInfo job)
	{
		Logger.Info("SaveJobNcPrograms : job = {0}", job.JOB_DATA_1);
		int jOB_MACHINE_NO = job.JOB_MACHINE_NO;
		if (!string.IsNullOrEmpty(_settings.PlateDeleteBatchPath))
		{
			PlateBatchActionHelper plateBatchActionHelper = new PlateBatchActionHelper(_provider);
			foreach (PlateInfo plate in job.Plates)
			{
				plateBatchActionHelper.PlateSaveBatch(job.JOB_DATA_1, plate.PLATE_HEADER_TXT_1, plate.Path, jOB_MACHINE_NO);
			}
		}
		return true;
	}

	private void ControlJobSavePath()
	{
		if (!string.IsNullOrEmpty(_settings.JobDataSavePath))
		{
			return;
		}
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.ShowNewFolderButton = true;
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			_settings.JobDataSavePath = folderBrowserDialog.SelectedPath;
		}
	}

	private void SaveJob(JobInfo job, string destination)
	{
		Logger.Verbose("     Job={0}", job.JOB_DATA_1);
		string destination2 = Path.Combine(destination, job.JOB_DATA_1);
		IOHelper.DirectoryCopy(Path.GetDirectoryName(job.Path), destination2, copySubDirectories: true);
		IOHelper.DirectoryDelete(Path.GetDirectoryName(job.Path));
		if (Jobs.ToList().Contains(job))
		{
			Jobs.Remove(job);
		}
		_originalJobs.Remove(job);
	}

	private bool CanSaveProducedJobs()
	{
		bool flag = _originalJobs.FindAll((JobInfo x) => x.Status == 3 || x.Status == 1).Count > 0;
		if (flag)
		{
			flag = _settings.IsJobSaveProducedEnabled;
		}
		return flag;
	}

	private void ReloadJobs()
	{
		Logger.Info("ReloadJobs : {0}", DateTime.Now.ToString("s"));
		LoadJobs();
	}

	private void ProduceJob()
	{
		Logger.Info("ProduceJob");
		if (MessageHelper.Question(FindStringResource("MsgAskProduceJobs")) != MessageBoxResult.Yes)
		{
			return;
		}
		Plates.Clear();
		Parts.Clear();
		foreach (JobInfo selectedItem in _gridJobController.SelectedItems)
		{
			if (Math.Round(selectedItem.JOB_PROGRESS, 1) >= 99.9)
			{
				string format = FindStringResource("MsgJobIsAlreadyProduced");
				MessageHelper.Information(string.Format(CultureInfo.InvariantCulture, format, selectedItem.JOB_DATA_1));
			}
			else
			{
				ProduceJob(selectedItem);
			}
		}
		PreviewChanged();
	}

	private string FindStringResource(string key)
	{
		object obj = System.Windows.Application.Current.FindResource(key);
		if (obj != null)
		{
			return obj.ToString().Replace("|", Environment.NewLine);
		}
		return key;
	}

	private void ProduceJob(JobInfo job)
	{
		Logger.Info("ProduceJob. Job={0}", job.JOB_DATA_1);
		ProductionInfo productionInfo = new ProductionInfo(_settings.UserNames, _settings.Machines);
		productionInfo.UpdateFromPlate(job.Plates.First());
		if (_settings.IsUserObligatory)
		{
			_settings.LastSelectedUser = string.Empty;
			productionInfo.UserName = string.Empty;
		}
		JobProductionStrategySelect jobProductionStrategySelect = new JobProductionStrategySelect();
		JobProductionStrategySelectViewModel dataContext = new JobProductionStrategySelectViewModel(_provider, productionInfo);
		jobProductionStrategySelect.DataContext = dataContext;
		if (jobProductionStrategySelect.ShowDialog() ?? false)
		{
			List<PlateInfo> list = job.Plates.FindAll((PlateInfo x) => x.RestOfProduction() > 0);
			if (EnumerableHelper.IsNullOrEmpty(list))
			{
				return;
			}
			if (productionInfo.ProduceWithRejection)
			{
				foreach (PlateInfo item in list)
				{
					ProducePlate(item, productionInfo);
				}
			}
			else
			{
				ProductionHelper productionHelper = new ProductionHelper(_settings);
				foreach (PlateInfo item2 in list)
				{
					PlateProductionInfo plateProductionInfo = new PlateProductionInfo(item2, productionInfo, _settings.PlateBookTimeValueType);
					try
					{
						if (CheckPlateProductionStatus(item2))
						{
							productionHelper.ProducePlate(plateProductionInfo);
						}
					}
					catch (Exception ex)
					{
						Logger.Exception(ex);
					}
					new Pn2PpsHelper(_settings).ExecuteProduce(plateProductionInfo);
				}
				productionHelper.Logger.Write(new LocalLogger());
			}
			if (_settings.HasToSaveProducedJobs)
			{
				ControlJobSavePath();
				SaveJob(job, _settings.JobDataSavePath);
			}
		}
		else
		{
			Logger.Verbose("Production cancelled. JOB={0}", job.JOB_DATA_1);
		}
	}

	private bool CheckPlateProductionStatus(PlateInfo plate)
	{
		if (plate.PLATE_STATUS == 0)
		{
			string format = FindStringResource("MsgWrongPlateStatus");
			MessageHelper.Error(string.Format(CultureInfo.InvariantCulture, format, plate.PLATE_HEADER_TXT_1));
			return false;
		}
		return true;
	}

	private bool CanProduceJob()
	{
		bool flag = _gridJobController.SelectedItems.Count > 0;
		if (flag)
		{
			flag = _gridJobController.SelectedItems[0].Status != 3 && _gridJobController.SelectedItems[0].Status != 1;
			if (flag)
			{
				flag = _settings.IsJobProduceEnabled;
			}
		}
		return flag;
	}

	private void ProducePlate()
	{
		Logger.Info("ProducePlate");
		foreach (PlateInfo selectedItem in _gridPlateController.SelectedItems)
		{
			if (selectedItem.RestOfProduction() > 0)
			{
				ProducePlate(selectedItem, null);
				continue;
			}
			Logger.Verbose("Already produced. {0}", selectedItem.PLATE_HEADER_TXT_1);
		}
	}

	private void ProducePlate(PlateInfo plate, ProductionInfo productionInfo)
	{
		Logger.Verbose("ProducePlate   " + plate.PLATE_HEADER_TXT_1);
		if (!CheckPlateProductionStatus(plate))
		{
			return;
		}
		if (productionInfo == null)
		{
			productionInfo = new ProductionInfo(_settings.UserNames, _settings.Machines);
			productionInfo.UpdateFromPlate(plate);
		}
		PlateProductionControlViewModel plateProductionControlViewModel = new PlateProductionControlViewModel(_provider);
		PlateProductionWindow obj = new PlateProductionWindow
		{
			DataContext = plateProductionControlViewModel
		};
		plateProductionControlViewModel.FontSize = FontSize;
		plateProductionControlViewModel.Plate = new PlateProductionInfo(plate, productionInfo, _settings.PlateBookTimeValueType);
		if (obj.ShowDialog() ?? false)
		{
			ProductionHelper productionHelper = new ProductionHelper(_settings);
			try
			{
				plateProductionControlViewModel.Plate.UpdateAdditionalData();
				productionHelper.ProducePlate(plateProductionControlViewModel.Plate);
			}
			catch (Exception exception)
			{
				productionHelper.Logger.Exception(exception);
			}
			new Pn2PpsHelper(_settings).ExecuteProduce(plateProductionControlViewModel.Plate);
			PreviewChanged();
			productionHelper.Logger.Write(new LocalLogger());
		}
		else
		{
			Logger.Verbose("Production cancelled. {0}", plate.PLATE_HEADER_TXT_1);
		}
	}

	private bool CanProducePlate()
	{
		bool flag = _gridPlateController.SelectedItems.Count > 0;
		if (flag)
		{
			flag = _gridPlateController.SelectedItems[0].RestOfProduction() > 0;
			if (flag)
			{
				flag = _settings.IsPlateProduceEnabled;
			}
		}
		return flag;
	}

	private void StornoPlate()
	{
		Logger.Info("StornoPlate");
		foreach (PlateInfo selectedItem in _gridPlateController.SelectedItems)
		{
			if (selectedItem.RestOfProduction() > 0)
			{
				Logger.Info("        storno    {0}", selectedItem.PLATE_HEADER_TXT_1);
				PlateStornoWindowViewModel plateStornoWindowViewModel = new PlateStornoWindowViewModel();
				PlateStornoWindow obj = new PlateStornoWindow
				{
					DataContext = plateStornoWindowViewModel
				};
				plateStornoWindowViewModel.Plate = new PlateProductionInfo(selectedItem, new ProductionInfo(_settings.UserNames, _settings.Machines), _settings.PlateBookTimeValueType);
				plateStornoWindowViewModel.Plate.ChangeStorno();
				if (obj.ShowDialog() ?? false)
				{
					new ProductionHelper(_settings).StornoPlate(plateStornoWindowViewModel.Plate);
					new Pn2PpsHelper(_settings).ExecuteStorno(plateStornoWindowViewModel.Plate);
				}
				else
				{
					Logger.Info("Storno cancelled. {0}", selectedItem.PLATE_HEADER_TXT_1);
				}
			}
			else
			{
				Logger.Info("Already produced. {0}", selectedItem.PLATE_HEADER_TXT_1);
			}
		}
		PreviewChanged();
	}

	private bool CanStornoPlate()
	{
		bool flag = _gridPlateController.SelectedItems.Count > 0;
		if (flag)
		{
			flag = _gridPlateController.SelectedItems[0].RestOfProduction() > 0;
			if (flag)
			{
				flag = _settings.IsPlateStornoEnabled;
			}
		}
		return flag;
	}

	private void ResetPlate()
	{
		Logger.Info("ResetPlate");
		ResetHelper resetHelper = new ResetHelper(_settings);
		JobInfo jobInfo = null;
		try
		{
			foreach (PlateInfo selectedItem in _gridPlateController.SelectedItems)
			{
				if (selectedItem.RestOfProduction() != selectedItem.NUMBER_OF_PLATES)
				{
					if (jobInfo == null)
					{
						jobInfo = selectedItem.JobReference;
					}
					resetHelper.Reset(selectedItem);
				}
			}
			JobInfo jobInfo2 = new JobReader().ReadJob(progress: new Progress<int>(), path: Path.GetDirectoryName(jobInfo.Path), strategy: _strategy);
			jobInfo.Plates.Clear();
			jobInfo.Plates.AddRange(jobInfo2.Plates);
			Plates.Clear();
			jobInfo.Plates.ForEach(Plates.Add);
			jobInfo.Parts.Clear();
			jobInfo.Parts.AddRange(jobInfo2.Parts);
			Parts.Clear();
			jobInfo.Parts.ForEach(Parts.Add);
			jobInfo.CheckStatus();
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	private bool CanResetPlate()
	{
		if (_gridPlateController.SelectedItems.Count <= 0)
		{
			return false;
		}
		foreach (PlateInfo selectedItem in _gridPlateController.SelectedItems)
		{
			if (selectedItem.RestOfProduction() < selectedItem.NUMBER_OF_PLATES)
			{
				return true;
			}
		}
		return false;
	}

	private void RejectPart()
	{
		if (_gridPartController.SelectedItems.Count <= 0)
		{
			return;
		}
		Logger.Info("RejectPart");
		PlateInfo plateInfo = _gridPlateController.SelectedItems[0];
		Plate plateDat = new JobDefaultReader().ReadPlate(plateInfo.Path);
		foreach (PartInfo part in _gridPartController.SelectedItems)
		{
			PlatePartProductionInfo platePartProductionInfo = new PlatePartProductionInfo(plateInfo.PlateParts.Find((PlatePartInfo x) => x.PLATE_PART_NUMBER == part.PART_NUMBER));
			if (platePartProductionInfo.RestOfRejection > 0)
			{
				platePartProductionInfo.AmountToReject = 1;
				PartRejectControlViewModel partRejectControlViewModel = new PartRejectControlViewModel();
				PartRejectWindow obj = new PartRejectWindow
				{
					DataContext = partRejectControlViewModel
				};
				partRejectControlViewModel.Part = platePartProductionInfo;
				if ((obj.ShowDialog() ?? false) && partRejectControlViewModel.Part.AmountToReject > 0)
				{
					ProductionHelper productionHelper = new ProductionHelper(_settings);
					PlateProductionInfo plateProductionInfo = new PlateProductionInfo(plateInfo, new ProductionInfo(_settings.UserNames, _settings.Machines), _settings.PlateBookTimeValueType);
					productionHelper.RejectPart(partRejectControlViewModel.Part, plateProductionInfo, plateDat);
					new Pn2PpsHelper(_settings).ExecuteReject(plateProductionInfo);
				}
			}
			else
			{
				string format = FindStringResource("MsgAllPartsAreProduced");
				MessageHelper.Error(string.Format(CultureInfo.InvariantCulture, format, part.PART_NAME));
			}
		}
	}

	private bool CanRejectPart()
	{
		bool flag = _gridPlateController.SelectedItems.Count > 0;
		if (flag)
		{
			flag = _gridPartController.SelectedItems.Count > 0;
			if (flag)
			{
				flag = _settings.IsPartRejectEnabled;
			}
		}
		return flag;
	}

	private void JobOpen()
	{
		foreach (JobInfo selectedItem in _gridJobController.SelectedItems)
		{
			string arguments = Path.Combine(_jobsPath, selectedItem.JOB_DATA_1);
			Process.Start("explorer.exe", arguments);
		}
	}

	private void PrintJobLabels()
	{
		LabelPrintManager labelPrintManager = new LabelPrintManager(_provider, _settings.JobDataPath);
		foreach (JobInfo selectedItem in _gridJobController.SelectedItems)
		{
			labelPrintManager.PrintJobLabels(selectedItem);
		}
	}

	private bool CanPrintJobLabels()
	{
		if (!_settings.IsJobPrintLabelEnabled)
		{
			return false;
		}
		return _gridJobController.SelectedItems.Count > 0;
	}

	private void PrintPlateLabels()
	{
		LabelPrintManager labelPrintManager = new LabelPrintManager(_provider, _settings.JobDataPath);
		List<PlateInfo> list = new List<PlateInfo>(_gridPlateController.SelectedItems);
		list.Sort((PlateInfo x, PlateInfo y) => x.PLATE_NUMBER.CompareTo(y.PLATE_NUMBER));
		foreach (PlateInfo item in list)
		{
			labelPrintManager.PrintPlateLabels(item);
		}
	}

	private bool CanPrintPlateLabels()
	{
		if (!_settings.IsPlatePrintLabelEnabled)
		{
			return false;
		}
		return _gridPlateController.SelectedItems.Count > 0;
	}

	private void PrintPartLabels()
	{
		LabelPrintManager labelPrintManager = new LabelPrintManager(_provider, _settings.JobDataPath);
		foreach (PartInfo selectedItem in _gridPartController.SelectedItems)
		{
			labelPrintManager.PrintSinglePartLabels(selectedItem);
		}
	}

	private bool CanPrintPartLabels()
	{
		if (!_settings.IsPartPrintLabelEnabled)
		{
			return false;
		}
		return _gridPartController.SelectedItems.Count > 0;
	}

	public void Initialize(IView view, IJobManagerServiceProvider provider)
	{
		View = view;
		_provider = provider;
		_settings = _provider.FindService<IJobManagerSettings>();
		_stateManager = _provider.FindService<IStateManager>();
		JobsPath = _settings.JobDataPath;
		GridJobHeight = CreateHeight(_settings.GridJobHeight);
		GridPlateHeight = CreateHeight(_settings.GridPlateHeight);
		ActivateGridControllers();
		JobStrategyHelper jobStrategyHelper = new JobStrategyHelper();
		_strategy = jobStrategyHelper.CreateReadStrategies(_settings);
		_stateManager.AttachImageObserver(this);
		_settings.IsJobDeleteEnabled = false;
    }

    public ICommand AddToBlechOptCommand
    {
        get
        {
            if (_AddToBlechOptCommand == null)
            {
                _AddToBlechOptCommand = new RelayCommand(delegate
                {
                    AddToBlechOpt();
                }, (object x) => true);
            }
            return _AddToBlechOptCommand;
        }
    }

    private void AddToBlechOpt()
    {
        foreach (JobInfo selectedItem in _gridJobController.SelectedItems)
        {
            string auftragsnr = selectedItem.JOB_DATA_2;
            string oberfl = selectedItem.JOB_DATA_4;
            string material = selectedItem.JOB_MATERIAL_NAME;
            string kunde = selectedItem.JOB_DATA_9;
            int platesAmount = selectedItem.TOTAL_AMOUNT_PLATES;
            double plateThick = selectedItem.JOB_THICKNESS;

            string arguments = Path.Combine(_jobsPath, System.IO.Path.GetDirectoryName(selectedItem.Path));
            var linesRead = File.ReadLines(arguments + "\\JOB.DAT");


            FolderBrowserDialog dlg = new FolderBrowserDialog();

            //if (!AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString().Contains("\\"))
            //{
            //    dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
            //}
            //else
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheetFileNames = null;
            Microsoft.Office.Interop.Excel.Workbook xlWorkbookFileNames = null;
            string blechOptDirectory;
            int count = auftragsnr.Count();
            string orderYear = auftragsnr.Remove(2, count - 2);
            string aNr = auftragsnr.Remove(6, count - 6);
            dlg.InitialDirectory = Path.GetDirectoryName(path: "S:\\cadzeich\\20" + orderYear + "\\" + aNr + "\\Blechfertigung\\");

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
            blechOptDirectory = dlg.SelectedPath;

            // Process open file dialog box results
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                // Open document

                Console.WriteLine("BlechOpt =  " + blechOptDirectory);
                Microsoft.Office.Interop.Excel.Application xlAppFileNames = new Microsoft.Office.Interop.Excel.Application();
                object misValue = System.Reflection.Missing.Value;

                if (!File.Exists(blechOptDirectory + "\\" + kunde + "-" + aNr + "-BlechOpt.xlsx"))
                {
                    xlWorkbookFileNames = xlAppFileNames.Workbooks.Add(misValue);
                }
                else
                {
                    xlWorkbookFileNames = xlAppFileNames.Workbooks.Open(blechOptDirectory + "\\" + kunde + "-" + aNr + "-BlechOpt.xlsx");

                }
                //Excel._Worksheet xlWorksheetFileNames = xlWorkbookFileNames.Sheets[1];
                xlWorksheetFileNames = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbookFileNames.Worksheets.get_Item(1);
                xlAppFileNames.Visible = true;
                Microsoft.Office.Interop.Excel.Range range = xlWorksheetFileNames.UsedRange;
                int rows = range.Rows.Count;
                Console.WriteLine("Creating Header  " + rows);
                if (!File.Exists(blechOptDirectory + "\\" + kunde + "-" + aNr + "-BlechOpt.xlsx"))
                {
                    rows = rows + 1;
                }
                else
                { rows = rows + 3; }
                Console.WriteLine("Kunde  " + kunde);
                xlWorksheetFileNames.Cells[rows, 1] = "Kunde";
                xlWorksheetFileNames.Cells[rows, 1].Interior.Color = System.Drawing.Color.FromArgb(333333);
                xlWorksheetFileNames.Cells[rows, 1].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows, 1].Font.Size = 16;
                xlWorksheetFileNames.Cells[rows, 2] = kunde;
                xlWorksheetFileNames.Cells[rows, 2].Interior.Color = System.Drawing.Color.FromArgb(333333);
                xlWorksheetFileNames.Cells[rows, 2].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows, 2].Font.Size = 13;

                Console.WriteLine("Auftrags_Nr.  " + auftragsnr);
                xlWorksheetFileNames.Cells[rows + 1, 1] = "AuftragsNr.";
                xlWorksheetFileNames.Cells[rows + 1, 1].Interior.Color = System.Drawing.Color.FromArgb(333333);
                xlWorksheetFileNames.Cells[rows + 1, 1].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows + 1, 1].Font.Size = 16;
                xlWorksheetFileNames.Cells[rows + 1, 2] = auftragsnr;
                xlWorksheetFileNames.Cells[rows + 1, 2].Interior.Color = System.Drawing.Color.FromArgb(333333);
                xlWorksheetFileNames.Cells[rows + 1, 2].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows + 1, 2].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                xlWorksheetFileNames.Cells[rows + 1, 2].Font.Size = 13;

                Console.WriteLine("Material  " + material);
                xlWorksheetFileNames.Cells[rows, 7] = "Material";
                xlWorksheetFileNames.Cells[rows, 7].Interior.Color = System.Drawing.Color.FromArgb(868686);
                xlWorksheetFileNames.Cells[rows, 7].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows, 7].Font.Size = 16;
                xlWorksheetFileNames.Cells[rows, 8] = material;
                xlWorksheetFileNames.Cells[rows, 8].Interior.Color = System.Drawing.Color.FromArgb(868686);
                xlWorksheetFileNames.Cells[rows, 8].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows, 8].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                xlWorksheetFileNames.Cells[rows, 8].Font.Size = 13;

                Console.WriteLine("Platine_Dicke  " + plateThick);
                xlWorksheetFileNames.Cells[rows + 1, 7] = "Dicke";
                xlWorksheetFileNames.Cells[rows + 1, 7].Interior.Color = System.Drawing.Color.FromArgb(868686);
                xlWorksheetFileNames.Cells[rows + 1, 7].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows + 1, 7].Font.Size = 16;
                xlWorksheetFileNames.Cells[rows + 1, 8] = plateThick;
                xlWorksheetFileNames.Cells[rows + 1, 8].Interior.Color = System.Drawing.Color.FromArgb(868686);
                xlWorksheetFileNames.Cells[rows + 1, 8].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[rows + 1, 8].Font.Size = 13;

                xlWorksheetFileNames.Cells[rows + 3, 1] = "Plate-Pos";
                xlWorksheetFileNames.Cells[rows + 3, 1].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 1].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 2] = "PlateDim_X";
                xlWorksheetFileNames.Cells[rows + 3, 2].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 2].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 3] = "PlateDim_Y";
                xlWorksheetFileNames.Cells[rows + 3, 3].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 3].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 4] = "Anzahl";
                xlWorksheetFileNames.Cells[rows + 3, 4].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 4].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 5] = "Material";
                xlWorksheetFileNames.Cells[rows + 3, 5].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 5].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 6] = "Dicke";
                xlWorksheetFileNames.Cells[rows + 3, 6].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[rows + 3, 6].Font.Bold = true;
                xlWorksheetFileNames.Cells[rows + 3, 7] = "Teil-Pos";
                xlWorksheetFileNames.Cells[rows + 3, 8] = "TeileDim_X";
                xlWorksheetFileNames.Cells[rows + 3, 9] = "TeileDim_Y";
                xlWorksheetFileNames.Cells[rows + 3, 10] = "Anzahl";
                xlWorksheetFileNames.Cells[rows + 3, 11] = "Material";
                xlWorksheetFileNames.Cells[rows + 3, 12] = "Dicke";
                xlWorksheetFileNames.Cells[rows + 3, 13] = "Teile-ID";
                xlWorksheetFileNames.Cells[rows + 3, 14] = "Oberfl";
                xlWorksheetFileNames.Cells[rows + 3, 15] = "Gewicht-Teil";
                xlWorksheetFileNames.Cells[rows + 3, 16] = "Gewicht-Gesamt";
                //xlWorksheetFileNames.Cells[3, 12] = "Bemerkungen";
                //xlWorksheetFileNames.Cells[3, 13] = "Gravur";
                xlrange = xlWorksheetFileNames.Range["A5:O5"];//xlWorksheetFileNames.UsedRange;
                if (!File.Exists(blechOptDirectory + "\\" + kunde + "-" + aNr + "-BlechOpt.xlsx"))
                {
                    xlrange.AutoFilter(1, "", Microsoft.Office.Interop.Excel.XlAutoFilterOperator.xlFilterValues, Type.Missing, true);
                }
                List<PlateInfo> list = new List<PlateInfo>(selectedItem.Plates);
                list.Sort((PlateInfo x, PlateInfo y) => x.PLATE_NUMBER.CompareTo(y.PLATE_NUMBER));
                int i = rows + 4;
                foreach (PlateInfo item in list)
                {
                    xlWorksheetFileNames.Cells[i, 1] = item.PLATE_NUMBER;
                    xlWorksheetFileNames.Cells[i, 1].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 1].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 1].Font.Bold = true;
                    xlWorksheetFileNames.Cells[i, 2] = item.PLATE_SIZE_X;
                    xlWorksheetFileNames.Cells[i, 2].NumberFormat = "#.0";
                    xlWorksheetFileNames.Cells[i, 2].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 2].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 2].Font.Bold = true;
                    xlWorksheetFileNames.Cells[i, 3] = item.PLATE_SIZE_Y;
                    xlWorksheetFileNames.Cells[i, 3].NumberFormat = "#.0";
                    xlWorksheetFileNames.Cells[i, 3].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 3].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 3].Font.Bold = true;
                    xlWorksheetFileNames.Cells[i, 4] = item.NUMBER_OF_PLATES;
                    xlWorksheetFileNames.Cells[i, 4].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 4].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 4].Font.Bold = true;
                    xlWorksheetFileNames.Cells[i, 5] = material;
                    xlWorksheetFileNames.Cells[i, 5].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 5].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 5].Font.Bold = true;
                    xlWorksheetFileNames.Cells[i, 6] = plateThick;
                    xlWorksheetFileNames.Cells[i, 6].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                    xlWorksheetFileNames.Cells[i, 6].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                    xlWorksheetFileNames.Cells[i, 6].Font.Bold = true;
                    Console.WriteLine("Platine_Nr.  " + item.PLATE_NUMBER);
                    Console.WriteLine("Platine_X.  " + item.PLATE_SIZE_X);
                    Console.WriteLine("Platine_Y.  " + item.PLATE_SIZE_Y);
                    Console.WriteLine("Anzahl Platinen  " + item.NUMBER_OF_PLATES);
                    i++;
                }
                xlWorksheetFileNames.Cells[i, 3] = "Summe:";
                xlWorksheetFileNames.Cells[i, 3].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbDarkSeaGreen;
                xlWorksheetFileNames.Cells[i, 3].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbBlack;
                xlWorksheetFileNames.Cells[i, 3].Font.Bold = true;
                i = i - 1;
                int iirows = rows + 4;
                xlWorksheetFileNames.Cells[i + 1, 4].Formula = "=Sum(D" + iirows + ":D" + i + ")";
                xlWorksheetFileNames.Cells[i + 1, 4].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbDarkSeaGreen;
                xlWorksheetFileNames.Cells[i + 1, 4].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbBlack;
                xlWorksheetFileNames.Cells[i + 1, 4].Font.Bold = true;

                i = rows + 4;

                string[] files = Directory.GetFiles(arguments);
                int iForFiles = 1;
                bool resulting;
                foreach (string file in files)
                {
                    resulting = Path.GetExtension(file).Equals(".DAT") && file.Contains("PART");
                    //if (resulting == false)
                    //{
                    //    resulting = Path.GetExtension(file).Equals(".dat");
                    //}

                    if (resulting == true)
                    {

                        Console.WriteLine(file + "   " + arguments + "\\PART_" + "00" + iForFiles.ToString() + ".DAT");

                        if (iForFiles < 10)
                        {
							if (File.Exists(arguments + "\\PART_" + "00" + iForFiles.ToString() + ".DAT"))
							{
								var linesReadPart = File.ReadLines(arguments + "\\PART_" + "00" + iForFiles.ToString() + ".DAT");
								foreach (var lineRead in linesReadPart)
								{
									if (lineRead.Contains("PART_NUMBER          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_NUMBER  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 7] = amount[1];
									}
									//   Console.WriteLine(lineRead);
									if (lineRead.Contains("PART_SIZE_X"))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_SIZE_X  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 8] = amount[1];
										xlWorksheetFileNames.Cells[i, 8].NumberFormat = "#.0";
									}
									if (lineRead.Contains("PART_SIZE_Y"))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_SIZE_Y  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 9] = amount[1];
										xlWorksheetFileNames.Cells[i, 9].NumberFormat = "#.0";
									}
									if (lineRead.Contains("PART_ACOUNT          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_ACOUNT  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 10] = amount[1];
                                        partsamount = amount[1];

                                    }
                                    if (lineRead.Contains("PART_NAME            ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_NAME  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 13] = amount[1];
									}
									if (lineRead.Contains("PART_REMARK          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("Oberfl  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 14] = amount[1];
									}
									if (lineRead.Contains("PART_WGHT_RE         ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_WGHT_RE  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 15] = amount[1];
                                        xlWorksheetFileNames.Cells[i, 16].Formula = "=Sum(" + partsamount + "*O" + i + ")"; //"=Sum(" + xlWorksheetFileNames.Cells[i, 15] + "*" + xlWorksheetFileNames.Cells[i, 10] + ")";

                                    }
                                    xlWorksheetFileNames.Cells[i, 11] = material;
									xlWorksheetFileNames.Cells[i, 12] = plateThick;

								}
								Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
								Console.WriteLine(linesReadPart);
								//     xlWorksheetFileNames.Cells[iForFiles + 1, 1] = Path.GetFileNameWithoutExtension(file);
								iForFiles++;
								i++;
							}
                        }

                        if (iForFiles >= 10)
                        {
							if (File.Exists(arguments + "\\PART_" + "0" + iForFiles.ToString() + ".DAT"))
							{
								var linesReadPart = File.ReadLines(arguments + "\\PART_" + "0" + iForFiles.ToString() + ".DAT");
								foreach (var lineRead in linesReadPart)
								{
									if (lineRead.Contains("PART_NUMBER          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_NUMBER  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 7] = amount[1];
									}
									//   Console.WriteLine(lineRead);
									if (lineRead.Contains("PART_SIZE_X"))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_SIZE_X  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 8] = amount[1];
										xlWorksheetFileNames.Cells[i, 8].NumberFormat = "#.0";
									}
									if (lineRead.Contains("PART_SIZE_Y"))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_SIZE_Y  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 9] = amount[1];
										xlWorksheetFileNames.Cells[i, 9].NumberFormat = "#.0";
									}
									if (lineRead.Contains("PART_ACOUNT          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_ACOUNT  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 10] = amount[1];
                                        partsamount = amount[1];
                                    }
                                    if (lineRead.Contains("PART_NAME            ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_NAME  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 13] = amount[1];
									}
									if (lineRead.Contains("PART_REMARK          ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("Oberfl  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 14] = amount[1];
									}
									if (lineRead.Contains("PART_WGHT_RE         ="))
									{
										//  Console.WriteLine(lineRead);
										string[] amount = lineRead.Split("=");
										Console.WriteLine("PART_WGHT_RE  " + amount[1]);
										xlWorksheetFileNames.Cells[i, 15] = amount[1];
                                        xlWorksheetFileNames.Cells[i, 16].Formula = "=Sum(" + partsamount + "*O" + i + ")"; //"=Sum(" + xlWorksheetFileNames.Cells[i, 15] + "*" + xlWorksheetFileNames.Cells[i, 10] + ")";

                                    }
                                    xlWorksheetFileNames.Cells[i, 11] = material;
									xlWorksheetFileNames.Cells[i, 12] = plateThick;

								}
								Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
								Console.WriteLine(linesReadPart);
								//     xlWorksheetFileNames.Cells[iForFiles + 1, 1] = Path.GetFileNameWithoutExtension(file);
								iForFiles++;
								i++;
							}
                        }

                    }
                }
                xlWorksheetFileNames.Cells[i, 9] = "Summe:";
                xlWorksheetFileNames.Cells[i, 9].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[i, 9].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[i, 9].Font.Bold = true;
                i = i - 1;
                int irows = rows + 4;
                xlWorksheetFileNames.Cells[i + 1, 10].Formula = "=Sum(J" + irows + ":J" + i + ")";
                xlWorksheetFileNames.Cells[i + 1, 10].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[i + 1, 10].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[i + 1, 10].Font.Bold = true;
                int iFrows = irows - 1;
                xlWorksheetFileNames.Range["G" + iFrows + ":O" + i].Font.Bold = true;
                xlWorksheetFileNames.Range["G" + iFrows + ":O" + i].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbDarkSeaGreen;

                xlWorksheetFileNames.Cells[i + 1, 15] = "Summe:";
                xlWorksheetFileNames.Cells[i + 1, 15].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbFuchsia;
                xlWorksheetFileNames.Cells[i + 1, 15].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
                xlWorksheetFileNames.Cells[i + 1, 15].Font.Bold = true;

                xlWorksheetFileNames.Cells[i + 1, 16].Formula = "=Sum(P" + irows + ":P" + i + ")";
                xlWorksheetFileNames.Cells[i + 1, 16].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbCadetBlue;
                xlWorksheetFileNames.Cells[i + 1, 16].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite; xlWorksheetFileNames.Cells[i + 1, 16].Font.Bold = true;

                xlWorksheetFileNames.Range["P" + iFrows + ":P" + i].Font.Bold = true;
                xlWorksheetFileNames.Range["P" + iFrows + ":P" + i].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbAzure;

                xlWorksheetFileNames.Columns.AutoFit();
                xlWorksheetFileNames.Rows.AutoFit();

                xlWorkbookFileNames.SaveAs(blechOptDirectory + "\\" + kunde + "-" + aNr + "-BlechOpt.xlsx", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //                    foreach (PlateInfo item in list)
                //{
                //	List<PartInfo> listparts = new List<PartInfo>();
                //	foreach (PlatePartInfo platePart in item.PlateParts)
                //	{
                //                    //listparts.Add(platePart.Part);
                //                    xlWorksheetFileNames.Cells[i, 7] = platePart.PLATE_PART_NUMBER;
                //                    xlWorksheetFileNames.Cells[i, 8] = platePart.PART_SIZE_X;
                //		xlWorksheetFileNames.Cells[i, 9] = platePart.Part.PART_SIZE_Y;
                //                    xlWorksheetFileNames.Cells[i, 10] = platePart.PLATE_PART_AMOUNT_PL;
                //		i++;
                //                }
                //}
                //xlrange2 = xlWorksheetFileNames.Range[xlWorksheetFileNames.Cells[i, 1], xlWorksheetFileNames.Cells[i, 4]];//xlWorksheetFileNames.UsedRange;
                //xlrange2.AutoFilter(1, "", Microsoft.Office.Interop.Excel.XlAutoFilterOperator.xlFilterValues, Type.Missing, true);
                // xlWorksheetFileNames.Cells[1, 1] = MainWindow.mainWindow.txtKundenName.Text;
                //xlWorksheetFileNames.Cells[1, 4] = MainWindow.mainWindow.txtAuftragsNummer.Text;



                //    Marshal.ReleaseComObject(xlWorksheetFileNames);
                // xlWorkbookFileNames.Close();
                //   Marshal.ReleaseComObject(xlWorkbookFileNames);
                // xlAppFileNames.Quit();
            }
            // iterate through each element within the array and
            // print it out
            //

            // Console.WriteLine("Platine_Oberfl.  " + oberfl);






            // xlWorkbookFileNames.Save();
            // xlWorkbookFileNames.SaveAs(dxfDirectory + "dxfFileNames.xlsx");
            //xlWorkbookFileNames.Close(true, misValue, misValue);



            // Process.Start("explorer.exe", arguments);
        }
    }

    private void JobsExport(List<JobInfo> list3)
    {
        FolderBrowserDialog dlg = new FolderBrowserDialog();
        Console.WriteLine("jobsexport");
        foreach (var jobinf in list3)
        {
            Console.WriteLine(jobinf.JOB_DATA_2);
            Console.WriteLine(jobinf.JOB_DATA_9);
            Console.WriteLine(jobinf.JOB_MATERIAL_NAME);
            Console.WriteLine(jobinf.JOB_THICKNESS);
            Console.WriteLine(jobinf.JOB_PROGRESS);
            Console.WriteLine(jobinf.JOB_COST_PUNCHING);
        }
        //if (!AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString().Contains("\\"))
        //{
        //    dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
        //}
        //else
        Microsoft.Office.Interop.Excel._Worksheet xlWorksheetFileNames = null;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbookFileNames = null;
        string AuftragOptDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\Kanterei\\AuftragOptDirectory\\");
        //   dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");

        // Show open file dialog box
        //    DialogResult result = dlg.ShowDialog();
        // AuftragOptDirectory = dlg.SelectedPath;

        // Process open file dialog box results
        //    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
        //   {
        // Open document

        Console.WriteLine("AuftragOpt =  " + AuftragOptDirectory);
        Microsoft.Office.Interop.Excel.Application xlAppFileNames = new Microsoft.Office.Interop.Excel.Application();
        object misValue = System.Reflection.Missing.Value;

        if (!File.Exists(AuftragOptDirectory + "\\AuftragOpt.xlsx"))
        {
            xlWorkbookFileNames = xlAppFileNames.Workbooks.Add(misValue);
        }
        else
        {
            xlWorkbookFileNames = xlAppFileNames.Workbooks.Open(AuftragOptDirectory + "\\AuftragOpt.xlsx");

        }
        //Excel._Worksheet xlWorksheetFileNames = xlWorkbookFileNames.Sheets[1];
        xlWorksheetFileNames = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbookFileNames.Worksheets.get_Item(1);
        xlAppFileNames.Visible = true;
        Microsoft.Office.Interop.Excel.Range range = xlWorksheetFileNames.UsedRange;
        int rows = range.Rows.Count;
        Console.WriteLine("Creating Header  " + rows);
        if (!File.Exists(AuftragOptDirectory + "\\AuftragOpt.xlsx"))
        {
            rows = rows + 1;
        }
        else
        { rows = rows + 3; }



        //  Console.WriteLine("Kunde  " + jobinf.JOB_DATA_9);
        xlWorksheetFileNames.Cells[rows, 1] = "Kunde";
        xlWorksheetFileNames.Cells[rows, 1].Interior.Color = System.Drawing.Color.FromArgb(333333);
        xlWorksheetFileNames.Cells[rows, 1].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
        xlWorksheetFileNames.Cells[rows, 1].Font.Size = 16;

        //     Console.WriteLine("Auftrags_Nr.  " + jobinf.JOB_DATA_2);
        xlWorksheetFileNames.Cells[rows, 2] = "AuftragsNr.";
        xlWorksheetFileNames.Cells[rows, 2].Interior.Color = System.Drawing.Color.FromArgb(333333);
        xlWorksheetFileNames.Cells[rows, 2].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
        xlWorksheetFileNames.Cells[rows, 2].Font.Size = 16;

        //       Console.WriteLine("Material  " + jobinf.JOB_MATERIAL_NAME);
        xlWorksheetFileNames.Cells[rows, 3] = "Material";
        xlWorksheetFileNames.Cells[rows, 3].Interior.Color = System.Drawing.Color.FromArgb(868686);
        xlWorksheetFileNames.Cells[rows, 3].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
        xlWorksheetFileNames.Cells[rows, 3].Font.Size = 16;

        //      Console.WriteLine("Platine_Dicke  " + jobinf.JOB_THICKNESS);
        xlWorksheetFileNames.Cells[rows, 4] = "Dicke";
        xlWorksheetFileNames.Cells[rows, 4].Interior.Color = System.Drawing.Color.FromArgb(868686);
        xlWorksheetFileNames.Cells[rows, 4].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
        xlWorksheetFileNames.Cells[rows, 4].Font.Size = 16;
        xlWorksheetFileNames.Cells[rows, 5] = "Progress";
        xlWorksheetFileNames.Cells[rows, 5].Interior.Color = System.Drawing.Color.FromArgb(868686);
        xlWorksheetFileNames.Cells[rows, 5].Font.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbWhite;
        xlWorksheetFileNames.Cells[rows, 5].Font.Size = 16;
        //     }
        // }
        ii = rows + 2;
        foreach (var jobinf in list3)
        {
            xlWorksheetFileNames.Cells[ii, 1] = jobinf.JOB_DATA_9;
            Console.WriteLine(jobinf.JOB_DATA_9);
            xlWorksheetFileNames.Cells[ii, 2] = jobinf.JOB_DATA_2;
            Console.WriteLine(jobinf.JOB_DATA_2);
            xlWorksheetFileNames.Cells[ii, 3] = jobinf.JOB_MATERIAL_NAME;
            Console.WriteLine(jobinf.JOB_MATERIAL_NAME);
            xlWorksheetFileNames.Cells[ii, 4] = jobinf.JOB_THICKNESS;
            Console.WriteLine(jobinf.JOB_THICKNESS);
            xlWorksheetFileNames.Cells[ii, 5] = jobinf.JOB_PROGRESS;
            Console.WriteLine(jobinf.JOB_PROGRESS);
            Console.WriteLine(jobinf.JOB_COST_PUNCHING);
            ii++;
        }

        xlWorksheetFileNames.Range["A3" + ":E" + ii].Font.Size = 14;
        xlWorksheetFileNames.Range["A3" + ":E" + ii].Font.Bold = true;
        xlWorksheetFileNames.Range["A3" + ":E" + ii].Interior.Color = Microsoft.Office.Interop.Excel.XlRgbColor.rgbAzure;
        xlWorksheetFileNames.Columns.AutoFit();
        xlWorksheetFileNames.Rows.AutoFit();

        xlWorkbookFileNames.SaveAs(AuftragOptDirectory + "\\AuftragOpt.xlsx", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

    }

    private GridLength CreateHeight(int value)
	{
		if (value <= 0)
		{
			return new GridLength(1.0, GridUnitType.Star);
		}
		return new GridLength(value, GridUnitType.Pixel);
	}

	public void LoadJobs()
	{
		Console.WriteLine("Loading Path " + PnPathBuilder.ArDrive + "\\u\\sfa\\jobdata");
		ProgressVisibility = Visibility.Visible;
        this._settings.JobDataPath = PnPathBuilder.ArDrive + "\\u\\sfa\\jobdata";
        JobsPath = this._settings.JobDataPath;

        Stopwatch sw = Stopwatch.StartNew();
		Task.Run(() => ReadJobs(_settings.JobDataPath)).ContinueWith(delegate(Task<List<JobInfo>> t)
		{
			sw.Stop();
			Logger.Verbose("Read Jobs : {0}", sw.ElapsedMilliseconds);
			ShowJobs(t);
		});

	}

	private void ShowJobs(Task<List<JobInfo>> task)
	{
		if (System.Windows.Application.Current.Dispatcher == null)
		{
			return;
		}
		if (System.Windows.Application.Current.Dispatcher.CheckAccess())
		{
			_originalJobs = task.Result;
			if (!EnumerableHelper.IsNullOrEmpty(_originalJobs))
			{
				_originalJobs.Sort((JobInfo x, JobInfo y) => CompareJobs(x, y));
				JobsAmount = _originalJobs.Count;
				List<JobInfo> jobs = FilterJobByMachine(_settings.Machines);
				ModifyJobs(jobs);
				PreviewChanged();
			}
			else
			{
				Logger.Error("Jobs are not found!");
			}
			ReadJobsAmount = 0;
			ProgressVisibility = Visibility.Hidden;
		}
		else
		{
			System.Windows.Application.Current.Dispatcher.Invoke(new Action<Task<List<JobInfo>>>(ShowJobs), DispatcherPriority.Normal, task);
		}
	}

	private int CompareJobsAsc(JobInfo j1, JobInfo j2)
	{
		int num = j1.DATE.CompareTo(j2.DATE);
		if (num == 0)
		{
			num = j1.TIME.CompareTo(j2.TIME);
		}
		return num;
	}

	private int CompareJobs(JobInfo j1, JobInfo j2)
	{
		int num = j2.DATE.CompareTo(j1.DATE);
		if (num == 0)
		{
			num = j2.TIME.CompareTo(j1.TIME);
		}
		return num;
	}

	public void PreviewChanged(string path = "")
	{
		if (System.Windows.Application.Current.Dispatcher == null)
		{
			return;
		}
		if (System.Windows.Application.Current.Dispatcher.CheckAccess())
		{
			foreach (MachineViewInfo item in _settings.Machines)
			{
				CalculateJobsInMachine(item, _originalJobs.FindAll((JobInfo x) => x.JOB_MACHINE_NO == item.Number));
			}
			return;
		}
		System.Windows.Application.Current.Dispatcher.Invoke(new Action<string>(PreviewChanged), DispatcherPriority.Normal, path);
	}

	public void SaveSettings()
	{
		_settings.GridJobHeight = (int)GridJobHeight.Value;
		_settings.GridPlateHeight = (int)GridPlateHeight.Value;
	}

	private void CalculateJobsInMachine(MachineViewInfo item, List<JobInfo> jobs)
	{
		try
		{
			if (!EnumerableHelper.IsNullOrEmpty(jobs))
			{
				List<JobInfo> list = jobs.FindAll((JobInfo x) => x.Status == 2);
				item.AmountTotal = ((list.Count > 0) ? list.Count : 0);
				List<JobInfo> list2 = jobs.FindAll((JobInfo x) => x.Status == 3 || x.Status == 1);
				item.AmountProduced = ((list2.Count > 0) ? list2.Count : 0);
				item.TimeProduction = CalculateTime(list);
				item.TimeProduced = CalculateProducedTime(list2);
				double num = CalculateProducedTime(list);
				item.TimeProduction -= num;
				item.TimeProduced += num;
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	private double CalculateTime(List<JobInfo> jobs)
	{
		double num = 0.0;
		foreach (JobInfo job in jobs)
		{
			num += job.JOB_TIME_TOTAL;
		}
		return num;
	}

	private double CalculateProducedTime(List<JobInfo> jobs)
	{
		double num = 0.0;
		foreach (JobInfo job in jobs)
		{
			foreach (PlateInfo plate in job.Plates)
			{
				num += plate.PLATE_TIME_TOTAL * (double)(plate.PLATE_PRODUCED + plate.PLATE_STORNO);
			}
		}
		return num;
	}

	public void ProduceJobUsingBarcode(string jobName)
	{
		JobInfo jobInfo = _originalJobs.Find((JobInfo x) => x.JOB_DATA_1.Equals(jobName, StringComparison.CurrentCultureIgnoreCase));
		if (jobInfo != null)
		{
			ProduceJob(jobInfo);
			return;
		}
		MessageHelper.Error(FindStringResource("MsgJobIsNotFound"));
		HasToIgnoreEnter = true;
	}

	public void ProducePlateUsingBarcode(string ncProgramNumber)
	{
		JobInfo jobInfo = _originalJobs.Find((JobInfo x) => x.Plates.Find((PlateInfo y) => y.PLATE_HEADER_TXT_1.Equals(ncProgramNumber, StringComparison.CurrentCultureIgnoreCase)) != null);
		if (jobInfo != null)
		{
			PlateInfo plateInfo = jobInfo.Plates.Find((PlateInfo x) => x.PLATE_HEADER_TXT_1.Equals(ncProgramNumber, StringComparison.CurrentCultureIgnoreCase));
			if (plateInfo != null)
			{
				ProducePlate(plateInfo, null);
				return;
			}
			MessageHelper.Error(FindStringResource("MsgPlateIsNotFound"));
			HasToIgnoreEnter = true;
		}
		else
		{
			MessageHelper.Error(FindStringResource("MsgJobIsNotFound"));
			HasToIgnoreEnter = true;
		}
	}

	public void Filter(IEnumerable<MachineViewInfo> machines)
	{
		Plates.Clear();
		Parts.Clear();
		List<JobInfo> jobs = FilterJobByMachine(machines);
		ModifyJobs(jobs);
	}

	private List<JobInfo> FilterJobByMachine(IEnumerable<MachineViewInfo> machines)
	{
		List<JobInfo> list = new List<JobInfo>();
		List<MachineViewInfo> list2 = new List<MachineViewInfo>(machines);
		List<MachineViewInfo> list3 = new List<MachineViewInfo>();
		bool flag = true;
		if (list2.Count == 1)
		{
			list3.AddRange(list2);
		}
		else
		{
			list3 = list2.FindAll((MachineViewInfo x) => x.IsSelected);
			if (list3.Count == list2.Count)
			{
				flag = false;
			}
		}
		if (flag)
		{
			foreach (MachineViewInfo item in list3)
			{
				if (item.IsSelected)
				{
					List<JobInfo> list4 = _originalJobs.FindAll((JobInfo x) => x.JOB_MACHINE_NO == item.Number);
					if (!EnumerableHelper.IsNullOrEmpty(list4))
					{
						list.AddRange(list4);
					}
				}
			}
		}
		else
		{
			list.AddRange(_originalJobs);
		}
		return list;
	}

	public void FilterJobs(List<FilterInfo> filters)
	{
		Logger.Info("FilterJobs");
		ListFilter<JobInfo> listFilter = new ListFilter<JobInfo>();
		List<JobInfo> list = FilterJobByMachine(_settings.Machines);
		list = listFilter.Filter(list, filters);
		ModifyJobs(list);
		Plates.Clear();
		Parts.Clear();
	}

	public void FilterPlates(List<FilterInfo> filters)
	{
		Logger.Info("FilterPlates");
		if (filters.Count == 0)
		{
			FilterJobs(new List<FilterInfo>());
			return;
		}
		List<JobInfo> list = FilterJobByMachine(_settings.Machines);
		List<PlateInfo> list2 = new List<PlateInfo>();
		foreach (JobInfo item in list)
		{
			list2.AddRange(item.Plates);
		}
		List<PlateInfo> list3 = new ListFilter<PlateInfo>().Filter(list2, filters);
		List<JobInfo> list4 = new List<JobInfo>();
		foreach (PlateInfo item2 in list3)
		{
			if (!list4.Contains(item2.JobReference))
			{
				list4.Add(item2.JobReference);
			}
		}
		ModifyJobs(list4);
		ModifyPlates(list3);
	}

	public void FilterParts(List<FilterInfo> filters)
	{
		Logger.Info("FilterParts");
		if (filters.Count == 0)
		{
			FilterJobs(new List<FilterInfo>());
			return;
		}
		List<JobInfo> list = FilterJobByMachine(_settings.Machines);
		List<PartInfo> list2 = new List<PartInfo>();
		foreach (JobInfo item in list)
		{
			list2.AddRange(item.Parts);
		}
		List<PartInfo> list3 = new ListFilter<PartInfo>().Filter(list2, filters);
		List<JobInfo> list4 = new List<JobInfo>();
		foreach (PartInfo item2 in list3)
		{
			if (!list4.Contains(item2.JobReference))
			{
				list4.Add(item2.JobReference);
			}
		}
		ModifyJobs(list4);
		List<PlateInfo> list5 = new List<PlateInfo>();
		foreach (PartInfo part in list3)
		{
			foreach (JobInfo item3 in list4)
			{
				List<PlateInfo> list6 = item3.Plates.FindAll((PlateInfo x) => x.PlateParts.Find((PlatePartInfo y) => y.PLATE_PART_NUMBER == part.PART_NUMBER) != null);
				if (EnumerableHelper.IsNullOrEmpty(list6))
				{
					continue;
				}
				foreach (PlateInfo item4 in list6)
				{
					if (!list5.Contains(item4))
					{
						list5.Add(item4);
					}
				}
			}
		}
		ModifyPlates(list5);
		ModifyParts(list3);
	}

	private void ActivateGridControllers()
	{
		if (View is JobDataControl jobDataControl)
		{
			Style rightAlignStyle = (Style)System.Windows.Application.Current.FindResource("StyleRightAlignedCell");
			DataTemplate template = (DataTemplate)jobDataControl.Resources["templateJobStatus"];
			_gridJobController = new DataGridManager<JobInfo>(jobDataControl.gridJobs, _settings.JobListConfiguration, rightAlignStyle, JobChanged, ShowJobHtml, template);
			_gridPlateController = new DataGridManager<PlateInfo>(jobDataControl.gridPlates, _settings.PlateListConfiguration, rightAlignStyle, PlateChanged, ShowPlateHtml, null);
			_gridPartController = new DataGridManager<PartInfo>(jobDataControl.gridPart, _settings.PartListConfiguration, rightAlignStyle, PartChanged, ShowPartHtml, null);
		}
	}

	private void ShowJobHtml(JobInfo job)
	{
		string directoryName = Path.GetDirectoryName(job.Path);
		if (_settings.HasToUseBatchForHtmlPreview)
		{
			string args = "1 " + directoryName + " 0 0";
			ShowWithBatch(_settings.HtmlPreviewBatchPath, args);
		}
		else
		{
			directoryName = Path.Combine(Path.GetDirectoryName(job.Path), "JOB.HTM");
			ShowHtmlPdf(directoryName);
		}
	}

	private List<JobInfo> ReadJobs(string path)
	{
		ConcurrentQueue<JobInfo> result = new ConcurrentQueue<JobInfo>();
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		Progress<int> progress = new Progress<int>(delegate
		{
			int readJobsAmount = ReadJobsAmount;
			ReadJobsAmount = readJobsAmount + 1;
		});
		List<Task> list = new List<Task>();
		List<DirectoryInfo> list2 = new List<DirectoryInfo>(directoryInfo.EnumerateDirectories());
		MaxJobsAmount = list2.Count;
		foreach (DirectoryInfo sdi in list2)
		{
			JobReader reader = new JobReader();
			Task item = Task.Factory.StartNew(delegate
			{
				JobInfo jobInfo = reader.ReadJob(sdi.FullName, _strategy, progress);
				if (jobInfo != null)
				{
					result.Enqueue(jobInfo);
				}
			});
			list.Add(item);
		}
		Task.WaitAll(list.ToArray());
		ReadJobsAmount = 0;
		List<JobInfo> list3 = result.ToList();
		list3.Sort((JobInfo x, JobInfo y) => y.DATE.CompareTo(x.DATE));
        if (exporting)
        {
            JobsExport(list3);
            exporting = false;
        }
        return list3;
	}

	private void JobChanged(List<JobInfo> jobs)
	{
		if (!_isUpdateFromPart)
		{
			Parts.Clear();
			_gridPartController.SelectedItems.Clear();
			_gridPlateController.SelectedItems.Clear();
			if (jobs.Count > 0)
			{
				UpdateJobs(jobs);
				ModifyPlates(jobs[0].Plates);
				_gridPlateController.SelectFirst();
			}
		}
	}

	private void UpdateJobs(List<JobInfo> jobs)
	{
		if (System.Windows.Application.Current.Dispatcher == null)
		{
			return;
		}
		if (System.Windows.Application.Current.Dispatcher.CheckAccess())
		{
			foreach (JobInfo job in jobs)
			{
				new JobUpdater(_strategy).Update(job);
			}
			return;
		}
		System.Windows.Application.Current.Dispatcher.Invoke(new Action<List<JobInfo>>(UpdateJobs), DispatcherPriority.Normal, jobs);
	}

	private void ShowPlateHtml(PlateInfo plate)
	{
		if (_settings.HasToUseBatchForHtmlPreview)
		{
			string directoryName = Path.GetDirectoryName(plate.Path);
			string args = $"2 {directoryName} {plate.PLATE_NUMBER} {plate.PLATE_NUMBER:D3}";
			ShowWithBatch(_settings.HtmlPreviewBatchPath, args);
		}
		else
		{
			ShowHtmlPdf(plate.HtmlPath());
		}
	}

	private void PlateChanged(List<PlateInfo> plates)
	{
		if (_isUpdateFromPart)
		{
			return;
		}
		Parts.Clear();
		_gridPartController.SelectedItems.Clear();
		if (plates.Count > 0)
		{
			new JobUpdater(_strategy).Update(plates[0].JobReference);
		}
		if (plates.Count <= 0)
		{
			return;
		}
		PlateInfo plateInfo = plates[0];
		List<PartInfo> list = new List<PartInfo>();
		foreach (PlatePartInfo platePart in plateInfo.PlateParts)
		{
			list.Add(platePart.Part);
		}
		ModifyParts(list);
		string path = plateInfo.PixelViewPath();
		_stateManager.NotifyPreviewChanged(path);
	}

	private void ShowPartHtml(PartInfo part)
	{
		if (_settings.HasToUseBatchForHtmlPreview)
		{
			string directoryName = Path.GetDirectoryName(part.Path);
			string args = $"3 {directoryName} {part.PART_NUMBER} {part.PART_NUMBER:D3}";
			ShowWithBatch(_settings.HtmlPreviewBatchPath, args);
		}
		else
		{
			ShowHtmlPdf(part.HtmlPath());
		}
	}

	private void PartChanged(List<PartInfo> parts)
	{
		if (parts.Count <= 0)
		{
			return;
		}
		PartInfo part = parts[0];
		string path = part.PixelViewPath();
		_stateManager.NotifyPreviewChanged(path);
		_isUpdateFromPart = true;
		if (_gridJobController.SelectedItems.Count == 0)
		{
			_gridJobController.Select(part.JobReference);
		}
		else if (_gridJobController.SelectedItems[0] != part.JobReference)
		{
			_gridJobController.Select(part.JobReference);
			List<PlateInfo> list = part.JobReference.Plates.FindAll((PlateInfo x) => x.PlateParts.Find((PlatePartInfo y) => y.PLATE_PART_NUMBER == part.PART_NUMBER) != null);
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				ModifyPlates(list);
				_gridPlateController.Select(list[0]);
			}
		}
		_isUpdateFromPart = false;
	}

	private void ModifyJobs(IEnumerable<JobInfo> jobs)
	{
		Jobs.Clear();
		foreach (JobInfo job in jobs)
		{
			Jobs.Add(job);
		}
		JobsFiltered = Jobs.Count;
	}

	private void ModifyPlates(IEnumerable<PlateInfo> plates)
	{
		Plates.Clear();
		foreach (PlateInfo plate in plates)
		{
			Plates.Add(plate);
		}
	}

	private void ModifyParts(IEnumerable<PartInfo> parts)
	{
		Parts.Clear();
		foreach (PartInfo part in parts)
		{
			Parts.Add(part);
		}
	}

	private void ShowHtmlPdf(string path)
	{
		if (!IOHelper.FileExists(path))
		{
			path = Path.ChangeExtension(path, "PDF");
		}
		if (IOHelper.FileExists(path))
		{
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo(path)
			{
				ErrorDialog = true,
				UseShellExecute = true
			};
			process.Start();
		}
	}

	private void ShowWithBatch(string path, string args)
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(path)
		{
			ErrorDialog = true,
			UseShellExecute = true,
			Arguments = args
		};
		process.Start();
	}
}

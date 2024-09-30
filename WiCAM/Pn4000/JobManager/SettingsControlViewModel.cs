using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class SettingsControlViewModel : ViewModelBase
{
	private readonly ConfigurableListControlViewModel _configurableListModel;

	private readonly IJobManagerSettings _settings;

	private bool _isChanged;

	private string _jobsPath;

	private string _jobsSavePath;

	private List<CppConfigurationLineInfo> _selectedListConfiguration;

	private ICommand _saveCommand;

	private ICommand _configureJobCommand;

	private ICommand _configurePlateCommand;

	private ICommand _configurePartCommand;

	private ICommand _configurePlatePartCommand;

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
			_isChanged = true;
		}
	}

	public string JobsSavePath
	{
		get
		{
			return _jobsSavePath;
		}
		set
		{
			_jobsSavePath = value;
			NotifyPropertyChanged("JobsSavePath");
		}
	}

	public ObservableCollection<ColumnConfigurationInfo> Columns { get; set; }

	public ICommand SaveCommand
	{
		get
		{
			if (_saveCommand == null)
			{
				_saveCommand = new RelayCommand(delegate
				{
					Save();
				}, (object x) => CanSave());
			}
			return _saveCommand;
		}
	}

	public ICommand ConfigureJobCommand
	{
		get
		{
			if (_configureJobCommand == null)
			{
				_configureJobCommand = new RelayCommand(delegate
				{
					ConfigureJob();
				}, (object x) => true);
			}
			return _configureJobCommand;
		}
	}

	public ICommand ConfigurePlateCommand
	{
		get
		{
			if (_configurePlateCommand == null)
			{
				_configurePlateCommand = new RelayCommand(delegate
				{
					ConfigurePlate();
				}, (object x) => true);
			}
			return _configurePlateCommand;
		}
	}

	public ICommand ConfigurePartCommand
	{
		get
		{
			if (_configurePartCommand == null)
			{
				_configurePartCommand = new RelayCommand(delegate
				{
					ConfigurePart();
				}, (object x) => true);
			}
			return _configurePartCommand;
		}
	}

	public ICommand ConfigurePlatePartCommand
	{
		get
		{
			if (_configurePlatePartCommand == null)
			{
				_configurePlatePartCommand = new RelayCommand(delegate
				{
					ConfigurePlatePart();
				}, (object x) => true);
			}
			return _configurePlatePartCommand;
		}
	}

	private void Save()
	{
		UpdateListSettings(_selectedListConfiguration);
		_settings.UserNames.Clear();
		_settings.UserNames.AddRange(_configurableListModel.Texts);
		_settings.JobDataPath = JobsPath;
		_settings.JobDataSavePath = JobsSavePath;
		_settings.SaveListSettings();
		Columns.Clear();
	}

	private bool CanSave()
	{
		bool flag = _configurableListModel.IsChanged;
		if (!flag)
		{
			flag = _isChanged;
			if (!flag)
			{
				flag = Columns.ToList().Find((ColumnConfigurationInfo x) => x.IsChanged) != null;
			}
		}
		return flag;
	}

	private void ConfigureJob()
	{
		AddToList(_settings.JobListConfiguration, typeof(JobInfo).GetProperties());
	}

	private void ConfigurePlate()
	{
		AddToList(_settings.PlateListConfiguration, typeof(PlateInfo).GetProperties());
	}

	private void ConfigurePart()
	{
		AddToList(_settings.PartListConfiguration, typeof(PartInfo).GetProperties());
	}

	private void ConfigurePlatePart()
	{
		AddToList(_settings.PlatePartListConfiguration, typeof(PlatePartInfo).GetProperties());
	}

	public SettingsControlViewModel(IJobManagerSettings settings, ConfigurableListControl configurableListView)
	{
		_settings = settings;
		Columns = new ObservableCollection<ColumnConfigurationInfo>();
		JobsPath = _settings.JobDataPath;
		JobsSavePath = _settings.JobDataSavePath;
		_configurableListModel = new ConfigurableListControlViewModel();
		_configurableListModel.AddTexts(_settings.UserNames);
		configurableListView.DataContext = _configurableListModel;
	}

	private void UpdateListSettings(List<CppConfigurationLineInfo> list)
	{
		foreach (ColumnConfigurationInfo item in Columns)
		{
			CppConfigurationLineInfo cppConfigurationLineInfo = list.Find((CppConfigurationLineInfo x) => x.Property.Name == item.Property.Name);
			if (item.IsSelected)
			{
				if (!item.IsDefault)
				{
					if (cppConfigurationLineInfo == null)
					{
						cppConfigurationLineInfo = new CppConfigurationLineInfo();
						cppConfigurationLineInfo.Key = item.KeyName;
						cppConfigurationLineInfo.Property = item.Property;
						cppConfigurationLineInfo.PropertyName = item.Property.Name;
						cppConfigurationLineInfo.Width = 70;
						list.Add(cppConfigurationLineInfo);
					}
					cppConfigurationLineInfo.Visibility = 1;
				}
				if (!string.IsNullOrEmpty(item.Translation))
				{
					cppConfigurationLineInfo.Caption = item.Translation;
				}
			}
			else if (!item.IsDefault && cppConfigurationLineInfo != null)
			{
				list.Remove(cppConfigurationLineInfo);
			}
		}
	}

	private void AddToList(List<CppConfigurationLineInfo> list, PropertyInfo[] properties)
	{
		if (!EnumerableHelper.IsNullOrEmpty(Columns.ToList().FindAll((ColumnConfigurationInfo x) => x.IsChanged)) && MessageHelper.Question(Application.Current.FindResource("MsgAskSaveListConfiguration").ToString()) == MessageBoxResult.Yes)
		{
			UpdateListSettings(list);
			_settings.SaveListSettings();
		}
		Columns.Clear();
		_selectedListConfiguration = list;
		foreach (PropertyInfo pi in properties)
		{
			if (pi.GetCustomAttribute<BrowsableAttribute>() != null)
			{
				continue;
			}
			ColumnConfigurationInfo columnConfigurationInfo = new ColumnConfigurationInfo(pi);
			CppConfigurationLineInfo cppConfigurationLineInfo = _selectedListConfiguration.Find((CppConfigurationLineInfo x) => x.Property.Name == pi.Name);
			if (cppConfigurationLineInfo != null)
			{
				columnConfigurationInfo.CppLine = cppConfigurationLineInfo;
				columnConfigurationInfo.Translate(cppConfigurationLineInfo.Caption);
				if (cppConfigurationLineInfo.Visibility > 0)
				{
					columnConfigurationInfo.Select();
				}
			}
			Columns.Add(columnConfigurationInfo);
		}
	}
}

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public abstract class FilterViewModelBase : ViewModelBase, IFilterViewModel, IViewModel
{
	protected IJobManagerServiceProvider Provider;

	protected DispatcherTimer Timer;

	private CppConfigurationLineInfo _selectedFirst;

	private CppConfigurationLineInfo _selectedSecond;

	private Visibility _searchStringVisibility;

	private string _searchStringFirst;

	private string _searchStringSecond;

	public IView View { get; protected set; }

	public ObservableCollection<CppConfigurationLineInfo> Values { get; set; }

	public CppConfigurationLineInfo SelectedFirst
	{
		get
		{
			return _selectedFirst;
		}
		set
		{
			_selectedFirst = value;
			NotifyPropertyChanged("SelectedFirst");
		}
	}

	public CppConfigurationLineInfo SelectedSecond
	{
		get
		{
			return _selectedSecond;
		}
		set
		{
			_selectedSecond = value;
			NotifyPropertyChanged("SelectedSecond");
		}
	}

	public Visibility SearchStringVisibility
	{
		get
		{
			return _searchStringVisibility;
		}
		set
		{
			_searchStringVisibility = value;
			NotifyPropertyChanged("SearchStringVisibility");
		}
	}

	public string SearchStringFirst
	{
		get
		{
			return _searchStringFirst;
		}
		set
		{
			_searchStringFirst = value;
			NotifyPropertyChanged("SearchStringFirst");
			if (value.Length > 0 && value.Length < 3)
			{
				SearchStringVisibility = Visibility.Visible;
				return;
			}
			SearchStringVisibility = Visibility.Hidden;
			Timer.Stop();
			Timer.Start();
		}
	}

	public string SearchStringSecond
	{
		get
		{
			return _searchStringSecond;
		}
		set
		{
			_searchStringSecond = value;
			NotifyPropertyChanged("SearchStringSecond");
			if (value.Length > 0 && value.Length < 3)
			{
				SearchStringVisibility = Visibility.Visible;
				return;
			}
			SearchStringVisibility = Visibility.Hidden;
			Timer.Stop();
			Timer.Start();
		}
	}

	public FilterViewModelBase()
	{
		SearchStringVisibility = Visibility.Hidden;
	}

	public void Initialize(IView view, IJobManagerServiceProvider provider)
	{
		View = view;
		Provider = provider;
		Values = new ObservableCollection<CppConfigurationLineInfo>();
	}

	public void ResetFilters()
	{
		SearchStringFirst = string.Empty;
		SearchStringSecond = string.Empty;
	}
}

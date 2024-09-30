using System;
using System.Collections.Generic;
using System.Windows.Threading;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public class FilterControlViewModel : FilterViewModelBase, IViewModel
{
	private IFilter _filter;

	public FilterControlViewModel()
	{
		if (Timer == null)
		{
			Timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(700.0)
			};
			Timer.Tick += Timer_Tick;
		}
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		RaiseFilterChanged();
	}

	public new void Initialize(IView view, IJobManagerServiceProvider provider)
	{
		base.Initialize(view, provider);
		_filter = Provider.JobFilter;
		CreateValues();
	}

	private void CreateValues()
	{
		List<CppConfigurationLineInfo> jobListConfiguration = Provider.FindService<IJobManagerSettings>().JobListConfiguration;
		if (EnumerableHelper.IsNullOrEmpty(jobListConfiguration))
		{
			return;
		}
		foreach (CppConfigurationLineInfo item in jobListConfiguration)
		{
			if (item.Visibility > 0)
			{
				base.Values.Add(item);
			}
		}
		base.SelectedFirst = jobListConfiguration[0];
		base.SelectedSecond = jobListConfiguration[1];
	}

	private void RaiseFilterChanged()
	{
		Timer.Stop();
		List<FilterInfo> list = new List<FilterInfo>();
		if (!string.IsNullOrEmpty(base.SearchStringFirst))
		{
			list.Add(new FilterInfo(base.SelectedFirst.PropertyName, string.Empty, base.SearchStringFirst));
		}
		if (!string.IsNullOrEmpty(base.SearchStringSecond))
		{
			list.Add(new FilterInfo(base.SelectedSecond.PropertyName, string.Empty, base.SearchStringSecond));
		}
		_filter.FilterJobs(list);
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Threading;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public class FilterPartControlViewModel : FilterViewModelBase, IViewModel
{
	private IFilterParts _filter;

	public FilterPartControlViewModel()
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
		_filter = Provider.PartFilter;
		CreateValues();
	}

	private void RaiseFilterChanged()
	{
		Timer.Stop();
		List<FilterInfo> list = new List<FilterInfo>();
		if (!string.IsNullOrEmpty(base.SearchStringFirst) && base.SearchStringFirst.Length > 2)
		{
			list.Add(new FilterInfo(base.SelectedFirst.PropertyName, string.Empty, base.SearchStringFirst));
		}
		if (!string.IsNullOrEmpty(base.SearchStringSecond) && base.SearchStringSecond.Length > 2)
		{
			list.Add(new FilterInfo(base.SelectedSecond.PropertyName, string.Empty, base.SearchStringSecond));
		}
		_filter.FilterParts(list);
	}

	private void CreateValues()
	{
		List<CppConfigurationLineInfo> partListConfiguration = Provider.FindService<IJobManagerSettings>().PartListConfiguration;
		if (EnumerableHelper.IsNullOrEmpty(partListConfiguration))
		{
			return;
		}
		foreach (CppConfigurationLineInfo item in partListConfiguration)
		{
			if (item.Visibility > 0)
			{
				base.Values.Add(item);
			}
		}
		base.SelectedFirst = partListConfiguration[0];
		base.SelectedSecond = partListConfiguration[1];
	}
}

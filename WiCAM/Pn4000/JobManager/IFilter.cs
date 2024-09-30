using System.Collections.Generic;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public interface IFilter
{
	void FilterJobs(List<FilterInfo> filters);
}

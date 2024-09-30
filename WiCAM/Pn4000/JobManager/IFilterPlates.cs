using System.Collections.Generic;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public interface IFilterPlates
{
	void FilterPlates(List<FilterInfo> filters);
}

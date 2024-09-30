using System.Collections.Generic;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.JobManager;

public interface IFilterParts
{
	void FilterParts(List<FilterInfo> filters);
}

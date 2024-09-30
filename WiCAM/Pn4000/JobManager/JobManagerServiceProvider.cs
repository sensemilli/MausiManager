using System.Collections.Generic;
using System.Linq;

namespace WiCAM.Pn4000.JobManager;

public class JobManagerServiceProvider : IJobManagerServiceProvider
{
	private readonly IEnumerable<IService> _services;

	public IFilter JobFilter { get; set; }

	public IFilterParts PartFilter { get; set; }

	public IFilterPlates PlateFilter { get; set; }

	public JobManagerServiceProvider(IEnumerable<IService> services)
	{
		_services = services;
	}

	public T FindService<T>() where T : IService
	{
		return _services.OfType<T>().First();
	}
}

namespace WiCAM.Pn4000.JobManager;

public interface IJobManagerServiceProvider
{
	IFilter JobFilter { get; set; }

	IFilterParts PartFilter { get; set; }

	IFilterPlates PlateFilter { get; set; }

	T FindService<T>() where T : IService;
}

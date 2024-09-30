namespace WiCAM.Pn4000.JobManager;

public interface IFilterViewModel : IViewModel
{
	string SearchStringFirst { get; set; }

	string SearchStringSecond { get; set; }

	void ResetFilters();
}

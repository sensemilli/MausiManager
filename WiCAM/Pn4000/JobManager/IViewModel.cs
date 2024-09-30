namespace WiCAM.Pn4000.JobManager;

public interface IViewModel
{
	IView View { get; }

	void Initialize(IView view, IJobManagerServiceProvider provider);
}

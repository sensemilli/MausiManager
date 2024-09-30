namespace WiCAM.Pn4000.JobManager;

public interface IDialogViewModel
{
	IDialogView View { get; }

	void Initialize(IDialogView view, IJobManagerServiceProvider provider);

	bool Show();
}

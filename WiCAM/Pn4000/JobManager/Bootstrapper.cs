namespace WiCAM.Pn4000.JobManager;

public class Bootstrapper
{
	private IDialogViewModel _model;

	private readonly IJobManagerSettings _settings;

	private readonly IJobManagerServiceProvider _provider;

	public Bootstrapper(IJobManagerServiceProvider provider)
	{
		_provider = provider;
		_settings = _provider.FindService<IJobManagerSettings>();
	}

	public bool Show()
	{
		_model = _settings.ModelManager.RegisterDialog<MainWindow, MainWindowViewModel>(_provider);
		return _model.Show();
	}
}

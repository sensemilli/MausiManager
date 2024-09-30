namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal class LabelPrinterFactory
{
	private readonly IJobManagerServiceProvider _provider;

	public LabelPrinterFactory(IJobManagerServiceProvider provider)
	{
		_provider = provider;
	}

	public ILabelPrinter Find()
	{
		return new LabelPrinterLocal();
	}
}

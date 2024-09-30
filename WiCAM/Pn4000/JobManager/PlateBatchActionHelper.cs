using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class PlateBatchActionHelper
{
	private readonly IJobManagerSettings _settings;

	public PlateBatchActionHelper(IJobManagerServiceProvider provider)
	{
		_settings = provider.FindService<IJobManagerSettings>();
	}

	internal void PlateDeleteBatch(string jobName, string ncProgram, string datPath, int machineNumber)
	{
		ExecutePlateBatch(_settings.PlateDeleteBatchPath, PlateActionType.Delete, ncProgram, machineNumber, datPath, jobName);
	}

	internal void PlateSaveBatch(string jobName, string ncProgram, string datPath, int machineNumber)
	{
		ExecutePlateBatch(_settings.PlateSaveBatchPath, PlateActionType.Save, ncProgram, machineNumber, datPath, jobName);
	}

	private void ExecutePlateBatch(string batchPath, PlateActionType action, string ncProgram, int machineNumber, string datPath, string jobName)
	{
		string processArguments = string.Join(" ", (int)action, ncProgram, machineNumber, "\"" + datPath + "\"", jobName);
		ProcessHelper.ExecuteNoWait(batchPath, processArguments);
	}
}

using System.Globalization;
using System.IO;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class Pn2PpsHelper
{
	public static readonly string __path = "u\\pn\\bin\\pn2pps.bat";

	private readonly IJobManagerSettings _settings;

	private readonly string _batchPath;

	public Pn2PpsHelper(IJobManagerSettings settings)
	{
		_settings = settings;
		_batchPath = PnPathBuilder.PathInPnDrive(__path);
	}

	public bool ExecuteProduce(PlateProductionInfo productionInfo)
	{
		return ExecuteBatch(productionInfo, FeedbackType.Produced, productionInfo.AmountToProduce);
	}

	public bool ExecuteStorno(PlateProductionInfo productionInfo)
	{
		return ExecuteBatch(productionInfo, FeedbackType.Storno, productionInfo.AmountStorno);
	}

	public bool ExecuteReject(PlateProductionInfo productionInfo)
	{
		return ExecuteBatch(productionInfo, FeedbackType.Reject, productionInfo.AmountStorno);
	}

	private bool ExecuteBatch(PlateProductionInfo productionInfo, FeedbackType actionNumber, int amount)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} \"{3}\" {4} {5}", (int)actionNumber, productionInfo.Plate.PLATE_HEADER_TXT_1, productionInfo.Plate.PLATE_MACHINE_NO, BuildDatPath(_settings.JobDataPath, productionInfo.Plate.JobReference.JOB_DATA_1, productionInfo), productionInfo.Plate.JobReference.JOB_DATA_1, amount);
		Logger.Info("   ARGS = '{0}'", text);
		ProcessHelper.ExecuteNoWait(_batchPath, text);
		return true;
	}

	private string BuildDatPath(string jobDataPath, string jobName, PlateProductionInfo plate)
	{
		return Path.Combine(jobDataPath, Path.Combine(jobName, string.Format(CultureInfo.InvariantCulture, "PLAT_{0:D3}.DAT", plate.Plate.PLATE_NUMBER)));
	}
}

using System.Collections.Generic;

namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal class LabelPrintManager
{
	private readonly IJobManagerServiceProvider _provider;

	private readonly ILabelPrinter _printer;

	private readonly string _jobDataPath;

	public LabelPrintManager(IJobManagerServiceProvider provider, string jobDataPath)
	{
		_provider = provider;
		_jobDataPath = jobDataPath;
		_printer = new LabelPrinterFactory(_provider).Find();
	}

	public void PrintJobLabels(JobInfo data)
	{
		IJobManagerSettings jobManagerSettings = _provider.FindService<IJobManagerSettings>();
		if (jobManagerSettings.LabelPrintOrder == 0)
		{
			PrintPartLabels(data.Parts);
			{
				foreach (PlateInfo plate in data.Plates)
				{
					PrintRestPlateLabels(plate);
				}
				return;
			}
		}
		if (jobManagerSettings.LabelPrintOrder != 1)
		{
			return;
		}
		new LabelCreator(data).Create();
		foreach (PlateInfo plate2 in data.Plates)
		{
			PrintPlateLabels(plate2);
			PrintRestPlateLabels(plate2);
		}
	}

	public void PrintPlateLabels(PlateInfo plate)
	{
		string jOB_DATA_ = plate.JobReference.JOB_DATA_1;
		_printer.PrintPlate(plate, _jobDataPath, jOB_DATA_);
	}

	public void PrintRestPlateLabels(PlateInfo plate)
	{
		if (!string.IsNullOrEmpty(plate.PLATE_NAME_REST))
		{
			_ = plate.JobReference.JOB_DATA_1;
		}
	}

	public void PrintSinglePartLabels(PartInfo part)
	{
		string jOB_DATA_ = part.JobReference.JOB_DATA_1;
		_printer.PrintSinglePlatePart(part.PART_NUMBER, _jobDataPath, jOB_DATA_);
	}

	public void PrintPartLabels(List<PartInfo> data)
	{
		string jOB_DATA_ = data[0].JobReference.JOB_DATA_1;
		_printer.PrintAllPartsInJob(data, _jobDataPath, jOB_DATA_);
	}
}

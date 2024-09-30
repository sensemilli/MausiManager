using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal class LabelPrinterLocal : LabelPrinterBase, ILabelPrinter
{
	private readonly string _labelPrinterBatchPath;

	public LabelPrinterLocal()
	{
		_labelPrinterBatchPath = PnPathBuilder.PathInPnDrive("u\\pn\\bin\\labelPrint.bat");
	}

	public void PrintAllPartsInJob(object partsObject, string jobDataPath, string jobName)
	{
		foreach (PartInfo item in (IEnumerable<PartInfo>)partsObject)
		{
			string processArguments = BuildPartPath(jobDataPath, jobName, item.PART_NUMBER);
			ProcessHelper.ExecuteProcess(_labelPrinterBatchPath, processArguments);
		}
	}

	public void PrintPlate(object plateObject, string jobDataPath, string jobName)
	{
		PlateInfo plateInfo = plateObject as PlateInfo;
		string processArguments = BuildPlatePath(jobDataPath, jobName, plateInfo.PLATE_NUMBER);
		ProcessHelper.ExecuteProcess(_labelPrinterBatchPath, processArguments);
	}

	public void PrintRestPlate(object plateObject, string jobDataPath, string jobName)
	{
		throw new NotImplementedException();
	}

	public void PrintSinglePart(object dataObject, string jobDataPath, string jobName)
	{
		PartInfo partInfo = dataObject as PartInfo;
		string processArguments = BuildPartPath(jobDataPath, jobName, partInfo.PART_NUMBER);
		ProcessHelper.ExecuteProcess(_labelPrinterBatchPath, processArguments);
	}

	public void PrintSinglePlatePart(int partNumber, string jobDataPath, string jobName)
	{
		string processArguments = BuildPartPath(jobDataPath, jobName, partNumber);
		ProcessHelper.ExecuteProcess(_labelPrinterBatchPath, processArguments);
	}
}

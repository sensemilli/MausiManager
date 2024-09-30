namespace WiCAM.Pn4000.JobManager.LabelPrinter;

public interface ILabelPrinter
{
	void PrintAllPartsInJob(object partsObject, string jobDataPath, string jobName);

	void PrintPlate(object plateObject, string jobDataPath, string jobName);

	void PrintRestPlate(object plateObject, string jobDataPath, string jobName);

	void PrintSinglePart(object dataObject, string jobDataPath, string jobName);

	void PrintSinglePlatePart(int partNumber, string jobDataPath, string jobName);
}

namespace WiCAM.Pn4000.JobManager;

public class JobUpdater
{
	private readonly ReadJobStrategyInfo _jobStrategy;

	public JobUpdater(ReadJobStrategyInfo strategy)
	{
		_jobStrategy = strategy;
	}

	public void Update(JobInfo job)
	{
		DatReader<PlateInfo> datReader = new DatReader<PlateInfo>();
		foreach (PlateInfo plate in job.Plates)
		{
			datReader.UpdatePlateFromDat(plate, plate.Path, _jobStrategy.PlateStrategy, _jobStrategy.PlatePartStrategy);
		}
		DatReader<PartInfo> datReader2 = new DatReader<PartInfo>();
		foreach (PartInfo part in job.Parts)
		{
			datReader2.UpdateFromDat(part, part.Path, _jobStrategy.PartStrategy);
		}
		job.CheckStatus();
	}
}

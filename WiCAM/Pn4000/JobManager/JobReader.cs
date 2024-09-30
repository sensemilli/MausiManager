using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class JobReader
{
	private static ReadJobStrategyInfo __jobStrategy;

	public JobInfo ReadJob(string path, ReadJobStrategyInfo strategy, IProgress<int> progress)
	{
		if (__jobStrategy == null)
		{
			__jobStrategy = strategy;
		}
		string path2 = Path.Combine(path, "JOB.DAT");
		if (!IOHelper.FileExists(path2))
		{
			progress?.Report(1);
			return null;
		}
		JobInfo jobInfo = new DatReader<JobInfo>().Read(path2, strategy.JobStrategy);
		if (jobInfo == null)
		{
			jobInfo = new DatReader<JobInfo>().Read(Path.Combine(path, "PART_001.DAT"), strategy.JobStrategy);
			if (jobInfo != null)
			{
				jobInfo.Status = -1;
			}
			else
			{
				Logger.Error("DAT files are not found. Path={0}", path);
			}
		}
		if (jobInfo != null)
		{
			jobInfo.Path = path2;
			jobInfo.Plates.AddRange(ReadPlatesLocal(jobInfo, path, strategy.PlateStrategy, strategy.PlatePartStrategy));
			jobInfo.Parts.AddRange(ReadPartsLocal(jobInfo, path, strategy.PartStrategy));
			jobInfo.CheckStatus();
			foreach (PlateInfo plate in jobInfo.Plates)
			{
				FindParts(jobInfo, plate);
			}
			if (jobInfo.Status == -1)
			{
				jobInfo.AMOUNT_DIFF_PLATES = jobInfo.Plates.Count;
				jobInfo.AMOUNT_DIFF_PARTS = jobInfo.Parts.Count;
				jobInfo.Status = 0;
			}
		}
		progress?.Report(1);
		return jobInfo;
	}

	private List<PartInfo> ReadPartsLocal(JobInfo job, string path, ReadStrategyInfo strategy)
	{
		List<PartInfo> list = new List<PartInfo>();
		IEnumerable<FileInfo> enumerable = IOHelper.DirectoryFileInfos(path, "PART_*.DAT");
		if (!enumerable.Any())
		{
			return list;
		}
		foreach (FileInfo item in enumerable)
		{
			if (item.Name.Length == 12)
			{
				PartInfo partInfo = ReadPart(item, strategy);
				partInfo.JobReference = job;
				list.Add(partInfo);
			}
		}
		return list;
	}

	private List<PlateInfo> ReadPlatesLocal(JobInfo job, string path, ReadStrategyInfo strategy, ReadStrategyInfo strategyPart)
	{
		List<PlateInfo> list = new List<PlateInfo>();
		IEnumerable<FileInfo> enumerable = IOHelper.DirectoryFileInfos(path, "PLAT_*.DAT");
		if (!enumerable.Any())
		{
			return list;
		}
		foreach (FileInfo item in enumerable)
		{
			if (item.Name.Length == 12)
			{
				PlateInfo plateInfo = ReadPlate(item, strategy, strategyPart);
				if (plateInfo != null)
				{
					plateInfo.JobReference = job;
					list.Add(plateInfo);
				}
			}
		}
		return list;
	}

	public List<PartInfo> FindParts(JobInfo job, PlateInfo plate)
	{
		List<PartInfo> list = new List<PartInfo>();
		foreach (PlatePartInfo platePart in plate.PlateParts)
		{
			PartInfo partInfo = job.Parts.Find((PartInfo x) => x.PART_NUMBER == platePart.PLATE_PART_NUMBER);
			if (partInfo != null)
			{
				platePart.Part = partInfo;
				list.Add(partInfo);
			}
		}
		return list;
	}

	private PartInfo ReadPart(FileInfo file, ReadStrategyInfo strategy)
	{
		PartInfo partInfo = new DatReader<PartInfo>().Read(file.FullName, strategy);
		partInfo.Path = file.FullName;
		return partInfo;
	}

	private PlateInfo ReadPlate(FileInfo file, ReadStrategyInfo strategy, ReadStrategyInfo strategyPart)
	{
		PlateInfo plateInfo = new DatReader<PlateInfo>().ReadPlate(file.FullName, strategy, strategyPart);
		if (plateInfo != null)
		{
			plateInfo.Path = file.FullName;
			if (plateInfo.RestOfProduction() <= 0)
			{
				plateInfo.PLATE_STATUS = 3;
			}
		}
		return plateInfo;
	}
}

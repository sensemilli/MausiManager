using System;
using System.IO;
using System.Text;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.SchachtelBasis;
using WiCAM.Pn4000.SchachtelBasis.Data;

namespace WiCAM.Pn4000.JobManager.Classes;

public class SchachtelBasisUpdater
{
	private readonly int _jobArchiveNumber;

	private readonly string _jobName;

	private readonly int _plateNumber;

	public SchachtelBasisUpdater(string jobArchiveNumber, string jobName, string plateNumber)
	{
		_jobArchiveNumber = StringHelper.ToInt(jobArchiveNumber);
		_jobName = jobName;
		_plateNumber = StringHelper.ToInt(plateNumber);
	}

	public bool ModifySchbas(int newStatus)
	{
		try
		{
			ArchiveStructureManager archiveStructureManager = new ArchiveStructureManager();
			ArchiveInfo archiveInfo = archiveStructureManager.Read().Find((ArchiveInfo x) => x.FullArchiveNumber() == _jobArchiveNumber);
			if (archiveInfo == null)
			{
				Logger.Error("Archive with number = {0} is not found", _jobArchiveNumber);
				return false;
			}
			archiveStructureManager.ReadArchivePaths(archiveInfo);
			string text = Path.Combine(archiveInfo.Paths.Find((ArchivePathInfo x) => x.FolderType == ArchiveFolderType.sba).Path, _jobName);
			if (!File.Exists(text))
			{
				Logger.Error("File " + text + " is not found!");
				return false;
			}
			return ModifyLocal(text, newStatus);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
		return false;
	}

	private bool ModifyLocal(string path, int newStatus)
	{
		SchbasReader schbasReader = new SchbasReader();
		SchbasWriter schbasWriter = new SchbasWriter();
		Schbas schbas = schbasReader.ReadFromSba(path);
		Plate plate = schbas.Plates[_plateNumber - 1];
		string oldValue = schbasWriter.ToPlateString(plate, schbas.Version);
		plate.INFOA0 = newStatus;
		string newValue = schbasWriter.ToPlateString(plate, schbas.Version);
		StringBuilder stringBuilder = new StringBuilder(IOHelper.FileReadAllText(path));
		stringBuilder.Replace(oldValue, newValue);
		return IOHelper.FileWriteAllText(path, stringBuilder.ToString());
	}
}

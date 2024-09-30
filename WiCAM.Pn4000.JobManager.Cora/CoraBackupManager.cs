using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Interfaces;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class CoraBackupManager
{
	private readonly string _path;

	private readonly string _backupPath;

	private readonly string _blockFilePath;

	private readonly BackupFilesManager _filesManager;

	public CoraBackupManager()
	{
		_path = PnPathBuilder.PathInPnDrive("u\\sfa\\jobdata\\cora");
		_backupPath = Path.Combine(_path, "backup");
		_blockFilePath = Path.Combine(_path, "coraUpdateRunning.txt");
		IOHelper.DirectoryCreate(_path);
		IOHelper.DirectoryCreate(_backupPath);
		_filesManager = new BackupFilesManager(_backupPath, string.Empty, 1000);
	}

	public bool CheckIsRunning()
	{
		bool flag = IOHelper.FileExists(_blockFilePath);
		if (flag)
		{
			string[] array = IOHelper.FileReadAllLines(_blockFilePath);
			Logger.Info("--- JobManager_TO_CORA : Cora update is running");
			Logger.Info("--- JobManager_TO_CORA : " + array[0]);
			Logger.Info("--- JobManager_TO_CORA : " + array[1]);
			if (array[1].Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
			{
				return false;
			}
		}
		return flag;
	}

	public void StartRunning()
	{
		StringBuilder stringBuilder = new StringBuilder(5000);
		stringBuilder.AppendLine(DateTime.Now.ToString("s"));
		stringBuilder.AppendLine(Environment.MachineName);
		IOHelper.FileWriteAllText(_blockFilePath, stringBuilder.ToString());
	}

	public void StopRunning()
	{
		IOHelper.FileDelete(_blockFilePath);
	}

	public void BackUp(string path)
	{
		string destination = Path.Combine(Path.GetDirectoryName(path), "backup", Path.GetFileName(path));
		IOHelper.FileCopy(path, destination);
		IOHelper.FileDelete(path);
		Task.Run(() => _filesManager.ControlAsync());
	}

	public void Save(PlateProductionInfo plate, IPlate plateDat, int action)
	{
		string path = Path.Combine(_path, $"{plate.Plate.PLATE_HEADER_TXT_1}_{DateTime.Now:yyyyMMdd_HHmmss_fffff}.xml");
		SaveUnit item = new SaveUnit
		{
			ProductionStatus = action,
			Path = plate.Plate.Path,
			Plate = plate
		};
		plate.IsSavedForCora = true;
		SerializeHelper.SerializeToXml(item, path);
	}

	public List<SaveUnit> ReadSaved()
	{
		List<SaveUnit> list = new List<SaveUnit>();
		IEnumerable<FileInfo> enumerable = IOHelper.DirectoryFileInfos(_path, "*.xml");
		if (!enumerable.Any())
		{
			return list;
		}
		foreach (FileInfo item in enumerable)
		{
			SaveUnit saveUnit = SerializeHelper.DeserialiseFromXml<SaveUnit>(item.FullName);
			if (saveUnit != null)
			{
				saveUnit.Path = item.FullName;
				list.Add(saveUnit);
			}
		}
		return list;
	}
}

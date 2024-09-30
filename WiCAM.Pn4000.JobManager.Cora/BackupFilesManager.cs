using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class BackupFilesManager
{
	private readonly string _path;

	private readonly string _searchMask;

	private readonly int _maxAmount;

	public BackupFilesManager(string path, string searchMask, int maxAmount)
	{
		_path = path;
		_searchMask = searchMask;
		_maxAmount = maxAmount;
		if (string.IsNullOrEmpty(_searchMask))
		{
			_searchMask = "*.*";
		}
	}

	public bool Control()
	{
		List<FileInfo> files = FindFilesToDelete(_maxAmount);
		return DeleteFiles(files);
	}

	public async Task<bool> ControlAsync()
	{
		return await DeleteFilesAsync(await FindFilesToDeleteAsync(_maxAmount));
	}

	private async Task<List<FileInfo>> FindFilesToDeleteAsync(int maxFiles)
	{
		return await Task.Run(() => FindFilesToDelete(_maxAmount));
	}

	private async Task<bool> DeleteFilesAsync(List<FileInfo> files)
	{
		return await Task.Run(() => DeleteFiles(files));
	}

	private bool DeleteFiles(List<FileInfo> files)
	{
		foreach (FileInfo file in files)
		{
			file.Delete();
		}
		return true;
	}

	private List<FileInfo> FindFilesToDelete(int maxFiles)
	{
		List<FileInfo> list = new List<FileInfo>();
		List<FileInfo> list2 = new List<FileInfo>(new DirectoryInfo(_path).GetFiles(_searchMask));
		if (maxFiles > list2.Count)
		{
			return list;
		}
		list2.Sort((FileInfo x, FileInfo y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
		while (list2.Count > maxFiles)
		{
			list.Add(list2[0]);
			list2.RemoveAt(0);
		}
		return list;
	}
}

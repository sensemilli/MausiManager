using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Zipper;

public class F7Zip
{
	private const string SERVICETXT = "service.txt";

	private const string MACHINEBEND = "machine_bend";

	private const string PNUSER = "pnuser";

	private const string MODEL3DEXT = ".c3do";

	private const string ExtractMachineBend = "u\\pn\\machine_bend";

	private const string DocFilename = "doc.c3do";

	private const string MetaInfoFilename = "info.json";

	private readonly IPnPathService _pnPathService;

	private readonly IConfigProvider _configProvider;

	private readonly IDoc3d _doc;

	private string _homePath;

	private string _pnFolderService;

	private string _pnFolderServiceTemplate;

	private MetaInfo _metaInfo;

	private Encoding Encoding => Encoding.UTF8;

	public string ZipFilepath { get; } = string.Empty;

	public string BundleId { get; private set; }

	private string TempPath => Path.GetTempPath();

	public F7Zip(string zipFilepath, IPnPathService pnPathService, IConfigProvider configProvider, IDoc3d doc)
	{
		this.ZipFilepath = zipFilepath;
		this._pnPathService = pnPathService;
		this._configProvider = configProvider;
		this._doc = doc;
	}

	public F7Zip(string zipFilepath, string serviceBase, string servicePath)
	{
		this.ZipFilepath = zipFilepath;
		if (string.IsNullOrEmpty(servicePath))
		{
			servicePath = "unnamed";
		}
		this._pnFolderService = Path.Combine(serviceBase, servicePath ?? "");
		this._pnFolderServiceTemplate = Path.Combine(serviceBase, "default");
	}

	private string ReplaceFilenameVariables(string filename)
	{
		return filename?.Replace("%PNHOMEPATH%", this._homePath).Replace("%PNDRIVE%", this._pnPathService.PNDRIVE);
	}

	public bool Create(string desc)
	{
		if (this._pnPathService == null)
		{
			this._metaInfo = null;
			return false;
		}
		this._homePath = Path.Combine(this._pnPathService.PNHOMEDRIVE, this._pnPathService.PNHOMEPATH.TrimStart('\\'));
		string machinePath = this._doc.MachinePath;
		F7Config f7Config = this._configProvider.InjectOrCreate<F7Config>();
		List<string> list = f7Config.ItemsToZip.ToList();
		if (f7Config.Version < 1)
		{
			list = list.Select(delegate(string x)
			{
				int num = x.LastIndexOf(' ');
				return (num >= 0) ? (x.Substring(0, num) + "|" + x.Substring(num + 1)) : x;
			}).ToList();
		}
		this.CreateBasis();
		this.AddFoldersToZip(list);
		this.Add3dModelToZip(this._doc, "doc.c3do");
		this.AddMachineFolderToZip(machinePath);
		string pnVersion = File.ReadAllText(Path.Combine(this._pnPathService.FolderGFiles, "version.txt")).Replace(Environment.NewLine, "");
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(this._pnPathService.FolderRun, "Pkernel.dll"));
		string abasId = File.ReadAllLines(this._pnPathService.PnMasterOrDrive + "\\u\\pn\\config\\licens\\SERVER")[4].Split(":")[1].Trim();
		string customerName = File.ReadAllLines(this._pnPathService.PnMasterOrDrive + "\\u\\pn\\config\\licens\\SERVER")[5].Split(":")[1].Trim();
		string userName = Environment.UserName;
		string machineName = Environment.MachineName;
		bool demoVersion = File.Exists(this._pnPathService.GetUserFilePath("WI223B"));
		this._metaInfo = new MetaInfo
		{
			Desc = desc?.Trim(),
			PnVersion = pnVersion,
			PkernelVersion = versionInfo.ProductVersion,
			PkernelDesc = versionInfo.FileDescription,
			AbasId = abasId,
			UserName = userName,
			MachineName = machineName,
			CustomerName = customerName,
			DemoVersion = demoVersion
		};
		this.AddMetaInfoToZip(this._metaInfo);
		return true;
	}

	private void CreateBasis()
	{
		if (!this.CreateDirectoryForZip(this.ZipFilepath) || !Directory.Exists(Path.GetDirectoryName(this.ZipFilepath)))
		{
			return;
		}
		if (File.Exists(this.ZipFilepath))
		{
			File.Delete(this.ZipFilepath);
		}
		using (ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Create))
		{
		}
		using (File.Create(Path.Combine(this.TempPath, "service.txt")))
		{
		}
	}

	private bool CreateDirectoryForZip(string filepath)
	{
		try
		{
			if (string.IsNullOrEmpty(filepath))
			{
				return false;
			}
			if (Directory.Exists(Path.GetDirectoryName(filepath)))
			{
				return true;
			}
			Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? string.Empty);
			if (Directory.Exists(Path.GetDirectoryName(filepath)))
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	public void AddFoldersToZip(List<string> folders)
	{
		if (folders == null || !folders.Any())
		{
			return;
		}
		using ZipArchive zipArchive = ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Update);
		for (int i = 0; i < folders.Count; i++)
		{
			string text = this.ReplaceFilenameVariables(folders[i].Split('|')?.FirstOrDefault());
			string text2 = folders[i].Split('|')?.LastOrDefault() ?? "*";
			if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
			{
				continue;
			}
			List<string> list = new List<string>();
			if (text2 == "*")
			{
				list = Directory.EnumerateFiles(text, text2, SearchOption.AllDirectories).ToList();
			}
			if (int.TryParse(text2, out var result))
			{
				list = (from f in Directory.EnumerateFiles(text, "*", SearchOption.TopDirectoryOnly).ToList()
					select new FileInfo(f) into f
					orderby f.LastWriteTimeUtc descending
					select f.FullName).ToList().Take(result).ToList();
			}
			if (text2.Contains("*") && text2 != "*")
			{
				list = Directory.EnumerateFiles(text, text2, SearchOption.TopDirectoryOnly).ToList();
			}
			if (!list.Any())
			{
				continue;
			}
			foreach (string item in list)
			{
				string entryName = Path.Combine(i.ToString(), Path.GetRelativePath(text, item));
				try
				{
					zipArchive.CreateEntryFromFile(item, entryName);
				}
				catch (Exception ex)
				{
					if (!(ex is IOException))
					{
						continue;
					}
					try
					{
						using (Process process = Process.Start(new ProcessStartInfo
						{
							UseShellExecute = false,
							RedirectStandardOutput = true,
							FileName = "cmd.exe",
							Arguments = $"/C copy \"{item}\" \"{this.TempPath}\""
						}))
						{
							process.WaitForExit();
						}
						zipArchive.CreateEntryFromFile(Path.Combine(this.TempPath, Path.GetFileName(item)), entryName);
						if (File.Exists(Path.Combine(this.TempPath, Path.GetFileName(item))))
						{
							File.Delete(Path.Combine(this.TempPath, Path.GetFileName(item)));
						}
					}
					catch (Exception)
					{
					}
				}
			}
		}
		using (StreamWriter streamWriter = File.AppendText(Path.Combine(this.TempPath, "service.txt")))
		{
			foreach (string folder in folders)
			{
				streamWriter.WriteLine(folder);
			}
		}
		zipArchive.GetEntry("service.txt")?.Delete();
		zipArchive.CreateEntryFromFile(Path.Combine(this.TempPath, "service.txt"), "service.txt");
	}

	public void Add3dModelToZip(IDoc3d doc3d, string modelFilename)
	{
		if (string.IsNullOrEmpty(modelFilename) || string.IsNullOrEmpty(this.ZipFilepath))
		{
			return;
		}
		string text = Path.Combine(this.TempPath, modelFilename);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		doc3d.Save(text);
		using ZipArchive destination = ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Update);
		destination.CreateEntryFromFile(text, Path.GetFileName(text));
	}

	public void AddMachineFolderToZip(string sourceFolderpath)
	{
		if (string.IsNullOrEmpty(this.ZipFilepath) || string.IsNullOrEmpty(sourceFolderpath) || !Directory.Exists(sourceFolderpath))
		{
			return;
		}
		string path = Path.Combine(new DirectoryInfo(sourceFolderpath).Parent.FullName, "globals");
		if (!Directory.Exists(path))
		{
			return;
		}
		string relativeTo = new DirectoryInfo(sourceFolderpath).Parent?.FullName ?? "";
		using ZipArchive destination = ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Update);
		foreach (string item in Directory.EnumerateFiles(sourceFolderpath, "*", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)).ToList())
		{
			destination.CreateEntryFromFile(item, Path.Combine("machine_bend", Path.GetRelativePath(relativeTo, item)));
		}
	}

	public void AddMetaInfoToZip(MetaInfo info)
	{
		if (string.IsNullOrEmpty(this.ZipFilepath))
		{
			return;
		}
		using ZipArchive destination = ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Update);
		string text = Path.Combine(this.TempPath, "info.json");
		File.WriteAllText(text, JsonSerializer.Serialize(info), this.Encoding);
		destination.CreateEntryFromFile(text, "info.json");
		File.Delete(text);
	}

	public async Task<string> Upload()
	{
		return await SyncFile.Upload(this.ZipFilepath, this._metaInfo);
	}

	private string ReplaceFilenameVariablesExtraction(string filename)
	{
		return filename?.Replace("%PNHOMEPATH%", Path.Combine(this._pnFolderService, "pnuser")).Replace("%PNDRIVE%", this._pnFolderService);
	}

	private void CopyDirectory(string sourceFolder, string destinationFolder)
	{
		if (Directory.Exists(sourceFolder))
		{
			Console.WriteLine("copy " + sourceFolder + " to " + destinationFolder);
			Directory.CreateDirectory(destinationFolder);
			string[] directories = Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.CreateDirectory(directories[i].Replace(sourceFolder, destinationFolder));
			}
			directories = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
			foreach (string obj in directories)
			{
				File.Copy(obj, obj.Replace(sourceFolder, destinationFolder), overwrite: true);
			}
		}
	}

	private void CopyFilesFromTempFolderToTheirDestinations(string tempFolder, List<string> folders)
	{
		for (int i = 0; i < folders.Count; i++)
		{
			this.CopyDirectory(destinationFolder: this.ReplaceFilenameVariablesExtraction(folders[i]), sourceFolder: Path.Combine(tempFolder, i.ToString()));
		}
	}

	private void ExtractBendMachine(string tempFolder)
	{
		this.CopyDirectory(destinationFolder: Path.Combine(this._pnFolderService, "u\\pn\\machine_bend"), sourceFolder: Path.Combine(tempFolder, "machine_bend"));
	}

	public string Extract()
	{
		string empty = string.Empty;
		if (string.IsNullOrEmpty(this.ZipFilepath))
		{
			return empty;
		}
		if (!File.Exists(this.ZipFilepath))
		{
			return empty;
		}
		using ZipArchive source = ZipFile.Open(this.ZipFilepath, ZipArchiveMode.Read);
		string text = Path.Combine(this.TempPath, Path.GetFileName(this.ZipFilepath));
		if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		source.ExtractToDirectory(text);
		if (!File.Exists(Path.Combine(this.TempPath, "service.txt")))
		{
			return empty;
		}
		List<string> list = (from i in File.ReadAllLines(Path.Combine(this.TempPath, "service.txt"))
			select i.Split('|')?.FirstOrDefault() ?? string.Empty).ToList();
		if (Directory.Exists(this._pnFolderService))
		{
			Directory.Delete(this._pnFolderService, recursive: true);
		}
		Directory.CreateDirectory(this._pnFolderService);
		this.CopyDirectory(this._pnFolderServiceTemplate, this._pnFolderService);
		this.CopyFilesFromTempFolderToTheirDestinations(text, list);
		this.ExtractBendMachine(text);
		File.Copy(Path.Combine(text, "doc.c3do"), Path.Combine(this._pnFolderService, "doc.c3do"));
		File.Copy(this.ZipFilepath, Path.Combine(this._pnFolderService, "snapshot.wf7"));
		DirectoryInfo directoryInfo = new DirectoryInfo(this._pnFolderService);
		string fullName = directoryInfo.Root.FullName;
		string path = directoryInfo.FullName.Remove(0, (fullName != null) ? (fullName.Length - 1) : 0) ?? "";
		string overridePnHomeDrive = fullName?.Replace("\\", "");
		new EnvironmentConfig().WriteTemplate(Path.Combine(this._pnFolderService, "environment.json"), overridePnHomeDrive, Path.Combine(path, "pnuser"), directoryInfo.FullName);
		return Path.Combine(text, list.FirstOrDefault((string i) => i.EndsWith(".c3do")) ?? string.Empty);
	}
}

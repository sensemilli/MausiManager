using System;
using System.Collections.Generic;
using System.IO;

namespace WiCAM.Pn4000.Contracts.PnPathServices;

public interface IPnPathService
{
	string PNMASTER { get; set; }

	string PNHOMEDRIVE { get; set; }

	string PNHOMEPATH { get; set; }

	string PNDRIVE { get; set; }

	string ARDRIVE { get; set; }

	string PNSERVICE { get; set; }

	string PNDRIVECAD3D2PN { get; set; }

	string PnMasterOrDrive { get; }

	string FolderCad3d2Pn { get; }

	string FolderGFiles { get; }

	string FolderRun { get; }

	string FolderBendMachine { get; }

	string? GetFileInGFilesExist(string filename);

	string GetFileInGFiles(string filename);

	IEnumerable<DirectoryInfo> GetMachineFolders();

	string GetMachine3DFolder(int machineId);

	string GetMachine3DFolder(string folder);

	string GetMachine3DFile(string file);

	string GetPFileImagePath(string name);

	string GetXCharFolder(int idx);

	string XCOLDF();

	bool CheckLinesAtFileCountBiggerThen(string fileName, int v);

	string BuildPath(params string[] list);

	string PixmapStl(string name);

	string PixmapPng(string group, string name);

	string GetPathAtHome(string sub);

	bool RfileModuleExist(string module_name);

	string RFile(string module_name, string name);

	bool CheckPkernel();

	void SetPnUsersDirectory();

	void RestoreInitialDirectory();

	string CutLatestPathPart(string input);

	string GetUserFilePath(string filename);

	string GetRunFilePath(string filename);

	string GetBinFilePath(string filename);

	string GetArchiveName(int id);

	string GetArchiveDirectory(int arnr);

	string GetArchivePath(int arnr, int line);

	void SimpleInitialization();

	void Initialization(bool initialMultiTask, string machineBendPath);

	int GetMajorPnVersion();

	void ShowHelpMessageBox();

	string GetFileTypeCurrentPath(string type);

	void SetFileTypeCurrentPath(string type, string path);

	string GetConficProviderLocalPath();

	string GetConficProviderSessionPath();

	string GetConficProviderGlobalPath();

	string GetConficProviderMachinePath(int machine);

	string GetMachinePath(int machine);

	string GetMachinePath3D(int machine);

	string GetMachinePath3D();

	void Init(Action<Exception> loggingDelegate);
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WiCAM.Pn4000.BendDoc.Contracts;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Threading;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.PN4000.SpatialStarters;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.BendDoc;

public class SpatialImport : ISpatialImport
{
	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	private readonly ILogCenterService _logCenterService;

	private readonly ITranslator _translator;

	private readonly IMessageLogGlobal _messageLogGlobal;

	private readonly IMaterialManager _materialManager;

	private readonly IDoc3dFactory _docFactory;

	private readonly ISpatialLoader _spatialLoader;

	private readonly IDoEvents _doEvents;

	private readonly ISpatialAssemblyLoader _spatialAssemblyLoader;

	private readonly IProcessPool _processPool;

	public SpatialImport(IPnPathService pathService, IConfigProvider configProvider, ILogCenterService logCenterService, ITranslator translator, IProcessPool processPool, IMessageLogGlobal messageLogGlobal, IMaterialManager materialManager, IDoc3dFactory docFactory, ISpatialAssemblyLoader spatialAssemblyLoader, ISpatialLoader spatialLoader, IDoEvents doEvents)
	{
		this._pathService = pathService;
		this._configProvider = configProvider;
		this._logCenterService = logCenterService;
		this._translator = translator;
		this._messageLogGlobal = messageLogGlobal;
		this._materialManager = materialManager;
		this._docFactory = docFactory;
		this._spatialAssemblyLoader = spatialAssemblyLoader;
		this._spatialLoader = spatialLoader;
		this._doEvents = doEvents;
		this._processPool = processPool;
	}

	public F2exeReturnCode StartSpatial(string fileName, int licenceKey, bool checkLicense, bool viewStyle, IGlobals globals, Assembly assembly)
	{
		if (checkLicense && GeneralSystemComponentsAdapter.ReserveLicense(licenceKey) == 0)
		{
			return F2exeReturnCode.ERROR_NO_LICENSE;
		}
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		string folderCad3d2Pn = this._pathService.FolderCad3d2Pn;
		FileInfo[] files = Directory.CreateDirectory(folderCad3d2Pn).GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			try
			{
				fileInfo.Delete();
			}
			catch (Exception)
			{
			}
		}
		try
		{
			using TextWriter textWriter = new StreamWriter(Path.Combine(folderCad3d2Pn, "name.txt"));
			textWriter.WriteLine(fileName);
		}
		catch (Exception e)
		{
			this._logCenterService.CatchRaport(e);
		}
		string disk = this._pathService.PNDRIVE;
		if (!string.IsNullOrEmpty(this._pathService.PNDRIVECAD3D2PN))
		{
			if (SpatialStarter.UpdateNeeded(disk, this._pathService.PNDRIVECAD3D2PN, out var onlyExe))
			{
				IWaitCancel waitingUpdate = globals.FallbackFrontCalls.ShowWaitWithCancel(globals);
				Task task = Task.Run(delegate
				{
					this.CopySpatial(disk, this._pathService.PNDRIVECAD3D2PN, onlyExe, waitingUpdate);
				});
				while (!task.IsCompleted)
				{
					this._doEvents.DoEvents(20);
				}
				globals.FallbackFrontCalls.CloseWaitWithCancel(waitingUpdate);
			}
			disk = this._pathService.PNDRIVECAD3D2PN;
		}
		if (!SpatialStarter.SpatialVersionIsCorrect(disk))
		{
			if (checkLicense)
			{
				GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
			}
			return F2exeReturnCode.ERROR_CORRECT_SPATIAL_MISSING;
		}
		string text = "";
		if (!general3DConfig.P3D_IncludeHiddenParts)
		{
			text += " -ignoreHidden";
		}
		if (general3DConfig.P3D_InventorIgnoreUnfoldPart)
		{
			text += " -ignoreInverntoUnfold";
		}
		if (general3DConfig.P3D_Ignore1DParts)
		{
			text += " -ignore1D";
		}
		if (!viewStyle && general3DConfig.P3D_PartExportMode)
		{
			text += " -stepExport";
		}
		if (viewStyle)
		{
			text += " -noHighTess";
		}
		ProcessStartInfo startInfo = SpatialStarter.GetSpatialStartInfoForImport(string.Concat(str3: text + " -base64", str0: "\"", str1: global::System.Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName)), str2: "\" "), folderCad3d2Pn, this._pathService.PNDRIVE, this._pathService.PNDRIVECAD3D2PN);
		IWaitCancel waiting = null;
		try
		{
			assembly.LoadingStatus = Assembly.EnumLoadingStatus.SpatialRunning;
			waiting = globals.FallbackFrontCalls.ShowWaitWithCancel(globals);
			waiting.Message = this._translator.Translate("SpatialImport.ProcessAssembly", fileName);
			string reference = startInfo.WorkingDirectory + "\\log.txt";
			string path = Path.Combine(new ReadOnlySpan<string>(in reference));
			File.WriteAllBytes(path, default(ReadOnlySpan<byte>));
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			SpatialObserver observer = new SpatialObserver();
			observer.Run(path, assembly, tokenSource.Token);
			assembly.OnSpatialProgress += delegate(SpatialImportProgress x)
			{
				if (x.TotalParts.HasValue)
				{
					waiting.Message = this._translator.Translate("SpatialImport.ProcessAssemblyProgress", fileName, x.PartId + 1, x.TotalParts);
					waiting.Progress = ((double?)x.PartId + 0.5) / (double?)x.TotalParts;
				}
			};
			Process p;
			Task task2 = Task.Factory.StartNew(delegate
			{
				p = Process.Start(startInfo);
				this._processPool.Register(p);
				while (!tokenSource.Token.IsCancellationRequested && !p.HasExited)
				{
					p.WaitForExit(10);
				}
				if (!p.HasExited)
				{
					p.Kill();
				}
				p.Close();
				observer.Stop();
			});
			while (!task2.IsCompleted)
			{
				bool flag = User32Wrap.PumpMesseges();
				IWaitCancel waitCancel = waiting;
				if (waitCancel != null && waitCancel.IsCancel)
				{
					waiting.IsCancel = false;
					tokenSource.Cancel();
					globals.FallbackFrontCalls.CloseWaitWithCancel(waiting);
					if (checkLicense)
					{
						GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
					}
					return F2exeReturnCode.CANCEL_BY_USER;
				}
				if (!flag)
				{
					global::System.Threading.Thread.Sleep(10);
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
			if (checkLicense)
			{
				GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
			}
			return F2exeReturnCode.ERROR_FILE_IMPORT;
		}
		globals.FallbackFrontCalls.CloseWaitWithCancel(waiting);
		if (checkLicense)
		{
			GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
		}
		string reference2 = startInfo.WorkingDirectory + "\\log.txt";
		string path2 = Path.Combine(new ReadOnlySpan<string>(in reference2));
		if (File.Exists(path2))
		{
			List<string> list = File.ReadAllText(path2).Split("\n").ToList();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (string item in list)
			{
				if (this.DigestString(item, "WARNING:", out string digested))
				{
					if (this.DigestString(ref digested, "no face found"))
					{
						list3.Add(this._translator.Translate("l_popup.Import.FaceMissingWarning", digested));
					}
					else if (this.DigestString(ref digested, "Missing file:"))
					{
						list3.Add(this._translator.Translate("l_popup.Import.FileMissingWarning", digested));
					}
					else
					{
						list3.Add(digested);
					}
				}
				else if (this.DigestString(item, "ERROR:", out digested))
				{
					if (this.DigestString(ref digested, "Part '"))
					{
						list2.Add(this._translator.Translate("l_popup.Import.PartError", digested));
					}
					else if (this.DigestString(ref digested, "Failed to export"))
					{
						list2.Add(this._translator.Translate("l_popup.Import.ExportError", digested));
					}
					else
					{
						list2.Add(digested);
					}
				}
			}
			if (!File.Exists(Path.Combine(startInfo.WorkingDirectory, viewStyle ? "lowTess_0.txt" : "0.txt")))
			{
				list3.Add(this._translator.Translate("l_popup.Import.GeometryMissing"));
			}
			if (list2.Count > 0)
			{
				this._messageLogGlobal.ShowErrorMessage(this._translator.Translate("l_popup.Import.ErrorHeader") + "\n" + string.Join("\n", list2));
			}
			if (list3.Count > 0)
			{
				this._messageLogGlobal.ShowWarningMessage(this._translator.Translate("l_popup.Import.WarningHeader") + "\n" + string.Join("\n", list3));
			}
			if (list2.Count > 0)
			{
				if (checkLicense)
				{
					GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
				}
				return F2exeReturnCode.ERROR_FILE_IMPORT;
			}
		}
		string reference3 = startInfo.WorkingDirectory + "\\assemblyInternal.bin";
		string path3 = Path.Combine(new ReadOnlySpan<string>(in reference3));
		if (File.Exists(path3) && File.ReadLines(path3).Any((string line) => line.Contains("\"Dimension\": \"2\"")))
		{
			string text2 = fileName;
			try
			{
				text2 = new FileInfo(text2).Name;
			}
			catch (Exception)
			{
			}
			GeneralSystemComponentsAdapter.WriteToLOPERR(1, "INFO: Assembly contains 2D bodies!", text2);
		}
		return F2exeReturnCode.OK;
	}

	private void CopySpatial(string sourceDrive, string destinationDrive, bool onlyExe, IWaitCancel waitingUpdate)
	{
		string text = sourceDrive + "\\u\\pn\\run\\cad3d2pn\\";
		string text2 = destinationDrive + "\\u\\pn\\run\\cad3d2pn\\";
		try
		{
			if (!onlyExe)
			{
				string text3 = text + "sp2025.1.0.0.12";
				string text4 = text2 + "sp2025.1.0.0.12";
				string text5 = text4 + "_temp\\";
				if (Directory.Exists(text5))
				{
					this._messageLogGlobal.ShowTranslatedErrorMessage("Spatial.CopyFailedLastTime", text3, text4, text5);
					return;
				}
				waitingUpdate.Message = this._translator.Translate("SpatialImport.CopySpatial");
				int length = text3.Length;
				List<string> list = Directory.EnumerateFiles(text3, "", SearchOption.AllDirectories).ToList();
				int num = 0;
				foreach (string item in list)
				{
					waitingUpdate.Progress = (double)num++ / ((double)list.Count + 1.0);
					string text6 = text5 + item.Remove(0, length);
					Directory.CreateDirectory(new FileInfo(text6).Directory.FullName);
					File.Copy(item, text6, overwrite: true);
				}
				Directory.Move(text5, text4);
				waitingUpdate.Progress = null;
			}
			waitingUpdate.Message = this._translator.Translate("SpatialImport.CopyCad3d2pn");
			File.Copy(text + "cad3d2pn.exe", text2 + "cad3d2pn.exe", overwrite: true);
		}
		catch (Exception)
		{
			this._messageLogGlobal.ShowTranslatedErrorMessage("Spatial.CopyError", text, text2);
		}
	}

	private bool DigestString(ref string totalString, string soughtSubstring)
	{
		string digested;
		bool num = this.DigestString(totalString, soughtSubstring, out digested);
		if (num)
		{
			totalString = digested;
		}
		return num;
	}

	private bool DigestString(string totalString, string soughtSubstring, out string digested)
	{
		int num = totalString.IndexOf(soughtSubstring);
		if (num == -1)
		{
			digested = null;
			return false;
		}
		digested = totalString.Substring(num + soughtSubstring.Length);
		return true;
	}

	public IDoc3d CreateByImportSpatial(out F2exeReturnCode code, string fileName, int licenceKey, bool checkLicense, bool viewStyle, IFactorio factorio, bool moveToCenter, bool analyze = true, int machineNum = -1, bool isDevImport = false, int materialNum = -1)
	{
		using (new ActivitySource("PnBndDoc").StartActivity("CreateByImportSpatial"))
		{
			code = F2exeReturnCode.OK;
			IGlobals globals = factorio.Resolve<IGlobals>();
			if (checkLicense && GeneralSystemComponentsAdapter.ReserveLicense(licenceKey) == 0)
			{
				code = F2exeReturnCode.ERROR_NO_LICENSE;
				return null;
			}
			General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
			string folderCad3d2Pn = this._pathService.FolderCad3d2Pn;
			FileInfo[] files = Directory.CreateDirectory(folderCad3d2Pn).GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				try
				{
					fileInfo.Delete();
				}
				catch (Exception)
				{
				}
			}
			try
			{
				using TextWriter textWriter = new StreamWriter(Path.Combine(folderCad3d2Pn, "name.txt"));
				textWriter.WriteLine(fileName);
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
			string pnDrive = this._pathService.PNDRIVE;
			if (!string.IsNullOrEmpty(this._pathService.PNDRIVECAD3D2PN))
			{
				pnDrive = this._pathService.PNDRIVECAD3D2PN;
			}
			if (!SpatialStarter.SpatialVersionIsCorrect(pnDrive))
			{
				code = F2exeReturnCode.ERROR_CORRECT_SPATIAL_MISSING;
				if (checkLicense)
				{
					GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
				}
				return null;
			}
			string text = "";
			if (!general3DConfig.P3D_IncludeHiddenParts)
			{
				text += " -ignoreHidden";
			}
			if (general3DConfig.P3D_InventorIgnoreUnfoldPart)
			{
				text += " -ignoreInverntoUnfold";
			}
			if (general3DConfig.P3D_Ignore1DParts)
			{
				text += " -ignore1D";
			}
			if (!viewStyle && general3DConfig.P3D_PartExportMode)
			{
				text += " -stepExport";
			}
			if (viewStyle)
			{
				text += " -noHighTess";
			}
			ProcessStartInfo startInfo = SpatialStarter.GetSpatialStartInfoForImport(string.Concat(str3: text + " -base64", str0: "\"", str1: global::System.Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName)), str2: "\" "), folderCad3d2Pn, this._pathService.PNDRIVE, this._pathService.PNDRIVECAD3D2PN);
			IWaitCancel waitCancel = null;
			try
			{
				waitCancel = globals.FallbackFrontCalls.ShowWaitWithCancel(globals);
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				CancellationToken token = cancellationTokenSource.Token;
				Process p;
				Task task = Task.Factory.StartNew(delegate
				{
					using (new ActivitySource("Pn4000").StartActivity("Cad3d2pn"))
					{
						p = Process.Start(startInfo);
						this._processPool.Register(p);
						while (!token.IsCancellationRequested && !p.HasExited)
						{
							p.WaitForExit(10);
						}
						if (!p.HasExited)
						{
							p.Kill();
						}
						p.Close();
					}
				});
				while (!task.IsCompleted)
				{
					bool num = User32Wrap.PumpMesseges();
					if (waitCancel != null && waitCancel.IsCancel)
					{
						waitCancel.IsCancel = false;
						cancellationTokenSource.Cancel();
					}
					if (!num)
					{
						global::System.Threading.Thread.Sleep(10);
					}
				}
			}
			catch (Exception e2)
			{
				this._logCenterService.CatchRaport(e2);
				if (checkLicense)
				{
					GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
				}
				code = F2exeReturnCode.ERROR_FILE_IMPORT;
				return null;
			}
			globals.FallbackFrontCalls.CloseWaitWithCancel(waitCancel);
			if (checkLicense)
			{
				GeneralSystemComponentsAdapter.FreeLicense(licenceKey);
			}
			string text2 = this.CheckSpatialAssembly(viewStyle, this._pathService);
			if (text2 == "?ASSEMBLY")
			{
				code = F2exeReturnCode.ERROR_TOO_COMPLEX_DATA;
				return null;
			}
			if (text2 == "?NOCAD")
			{
				code = F2exeReturnCode.ERROR_NO_BREPS;
				return null;
			}
			text2 = Path.Combine(this._pathService.FolderCad3d2Pn, text2);
			IDoc3d doc3d = this._docFactory.CreateDoc(fileName, isAssemblyLoading: false, null, isDevImport);
			if (viewStyle)
			{
				doc3d.View3DModel = this._spatialAssemblyLoader.LoadSpatialAssemblyFile(text2, lowTess: true, null);
				doc3d.View3DModel.ModelType = ModelType.Assembly;
				if (moveToCenter)
				{
					this.CenterAssembly(doc3d.View3DModel);
				}
			}
			else
			{
				if (machineNum != -1)
				{
					doc3d.MachinePath = this._pathService.GetMachine3DFolder(machineNum);
					if (!string.IsNullOrEmpty(doc3d.MachinePath) && Directory.Exists(doc3d.MachinePath))
					{
						doc3d.BendMachineConfig = factorio.Resolve<IBendMachineSimulation>().Init(doc3d.MachinePath, null, doc3d, out var loadingError);
						if (loadingError)
						{
							code = F2exeReturnCode.ERROR_LOADING;
						}
						else
						{
							doc3d.ToolSelectionType = ToolSelectionType.PreferredTools;
						}
					}
					else
					{
						code = F2exeReturnCode.ERROR_WRONG_MACHINE_NUMBER;
					}
				}
				else
				{
					doc3d.SetDefautMachine(viewStyle);
				}
				if (materialNum != -1)
				{
					doc3d.MaterialNumber = materialNum;
				}
				try
				{
					doc3d.InputModel3D = this._spatialLoader.LoadSpatialFile(text2, fileName, analyze, ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService));
				}
				catch (Exception exception)
				{
					doc3d.MessageDisplay.ShowErrorMessage(exception);
					code = F2exeReturnCode.ERROR_FILE_IMPORT;
				}
				if (doc3d.InputModel3D == null)
				{
					return doc3d;
				}
				if (code == F2exeReturnCode.OK && !doc3d.CombinedBendDescriptors.Any())
				{
					code = F2exeReturnCode.OK_FLAT;
				}
				if (doc3d.InputModel3D.Shell.Macros.Any(delegate(Macro macro)
				{
					ManufacturingMacro obj2 = macro as ManufacturingMacro;
					return obj2 != null && obj2.Faces.Count == 0;
				}))
				{
					string message = this._translator.Translate("PnBndDoc.CreateByImportSpatial.WarnManufacturingDataBody");
					string caption = this._translator.Translate("PnBndDoc.CreateByImportSpatial.WarnManufacturingDataTitle");
					doc3d.MessageDisplay.ShowWarningMessage(message, caption);
				}
				doc3d.ReconstructedEntryModel.ModelType = ModelType.Part;
				doc3d.UnfoldModel3D.ModelType = ModelType.Part;
				doc3d.BendModel3D.ModelType = ModelType.Part;
				doc3d.ReconstructedEntryModel.FileName = fileName.Split('\\').Last();
				doc3d.UnfoldModel3D.FileName = doc3d.ReconstructedEntryModel.FileName;
				doc3d.BendModel3D.FileName = doc3d.ReconstructedEntryModel.FileName;
				if (moveToCenter)
				{
					doc3d.CenterModel(doc3d.InputModel3D);
				}
			}
			if (general3DConfig.P3D_UseExternalApp3D)
			{
				this.UseExternalApp3D(text2, this._logCenterService);
			}
			doc3d.SetModelDefaultColors();
			return doc3d;
		}
	}

	private string CheckSpatialAssembly(bool viewStyle, IPnPathService pathService)
	{
		List<FileInfo> list = (from file in Directory.CreateDirectory(pathService.GetPathAtHome("\\cad3d2pn")).GetFiles()
			where file.Extension != ".step"
			select file).ToList();
		foreach (FileInfo item in list)
		{
			if (item.Name == "GeometryOnly.txt")
			{
				return item.Name;
			}
		}
		if (viewStyle)
		{
			foreach (FileInfo item2 in list)
			{
				if (item2.Name == "assemblyInternal.bin")
				{
					return item2.Name;
				}
			}
		}
		if (list.Count <= 4)
		{
			return "?NOCAD";
		}
		if (this.CheckIfIsOnlyOnePart(list.FirstOrDefault((FileInfo x) => x.Name == "assemblyInternal.bin"), pathService))
		{
			return list.First((FileInfo x) => x.Name == "0.txt").Name;
		}
		string text = this.CheckIfMaybeOneAtAssemblyHaveOnlyData(list.ToArray(), pathService);
		if (!(text != string.Empty))
		{
			return "?ASSEMBLY";
		}
		return text;
	}

	private bool CheckIfIsOnlyOnePart(FileSystemInfo file, IPnPathService pathService)
	{
		if (file == null)
		{
			return false;
		}
		HashSet<AssemblyBody> bodies = new HashSet<AssemblyBody>();
		using (FileStream stream = new FileStream(file.FullName, FileMode.Open))
		{
			using StreamReader reader = new StreamReader(stream);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			foreach (AssemblyNode item in new JsonSerializer
			{
				Formatting = Formatting.None,
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto
			}.Deserialize<List<AssemblyNode>>(reader2))
			{
				GetBodies(item);
			}
		}
		if (bodies.Count == 1)
		{
			return true;
		}
		return false;
		void GetBodies(AssemblyNode node)
		{
			foreach (AssemblyBody item2 in node.Bodies.Where((AssemblyBody x) => x.BodyId.HasValue))
			{
				bodies.Add(item2);
			}
			foreach (AssemblyNode child in node.Children)
			{
				GetBodies(child);
			}
		}
	}

	private bool NoSpatialFile(string name)
	{
		if (name != "LowQualityTessellation.txt" && name != "type.txt")
		{
			return name != "name.txt";
		}
		return false;
	}

	private string CheckIfMaybeOneAtAssemblyHaveOnlyData(FileInfo[] files, IPnPathService pathService)
	{
		string result = string.Empty;
		int num = 0;
		foreach (FileInfo fileInfo in files)
		{
			if (this.NoSpatialFile(fileInfo.Name) && pathService.CheckLinesAtFileCountBiggerThen(fileInfo.FullName, 2))
			{
				num++;
				if (num > 1)
				{
					return string.Empty;
				}
				result = fileInfo.Name;
			}
		}
		return result;
	}

	private void CheckForMatUnf(string filePath, IPnBndDoc doc)
	{
		if (!File.Exists(filePath))
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("Materials.MatUnfNotExist", filePath);
		}
	}

	private void CenterAssembly(Model model)
	{
		Pair<Vector3d, Vector3d> boundary = model.GetBoundary(Matrix4d.Identity);
		Vector3d item = boundary.Item1;
		Vector3d item2 = boundary.Item2;
		Vector3d v = new Vector3d(item2.X, item.Y, item.Z) - item;
		Vector3d normalized = v.Cross(new Vector3d(item.X, item2.Y, item.Z) - item).Normalized;
		Vector3d normalized2 = normalized.Cross(v).Normalized;
        // Fix for CS8323: Ensure named arguments are used in the correct order or remove named arguments if unnecessary.

        model.Transform = Matrix4d.TransformationToUnitCoordinateSystem(
            normalized2, // x
            normalized.Cross(normalized2).Normalized, // y
            normalized, // z
            new Vector3d((item.X + item2.X) / 2.0, (item.Y + item2.Y) / 2.0, (item.Z + item2.Z) / 2.0) // origin
        );
	//	model.Transform = Matrix4d.TransformationToUnitCoordinateSystem(y: normalized.Cross(normalized2).Normalized,  new Vector3d((item.X + item2.X) / 2.0, (item.Y + item2.Y) / 2.0, (item.Z + item2.Z) / 2.0), x: normalized2, z: normalized);
		double num = v.SignedAngle(v.Cross(normalized), normalized);
		if (Math.Abs(num % Math.PI) > 1E-05)
		{
			model.Transform *= Matrix4d.RotationZ(num);
		}
	}

	public void UseExternalApp3D(string fileToLoad, ILogCenterService logCenterService)
	{
		string text = fileToLoad.Replace(".txt", ".step");
		if (text[1] != ':')
		{
			text = Directory.GetCurrentDirectory() + "\\" + text;
		}
		if (File.Exists(text))
		{
			PnExternalCall.Start("extapp3d.bat \"" + text + "\"", logCenterService);
		}
	}
}

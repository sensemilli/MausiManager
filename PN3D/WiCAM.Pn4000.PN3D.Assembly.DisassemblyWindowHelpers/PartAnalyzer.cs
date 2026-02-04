using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Writer;
using WiCAM.Pn4000.BendModel.Writer.Writer;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PartsReader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.CAD.Converter;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;

public class PartAnalyzer : IPartAnalyzer
{
	private readonly IMessageLogGlobal _messageLogGlobal;

	private readonly IDoc3dFactory _doc3dFactory;

	private readonly IPrefabricatedPartsManager _prefabricatedPartsManager;

	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	private readonly ILogCenterService _logCenterService;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IMaterialManager _materials;

	private readonly ISpatialLoader _spatialLoader;

	private readonly ITranslator _translator;

	public PartAnalyzer(IMessageLogGlobal messageLogGlobal, IDoc3dFactory doc3dFactory, IPrefabricatedPartsManager prefabricatedPartsManager, IPnPathService pathService, IConfigProvider configProvider, ILogCenterService logCenterService, IImportMaterialMapper importMaterialMapper, IMaterialManager materials, ISpatialLoader spatialLoader, ITranslator translator)
	{
		this._messageLogGlobal = messageLogGlobal;
		this._doc3dFactory = doc3dFactory;
		this._prefabricatedPartsManager = prefabricatedPartsManager;
		this._pathService = pathService;
		this._configProvider = configProvider;
		this._logCenterService = logCenterService;
		this._importMaterialMapper = importMaterialMapper;
		this._materials = materials;
		this._spatialLoader = spatialLoader;
		this._translator = translator;
	}

	public void AnalyzeParts(DisassemblySimpleModel mainModel, IDoc3d doc, string fileName, Dictionary<string, int> materialImportToPnMatId, out List<DisassemblyPart> disassemblyParts, bool useBackgroundWorker, IScreenshotScreen screenshotScreen)
	{
		disassemblyParts = new List<DisassemblyPart>();
		if (mainModel.Parts == null)
		{
			return;
		}
		foreach (AssemblyPartInfo part in mainModel.Parts)
		{
			if (part.PartModel == null)
			{
				continue;
			}
			DisassemblySimpleModel disassemblySimpleModel = new DisassemblySimpleModel();
			disassemblySimpleModel.GetModelFromDisassemblySimpleModel(mainModel, part.ID);
			if (!double.IsInfinity(disassemblySimpleModel.ModelBoundary.LenX()) && disassemblySimpleModel.Parts.Count == 1)
			{
				global::WiCAM.Pn4000.BendModel.Base.Matrix4d identity = global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity;
				identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationZ(0.7853981852531433);
				identity *= global::WiCAM.Pn4000.BendModel.Base.Matrix4d.RotationX(1.0471975803375244);
				int width = 400;
				int height = 400;
				int border = 10;
				string text = Path.Combine(this._pathService.FolderCad3d2Pn, $"{part.ID}.png");
				screenshotScreen.PrintScreen(part.PartModel, text, identity, border, width, height);
				while (!File.Exists(text))
				{
					global::System.Threading.Thread.Sleep(1);
				}
				AssemblyPartInfo assemblyPartInfo = part;
				if (assemblyPartInfo.Name == null)
				{
					string text3 = (assemblyPartInfo.Name = part.ID.ToString());
				}
				DisassemblyPart item = new DisassemblyPart(part, disassemblySimpleModel.ModelBoundary);
				disassemblyParts.Add(item);
			}
		}
		PartAnalyzer.UpdatePartsName(flag: this._configProvider.InjectOrCreate<General3DConfig>().P3D_UseAssemblyName, disassemblyParts: disassemblyParts, pathService: this._pathService);
		if (!useBackgroundWorker)
		{
			this.AnalyzeDisassemblyParts(disassemblyParts, doc, this._messageLogGlobal, materialImportToPnMatId);
			this.AnalyzeDisassemblyPartsCompletion(disassemblyParts, fileName, mainModel);
		}
	}

	private void AnalyzeDisassemblyParts(ICollection<DisassemblyPart> parts, IDoc3d doc, IMessageDisplay messageDisplay, Dictionary<string, int> materialImportToPnMatId)
	{
		if (parts == null || parts.Count == 0)
		{
			return;
		}
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (!general3DConfig.P3D_TestTypeOfPart)
		{
			return;
		}
		Model entryModel3D;
		if (parts.Count == 1)
		{
			DisassemblyPart disassemblyPart = parts.First();
			try
			{
				this.Analyze(disassemblyPart, messageDisplay, parts.Count, doc, isAssembly: false, materialImportToPnMatId, out var _, out var _, out entryModel3D);
				return;
			}
			catch (Exception exc)
			{
				this.ErrorAtAnalyze(messageDisplay, exc, null);
				disassemblyPart.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
				return;
			}
		}
		Stopwatch stp = Stopwatch.StartNew();
		List<DisassemblyPart> partsPurchased = parts.Where(delegate(DisassemblyPart x)
		{
			IPrefabricatedPart prefabricatedPart = this._prefabricatedPartsManager.FindPart(x.OriginalAssemblyName, checkDetectionEnabled: true);
			if (prefabricatedPart != null)
			{
				x.PartInfo.PurchasedPart = prefabricatedPart.Type;
				x.PartInfo.IgnoreCollision = !prefabricatedPart.IsMountedBeforeBending;
				return true;
			}
			return false;
		}).ToList();
		List<DisassemblyPart> list = parts.Where((DisassemblyPart x) => !partsPurchased.Contains(x)).ToList();
		if (general3DConfig.P3D_AnalyzeParallel)
		{
			List<PurchasedPartsMerger.PurchasedPart> purchasedModels = partsPurchased.AsParallel().Select(delegate(DisassemblyPart p)
			{
				Model entryModel3D2 = null;
				try
				{
					this.Analyze(p, messageDisplay, parts.Count, null, isAssembly: true, materialImportToPnMatId, out var _, out var _, out entryModel3D2);
					p.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
				}
				catch (Exception exc2)
				{
					this.ErrorAtAnalyze(messageDisplay, exc2, stp);
					p.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
				}
				lock (p)
				{
					p.IsLoaded = true;
				}
				p.LoadingCompleted?.Invoke(p);
				return new PurchasedPartsMerger.PurchasedPart
				{
					EntryModel = entryModel3D2,
					AssemblyName = p.OriginalAssemblyName,
					WorldMatrices = p.Matrixes,
					IgnoreCollision = p.PartInfo.IgnoreCollision,
					UserProperties = p.UserProperties.Select((UserProperty x) => (Name: x.Name, Properties: x.Properties)).ToList()
				};
			}).ToList();
			Parallel.ForEach((IEnumerable<DisassemblyPart>)list, (Action<DisassemblyPart>)delegate(DisassemblyPart part)
			{
				try
				{
					this.Analyze(part, messageDisplay, parts.Count, null, isAssembly: true, materialImportToPnMatId, out var _, out var _, out var _, purchasedModels);
				}
				catch (Exception exc3)
				{
					this.ErrorAtAnalyze(messageDisplay, exc3, stp);
					part.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
				}
				lock (part)
				{
					part.IsLoaded = true;
				}
				part.LoadingCompleted?.Invoke(part);
			});
			stp.Stop();
			return;
		}
		List<PurchasedPartsMerger.PurchasedPart> purchasedParts = partsPurchased.Select(delegate(DisassemblyPart p)
		{
			Model entryModel3D4 = null;
			try
			{
				this.Analyze(p, messageDisplay, parts.Count, null, isAssembly: true, materialImportToPnMatId, out var _, out var _, out entryModel3D4);
				p.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
			}
			catch (Exception exc4)
			{
				this.ErrorAtAnalyze(messageDisplay, exc4, stp);
				p.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
			}
			lock (p)
			{
				p.IsLoaded = true;
			}
			p.LoadingCompleted?.Invoke(p);
			return new PurchasedPartsMerger.PurchasedPart
			{
				EntryModel = entryModel3D4,
				AssemblyName = p.OriginalAssemblyName,
				WorldMatrices = p.Matrixes,
				IgnoreCollision = p.PartInfo.IgnoreCollision,
				UserProperties = p.UserProperties.Select((UserProperty x) => (Name: x.Name, Properties: x.Properties)).ToList()
			};
		}).ToList();
		foreach (DisassemblyPart item in list)
		{
			try
			{
				this.Analyze(item, messageDisplay, parts.Count, null, isAssembly: true, materialImportToPnMatId, out var _, out var _, out entryModel3D, purchasedParts);
			}
			catch (Exception exc5)
			{
				this.ErrorAtAnalyze(messageDisplay, exc5, stp);
				item.PartInfo.OriginalPartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
			}
			lock (item)
			{
				item.IsLoaded = true;
			}
			item.LoadingCompleted?.Invoke(item);
		}
		stp.Stop();
	}

	public void ErrorAtAnalyze(IMessageDisplay messageDisplay, Exception exc, Stopwatch stp)
	{
		stp?.Stop();
		string message = this._translator.Translate("PartAnalyzer.Error", exc.Message.ToString());
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			this._logCenterService.CatchRaport(exc);
			messageDisplay.ShowErrorMessage(message);
		});
	}

	public void AnalyzeDisassemblyPartsCompletion(ICollection<DisassemblyPart> parts, string fileName, DisassemblySimpleModel mainModel)
	{
		string text = "";
		if (!string.IsNullOrEmpty(fileName))
		{
			text = Path.GetExtension(fileName).ToLower();
		}
		if (this._configProvider.InjectOrCreate<General3DConfig>().P3D_InventorIgnoreUnfoldPart && text == ".ipt" && parts.Count == 2)
		{
			DisassemblyPart firstFlat = parts.FirstOrDefault((DisassemblyPart x) => x.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.FlatSheetMetal));
			if (firstFlat != null)
			{
				parts.Remove(firstFlat);
				AssemblyPartInfo assemblyPartInfo = mainModel.Parts.FirstOrDefault((AssemblyPartInfo m) => m.ID == firstFlat.ID);
				if (assemblyPartInfo != null)
				{
					mainModel.Parts.Remove(assemblyPartInfo);
				}
				File.Delete(Path.Combine(this._pathService.FolderCad3d2Pn, "lowTess_" + firstFlat.ID + ".txt"));
				File.Delete(Path.Combine(this._pathService.FolderCad3d2Pn, firstFlat.ID + ".txt"));
				File.Delete(Path.Combine(this._pathService.FolderCad3d2Pn, firstFlat.ID + ".png"));
			}
		}
		IPartsReader partsReader = new global::WiCAM.Pn4000.PartsReader.PartsReader();
		partsReader.SerializeAssembly(new global::WiCAM.Pn4000.PartsReader.DataClasses.Assembly
		{
			MajorVersion = partsReader.MajorVersion,
			MinorVersion = partsReader.MinorVersion,
			RootPartName = mainModel?.StructureRootNode.Name,
			DisassemblyParts = parts.Select((DisassemblyPart part) => part.GetConvertedPart()).ToList()
		}, Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.xml"));
	}

	public void Analyze(DisassemblyPart part, IMessageDisplay messageDisplay, int partsCount, IDoc3d orgDoc, bool isAssembly, Dictionary<string, int> materialImportToPnMatId, out HashSet<Triple<FaceGroup, double, double>> notAdjustableBends, out Dictionary<int, BendTableReturnValues> bendTableResults, out Model entryModel3D, List<PurchasedPartsMerger.PurchasedPart> purchasedParts = null, List<Parts1dMerger.Part1d> parts1d = null, IImportArg importSettings = null)
	{
		notAdjustableBends = new HashSet<Triple<FaceGroup, double, double>>();
		bendTableResults = new Dictionary<int, BendTableReturnValues>();
		string text = Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".c3do");
		File.Exists(text);
		Stopwatch stp = Stopwatch.StartNew();
		IDoc3d doc3d = orgDoc;
		if (orgDoc == null)
		{
			KeyValuePair<string, string> keyValuePair = part.UserProperties.SelectMany((UserProperty x) => x.Properties).FirstOrDefault((KeyValuePair<string, string> x) => x.Key == "ASidePointsOutline");
			if (keyValuePair.Key != null)
			{
				string[] array = keyValuePair.Value.Replace(',', '.').Split(' ', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length / 3; i++)
				{
					global::WiCAM.Pn4000.BendModel.Base.Vector3d item = new global::WiCAM.Pn4000.BendModel.Base.Vector3d(double.Parse(array[i * 3].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0, double.Parse(array[i * 3 + 1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0, double.Parse(array[i * 3 + 2].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0);
					importSettings.ASidePointsOutline.Add(item);
				}
			}
			KeyValuePair<string, string> keyValuePair2 = part.UserProperties.SelectMany((UserProperty x) => x.Properties).FirstOrDefault((KeyValuePair<string, string> x) => x.Key == "ASidePointsHoles");
			if (keyValuePair2.Key != null)
			{
				string[] array2 = keyValuePair2.Value.Replace(',', '.').Split(' ', StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < array2.Length / 3; j++)
				{
					global::WiCAM.Pn4000.BendModel.Base.Vector3d item2 = new global::WiCAM.Pn4000.BendModel.Base.Vector3d(double.Parse(array2[j * 3].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0, double.Parse(array2[j * 3 + 1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0, double.Parse(array2[j * 3 + 2].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture) * 10.0);
					importSettings.ASidePointsHoles.Add(item2);
				}
			}
			entryModel3D = this._spatialLoader.LoadSpatialFile(Path.Combine(this._pathService.FolderCad3d2Pn, part.ID + ".txt"), part.ID.ToString(), cleanAndAnalyze: true, ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService), importSettings);
			entryModel3D.ModelType = ModelType.Part;
			string key = entryModel3D.OriginalMaterialName ?? "";
			int matId = part.PnMaterialID;
			bool pnMaterialByUser = part.PnMaterialByUser;
			if (matId < 0 && materialImportToPnMatId != null && materialImportToPnMatId.TryGetValue(key, out var value))
			{
				matId = value;
			}
			if (matId < 0)
			{
				matId = this._importMaterialMapper.GetMaterialId(key);
			}
			IMaterialArt material = this._materials.MaterialList.FirstOrDefault((IMaterialArt m) => m.Number == matId);
			doc3d = this._doc3dFactory.CreateDoc(part.ID.ToString(), isAssembly, part.AssemblyGuid);
			doc3d.Material = material;
			doc3d.PnMaterialByUser = pnMaterialByUser;
			doc3d.SimulationInstancesAdditionalParts = FindTouchingModels(entryModel3D, part.Matrixes, purchasedParts, parts1d);
			entryModel3D.AddAdditionalParts(doc3d.CurrentSimulationInstancesAdditionalPart);
			try
			{
				doc3d.InputModel3D = entryModel3D;
			}
			catch (Exception)
			{
				doc3d.InputModel3D.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error;
			}
			doc3d.CenterModel(doc3d.InputModel3D);
			this.ReconstructBendsIfNecessary(doc3d, importSettings);
		}
		else
		{
			doc3d.SimulationInstancesAdditionalParts = FindTouchingModels(doc3d.EntryModel3D, part.Matrixes, purchasedParts, parts1d);
			doc3d.EntryModel3D.AddAdditionalParts(doc3d.CurrentSimulationInstancesAdditionalPart);
		}
		doc3d.AmountInAssembly = part.Matrixes.Count;
		doc3d.DocAssemblyId = part.ID;
		entryModel3D = doc3d.EntryModel3D;
		if (!string.IsNullOrEmpty(part.ImportedFilename))
		{
			doc3d.DiskFile.Header.ImportedFilename = part.ImportedFilename;
		}
		doc3d.GetApplicableBendTable(out var _);
		part.PartInfo.SimulationInstances = doc3d.SimulationInstancesAdditionalParts;
		doc3d.EntryModel3D.PartInfo.PurchasedPart = part.PartInfo.PurchasedPart;
		doc3d.EntryModel3D.PartInfo.IgnoreCollision = part.PartInfo.IgnoreCollision;
		part.PartInfo.TubeType = doc3d.EntryModel3D.PartInfo.TubeType;
		part.PartInfo.TubeInfo = doc3d.EntryModel3D.PartInfo.TubeInfo;
		if (part.IsAdditionalPart)
		{
			doc3d.EntryModel3D.PartInfo.PartType = global::WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
		}
		part.PartInfo.OriginalPartType = doc3d.EntryModel3D.PartInfo.PartType;
		part.PartInfo.PartType = part.PartInfo.OriginalPartType;
		part.Thickness = Math.Round(doc3d.Thickness, 2);
		doc3d.UserProperties = part.UserProperties;
		doc3d.Thickness = part.Thickness;
		if (doc3d.Material != null)
		{
			part.OriginalMaterialName = doc3d.EntryModel3D.OriginalMaterialName;
			part.MaterialName = this._materials.GetMaterialName(part.PnMaterialID);
		}
		part.BendsCount = doc3d.BendDescriptors.Count;
		if (part.PartInfo.OriginalPartType == global::WiCAM.Pn4000.BendModel.BendTools.PartType.Unassigned || part.PartInfo.OriginalPartType >= global::WiCAM.Pn4000.BendModel.BendTools.PartType.Error)
		{
			this.AnalyzeEnded(part, partsCount, doc3d, stp, text);
			return;
		}
		if (part.PartInfo.OriginalPartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.Unknown) && doc3d.EntryModel3D.Shells.Any((Shell s) => s.FlatFaceGroups.Count == 0 && s.RoundFaceGroups.Count == 0))
		{
			this.AnalyzeEnded(part, partsCount, doc3d, stp, text);
			return;
		}
		if (doc3d.UnfoldModel3D == null)
		{
			this.AnalyzeEnded(part, partsCount, doc3d, stp, text);
			return;
		}
		var (visibleFace, visibleFaceModel) = doc3d.UnfoldModel3D.GetFirstFaceOfGroupModel(doc3d.VisibleFaceGroupId, doc3d.VisibleFaceGroupSide);
		PartAnalyzer.GetMacros(doc3d, doc3d.UnfoldModel3D, part);
		if (!part.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.Tube) || (part.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.Tube) && part.PartInfo.TubeType != global::WiCAM.Pn4000.BendModel.BendTools.TubeType.RoundTube))
		{
			PartAnalyzer.UpdateCommonBends(part, doc3d);
			this.CheckVisibleFaceDirectionBySpecialTools(part, doc3d, ref visibleFace, visibleFaceModel);
			Dictionary<Face, int> dictionary = new Dictionary<Face, int>();
			List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> aSidePointsOutline = importSettings.ASidePointsOutline;
			if (aSidePointsOutline != null && aSidePointsOutline.Any())
			{
				foreach (global::WiCAM.Pn4000.BendModel.Base.Vector3d item5 in importSettings.ASidePointsOutline)
				{
					foreach (Triangle item6 in doc3d.ReconstructedEntryModel.Shell.AABBTree.PointQuery(item5, 0.1))
					{
						double t = 0.0;
						double d = 0.0;
						TriangleFunctions.RayTriangleClosestPoint(item5, item6.CalculatedTriangleNormal, item6.V0.Pos, item6.V1.Pos, item6.V2.Pos, ref t, ref d, ignoreDirection: true);
						if (d < 0.001 && Math.Abs(t) < 0.01 && !dictionary.TryAdd(item6.Face, 1))
						{
							dictionary[item6.Face]++;
						}
					}
				}
			}
			List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> aSidePointsHoles = importSettings.ASidePointsHoles;
			if (aSidePointsHoles != null && aSidePointsHoles.Any())
			{
				foreach (global::WiCAM.Pn4000.BendModel.Base.Vector3d aSidePointsHole in importSettings.ASidePointsHoles)
				{
					foreach (Triangle item7 in doc3d.ReconstructedEntryModel.Shell.AABBTree.PointQuery(aSidePointsHole, 0.1))
					{
						double t2 = 0.0;
						double d2 = 0.0;
						TriangleFunctions.RayTriangleClosestPoint(aSidePointsHole, item7.CalculatedTriangleNormal, item7.V0.Pos, item7.V1.Pos, item7.V2.Pos, ref t2, ref d2, ignoreDirection: true);
						if (d2 < 0.001 && Math.Abs(t2) < 0.01 && !dictionary.TryAdd(item7.Face, 1))
						{
							dictionary[item7.Face]++;
						}
					}
				}
			}
			if (dictionary.Any())
			{
				Face key2 = dictionary.MaxBy((KeyValuePair<Face, int> x) => x.Value).Key;
				visibleFace = key2;
				FaceGroup faceGroup = key2.FaceGroup;
				if (faceGroup.Side0.Contains(key2))
				{
					doc3d.VisibleFaceGroupId = faceGroup.ID;
					doc3d.VisibleFaceGroupSide = 0;
				}
				else if (faceGroup.Side1.Contains(key2))
				{
					doc3d.VisibleFaceGroupId = faceGroup.ID;
					doc3d.VisibleFaceGroupSide = 1;
				}
			}
			if (importSettings != null && importSettings.UseOppositeViewingSide)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc3d, ref visibleFace, visibleFaceModel);
			}
			(Face face, Model model) firstFaceOfGroupModel = doc3d.EntryModel3D.GetFirstFaceOfGroupModel(doc3d.VisibleFaceGroupId, doc3d.VisibleFaceGroupSide);
			doc3d.SetTopFace(face: firstFaceOfGroupModel.face, faceModel: firstFaceOfGroupModel.model, model: doc3d.EntryModel3D);
			PartAnalyzer.UpdateCommonBends(part, doc3d);
		}
		if (visibleFace == null)
		{
			this.AnalyzeEnded(part, partsCount, doc3d, stp, text);
			return;
		}
		global::WiCAM.Pn4000.PN3D.Unfold.Unfold.MoveUnfoldModelToZero(doc3d.UnfoldModel3D, doc3d.VisibleFaceGroupId, doc3d.VisibleFaceGroupSide);
		ValidationSettingsConfig validationSettingsConfig = this._configProvider.InjectOrCreate<ValidationSettingsConfig>();
		if (!validationSettingsConfig.ValidationTestsAuto)
		{
			doc3d.ValidationResults = null;
			part.ValidationIntrinsicErrors = null;
			part.ValidationDistanceErrors = null;
		}
		else
		{
			doc3d.ValidationResults = ModelValidation.ValidateModel(doc3d.UnfoldModel3D, validationSettingsConfig, doc3d.CheckSelfCollisionUnfoldModel);
			if (validationSettingsConfig.ValiSelfCollision)
			{
				part.ValidationSelfCollision = doc3d.ValidationResults.Any((ValidationResult x) => x.Type == ValidationResult.ResultTypes.SelfCollision);
			}
			part.ValidationIntrinsicErrors = new List<ValidationIntrinsicError>(doc3d.ValidationResults.Where((ValidationResult x) => x.IntrinsicErrors != null).Select(delegate(ValidationResult obj)
			{
				Tuple<ValidationIntrinsicError.ValidationObjectType, int> tuple2 = GetObj(obj);
				return new ValidationIntrinsicError
				{
					ObjType = tuple2.Item1,
					ObjId = tuple2.Item2,
					Messages = obj.IntrinsicErrors.Select(GetIntrinsicError).ToList()
				};
			}));
			part.ValidationDistanceErrors = new List<ValidationDistanceError>(doc3d.ValidationResults.Where((ValidationResult x) => x.DistanceErrors != null).SelectMany(delegate(ValidationResult obj)
			{
				Tuple<ValidationIntrinsicError.ValidationObjectType, int> desc = GetObj(obj);
				return obj.DistanceErrors.Select(delegate(ValidationResult obj2)
				{
					Tuple<ValidationIntrinsicError.ValidationObjectType, int> tuple3 = GetObj(obj2);
					return new ValidationDistanceError
					{
						Obj1Type = desc.Item1,
						Obj1Id = desc.Item2,
						Obj2Type = tuple3.Item1,
						Obj2Id = tuple3.Item2
					};
				});
			}));
		}
		doc3d.SetModelDefaultColors();
		this.AnalyzeEnded(part, partsCount, doc3d, stp, text);
		List<global::WiCAM.Pn4000.BendModel.BendTools.SimulationInstance> FindTouchingModels(Model model, List<global::WiCAM.Pn4000.BendModel.Base.Matrix4d> matrixes, List<PurchasedPartsMerger.PurchasedPart> purchasedPart, List<Parts1dMerger.Part1d> parts1D)
		{
			List<global::WiCAM.Pn4000.BendModel.BendTools.SimulationInstance> list = new List<global::WiCAM.Pn4000.BendModel.BendTools.SimulationInstance>();
			if (purchasedPart != null && purchasedPart.Count > 0)
			{
				list.AddRange(PurchasedPartsMerger.FindTouchingModels(model, matrixes, purchasedPart, this._prefabricatedPartsManager.MaxDistOfPurchasePartsToSheetMetal));
			}
			if (parts1D != null && parts1D.Count > 0)
			{
				list.AddRange(Parts1dMerger.FindTouchingModels(model, matrixes, parts1D));
			}
			return list;
		}
		static Tuple<ValidationIntrinsicError.ValidationObjectType, int> GetObj(ValidationResult result)
		{
			if (result.Type == ValidationResult.ResultTypes.Macro)
			{
				return new Tuple<ValidationIntrinsicError.ValidationObjectType, int>(ValidationIntrinsicError.ValidationObjectType.Macro, result.Macro.ID);
			}
			if (result.Type == ValidationResult.ResultTypes.BendingGroup)
			{
				return new Tuple<ValidationIntrinsicError.ValidationObjectType, int>(ValidationIntrinsicError.ValidationObjectType.Bending, result.BendingGroup.ID);
			}
			if (result.Type == ValidationResult.ResultTypes.Edge)
			{
				return new Tuple<ValidationIntrinsicError.ValidationObjectType, int>(ValidationIntrinsicError.ValidationObjectType.Edge, result.Fhe.ID);
			}
			return null;
		}
	}

	private void AnalyzeEnded(DisassemblyPart part, int partsCount, IDoc3d doc, Stopwatch stp, string tempFilename)
	{
		this.WriteObj(doc, part);
		stp.Stop();
		doc.Save(tempFilename);
		part.Metadata = new DocMetadata();
		part.Metadata.CopyFromMetaData(doc.MetaData);
		part.Metadata.Save(this._pathService);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		part.DocTemp = new WeakReference<IDoc3d>(doc);
		if (general3DConfig.P3D_AnalyzeAssemblyKeepDocs)
		{
			part.Doc = doc;
		}
	}

	private void CheckVisibleFaceDirectionBySpecialTools(DisassemblyPart part, IDoc3d doc, ref Face visibleFace, Model visibleFaceModel)
	{
		if (part.Tools.EmbossmentStamps.Count > 0)
		{
			EmbossmentStampXml embossmentStampXml = part.Tools.EmbossmentStamps.FirstOrDefault((EmbossmentStampXml x) => x.IsSpecialVisible);
			if (embossmentStampXml != null)
			{
				if (embossmentStampXml.Direction == -1)
				{
					PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
				}
				return;
			}
		}
		if (part.Tools.Deepenings.Count > 0)
		{
			DeepeningXml deepeningXml = part.Tools.Deepenings.FirstOrDefault((DeepeningXml x) => x.IsSpecialVisible);
			if (deepeningXml != null)
			{
				if (deepeningXml.Direction == -1)
				{
					PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
				}
				return;
			}
		}
		global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig analyzeConfig = ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService);
		bool searchSpecialVisibleFaceColor = analyzeConfig.SearchSpecialVisibleFaceColor;
		Color specialVisibleFaceColor = analyzeConfig.SpecialVisibleFaceColor;
		if (searchSpecialVisibleFaceColor)
		{
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v = new global::WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 1.0);
			foreach (Model item in doc.UnfoldModel3D.GetAllSubModelsWithSelf())
			{
				foreach (Face item2 in from f in item.Shells.SelectMany((Shell s) => s.FlatFaceGroups).SelectMany((FaceGroup fg) => fg.Side0.Concat(fg.Side1))
					where f.Macro == null
					select f)
				{
					if (!(item2.ColorInitial == specialVisibleFaceColor))
					{
						continue;
					}
					global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix = item2.Shell.GetWorldMatrix(item);
					global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d = worldMatrix.Transform(item2.FlatFacePlane.Origin);
					if (worldMatrix.TransformNormal(item2.FlatFacePlane.Normal).IsParallel(v, out var direction) && Math.Sign(vector3d.Z) * direction == 1)
					{
						if (direction < 0)
						{
							PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
						}
						return;
					}
				}
			}
		}
		int num = GetEmbossmentCounts(1);
		int num2 = GetEmbossmentCounts(-1);
		if (num != num2)
		{
			if (num2 > num)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
			}
			return;
		}
		int num3 = part.Tools.Deepenings.Count((DeepeningXml x) => x.Direction == 1) + part.Tools.EmbossmentStamps.Count((EmbossmentStampXml x) => x.Direction == 1);
		int num4 = part.Tools.Deepenings.Count((DeepeningXml x) => x.Direction == -1) + part.Tools.EmbossmentStamps.Count((EmbossmentStampXml x) => x.Direction == -1);
		if (num3 != num4)
		{
			if (num4 > num3)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
			}
			return;
		}
		int num5 = part.Tools.CounterSinks.Count((CounterSinkXml x) => x.Direction == 1);
		int num6 = part.Tools.CounterSinks.Count((CounterSinkXml x) => x.Direction == -1);
		if (num5 != num6)
		{
			if (num6 > num5)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
			}
			return;
		}
		int num7 = part.Tools.BlindHoles.Count((BlindHoleXml x) => x.Direction == 1);
		int num8 = part.Tools.BlindHoles.Count((BlindHoleXml x) => x.Direction == -1);
		if (num7 != num8)
		{
			if (num8 > num7)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
			}
		}
		else if (part.BendsCount > 0)
		{
			int num9 = part.CommonBends.Count((AssemblyCommonBendInfo x) => x.Angle > 0.0);
			if (part.CommonBends.Count((AssemblyCommonBendInfo x) => x.Angle < 0.0) < num9)
			{
				PartAnalyzer.SwitchVisibleFaceToOpposite(part, doc, ref visibleFace, visibleFaceModel);
			}
		}
		int GetEmbossmentCounts(int dir)
		{
			return part.Tools.Louvers.Count((LouverXml x) => x.Direction == dir) + part.Tools.Bridges.Count((BridgeXml x) => x.Direction == dir) + part.Tools.Lances.Count((LanceXml x) => x.Direction == dir) + part.Tools.EmbossedCountersinks.Count((EmbossedCounterSinkXml x) => x.Direction == dir) + part.Tools.EmbossedCircles.Count((EmbossedCircleXml x) => x.Direction == dir) + part.Tools.EmbossedLines.Count((EmbossedLineXml x) => x.Direction == dir) + part.Tools.EmbossedRectangles.Count((EmbossedRectangleXml x) => x.Direction == dir) + part.Tools.EmbossedSquares.Count((EmbossedSquareXml x) => x.Direction == dir) + part.Tools.EmbossedSquaresRounded.Count((EmbossedSquareRoundedXml x) => x.Direction == dir) + part.Tools.EmbossedRectanglesRounded.Count((EmbossedRectangleRoundedXml x) => x.Direction == dir) + part.Tools.EmbossmentStamps.Count((EmbossmentStampXml x) => x.Direction == dir) + part.Tools.EmbossedFreeform.Count((EmbossedFreeformXml x) => x.Direction == dir);
		}
	}

	private static void SwitchVisibleFaceToOpposite(DisassemblyPart part, IDoc3d doc, ref Face visibleFace, Model visibleFaceModel)
	{
		if (visibleFace != null && !visibleFace.FaceGroup.ConnectingFaces.Contains(visibleFace))
		{
			visibleFace = ((!visibleFace.FaceGroup.Side0.Contains(visibleFace)) ? visibleFace.FaceGroup.Side0.First() : visibleFace.FaceGroup.Side1.First());
			doc.SetTopFace(doc.UnfoldModel3D, visibleFace, visibleFaceModel);
		}
		part.Tools = new SpecialTools();
		PartAnalyzer.GetMacros(doc, doc.UnfoldModel3D, part);
	}

	private static void UpdateCommonBends(DisassemblyPart part, IDoc3d doc)
	{
		if (part.BendsCount < 1)
		{
			return;
		}
		part.CommonBends.Clear();
		int num = 0;
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			part.CommonBends.Add(new AssemblyCommonBendInfo(num, string.Join(',', from x in combinedBendDescriptor.Enumerable.Select((IBendDescriptor x) => x.BendParams.EntryFaceGroup).SelectMany((FaceGroup x) => (x.SubGroups.Count <= 0) ? x.ToIEnumerable() : x.SubGroups)
				select x.ID), combinedBendDescriptor.Count, combinedBendDescriptor.TotalLength, combinedBendDescriptor.TotalLengthWithoutGaps, combinedBendDescriptor[0].BendParams.Angle, combinedBendDescriptor[0].BendParams.FinalRadius));
			num++;
		}
	}

	private void WriteObj(IDoc3d doc, DisassemblyPart part)
	{
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (!general3DConfig.P3D_ObjExportMode && !general3DConfig.P3D_ObjExportModeUnfoldModel && !general3DConfig.P3D_GlbExportModel && !general3DConfig.P3D_GlbExportModeUnfoldModel)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			foreach (IBendDescriptor item in combinedBendDescriptor.Enumerable)
			{
				dictionary.TryAdd(item.BendParams.UnfoldFaceGroup.ID, combinedBendDescriptor.Order);
			}
		}
		ModelColors3DConfig modelColors = this._configProvider.InjectOrCreate<ModelColors3DConfig>();
		if (general3DConfig.P3D_ObjExportMode)
		{
			new ObjWriter().WriteObj(doc.EntryModel3D, doc.VisibleFaceGroupId, doc.VisibleFaceGroupSide, dictionary, Path.Combine(this._pathService.FolderCad3d2Pn, $"{part.ID}_original.obj"), modelColors);
		}
		if (general3DConfig.P3D_ObjExportModeUnfoldModel)
		{
			new ObjWriter().WriteObj(doc.UnfoldModel3D, doc.VisibleFaceGroupId, doc.VisibleFaceGroupSide, dictionary, Path.Combine(this._pathService.FolderCad3d2Pn, $"{part.ID}_unfolded.obj"), modelColors);
		}
		if (general3DConfig.P3D_GlbExportModel)
		{
			new GltfWriter(doc.EntryModel3D).WriteGltf(Path.Combine(this._pathService.FolderCad3d2Pn, $"{part.ID}_original.glb"));
		}
		if (general3DConfig.P3D_GlbExportModeUnfoldModel)
		{
			new GltfWriter(doc.UnfoldModel3D).WriteGltf(Path.Combine(this._pathService.FolderCad3d2Pn, $"{part.ID}_unfolded.glb"));
		}
	}

	private static void GetMacros(IDoc3d doc, Model model, DisassemblyPart part)
	{
		var (face, model2) = doc.UnfoldModel3D.GetFirstFaceOfGroupModel(doc.VisibleFaceGroupId, doc.VisibleFaceGroupSide);
		if (face == null)
		{
			return;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = face.Mesh.First().TriangleNormal;
		face.Shell.GetWorldMatrix(model2).TransformNormalInPlace(ref v);
		foreach (Model item in model.GetAllSubModelsWithSelf())
		{
			foreach (Shell shell in item.Shells)
			{
				global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix = shell.GetWorldMatrix(item);
				foreach (Macro macro in shell.Macros)
				{
					if (macro.GetType() == typeof(StepDrilling))
					{
						part.Tools.StepDrillings.Add(StepDrillingToCadConverter.GetXmlElement(macro as StepDrilling, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(CounterSink))
					{
						part.Tools.CounterSinks.Add(CounterSinkToCadConverter.GetXmlElement(macro as CounterSink, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(TwoSidedCounterSink))
					{
						part.Tools.CounterSinks.Add(TwoSideCountersinkToCadConverter.GetXmlElement(macro as TwoSidedCounterSink, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(EmbossedCounterSink))
					{
						part.Tools.EmbossedCountersinks.Add(EmbossedCounterSinkToCadConverter.GetXmlElement(macro as EmbossedCounterSink, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(Louver))
					{
						part.Tools.Louvers.Add(LouverToCadConverter.GetXmlElement(macro as Louver, worldMatrix));
					}
					else if (macro.GetType() == typeof(PressNut))
					{
						part.Tools.PressNuts.Add(PressNutToCadConverter.GetXmlElement(macro as PressNut, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(Lance))
					{
						part.Tools.Lances.Add(LanceToCadConverter.GetXmlElement(macro as Lance, worldMatrix));
					}
					else if (macro.GetType() == typeof(BridgeLance))
					{
						part.Tools.Bridges.Add(BridgeLanceToCadConverter.GetXmlElement(macro as BridgeLance, worldMatrix));
					}
					else if (macro.GetType() == typeof(Deepening))
					{
						part.Tools.Deepenings.Add(DeepeningToCadConverter.GetXmlElement(macro as Deepening, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossmentStamp))
					{
						part.Tools.EmbossmentStamps.Add(EmbossmentStampToCadConverter.GetXmlElement(macro as EmbossmentStamp, worldMatrix));
					}
					else if (macro.GetType() == typeof(BlindHole))
					{
						part.Tools.BlindHoles.Add(BlindHoleToCadConverter.GetXmlElement(macro as BlindHole, worldMatrix));
					}
					else if (macro.GetType() == typeof(ConicBlindHole))
					{
						part.Tools.ConicBlindHoles.Add(ConicBlindHoleToCadConverter.GetXmlElement(macro as ConicBlindHole, worldMatrix));
					}
					else if (macro.GetType() == typeof(SphericalBlindHole))
					{
						part.Tools.SphericalBlindHoles.Add(SphericalBlindHoleToCadConverter.GetXmlElement(macro as SphericalBlindHole, worldMatrix));
					}
					else if (macro.GetType() == typeof(Bolt))
					{
						part.Tools.Bolts.Add(BoltToCadConverter.GetXmlElement(macro as Bolt, worldMatrix, v));
					}
					else if (macro.GetType() == typeof(global::WiCAM.Pn4000.BendModel.BendTools.Macros.Thread))
					{
						part.Tools.Threads.Add(ThreadToCadConverter.GetXmlElement(macro as global::WiCAM.Pn4000.BendModel.BendTools.Macros.Thread, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedCircle))
					{
						part.Tools.EmbossedCircles.Add(EmbossedCircleToCadConverter.GetXmlElement(macro as EmbossedCircle, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedLine))
					{
						part.Tools.EmbossedLines.Add(EmbossedLineToCadConverter.GetXmlElement(macro as EmbossedLine, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedSquare))
					{
						part.Tools.EmbossedSquares.Add(EmbossedSquareToCadConverter.GetXmlElement(macro as EmbossedSquare, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedRectangle))
					{
						part.Tools.EmbossedRectangles.Add(EmbossedRectangleToCadConverter.GetXmlElement(macro as EmbossedRectangle, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedSquareRounded))
					{
						part.Tools.EmbossedSquaresRounded.Add(EmbossedSquareRoundedToCadConverter.GetXmlElement(macro as EmbossedSquareRounded, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedRectangleRounded))
					{
						part.Tools.EmbossedRectanglesRounded.Add(EmbossedRectangleRoundedToCadConverter.GetXmlElement(macro as EmbossedRectangleRounded, worldMatrix));
					}
					else if (macro.GetType() == typeof(EmbossedFreeform))
					{
						part.Tools.EmbossedFreeform.Add(EmbossedFreeformToCadConverter.GetXmlElement(macro as EmbossedFreeform, worldMatrix));
					}
					else if (macro.GetType() == typeof(SimpleHole))
					{
						part.Tools.SimpleHoles.Add(SimpleHoleToCadConverter.GetXmlElement(macro as SimpleHole, worldMatrix));
					}
					else if (macro.GetType() == typeof(Chamfer))
					{
						part.Tools.Chamfers.Add(ChamferToCadConverter.GetXmlElement(macro as Chamfer, worldMatrix));
					}
					else if (macro is ManufacturingMacro mm)
					{
						ManufacturingMacroToCadConverter.AddToSpecialTools(mm, part.Tools, worldMatrix);
					}
				}
			}
		}
	}

	internal static void UpdatePartsName(List<DisassemblyPart> disassemblyParts, bool flag, IPnPathService pathService)
	{
		PartAnalyzer.SetNamesByConfig(disassemblyParts, flag, pathService);
		PartAnalyzer.PartNameModification(disassemblyParts);
	}

	private static void SetNamesByConfig(IReadOnlyList<DisassemblyPart> disassemblyParts, bool flag, IPnPathService pathService)
	{
		if (disassemblyParts.Count == 1)
		{
			DisassemblyPart disassemblyPart = disassemblyParts[0];
			disassemblyPart.IsAssemblyName = false;
			disassemblyPart.Name = (disassemblyPart.OriginalGeometryName = (disassemblyPart.OriginalName = PartAnalyzer.GetFileName(pathService)));
			return;
		}
		if (flag)
		{
			foreach (DisassemblyPart disassemblyPart2 in disassemblyParts)
			{
				disassemblyPart2.Name = disassemblyPart2.OriginalAssemblyName;
				disassemblyPart2.IsAssemblyName = true;
			}
			return;
		}
		foreach (DisassemblyPart disassemblyPart3 in disassemblyParts)
		{
			disassemblyPart3.Name = disassemblyPart3.OriginalGeometryName;
			disassemblyPart3.IsAssemblyName = false;
		}
	}

	private static string GetFileName(IPnPathService pathService)
	{
		string path = Path.Combine(pathService.FolderCad3d2Pn, "name.txt");
		if (!File.Exists(path))
		{
			return "ErrorName";
		}
		return Path.GetFileNameWithoutExtension(File.ReadAllText(path).Trim());
	}

	private static void PartNameModification(IReadOnlyList<DisassemblyPart> disassemblyParts)
	{
		int num = disassemblyParts.Count.ToString().Length + 2;
		foreach (DisassemblyPart disassemblyPart in disassemblyParts)
		{
			disassemblyPart.OriginalName = disassemblyPart.Name;
			disassemblyPart.Name = disassemblyPart.Name.Replace(';', '_').Replace('|', '_').Replace("#", "!")
				.Replace(".", "_")
				.Replace("<", "(")
				.Replace(">", ")")
				.Replace("\ufffd", "Ö")
				.Replace("?", "!");
			if (disassemblyPart.Name.Length > 80 - num)
			{
				disassemblyPart.Name = disassemblyPart.Name.Substring(0, 80 - num);
			}
		}
		for (int i = 0; i < disassemblyParts.Count; i++)
		{
			List<DisassemblyPart> list = new List<DisassemblyPart> { disassemblyParts[i] };
			for (int j = i + 1; j < disassemblyParts.Count; j++)
			{
				if (disassemblyParts[j].Name == disassemblyParts[i].Name)
				{
					list.Add(disassemblyParts[j]);
				}
			}
			if (list.Count > 1)
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].Name = $"{list[k].Name}[{k + 1}]";
				}
			}
		}
	}

	private static global::WiCAM.Pn4000.PartsReader.DataClasses.ValidationResultIntrinsic GetIntrinsicError(global::WiCAM.Pn4000.BendModel.BendTools.Validations.ValidationResultIntrinsic error)
	{
		if (!(error is global::WiCAM.Pn4000.BendModel.BendTools.Validations.ValidationResultIntrinsicSimpelHoleR validationResultIntrinsicSimpelHoleR))
		{
			if (!(error is global::WiCAM.Pn4000.BendModel.BendTools.Validations.ValidationResultIntrinsicBendingR validationResultIntrinsicBendingR))
			{
				if (error is global::WiCAM.Pn4000.BendModel.BendTools.Validations.ValidationResultIntrinsicBendingH validationResultIntrinsicBendingH)
				{
					return new global::WiCAM.Pn4000.PartsReader.DataClasses.ValidationResultIntrinsicBendingH
					{
						HMin = validationResultIntrinsicBendingH.HMin,
						H = validationResultIntrinsicBendingH.H
					};
				}
				return null;
			}
			return new global::WiCAM.Pn4000.PartsReader.DataClasses.ValidationResultIntrinsicBendingR
			{
				Radius = validationResultIntrinsicBendingR.Radius,
				RadiusMin = validationResultIntrinsicBendingR.RadiusMin
			};
		}
		return new global::WiCAM.Pn4000.PartsReader.DataClasses.ValidationResultIntrinsicSimpelHoleR
		{
			Radius = validationResultIntrinsicSimpelHoleR.Radius,
			RadiusMin = validationResultIntrinsicSimpelHoleR.RadiusMin
		};
	}

	public void ReconstructBendsIfNecessary(IDoc3d doc, IImportArg? importSettings)
	{
		global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig analyzeConfig = ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService);
		if (((importSettings != null && importSettings.ReconstructBends == IImportArg.ReconstructBendsMode.ReconstructIfNecessary) || ((importSettings == null || !importSettings.ReconstructBends.HasValue) && analyzeConfig.ReconstructBendsConfig.ReconstructAfterImportMode == ReconstructionMode.ReconstructIfNecessary)) && !analyzeConfig.ReconstructBendsConfig.Enabled && doc.InputModel3D.PartInfo.PartType.HasFlag(global::WiCAM.Pn4000.BendModel.BendTools.PartType.Unknown))
		{
			doc.ReconstructIrregularBends();
		}
	}
}

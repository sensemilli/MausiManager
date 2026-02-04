using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig;
using BendDataSourceModel;
using Microsoft.Win32;
using WiCAM.OldPn;
using WiCAM.Pn4000.Archives.N3d;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.BendModel.Writer;
using WiCAM.Pn4000.BendModel.Writer.Writer;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.ModelConverter.GEOCAD;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.DeveloperTests;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.pn4UILib.Popup;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.pn4.Sym3D.Popup.View;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.Popup.StandardPopups.Model;
using WiCAM.Pn4000.Popup.StandardPopups.ViewModel;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Pipes;

public class PN3DDocPipe : IPN3DDocPipe
{
	private const string NewExt3DModel = ".c3mo";

	private const string NewExt3DDoc = ".c3do";

	private readonly Pn3DKernel _pN3DKernel;

	private readonly IFactorio _factorio;

	private readonly IPnPathService _pathService;

	private readonly ITranslator _translator;

	private readonly IPnBndDocImporter _docImporter;

	private readonly IShowPopupService _showPopupService;

	private readonly IModelFactory _modelFactory;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IAutoMode _autoMode;

	private readonly IPN3DBendPipe _bendPipe;

	private readonly IConfigProvider _configProvider;

	private readonly IMessageLogGlobal _logGlobal;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IUndo3dService _undo3dService;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IArvLoader _arvLoader;

	private readonly IAssemblyFactory _assemblyFactory;

	public ModelViewMode ModelType { get; set; }

	public bool IsShowingUnfold => this.ModelType == ModelViewMode.UnfoldModel;

	public PN3DDocPipe(Pn3DKernel pN3DKernel, IFactorio factorio, IPnPathService pathService, ITranslator translator, IPnBndDocImporter docImporter, IShowPopupService showPopupService, IModelFactory modelFactory, IScreen3DMain screen3DMain, IAutoMode autoMode, IPN3DBendPipe bendPipe, IConfigProvider configProvider, IMessageLogGlobal logGlobal, ICurrentDocProvider currentDocProvider, IMainWindowBlock mainWindowBlock, IArvLoader arvLoader, IAssemblyFactory assemblyFactory, IUndo3dService undo3dService)
	{
		this._pN3DKernel = pN3DKernel;
		this._factorio = factorio;
		this._pathService = pathService;
		this._translator = translator;
		this._docImporter = docImporter;
		this._showPopupService = showPopupService;
		this._modelFactory = modelFactory;
		this._screen3DMain = screen3DMain;
		this._autoMode = autoMode;
		this._bendPipe = bendPipe;
		this._configProvider = configProvider;
		this._logGlobal = logGlobal;
		this._currentDocProvider = currentDocProvider;
		this._undo3dService = undo3dService;
		this._mainWindowBlock = mainWindowBlock;
		this._arvLoader = arvLoader;
		this._assemblyFactory = assemblyFactory;
	}

	public void ExportGeoCadAction(IDoc3d doc)
	{
		if (doc?.EntryModel3D != null)
		{
			ExportGEOCAD.Export(doc.EntryModel3D, "GEOCAD", doc.DiskFile.Header.ModelName, doc.Thickness, doc.Material?.Number ?? 1);
		}
	}

	public void ExportStepStructure(IDoc3d doc)
	{
		if (!this._configProvider.InjectOrCreate<General3DConfig>().P3D_PartExportMode)
		{
			this._logGlobal.WithContext(doc).ShowErrorMessage("Option not available in current configuration");
			return;
		}
		string fileTypeCurrentPath = this._pathService.GetFileTypeCurrentPath("P3DDISSTEPEXP");
		string path = Path.Combine(this._pathService.PNHOMEPATH, "cad3d2pn", "name.txt");
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		if (array.Length < 1)
		{
			return;
		}
		PopupFolderSelectionModel popupFolderSelectionModel = this._modelFactory.Resolve<PopupFolderSelectionModel>();
		popupFolderSelectionModel.DefaultFolder = fileTypeCurrentPath;
		popupFolderSelectionModel.Name = Path.GetFileNameWithoutExtension(array[0]);
		PopupFolderSelectionViewModel popupFolderSelectionViewModel = this._modelFactory.Resolve<PopupFolderSelectionViewModel>().Init(popupFolderSelectionModel);
		PopupFolderSelectionView popupFolderSelectionView = this._modelFactory.Resolve<PopupFolderSelectionView>();
		popupFolderSelectionView.Owner = this._pN3DKernel.MainWindow;
		popupFolderSelectionView.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(popupFolderSelectionView.OnClosingAction, new Action<EPopupCloseReason>(popupFolderSelectionViewModel.ViewCloseAction));
		popupFolderSelectionView.DataContext = popupFolderSelectionViewModel;
		this._showPopupService.Show(popupFolderSelectionView, popupFolderSelectionView.CloseByRightButtonClickOutsideWindow);
		if (!string.IsNullOrEmpty(popupFolderSelectionModel.Result))
		{
			this._factorio.Resolve<IExportAsStepStructure>().Export(popupFolderSelectionModel.Result, doc);
			string text = this._pathService.CutLatestPathPart(popupFolderSelectionModel.Result);
			if (text != null)
			{
				this._pathService.SetFileTypeCurrentPath("P3DDISSTEPEXP", text);
			}
		}
	}

	public void MaterialAlliancePopupShow(IGlobals globals, IScreen3D screen3d)
	{
		PopupMaterialAllianceViewModel popupMaterialAllianceViewModel = this._factorio.Resolve<PopupMaterialAllianceViewModel>();
		PopupMaterialAllianceView popupMaterialAllianceView = this._factorio.Resolve<PopupMaterialAllianceView>();
		popupMaterialAllianceView.Owner = this._factorio.Resolve<IMainWindowDataProvider>() as Window;
		popupMaterialAllianceView.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(popupMaterialAllianceView.OnClosingAction, new Action<EPopupCloseReason>(popupMaterialAllianceViewModel.ViewCloseAction));
		popupMaterialAllianceView.DataContext = popupMaterialAllianceViewModel;
		screen3d.IgnoreMouseMove(ignore: true);
		this._showPopupService.Show(popupMaterialAllianceView, popupMaterialAllianceView.CloseByRightButtonClickOutsideWindow);
		screen3d.IgnoreMouseMove(ignore: false);
	}

	public void ExportAsmAsStepExternal(string folder)
	{
		this._factorio.Resolve<IExportAsStepStructure>().Export(folder, this._currentDocProvider.CurrentDoc);
	}

	public global::WiCAM.Pn4000.PN3D.Assembly.Assembly? CreateAssemblyAfterSpatial()
	{
		try
		{
			string folderCad3d2Pn = this._pathService.FolderCad3d2Pn;
			string path = Path.Combine(folderCad3d2Pn, "name.txt");
			string path2 = Path.Combine(folderCad3d2Pn, "log.txt");
			if (File.Exists(path) && File.Exists(path2) && (from x in File.ReadAllLines(path2)
				where !string.IsNullOrWhiteSpace(x)
				select x).ToList().Last().EndsWith("Finished"))
			{
				global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly = this._assemblyFactory.Create();
				assembly.FilenameImport = File.ReadAllLines(path).First();
				assembly.LoadingStatus = global::WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.SpatialFinished;
				this._factorio.Resolve<IAssemblyAnalysisManagement>().ImportPartList(assembly);
				return assembly;
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public F2exeReturnCode ImportSpatialAssembly(string fileName, bool checkLicense, bool moveToCenter, bool useBackground, out global::WiCAM.Pn4000.PN3D.Assembly.Assembly assembly, bool viewStyle = false, int machineNum = -1)
	{
		checkLicense = false;
		assembly = this._assemblyFactory.Create();
		assembly.FilenameImport = fileName;
		if (!File.Exists(fileName))
		{
			return F2exeReturnCode.ERROR_FILE_NOT_EXIST;
		}
		IGlobals pN3DKernel = this._pN3DKernel;
		IDoc3d emptyDoc = this._factorio.Resolve<IDoc3dFactory>().EmptyDoc;
		emptyDoc.PpModel = null;
		emptyDoc.MachineFullyLoaded = false;
		emptyDoc.MachinePath = null;
		emptyDoc.BendMachineConfig = null;
		this._currentDocProvider.CurrentDoc = emptyDoc;
		(string, string, int)? formatInfo = Import3DTypes.GetFormatInfo(fileName);
		if (!formatInfo.HasValue && !fileName.ToLower().EndsWith(".c3mo") && !fileName.ToLower().EndsWith(".c3do"))
		{
			pN3DKernel.MessageDisplay.ShowErrorMessage("Unsupported format!");
			return F2exeReturnCode.ERROR_FILE_FORMAT_NOT_SUPPORTED;
		}
		F2exeReturnCode f2exeReturnCode = this._docImporter.StartSpatial(fileName, formatInfo.Value.Item3, checkLicense, viewStyle, pN3DKernel, assembly);
		if (f2exeReturnCode == F2exeReturnCode.OK)
		{
			assembly.LoadingStatus = global::WiCAM.Pn4000.PN3D.Assembly.Assembly.EnumLoadingStatus.SpatialFinished;
			this._factorio.Resolve<IAssemblyAnalysisManagement>().ImportPartList(assembly);
			f2exeReturnCode = assembly.ProcessCode;
		}
		return f2exeReturnCode;
	}

	public void Print()
	{
		this._screen3DMain.Screen3D.ScreenD3D.PrintScreen();
		this._pN3DKernel.MainWindowDataProvider.Print3D();
	}

	public void OldPnStart(string param, bool for3D)
	{
		if (!for3D || (this._currentDocProvider.CurrentDoc != null && this._currentDocProvider.CurrentDoc.EntryModel3D != null))
		{
			StartOldPn.Start(this._pathService, param);
			if (for3D)
			{
				string filename = this._currentDocProvider.CurrentDoc?.DiskFile.Header.ImportedFilename;
				IDoc3d currentDoc = this._factorio.Resolve<IDoc3dFactory>().CreateDoc(filename);
				this._currentDocProvider.CurrentDoc = currentDoc;
			}
		}
	}

	private void Active3DViewTab()
	{
		this._pN3DKernel.MainWindowDataProvider.Ribbon_Activate3DViewTab();
	}

	private void UpdateInterfaceData(IDoc3d doc)
	{
		if (doc != null && string.IsNullOrEmpty(doc.DiskFile.Header.ModelName))
		{
			doc.DiskFile.Header.ModelName = Path.GetFileNameWithoutExtension(doc.DiskFile.Header.ModelSourceFileName);
		}
		doc?.UpdateGeneralInfo();
	}

	public int OpenFile3dWithParameters(string fileName, int overrideMachine, IMaterialArt overrideMaterial, string overrideModelname, out IDoc3d? newDoc)
	{
		newDoc = null;
		if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
		{
			this._pN3DKernel.MessageDisplay.ShowTranslatedErrorMessage("l_popup.ERROR_FILE_NOT_EXIST", fileName);
			return 5;
		}
		int num = this._pN3DKernel.MainWindowDataProvider.Ribbon_GetActiveTabID();
		if (num != 899 && num != 898)
		{
			this._pN3DKernel.Pn3DRootPipe.PN3DDocPipe.ActivateUnfoldTab();
		}
		this._currentDocProvider.CurrentDoc?.BendSimulation?.Pause();
		_ = this._pN3DKernel;
		string text = Path.GetExtension(fileName).ToUpper();
		if (!(text == ".C3DO"))
		{
			if (!(text == ".C3MO"))
			{
				return 6;
			}
			newDoc = this._docImporter.ImportC3MO(fileName, viewStyle: false);
			this._pathService.SetFileTypeCurrentPath("C3MO", Path.GetDirectoryName(fileName));
		}
		else
		{
			newDoc = this._docImporter.ImportC3DO(fileName, viewStyle: false);
			if (newDoc == null)
			{
				return 27;
			}
			if (!string.IsNullOrEmpty(overrideModelname))
			{
				newDoc.DiskFile.Header.ModelName = overrideModelname;
			}
			newDoc.PpModel = new BdsmDataModel();
			this._pathService.SetFileTypeCurrentPath("C3DO", Path.GetDirectoryName(fileName));
		}
		int result = 0;
		if (overrideMachine >= 0)
		{
			result = (int)this._pN3DKernel.Pn3DRootPipe.PN3DBendPipe.SelectBendMachineById(overrideMachine, newDoc);
		}
		if (overrideMaterial != null && overrideMaterial.Number != newDoc.Material?.Number)
		{
			newDoc.Material = overrideMaterial;
		}
		if (newDoc.BendMachine != null && !string.IsNullOrEmpty(newDoc.BendMachine.MachinePath) && Directory.Exists(newDoc.BendMachine.MachinePath))
		{
			IToolsAndBends? toolsAndBends = newDoc.ToolsAndBends;
			if (toolsAndBends != null && toolsAndBends.ToolSetups.Count > 0)
			{
				_ = newDoc.BendSimulation;
			}
		}
		if (newDoc.BendSimulation != null)
		{
			newDoc.BendSimulation.GoToBend(0);
		}
		try
		{
			string text2 = new FileInfo(fileName).Directory?.Parent?.Name ?? "";
			if (text2.StartsWith("ar") && int.TryParse(text2.Remove(0, 2), out var result2))
			{
				newDoc.SavedFileName = Path.GetFileNameWithoutExtension(fileName);
				newDoc.SavedArchiveNumber = result2;
			}
		}
		catch (Exception)
		{
		}
		newDoc?.UpdateGeneralInfo();
		return result;
	}

	public F2exeReturnCode OpenFileP3DExternal(string fileName, out IDoc3d? newDoc)
	{
		int result = this.OpenFile3dWithParameters(fileName, -1, null, null, out newDoc);
		this._currentDocProvider.CurrentDoc = newDoc;
		return (F2exeReturnCode)result;
	}

	public F2exeReturnCode Activate3DTab()
	{
		this._pN3DKernel.MainWindowDataProvider?.Ribbon_Activate3DTab();
		return F2exeReturnCode.OK;
	}

	public F2exeReturnCode ActivateUnfoldTab()
	{
		this._pN3DKernel.MainWindowDataProvider?.Ribbon_ActivateUnfoldTab();
		return F2exeReturnCode.OK;
	}

	public F2exeReturnCode UnfoldTube(IDoc3d doc, IGlobals globals, bool render = true)
	{
		if (doc == null || !doc.HasModel)
		{
			return F2exeReturnCode.ERROR_NO_DATA;
		}
		if (!doc.EntryModel3D.PartInfo.PartType.HasFlag(PartType.Tube))
		{
			return F2exeReturnCode.ERROR_INVALID_GEOMETRY;
		}
		return global::WiCAM.Pn4000.PN3D.Unfold.Unfold.CheckModelResultForBendZonesAndExtraElements(doc);
	}

	public void SetMaterialByUser(IDoc3d doc, IGlobals globals)
	{
		if (doc == null || !doc.HasModel)
		{
			globals.MessageDisplay.WithContext(doc).ShowErrorMessage(this._translator.Translate("BendView.BendPipe.SelectEntryModel"), this._translator.Translate("BendView.BendPipe.NoEntryModel"));
			return;
		}
		IMaterialArt material = doc.FrontCalls.GetMaterial(doc, globals, doc.MessageDisplay);
		if (material != null && material.Number != doc.Material?.Number)
		{
			this._undo3dService.Save(doc, this._translator.Translate("Undo3d.SetMaterial", doc.Material?.Number, doc.Material?.Name, material?.Number, material?.Name));
			doc.Material = material;
		}
	}

	public F2exeReturnCode UnfoldWithMessage(IGlobals globals, IDoc3d doc, bool render = true)
	{
		return F2exeReturnCode.OK;
	}

	public bool ReconstructFromSelectedFace(Face selectedFace, Model selectedFaceModel, IDoc3d doc, IGlobals globals)
	{
		if (doc == null)
		{
			globals.MessageDisplay.ShowErrorMessage(globals.LanguageDictionary.GetMsg2Int("Unfold Error") + ": " + F2exeReturnCode.ERROR_NO_DATA);
			return false;
		}
		if (doc.EntryModel3D.PartInfo.PartType.HasFlag(PartType.Unknown) || doc.EntryModel3D.PartInfo.PartType == PartType.Unassigned)
		{
			F2exeReturnCode f2exeReturnCode = F2exeReturnCode.ERROR_NOTUNFOLDABLE_GEOMETRY;
			return false;
		}
		if (selectedFace != null)
		{
			if (doc.ReconstructFromFace(selectedFace, selectedFaceModel))
			{
				this.WriteReconstructedModelToObj(doc);
				this.WriteReconstructedModelToGlb(doc);
				return true;
			}
			doc.MessageDisplay.ShowErrorMessage(globals.LanguageDictionary.GetMsg2Int("The product couldn't be reconstructed by that face."));
		}
		return false;
	}

	private void WriteReconstructedModelToObj(IDoc3d doc)
	{
		if (!this._configProvider.InjectOrCreate<General3DConfig>().P3D_ObjExportModeReconstructedModel)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			foreach (int item in from x in combinedBendDescriptor.Enumerable
				select x.BendParams.ModifiedEntryFaceGroup into x
				select x.ParentGroup?.ID ?? x.ID)
			{
				if (!dictionary.ContainsKey(item))
				{
					dictionary.TryAdd(item, combinedBendDescriptor.Order);
				}
			}
		}
		new ObjWriter().WriteObj(modelColors: this._configProvider.InjectOrCreate<ModelColors3DConfig>(), model: doc.ModifiedEntryModel3D, visibleFaceGroupId: doc.VisibleFaceGroupId, visibleFaceGroupSide: doc.VisibleFaceGroupSide, commonBendFaceGroupIds: dictionary, filename: Path.Combine(this._pathService.FolderCad3d2Pn, $"{doc.EntryModel3D.PartId}_reconstructed.obj"));
	}

	private void WriteReconstructedModelToGlb(IDoc3d doc)
	{
		if (!this._configProvider.InjectOrCreate<General3DConfig>().P3D_GlbExportModeReconstructedModel)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			foreach (int item in from x in combinedBendDescriptor.Enumerable
				select x.BendParams.ModifiedEntryFaceGroup into x
				select x.ParentGroup?.ID ?? x.ID)
			{
				if (!dictionary.ContainsKey(item))
				{
					dictionary.TryAdd(item, combinedBendDescriptor.Order);
				}
			}
		}
		new GltfWriter(doc.ModifiedEntryModel3D).WriteGltf(Path.Combine(this._pathService.FolderCad3d2Pn, $"{doc.EntryModel3D.PartId}_reconstructed.glb"));
	}

	public void ValidateGeometry(IDoc3d doc, IGlobals globals)
	{
		if (doc?.EntryModel3D != null)
		{
			ValidationSettingsConfig cfg = globals.ConfigProvider.InjectOrCreate<ValidationSettingsConfig>();
			if (!doc.HasModel)
			{
				string message = this._translator.Translate("BendView.BendPipe.SelectEntryModel");
				string caption = this._translator.Translate("BendView.BendPipe.NoEntryModel");
				doc.MessageDisplay.ShowErrorMessage(message, caption);
				doc.ValidationResults = null;
			}
			else
			{
				doc.ValidationResults = ModelValidation.ValidateModel(doc.UnfoldModel3D, cfg, doc.CheckSelfCollisionUnfoldModel);
				doc.SetModelDefaultColors();
			}
		}
	}

	public void ValidateGeometryReset(IDoc3d doc)
	{
		if (doc != null)
		{
			doc.ValidationResults = null;
			if (doc.HasModel)
			{
				doc.SetModelDefaultColors();
			}
		}
	}

	public void UnfoldFromSelectedFace(Triangle triangle, IGlobals globals, IDoc3d doc)
	{
		if (doc == null)
		{
			globals.MessageDisplay.ShowErrorMessage(globals.LanguageDictionary.GetMsg2Int("Unfold Error") + ": " + F2exeReturnCode.ERROR_NO_DATA);
		}
		else if (doc.EntryModel3D.PartInfo.PartType != PartType.Unassigned)
		{
			doc.EntryModel3D.UnHighLightModel();
			Face face = null;
			Model faceModel = null;
			if (triangle != null && triangle.Face.ID >= 0)
			{
				(face, faceModel) = doc.UnfoldModel3D.GetFaceModelById(triangle.Face.ID);
			}
			doc.SetTopFace(doc.UnfoldModel3D, face, faceModel);
			doc.SetModelDefaultColors();
		}
	}

	public F2exeReturnCode OpenP3D()
	{
		this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: true);
		RecentlyUsedRecord recentlyUsedRecord = this.CallPnArBr(this._pN3DKernel, 0).FirstOrDefault();
		if (recentlyUsedRecord == null)
		{
			this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
			return F2exeReturnCode.CANCEL_BY_USER;
		}
		string text = "." + recentlyUsedRecord.FileName.Split('.').Last();
		if (text != ".c3mo" && text != ".c3do")
		{
			this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
			return F2exeReturnCode.ERROR_FILE_FORMAT_NOT_SUPPORTED;
		}
		this.OpenFileP3DExternal(recentlyUsedRecord.FullPath + recentlyUsedRecord.FileName, out IDoc3d newDoc);
		if (newDoc != null)
		{
			this._pN3DKernel.MainWindowDataProvider.AddRecentlyUsedRecord(recentlyUsedRecord);
			newDoc.SavedArchiveNumber = recentlyUsedRecord.ArchiveID;
			newDoc.SavedFileName = recentlyUsedRecord.FileName.Remove(recentlyUsedRecord.FileName.LastIndexOf('.'));
			newDoc.UpdateGeneralInfo();
		}
		this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
		return F2exeReturnCode.OK;
	}

	public F2exeReturnCode DeleteP3D()
	{
		int multiSelect = 1;
		this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: true);
		int num = this._pN3DKernel.MainWindowDataProvider.Ribbon_GetActiveTabID();
		if (num != 899 && num != 898)
		{
			this._pN3DKernel.Pn3DRootPipe.PN3DDocPipe.ActivateUnfoldTab();
		}
		List<RecentlyUsedRecord> list = this.CallPnArBr(this._pN3DKernel, multiSelect);
		if (list.Count > 0)
		{
			foreach (RecentlyUsedRecord item in list)
			{
				File.Delete(item.FullPath + item.FileName);
			}
			this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
			return F2exeReturnCode.OK;
		}
		this._screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
		return F2exeReturnCode.CANCEL_BY_USER;
	}

	private List<RecentlyUsedRecord> CallPnArBr(IGlobals globals, int multiSelect)
	{
		int archiveID = ArchiveAdapter.ArchiveID;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(this._pathService.PNDRIVE);
		stringBuilder.Append("\\u\\pn\\run\\pnarbrdb.exe ");
		stringBuilder.AppendFormat("/archive:{0} /multiselect:{1} /type:n3d", archiveID, multiSelect);
		List<RecentlyUsedRecord> result = new List<RecentlyUsedRecord>();
		PnExternalCall.Start(stringBuilder.ToString(), globals.logCenterService);
		try
		{
			result = (from x in new Erg3dHandler(null).Read()
				select new RecentlyUsedRecord
				{
					ArchiveID = x.ArchiveNumber,
					FullPath = new FileInfo(x.Path).DirectoryName + "\\",
					Type = "P3D",
					FileName = new FileInfo(x.Path).Name
				}).ToList();
		}
		catch
		{
		}
		return result;
	}

	public F2exeReturnCode SaveP3DFileWithUi(IDoc3d doc, IGlobals globals, Window mainWindow, IAutoMode autoMode)
	{
		int num = this._pN3DKernel.MainWindowDataProvider.Ribbon_GetActiveTabID();
		if (num != 899 && num != 898)
		{
			this._pN3DKernel.Pn3DRootPipe.PN3DDocPipe.ActivateUnfoldTab();
		}
		if (!doc.HasRequiredUserComments() && this._configProvider.InjectOrCreate<UserCommentsConfig>().CheckBeforeSave)
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("DocSaveError.RequiredCommentsMissing");
			return F2exeReturnCode.ERROR_REQUIRED_COMMENTS_MISSING;
		}
		int archiveNumber = doc.SavedArchiveNumber;
		
		if (!GeneralSystemComponentsAdapter.Save3DPopup(out var path, out var fileName, ref archiveNumber))
		{
			return F2exeReturnCode.CANCEL_BY_USER;
		}
		if (doc == null)
		{
			return F2exeReturnCode.ERROR_PROBLEM_WITH_DOC;
		}
		//path = "C:\\u\\ar\\ar0007\\n3d\\";
		//fileName = doc.DiskFile.Header.ToString();
        string text = path + fileName;
		if ((File.Exists(text) || File.Exists(text + ".c3do")) && autoMode.PopupsEnabled && MessageBox.Show(mainWindow, globals.LanguageDictionary.GetMsg2Int("File exist. Owerwrite?"), globals.LanguageDictionary.GetMsg2Int("File"), MessageBoxButton.YesNo) == MessageBoxResult.No)
		{
			return F2exeReturnCode.ERROR_NO_OVERWRITE_PERMISSION;
		}
		doc.SavedFileName = fileName;
		doc.SavedArchiveNumber = archiveNumber;
		F2exeReturnCode result = this.SaveDocP3D(text, doc, archiveNumber);
		doc.UpdateGeneralInfo();
		return result;
	}

	public F2exeReturnCode SaveP3DFileWithLoop(string path, int archiveNumber, IDoc3d doc)
	{
		try
		{
			return this.SaveDocP3D(path, doc, archiveNumber);
		}
		catch
		{
			return F2exeReturnCode.ERROR_SAVE_3D_FILE;
		}
	}

	private void SaveModelP3D(IDoc3d doc, string fileName)
	{
		fileName += ".c3mo";
		doc.DiskFile.Header.ImportedFilename = fileName;
		ModelSerializer.Serialize(fileName, doc.EntryModel3D);
		this._pN3DKernel.MainWindowDataProvider.AddRecentlyUsedRecord(new RecentlyUsedRecord
		{
			FileName = Path.GetFileName(fileName),
			FullPath = Path.GetDirectoryName(fileName) + "\\",
			ArchiveID = 1,
			Type = "P3D"
		});
		this.UpdateInterfaceData(doc);
	}

	public F2exeReturnCode SaveDocP3D(string fileName, IDoc3d doc, int archiveNumber = 1)
	{

		fileName += ".c3do";
		if (!doc.HasRequiredUserComments() && this._configProvider.InjectOrCreate<UserCommentsConfig>().CheckBeforeSave)
		{
			return F2exeReturnCode.ERROR_REQUIRED_COMMENTS_MISSING;
		}
		doc.Save(fileName);
		this._pN3DKernel.MainWindowDataProvider?.AddRecentlyUsedRecord(new RecentlyUsedRecord
		{
			FileName = Path.GetFileName(fileName),
			FullPath = Path.GetDirectoryName(fileName) + "\\",
			ArchiveID = archiveNumber,
			Type = "P3D"
		});
		this.UpdateInterfaceData(doc);
		return F2exeReturnCode.OK;
	}

	public void AnalyzeDisassemblyData(IDoc3d doc)
	{
		if (AssemblyAnalysisManagement.IsModelForDisassemblyAtFiles())
		{
			this._factorio.Resolve<IAssemblyAnalysisManagement>().AnalyzeModel(doc, null, out var _, useBackgroundWorker: false, null, out var _);
		}
	}

	public static void ModelClean()
	{
	}

	public F2exeReturnCode Generate2D(IDoc3d doc, IGlobals globals)
	{
		if (doc.SafeModeUnfold)
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.WarningSafeMode");
			return F2exeReturnCode.ERROR_SAFEMODEUNFOLD;
		}
		if (doc.IsUpdateDocNeeded())
		{
			doc.UpdateDoc();
		}
		return this.Generate2D(doc, globals, doc.UnfoldModel3D, null, null, removeProjectionHoles: false, includeZeroBorders: true);
	}

	public F2exeReturnCode Generate2D(IDoc3d doc, IGlobals globals, Model model, Face face, Model faceModel, bool removeProjectionHoles, bool includeZeroBorders)
	{
		return this._factorio.Resolve<IModelForPnStdPreparationWrapper>().Apply2D<IModelForPnStdPreparation>(doc, model, face, faceModel, removeProjectionHoles, includeZeroBorders);
	}

	public void Node3DInfo()
	{
	}

	public void Show3DModelViewInfo(IDoc3d doc, IGlobals globals)
	{
		EasyInternalPopup easyInternalPopup = new EasyInternalPopup
		{
			Title = globals.LanguageDictionary.GetMsg2Int("Model3D Information"),
			Owner = this._pN3DKernel.MainWindow
		};
		easyInternalPopup.SetOkModel();
		easyInternalPopup.SetListModel();
		if (doc == null || doc.View3DModel.GetFaceCount() == 0)
		{
			easyInternalPopup.AddTextToListBox(globals.LanguageDictionary.GetMsg2Int("No model"));
		}
		else
		{
			easyInternalPopup.AddTextToListBox(globals.LanguageDictionary.GetMsg2Int("Model name") + ": " + doc.DiskFile.Header.ModelName);
			if (File.Exists("cad3d2pn\\type.txt"))
			{
				string[] array = File.ReadAllLines("cad3d2pn\\type.txt");
				for (int i = 0; i < array.Length; i++)
				{
					easyInternalPopup.AddTextToListBox(array[i]);
				}
			}
		}
		easyInternalPopup.ShowDialog();
	}

	public F2exeReturnCode DevImportHighTess(IPnPathService pathService, IGlobals globals)
	{
		return this.DevImport(pathService, viewStyle: false, globals, highTesselation: true);
	}

	public F2exeReturnCode DevImportLowTess(IPnPathService pathService, IGlobals globals)
	{
		return this.DevImport(pathService, viewStyle: true, globals, highTesselation: false);
	}

	public F2exeReturnCode DevImport(IPnPathService pathService, bool viewStyle, IGlobals globals, bool highTesselation)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			InitialDirectory = pathService.GetFileTypeCurrentPath("SMX"),
			Filter = "All files (*.*)|*.*|C3MO (*.c3mo)|*.c3mo| C3DO (*.c3do)|*.c3do|STL (*.stl)|*.stl|SMX (*.smx)|*.smx"
		};
		if (openFileDialog.ShowDialog() == false)
		{
			return F2exeReturnCode.CANCEL_BY_USER;
		}
		this._screen3DMain.Screen3D.InitWait(!this._autoMode.HasGui);
		string type = "";
		string fileName = openFileDialog.FileName;
		IDoc3d doc3d = null;
		string text = Path.GetExtension(fileName).ToUpper();
		if (text == null)
		{
			goto IL_0255;
		}
		int length = text.Length;
		if (length != 4)
		{
			if (length != 5)
			{
				goto IL_0255;
			}
			char c = text[3];
			if (c != 'D')
			{
				if (c != 'M' || !(text == ".C3MO"))
				{
					goto IL_0255;
				}
				type = "C3MO";
				doc3d = this._docImporter.ImportC3MO(fileName, viewStyle: false);
			}
			else
			{
				if (!(text == ".C3DO"))
				{
					goto IL_0255;
				}
				type = "C3DO";
				doc3d = this._docImporter.ImportC3DO(fileName, viewStyle: false);
			}
		}
		else
		{
			char c = text[2];
			if ((uint)c <= 77u)
			{
				if (c != 'L')
				{
					if (c != 'M' || !(text == ".SMX"))
					{
						goto IL_0255;
					}
					type = "SMX";
					doc3d = this._docImporter.ImportSmx(fileName, viewStyle: false, highTesselation);
				}
				else
				{
					if (!(text == ".DLD"))
					{
						goto IL_0255;
					}
					IDoc3dFactory doc3dFactory = this._factorio.Resolve<IDoc3dFactory>();
					List<(string, Model)> source = DLDLoader.LoadDLD(fileName);
					doc3d = doc3dFactory.CreateDoc(fileName);
					doc3d.EntryModel3D = source.First().Item2;
				}
			}
			else if (c != 'R')
			{
				if (c != 'T')
				{
					if (c != 'Z' || !(text == ".WZG"))
					{
						goto IL_0255;
					}
					type = "WZG";
					ConvertToPn.WriteCadGeo(WzgLoader.ReadWzg(File.ReadAllLines(fileName)), Path.ChangeExtension(fileName, ""));
				}
				else
				{
					if (!(text == ".STL"))
					{
						goto IL_0255;
					}
					type = "STL";
					doc3d = this._docImporter.ImportStl(fileName, viewStyle: false);
				}
			}
			else
			{
				if (!(text == ".ARV"))
				{
					goto IL_0255;
				}
				type = "ARV";
				this._arvLoader.Import(fileName);
			}
		}
		goto IL_0265;
		IL_0255:
		type = "PN3D";
		doc3d = this.ImportSpatialTessellation(fileName, viewStyle, isDevImport: true);
		goto IL_0265;
		IL_0265:
		if (doc3d != null)
		{
			doc3d.DiskFile.Header.ModelName = Path.GetFileNameWithoutExtension((!string.IsNullOrEmpty(fileName)) ? fileName.Trim() : doc3d.DiskFile.Header.ModelSourceFileName);
			doc3d.EntryModel3D.GetAllFaces().ForEach(delegate(Face f)
			{
				f.Color = f.ColorInitial;
			});
			this._currentDocProvider.CurrentDoc = doc3d;
			this.UpdateInterfaceData(doc3d);
		}
		pathService.SetFileTypeCurrentPath(type, Path.GetDirectoryName(fileName));
		this._screen3DMain.Screen3D.CloseWait(!this._autoMode.HasGui);
		return F2exeReturnCode.OK;
	}

	public F2exeReturnCode DevSave3D(IDoc3d doc)
	{
		if (doc == null)
		{
			return F2exeReturnCode.ERROR_PROBLEM_WITH_DOC;
		}
		string text = (string.IsNullOrEmpty(this._pathService.PNMASTER) ? this._pathService.PNDRIVE : this._pathService.PNMASTER);
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = this._pathService.BuildPath(text, "\\u\\pn\\machine_bend\\", "machine_bend_0000", "\\n3d");
		saveFileDialog.FileName = Path.GetFileNameWithoutExtension(doc.DiskFile.Header.ImportedFilename);
		SaveFileDialog saveFileDialog2 = saveFileDialog;
		if (saveFileDialog2.ShowDialog() == false)
		{
			return F2exeReturnCode.CANCEL_BY_USER;
		}
		this.SaveModelP3D(doc, saveFileDialog2.FileName);
		return F2exeReturnCode.OK;
	}

	private IDoc3d ImportSpatialTessellation(string fileName, bool viewStyle, bool isDevImport)
	{
		(string, string, int)? formatInfo = Import3DTypes.GetFormatInfo(fileName);
		if (formatInfo.HasValue)
		{
			bool flag = false;
			IMainWindowBlock mainWindowBlock = this._mainWindowBlock;
			if (!mainWindowBlock.BlockUI_IsBlock())
			{
				flag = true;
				mainWindowBlock.BlockUI_Block();
			}
			F2exeReturnCode code;
			IDoc3d doc3d = this._docImporter.CreateByImportSpatial(out code, fileName, formatInfo.Value.Item3, checkLicense: true, viewStyle, this._factorio, moveToCenter: false, analyze: true, -1, isDevImport);
			if (flag)
			{
				mainWindowBlock.BlockUI_Unblock();
			}
			if (viewStyle)
			{
				doc3d.InputModel3D = doc3d.View3DModel?.SubModels.FirstOrDefault();
			}
			return doc3d;
		}
		return null;
	}

	public void SetDocNameExternally(string extModelName, IDoc3d doc)
	{
		if (doc != null)
		{
			doc.DiskFile.Header.ModelName = extModelName;
		}
	}

	public F2exeReturnCode SetDocNcFilenameExternally(string extNcFilename, SetNcTimestampTypes addTimestamp, IDoc3d doc)
	{
		if (doc != null)
		{
			doc.SetNamesPpBase(extNcFilename.ToIEnumerable());
			doc.NamePPBase = extNcFilename;
			doc.NamePpTimestamps = addTimestamp;
			return F2exeReturnCode.OK;
		}
		return F2exeReturnCode.ERROR_PROBLEM_WITH_DOC;
	}

	public F2exeReturnCode Simulation(int collisionCheck, IDoc3d doc)
	{
		if (doc.BendSimulation != null)
		{
			doc.BendSimulation.State.CheckCollisions = Convert.ToBoolean(collisionCheck);
			doc.BendSimulation.State.CheckSelfCollisions = Convert.ToBoolean(collisionCheck);
		}
		return this._bendPipe.AutoBend(doc);
	}

	private static void AllowUiToUpdate()
	{
		DispatcherFrame frame = new DispatcherFrame();
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, (DispatcherOperationCallback)delegate
		{
			frame.Continue = false;
			return (object)null;
		}, null);
		Dispatcher.PushFrame(frame);
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate
		{
		});
	}
}

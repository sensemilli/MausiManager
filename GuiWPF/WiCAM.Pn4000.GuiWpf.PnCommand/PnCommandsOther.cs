using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.Assembly;
using WiCAM.Pn4000.GuiWpf.Ui3D;
using WiCAM.Pn4000.GuiWpf.Ui3D.Legend;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

public class PnCommandsOther : IPnCommandsOther
{
	private readonly IFactorio _factorio;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IPnPathService _pathService;

	private readonly IModelFactory _modelFactory;

	private IShowPopupService _showPopupService;

	private readonly IAutoMode _autoMode;

	private readonly IImportMaterialMapper _importMaterialMapper;

	private readonly IMaterialManager _materials;

	private readonly IConfigProvider _configProvider;

	private readonly ITranslator _translator;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IMaterial3dFortran _material3dFortran;

	private readonly IDoEvents _doEvents;

	private readonly IDocManager _docManager;

	private readonly IUndo3dService _undo3dService;

	private readonly Window _mainWindow;

	private IGlobals Globals { get; }

	private IPnCommandBasics Cmd { get; }

	private IMainWindowDataProvider _mainWindowDataProvider { get; }

	private IPN3DDocPipe DocPipe { get; }

	private IScreen3D Screen3D => _screen3DMain.Screen3D;

	public PnCommandsOther(IPnCommandBasics cmd, IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, IFactorio factorio, ICurrentDocProvider currentDocProvider, IPN3DDocPipe docPipe, IPnPathService pathService, IModelFactory modelFactory, IShowPopupService showPopupService, IAutoMode autoMode, IImportMaterialMapper importMaterialMapper, IMaterialManager materials, IConfigProvider configProvider, ITranslator translator, IScreen3DMain screen3D, IMainWindowBlock mainWindowBlock, IMaterial3dFortran material3dFortran, IDoEvents doEvents, IDocManager docManager, IUndo3dService undo3dService)
	{
		_factorio = factorio;
		DocPipe = docPipe;
		_pathService = pathService;
		_modelFactory = modelFactory;
		_showPopupService = showPopupService;
		_autoMode = autoMode;
		_importMaterialMapper = importMaterialMapper;
		_materials = materials;
		_configProvider = configProvider;
		_translator = translator;
		Cmd = cmd;
		Globals = globals;
		_currentDocProvider = currentDocProvider;
		_mainWindowDataProvider = mainWindowDataProvider;
		_screen3DMain = screen3D;
		_mainWindowBlock = mainWindowBlock;
		_material3dFortran = material3dFortran;
		_doEvents = doEvents;
		_docManager = docManager;
		_undo3dService = undo3dService;
		_mainWindow = mainWindowDataProvider as Window;
	}

	public F2exeReturnCode ImportUniversal(IPnCommandArg arg, IImportArg importSetting)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => ImportUniversal(importSetting), showWaitCursor: false, blockRibbon: false, mainThread: true, "ImportUniversal", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	private F2exeReturnCode ImportUniversal(IImportArg importSetting)
	{
		if (importSetting.CloseAllDocs)
		{
			_docManager.CloseAllDocuments();
		}
		importSetting.LoadLastAssembly = false;
		importSetting.NoPopups = !_factorio.Resolve<IAutoMode>().PopupsEnabled;
		if (importSetting.MachineNumber.HasValue && importSetting.MachineNumber.Value <= 0)
		{
			importSetting.MachineNumber = null;
		}
		if (importSetting.MaterialNumber.HasValue && importSetting.MaterialNumber.Value <= 0)
		{
			importSetting.MaterialNumber = null;
		}
		string type = (importSetting.UseHd ? "PN3D" : "VIEW3D");
		if (string.IsNullOrEmpty(importSetting.Filename) && !importSetting.NoPopups)
		{
			try
			{
				OpenFileDialog openFileDialog = new OpenFileDialog
				{
					InitialDirectory = _pathService.GetFileTypeCurrentPath(type),
					Multiselect = true,
					Filter = Import3DTypes.GetFileDialogFilter()
				};
				if (openFileDialog.ShowDialog() == false)
				{
					return F2exeReturnCode.CANCEL_BY_USER;
				}
				if (openFileDialog.FileNames.Length > 1)
				{
					F2exeReturnCode f2exeReturnCode = F2exeReturnCode.OK;
					string[] fileNames = openFileDialog.FileNames;
					foreach (string filename in fileNames)
					{
						importSetting.Filename = filename;
						f2exeReturnCode = ImportUniversal(importSetting);
						if (f2exeReturnCode == F2exeReturnCode.CANCEL_BY_USER)
						{
							return f2exeReturnCode;
						}
					}
					return f2exeReturnCode;
				}
				importSetting.Filename = openFileDialog.FileName;
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
				throw;
			}
		}
		if (!importSetting.UseHd)
		{
			WiCAM.Pn4000.PN3D.Assembly.Assembly assembly;
			F2exeReturnCode f2exeReturnCode2 = DocPipe.ImportSpatialAssembly(importSetting.Filename, importSetting.CheckLicense, importSetting.MoveToCenter, useBackground: false, out assembly, !importSetting.UseHd);
			if (f2exeReturnCode2 != 0)
			{
				return f2exeReturnCode2;
			}
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			assembly.SetDefaultMaterialsCalcPositions(importSetting, _importMaterialMapper, _material3dFortran.GetActiveMaterial(isAssembly: false)?.Number ?? (-1), _configProvider.InjectOrCreate<General3DConfig>());
			IAssemblyAnalysisManagement assemblyAnalysisManagement = _factorio.Resolve<IAssemblyAnalysisManagement>();
			assemblyAnalysisManagement.Init(assembly, importSetting);
			assemblyAnalysisManagement.Step1LoadLowTesselation(cancellationTokenSource.Token);
			_mainWindowDataProvider.Ribbon_Activate3DViewTab();
			IDoc3d doc3d = _factorio.Resolve<IDoc3dFactory>().CreateDoc(importSetting.Filename, isAssemblyLoading: true, assembly.Guid);
			doc3d.View3DModel = new Model();
			doc3d.View3DModel.Name = "Root";
			processNode(assembly.RootNode, doc3d.View3DModel);
			_currentDocProvider.CurrentDoc = doc3d;
			return f2exeReturnCode2;
		}
		string text = Path.GetExtension(importSetting.Filename).ToUpper();
		IDoc3d newDoc;
		if (text == ".C3DO")
		{
			return DocPipe.OpenFileP3DExternal(importSetting.Filename, out newDoc);
		}
		if (text == ".C3MO")
		{
			return DocPipe.OpenFileP3DExternal(importSetting.Filename, out newDoc);
		}
		_mainWindowBlock.BlockUI_Block();
		IAssemblyView assemblyView = _factorio.Resolve<IAssemblyView>();
		bool waitingForAnswer = true;
		F2exeReturnCode returnCode = F2exeReturnCode.Undefined;
		if (!assemblyView.Init(delegate(UserControl x)
		{
			_mainWindowDataProvider.SetViewForConfig(x);
			_mainWindowBlock.BlockUI_Unblock();
		}, null, importSetting, delegate(F2exeReturnCode x)
		{
			returnCode = x;
			waitingForAnswer = false;
		}))
		{
			return returnCode;
		}
		try
		{
			if (!string.IsNullOrWhiteSpace(importSetting.Filename) && !importSetting.NoPopups)
			{
				_pathService.SetFileTypeCurrentPath(type, Path.GetDirectoryName(importSetting.Filename));
				_mainWindowDataProvider.AddRecentlyUsedRecord(new RecentlyUsedRecord
				{
					FileName = Path.GetFileName(importSetting.Filename),
					FullPath = Path.GetDirectoryName(importSetting.Filename) + "\\",
					ArchiveID = 0,
					Type = type
				});
			}
		}
		catch (Exception)
		{
		}
		if (waitingForAnswer)
		{
			_mainWindowDataProvider.SetViewForConfig(assemblyView);
		}
		while (waitingForAnswer)
		{
			_doEvents.DoEvents(2);
		}
		return returnCode;
		static void processNode(DisassemblyPartNode node, Model model)
		{
			if (node.Part != null)
			{
				model.Shell = node.Part.ModelLowTesselation.Shell;
				model.Transform = node.WorldMatrix * (model.Parent?.WorldMatrix.Inverted ?? Matrix4d.Identity);
				model.Name = node.Part.OriginalAssemblyName;
			}
			foreach (DisassemblyPartNode child in node.Children)
			{
				Model model2 = new Model(model);
				processNode(child, model2);
			}
		}
	}

	private F2exeReturnCode OpenCurrentAssembly(IImportArg importSetting)
	{
		importSetting.LoadLastAssembly = true;
		importSetting.OpenSingleParts = false;
		importSetting.MaterialNumber = null;
		importSetting.MachineNumber = null;
		importSetting.NoPopups = !_factorio.Resolve<IAutoMode>().PopupsEnabled || importSetting.OpenPartId.HasValue;
		IDoc3d doc = _currentDocProvider.CurrentDoc;
		_mainWindowBlock.BlockUI_Block();
		WiCAM.Pn4000.PN3D.Assembly.Assembly assembly = doc?.Assembly;
		if (assembly != null)
		{
			DocMetadata docMetadata = new DocMetadata();
			docMetadata.CopyFromMetaData(doc.MetaData);
			docMetadata.Save(_pathService);
			DisassemblyPart disassemblyPart = assembly.DisassemblyParts.FirstOrDefault((DisassemblyPart x) => x.GetDoc(force: false) == doc);
			if (disassemblyPart != null)
			{
				disassemblyPart.PnMaterialID = doc.MaterialNumber;
			}
			string filename = Path.Combine(_pathService.FolderCad3d2Pn, docMetadata.DocAssemblyId + ".c3do");
			doc.Save(filename);
			foreach (DisassemblyPart item in assembly.DisassemblyParts.Where((DisassemblyPart x) => x.ID == doc.DocAssemblyId))
			{
				item.Metadata = new DocMetadata();
				item.Metadata.CopyFromMetaData(docMetadata);
				item.LoadDataFromMetadata();
			}
		}
		F2exeReturnCode result = F2exeReturnCode.Undefined;
		IAssemblyView assemblyView = _factorio.Resolve<IAssemblyView>();
		if (!assemblyView.Init(delegate(UserControl x)
		{
			_mainWindowDataProvider.SetViewForConfig(x);
			_mainWindowBlock.BlockUI_Unblock();
		}, assembly, importSetting, delegate(F2exeReturnCode x)
		{
			result = x;
		}))
		{
			return result;
		}
		if (importSetting.OpenPartId.HasValue)
		{
			return result;
		}
		_mainWindowDataProvider.SetViewForConfig(assemblyView);
		return F2exeReturnCode.Undefined;
	}

	public F2exeReturnCode Import(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => ImportUniversal(new ImportArg
		{
			SpecialPartSelectionMode = IImportArg.SpecialPartSelectionModes.None
		}), showWaitCursor: false, blockRibbon: true, mainThread: true, "Import", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode ImportFlat(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => ImportUniversal(new ImportArg
		{
			SpecialPartSelectionMode = IImportArg.SpecialPartSelectionModes.Flat
		}), showWaitCursor: true, blockRibbon: true, mainThread: true, "ImportFlat", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode ImportUnfold(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => ImportUniversal(new ImportArg
		{
			SpecialPartSelectionMode = IImportArg.SpecialPartSelectionModes.NotFlat
		}), showWaitCursor: true, blockRibbon: true, mainThread: true, "ImportUnfold", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode OpenView(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => ImportUniversal(new ImportArg
		{
			UseHd = false,
			SpecialPartSelectionMode = IImportArg.SpecialPartSelectionModes.None
		}), showWaitCursor: true, blockRibbon: true, mainThread: true, "OpenView", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Disassembly(IPnCommandArg arg, int? partId)
	{
		Cmd.DoCmd(arg, (IPnCommandArg arg) => OpenCurrentAssembly(new ImportArg
		{
			LoadLastAssembly = true,
			OpenPartId = partId
		}), showWaitCursor: true, blockRibbon: true, mainThread: true, "Disassembly", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void DisStepExp(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.ExportStepStructure(arg.doc());
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "DisStepExp", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void ExportCadGeo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.ExportGeoCadAction(arg.doc());
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ExportCadGeo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode Open(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.OpenP3D(), showWaitCursor: true, blockRibbon: true, mainThread: true, "Open", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode Delete(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.DeleteP3D(), showWaitCursor: true, blockRibbon: true, mainThread: true, "Delete", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode Save(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.SaveP3DFileWithUi(arg.doc(), Globals, _mainWindow, _autoMode), showWaitCursor: false, blockRibbon: true, mainThread: true, "Save", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void AnalyzeAssembly(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.AnalyzeDisassemblyData(arg.doc());
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "AnalyzeAssembly", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Clean(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			PN3DDocPipe.ModelClean();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Clean", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void CloseDocument(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			arg.Doc.Close();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "CloseDocument", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Undo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_undo3dService.Undo(arg.Doc);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Undo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Redo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_undo3dService.Redo(arg.Doc);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Redo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void MaterialAlliance(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			DocPipe.MaterialAlliancePopupShow(Globals, Screen3D);
		}, showWaitCursor: false, blockRibbon: true, mainThread: true, "MaterialAlliance", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void UnfoldConfig(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldSettingPopupManager.Start(_pathService, _configProvider, Screen3D, _modelFactory);
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "UnfoldConfig", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void ShowInfo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, (Action<IPnCommandArg>)ShowInfoInternal, showWaitCursor: false, blockRibbon: false, mainThread: true, "ShowInfo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	private void ShowInfoInternal(IPnCommandArg arg)
	{
		IDoc3d doc3d = arg.doc();
		if (!doc3d.HasModel)
		{
			string message = _translator.Translate("BendView.BendPipe.SelectEntryModel");
			string caption = _translator.Translate("BendView.BendPipe.NoEntryModel");
			doc3d.MessageDisplay.ShowErrorMessage(message, caption);
		}
		else if (_autoMode.PopupsEnabled)
		{
			PopupUnfoldInfoView popupUnfoldInfoView = _factorio.Resolve<PopupUnfoldInfoView>();
			popupUnfoldInfoView.Init(doc3d);
			_screen3DMain.Screen3D.IgnoreMouseMove(ignore: true);
			_showPopupService.Show(popupUnfoldInfoView, popupUnfoldInfoView.CloseByRightButtonClickOutsideWindow);
			_screen3DMain.Screen3D.IgnoreMouseMove(ignore: false);
		}
	}

	public void ShowLegend(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			if (_factorio.Resolve<IAutoMode>().PopupsEnabled)
			{
				LegendView legendView = _factorio.Resolve<LegendView>();
				ILegendViewModel legendViewModel = _factorio.Resolve<ILegendViewModel>();
				legendViewModel.Init(arg.Doc);
				legendView.Init(legendViewModel);
				Screen3D.IgnoreMouseMove(ignore: true);
				_showPopupService.Show(legendView, legendView.CloseByRightButtonClickOutsideWindow);
				Screen3D.IgnoreMouseMove(ignore: false);
			}
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "ShowLegend", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Node3DInfo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			DocPipe.Node3DInfo();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Node3DInfo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void View3DInfo(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.Show3DModelViewInfo(arg.doc(), Globals);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "View3DInfo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode DevImport(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.DevImportHighTess(_pathService, Globals), showWaitCursor: true, blockRibbon: true, mainThread: true, "DevImport", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode DevImportLowTess(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.DevImportLowTess(_pathService, Globals), showWaitCursor: true, blockRibbon: true, mainThread: true, "DevImportLowTess", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public F2exeReturnCode DevSave(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.DevSave3D(arg.doc()), showWaitCursor: true, blockRibbon: true, mainThread: true, "DevSave", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void Print(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			DocPipe.Print();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Print", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void OldPnStart(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.OldPnStart(arg.CommandParam, for3D: false);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "OldPnStart", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}

	public void OldPnStart3D(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.OldPnStart(arg.CommandParam, for3D: true);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "OldPnStart3D", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsOther.cs");
	}
}

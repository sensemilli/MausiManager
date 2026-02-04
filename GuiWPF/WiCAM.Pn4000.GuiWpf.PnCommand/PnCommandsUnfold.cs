using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiWpf.TabUnfold;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

internal class PnCommandsUnfold : IPnCommandsUnfold
{
	private readonly MainWindowViewModel _mainWindowViewModel;

	private readonly IFactorio _factorio;

	private readonly IAutoMode _autoMode;

	private readonly IScreen3DMain _screen3D;

	private readonly IConfigProvider _configProvider;

	private readonly IDoEvents _doEvents;

	private IPnCommandBasics Cmd { get; }

	private IGlobals Globals { get; }

	private IMainWindowDataProvider _mainWindowDataProvider { get; }

	private IUnfoldViewModel UnfoldViewModel => _mainWindowViewModel.UnfoldViewModel;

	private IPN3DDocPipe DocPipe { get; }

	public PnCommandsUnfold(IPnCommandBasics cmd, IGlobals globals, IMainWindowDataProvider mainWindowDataProvider, MainWindowViewModel mainWindowViewModel, IFactorio factorio, IPN3DDocPipe docPipe, IAutoMode autoMode, IScreen3DMain screen3D, IConfigProvider configProvider, IDoEvents doEvents)
	{
		_mainWindowViewModel = mainWindowViewModel;
		_factorio = factorio;
		_autoMode = autoMode;
		_screen3D = screen3D;
		_configProvider = configProvider;
		_doEvents = doEvents;
		DocPipe = docPipe;
		Cmd = cmd;
		Globals = globals;
		_mainWindowDataProvider = mainWindowDataProvider;
	}

	public void PipetteFaceColorForVisibleFace(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel?.PipetteFaceColorForVisibleFace();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "PipetteFaceColorForVisibleFace", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ReconstructFromFaceSelectFace(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel?.ReconstructFromSelectFace();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "ReconstructFromFaceSelectFace", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ReconstructIrregularBends(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg x)
		{
			F2exeReturnCode result = x.Doc.ReconstructIrregularBends();
			UnfoldViewModel.ShowModel(ModelViewMode.OriginalEntryModel);
			_mainWindowViewModel.RefreshScreenActiveTab();
			return result;
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ReconstructIrregularBends", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ReconstructBendsExperimental(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg x)
		{
			F2exeReturnCode result = x.Doc.ReconstructIrregularBends(experimental: true);
			UnfoldViewModel.ShowModel(ModelViewMode.OriginalEntryModel);
			_mainWindowViewModel.RefreshScreenActiveTab();
			return result;
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ReconstructBendsExperimental", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ValidateGeometry(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.ValidateGeometry(arg.doc(), Globals);
			if (_mainWindowViewModel.GetActiveTab() == UnfoldViewModel)
			{
				UnfoldViewModel.ShowModel(ModelViewMode.UnfoldModel);
			}
			else
			{
				_screen3D.ScreenD3D.UpdateAllModelAppearance(render: true);
			}
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ValidateGeometry", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ValidateGeometryReset(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.ValidateGeometryReset(arg.doc());
			_screen3D.ScreenD3D.UpdateAllModelAppearance(render: true);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ValidateGeometryReset", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void UnfoldFromFaceSelectFace(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel?.UnfoldFromSelectFace();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "UnfoldFromFaceSelectFace", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public F2exeReturnCode UnfoldTube(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, (IPnCommandArg arg) => DocPipe.UnfoldTube(arg.doc(), Globals), showWaitCursor: true, blockRibbon: true, mainThread: true, "UnfoldTube", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ViewInputModel(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel.ShowModel(ModelViewMode.InputModel);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ViewInputModel", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ViewEntryModel(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel.ShowModel(ModelViewMode.OriginalEntryModel);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ViewEntryModel", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ViewModifiedModel(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel.ShowModel(ModelViewMode.ModifiedEntryModel);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ViewModifiedModel", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void ViewUnfoldModel(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel.ShowModel(ModelViewMode.UnfoldModel);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ViewUnfoldModel", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public F2exeReturnCode UnfoldWithMessage(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, delegate
		{
			_screen3D?.ScreenD3D.RemoveModel(null);
			_screen3D?.ScreenD3D.RemoveBillboard(null);
			UnfoldViewModel.ShowModel(ModelViewMode.UnfoldModel, setView: true, zoomExtend: true, animate: true);
			if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().UnfoldTime > 0)
			{
				bool done = false;
				UnfoldViewModel.UnfoldAnimation(delegate
				{
					done = true;
				});
				while (!done)
				{
					_doEvents.DoEvents();
				}
			}
			return F2exeReturnCode.OK;
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "UnfoldWithMessage", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void SetMaterial(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			DocPipe.SetMaterialByUser(arg.doc(), Globals);
			_mainWindowViewModel.RefreshScreenActiveTab();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "SetMaterial", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public F2exeReturnCode Transfer2D(IPnCommandArg arg)
	{
		return Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			F2exeReturnCode f2exeReturnCode = DocPipe.Generate2D(arg.doc(), Globals);
			if (_autoMode.HasGui && f2exeReturnCode <= F2exeReturnCode.OK)
			{
				_mainWindowDataProvider.Ribbon_ActivateCadTab();
			}
			return f2exeReturnCode;
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "Transfer2D", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}

	public void Transfer2DFromModifiedFace(IPnCommandArg arg, bool removeProjectionHoles)
	{
		Cmd.DoCmd(arg, delegate
		{
			UnfoldViewModel?.Transfer2DFromSelectModifiedFace(removeProjectionHoles);
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "Transfer2DFromModifiedFace", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsUnfold.cs");
	}
}

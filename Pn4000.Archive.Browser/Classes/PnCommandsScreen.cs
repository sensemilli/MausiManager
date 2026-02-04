using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiWpf.GeneralSubWindow;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

internal class PnCommandsScreen : IPnCommandsScreen
{
	private readonly MainWindowViewModel _mainWindowViewModel;

	private readonly IFactorio _factorio;

	private readonly IScreen3DMain _screen3DMain;

	private IPnCommandBasics Cmd { get; }

	private Screen3D Screen3D => _screen3DMain.Screen3D;

	public PnCommandsScreen(IPnCommandBasics cmd, MainWindowViewModel mainWindowViewModel, IFactorio factorio, IScreen3DMain screen3DMain)
	{
		_mainWindowViewModel = mainWindowViewModel;
		_factorio = factorio;
		_screen3DMain = screen3DMain;
		Cmd = cmd;
	}

	public void Angle3D(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			//AngleMeasurementViewModel angleMeasurementViewModel = _factorio.Resolve<AngleMeasurementViewModel>();
			ITab activeTab = _mainWindowViewModel.GetActiveTab();
			if (activeTab != null)
			{
			//	activeTab.ActiveSubViewModel = angleMeasurementViewModel;
			}
		//	angleMeasurementViewModel.MeasureAngle();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "Angle3D", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void SelectPoint3D(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			GeometryMeasureToolsViewModel geometryMeasureToolsViewModel = _factorio.Resolve<GeometryMeasureToolsViewModel>();
			geometryMeasureToolsViewModel.Init(arg.doc());
			ITab activeTab = _mainWindowViewModel.GetActiveTab();
			if (activeTab != null)
			{
				activeTab.ActiveSubViewModel = geometryMeasureToolsViewModel;
			}
			geometryMeasureToolsViewModel.SelectPoint3D();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "SelectPoint3D", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void Distance3D(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			GeometryMeasureToolsViewModel geometryMeasureToolsViewModel = _factorio.Resolve<GeometryMeasureToolsViewModel>();
			geometryMeasureToolsViewModel.Init(arg.doc());
			ITab activeTab = _mainWindowViewModel.GetActiveTab();
			if (activeTab != null)
			{
				activeTab.ActiveSubViewModel = geometryMeasureToolsViewModel;
			}
			geometryMeasureToolsViewModel.Distance3D();
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "Distance3D", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_J3CSHA(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.ShowFacesAndWires(showFaces: true, showWires: true);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_J3CSHA", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_J3FO(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.ShowFacesAndWires(showFaces: true, showWires: false);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_J3FO", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_J3BLCH(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.ShowFacesAndWires(showFaces: false, showWires: true);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_J3BLCH", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_WIRESMODE(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.ChangeWiresType();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_WIRESMODE", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_TRA3D100(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetDefaultOpacity(1f);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_TRA3D100", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_TRA3D75(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetDefaultOpacity(0.75f);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_TRA3D75", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_TRA3D50(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetDefaultOpacity(0.5f);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_TRA3D50", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_TRA3D25(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetDefaultOpacity(0.25f);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_TRA3D25", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_ROT3DFREE(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetMouseRotationMode(MouseRotationMode.Free);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_ROT3DFREE", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_ROT3DX(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetMouseRotationMode(MouseRotationMode.X);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_ROT3DX", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_ROT3DY(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetMouseRotationMode(MouseRotationMode.Y);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_ROT3DY", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}

	public void V3D_ROT3DZ(IPnCommandArg arg)
	{
		Cmd.DoCmd(arg, delegate
		{
			Screen3D.SetMouseRotationMode(MouseRotationMode.Z);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "V3D_ROT3DZ", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsScreen.cs");
	}
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.ManualCameraStateView;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP;

internal class PPManualSetupViewModel : PopupViewModelBase, IDisposable, IPPManualSetupViewModel
{
	private IFactorio _factorio;

	private IConfigProvider _configProvider;

	private readonly IGlobalPpScreenshotConfig _globalPpScreenshotConfig;

	private Screen3D _adjustmentScreen;

	private ICommand _finalizeCommand;

	private ICommand _resetCommand;

	private double _screen3DWidth = 800.0;

	private double _screen3DHeight = 600.0;

	private Action<ISimulationScreen> _setupAction;

	private ICameraState? _cameraState;

	private string _title = "hiya :3";

	public FrameworkElement AdjustmentScreen => _adjustmentScreen;

	public ICommand FinilizeCommand => _finalizeCommand = new RelayCommand(Finish);

	public ICommand ResetCommand => _resetCommand = new RelayCommand(Reset);

	public double Screen3DWidth => _screen3DWidth;

	public double Screen3DHeight => _screen3DHeight;

	public ICameraState? CameraState => _cameraState;

	public string Title => _title;

	public PPManualSetupViewModel(IConfigProvider configProvider, IFactorio factorio, IGlobalPpScreenshotConfig globalPpScreenshotConfig)
	{
		_adjustmentScreen = new Screen3D();
		_adjustmentScreen.SetConfigProviderAndApplySettings(configProvider);
		_adjustmentScreen.Loaded += delegate
		{
			_adjustmentScreen.ScreenD3D.Renderer.RenderData.ShadowMode = RenderData.Shadows.None;
		};
		_factorio = factorio;
		_globalPpScreenshotConfig = globalPpScreenshotConfig;
		_configProvider = configProvider;
	}

	public IPPManualSetupViewModel Init(double screenWidth, double screenHeight, string title, Action<ISimulationScreen> setupAction)
	{
		_screen3DWidth = screenWidth;
		_screen3DHeight = screenHeight;
		_title = title;
		_setupAction = setupAction;
		OnPropertyChanged("Screen3DWidth");
		OnPropertyChanged("Screen3DHeight");
		OnPropertyChanged("Title");
		Screen3D adjustmentScreen = _adjustmentScreen;
		if (adjustmentScreen != null && adjustmentScreen.IsLoaded)
		{
			setupAction(_adjustmentScreen.ScreenD3D);
		}
		else
		{
			_adjustmentScreen.Loaded += delegate
			{
				setupAction(_adjustmentScreen.ScreenD3D);
			};
		}
		return this;
	}

	public void Finish(EPopupCloseReason reason)
	{
		Finish();
	}

	public void Finish()
	{
		_cameraState = _adjustmentScreen.ScreenD3D.Renderer.ExportCameraState();
		CloseView();
	}

	public void Reset()
	{
		if (_adjustmentScreen.IsLoaded)
		{
			_setupAction(_adjustmentScreen.ScreenD3D);
			_adjustmentScreen.ScreenD3D.Render(skipQueuedFrames: false);
		}
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		throw new NotImplementedException();
	}

	public ICameraState GetCameraState(double screenWidth, double screenHeight, string title, Action<ISimulationScreen> initScreenAction, IBendMachine machine, ScreenshotType type, bool dontDefault, out ProjectionType? projectionType, out Matrix4d? viewMatrix, out Dictionary<MachineParts, double> partRoleOpacities)
	{
		IPpScreenshotConfigItem value;
		IPpScreenshotConfigItem ppScreenshotConfigItem = (machine.PpScreenshotConfig.Items.TryGetValue(type, out value) ? value : _globalPpScreenshotConfig.GetDefaultCustomizeValues(type));
		projectionType = ppScreenshotConfigItem.ProjectionType;
		viewMatrix = ppScreenshotConfigItem.ViewRotation;
		partRoleOpacities = ppScreenshotConfigItem.Opacities;
		if (ppScreenshotConfigItem != null && ppScreenshotConfigItem.AdjustManually)
		{
			Init(screenWidth, screenHeight, title, initScreenAction);
			PPManualSetupView pPManualSetupView = new PPManualSetupView(_factorio.Resolve<ILogCenterService>(), _factorio.Resolve<IConfigProvider>(), _factorio.Resolve<IShowPopupService>(), _factorio.Resolve<IPKernelFlowGlobalDataService>());
			pPManualSetupView.AllowRightClicking = true;
			pPManualSetupView.Owner = _factorio.Resolve<IPKernelFlowGlobalDataService>().MainWindow;
			pPManualSetupView.OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(pPManualSetupView.OnClosingAction, new Action<EPopupCloseReason>(Finish));
			pPManualSetupView.DataContext = this;
			_factorio.Resolve<IShowPopupService>().Show(pPManualSetupView, pPManualSetupView.CloseByRightButtonClickOutsideWindow);
			return CameraState;
		}
		return null;
	}

	private static string GetMachineIdentifier(IBendMachine machine, ScreenshotType type)
	{
		string arg = machine?.Name ?? "NULL";
		int num = (int)type;
		return $"{arg}{num.ToString()}";
	}
}

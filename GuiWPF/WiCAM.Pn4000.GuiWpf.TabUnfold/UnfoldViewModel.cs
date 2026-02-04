using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.GuiWpf.GeneralSubWindow;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.PN3D.Unfold.UI.Model;
using WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

internal class UnfoldViewModel : ViewModelBase, IUnfoldViewModel, ITab
{
	private Visibility _visible = Visibility.Collapsed;

	private ContextMenuViewModel? _contextMenuViewModel;

	private ContextMenuStepBendWithoutMachineViewModel _contextMenuStepBendWithoutMachineViewModel;

	private ISubViewModel? _activeSubViewModel;

	private readonly IInfoViewModel _infoViewModel;

	private readonly IScreen3DMain _screen3DMain;

	private readonly Pn3DKernel _kernel3D;

	private readonly IGlobals _globals;

	private readonly IRibbon _ribbon;

	private readonly ITranslator _translator;

	private readonly IAutoMode _autoMode;

	private readonly IPnPathService _pnPathService;

	private readonly IStatus3dDefault _status3dDefault;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IPN3DDocPipe _pn3DDocPipe;

	private readonly IConfigProvider _configProvider;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IDoc3d _currentDoc;

	private readonly IPaintTool _painter;

	private readonly IDoEvents _doEvents;

	private ModelViewMode _modelTypeActive;

	private CameraState? _cameraStateInputModel;

	private CameraState? _cameraStateEntryModel;

	private CameraState? _cameraStateModifiedModel;

	private CameraState? _cameraStateUnfoldModel;

	public bool IsActive { get; private set; }
    public double Left
    {
        get => _left;
        set
        {
            if (_left != value)
            {
                _left = value;
                NotifyPropertyChanged(nameof(Left));
            }
        }
    }

    public double Top
    {
        get => _top;
        set
        {
            if (_top != value)
            {
                _top = value;
                NotifyPropertyChanged(nameof(Top));
            }
        }
    }

    private double _left;
    private double _top;
    public Visibility Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible != value)
			{
				_visible = value;
				NotifyPropertyChanged("Visible");
			}
		}
	}

	public ContextMenuViewModel? ContextMenuViewModel
	{
		get
		{
			return _contextMenuViewModel;
		}
		set
		{
			if (_contextMenuViewModel != value)
			{
				_contextMenuViewModel = value;
				NotifyPropertyChanged("ContextMenuViewModel");
				NotifyPropertyChanged("ContextMenuViewModelVisibility");
			}
		}
	}

	public ContextMenuStepBendWithoutMachineViewModel ContextMenuStepBendWithoutMachineViewModel
	{
		get
		{
			return _contextMenuStepBendWithoutMachineViewModel;
		}
		set
		{
			if (_contextMenuStepBendWithoutMachineViewModel != value)
			{
				_contextMenuStepBendWithoutMachineViewModel = value;
				NotifyPropertyChanged("ContextMenuStepBendWithoutMachineViewModel");
				NotifyPropertyChanged("ContextMenuStepBendWithoutMachineViewVisibility");
			}
		}
	}

	public Visibility ContextMenuViewModelVisibility
	{
		get
		{
			if (_contextMenuViewModel != null)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility ContextMenuStepBendWithoutMachineViewVisibility
	{
		get
		{
			if (_contextMenuStepBendWithoutMachineViewModel != null)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public IValidationViewModel ValidationViewModel { get; }

	public IIssueViewModel IssueViewModel { get; }

	public Visibility ValidationVisibility => Visibility.Visible;

	public ISubViewModel? ActiveSubViewModel
	{
		get
		{
			return _activeSubViewModel;
		}
		set
		{
			if (_activeSubViewModel == value)
			{
				return;
			}
			if (_activeSubViewModel != null)
			{
				_activeSubViewModel.Close();
				if (_activeSubViewModel != null)
				{
					_activeSubViewModel.RequestRepaint -= RepaintModel;
				}
			}
			_activeSubViewModel = value;
			if (_activeSubViewModel != null)
			{
				_activeSubViewModel.Closed += SubViewModelClosed;
				_activeSubViewModel.RequestRepaint += RepaintModel;
				_infoViewModel?.SetActive(active: false);
			}
			NotifyPropertyChanged("ActiveSubViewModel");
		}
	}

	private Screen3D Screen3D => _screen3DMain.Screen3D;

	public ModelViewMode ModelTypeActive
	{
		get
		{
			return _modelTypeActive;
		}
		set
		{
			_kernel3D.Pn3DRootPipe.PN3DDocPipe.ModelType = value;
			if (_modelTypeActive != value)
			{
				ModelViewMode modelTypeActive = _modelTypeActive;
				_modelTypeActive = value;
				ModelTypeChanged(modelTypeActive, _modelTypeActive);
			}
		}
	}

	public Model CurrentRenderModel { get; set; }

	public void ShowModel(ModelViewMode mode, bool setView = true, bool zoomExtend = true, bool animate = false)
	{
		SaveCameraState();
		Screen3D?.ScreenD3D?.RemoveModel(null, render: false);
		Screen3D?.ScreenD3D?.RemoveBillboard(null, render: false);
		if (mode == ModelViewMode.ModifiedEntryModel && _currentDoc.ModifiedEntryModel3D == null)
		{
			mode = ModelViewMode.OriginalEntryModel;
		}
		if (mode == ModelViewMode.UnfoldModel && _currentDoc.UnfoldModel3D == null)
		{
			mode = ModelViewMode.OriginalEntryModel;
		}
		_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_INP3D", value: false);
		_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_ENT3D", value: false);
		_ribbon?.Ribbon_SelectButton(91, "V3D_CNT3D", value: false);
		_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_UNF3D", value: false);
		if (mode == ModelViewMode.InputModel && _currentDoc.InputModel3D == null)
		{
			return;
		}
		switch (mode)
		{
		case ModelViewMode.InputModel:
			CurrentRenderModel = _currentDoc.InputModel3D;
			_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_INP3D", value: true);
			break;
		case ModelViewMode.UnfoldModel:
			CurrentRenderModel = _currentDoc.UnfoldModel3D;
			_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_UNF3D", value: true);
			break;
		case ModelViewMode.ModifiedEntryModel:
			CurrentRenderModel = _currentDoc.ModifiedEntryModel3D;
			_ribbon?.Ribbon_SelectButton(91, "V3D_CNT3D", value: true);
			break;
		case ModelViewMode.OriginalEntryModel:
			CurrentRenderModel = _currentDoc.EntryModel3D;
			_ribbon?.Ribbon_SelectButton(91, "V3D_CNT_ENT3D", value: true);
			break;
		}
		ModelTypeActive = mode;
		Screen3D.ScreenD3D.AddModel(CurrentRenderModel, render: false);
		_currentDoc.UpdateGeneralInfo(mode);
		CameraState? cameraState = null;
		switch (mode)
		{
		case ModelViewMode.InputModel:
			if (_cameraStateInputModel.HasValue)
			{
				cameraState = _cameraStateInputModel;
			}
			break;
		case ModelViewMode.OriginalEntryModel:
			if (_cameraStateEntryModel.HasValue)
			{
				cameraState = _cameraStateEntryModel;
			}
			break;
		case ModelViewMode.ModifiedEntryModel:
			if (_cameraStateModifiedModel.HasValue)
			{
				cameraState = _cameraStateModifiedModel;
			}
			break;
		case ModelViewMode.UnfoldModel:
			if (_cameraStateUnfoldModel.HasValue)
			{
				cameraState = _cameraStateUnfoldModel;
			}
			break;
		}
		if (cameraState.HasValue)
		{
			Screen3D.ScreenD3D.Renderer.ImportCameraState(cameraState.Value);
		}
		else
		{
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(Math.PI / 4.0);
			identity *= Matrix4d.RotationX(Math.PI / 3.0);
			Screen3D.ScreenD3D.SetViewDirectionByMatrix4d(identity, render: false);
			Screen3D.ScreenD3D.ZoomExtend();
		}
		EventWaitHandle wait = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);
		Screen3D.ScreenD3D.Render(skipQueuedFrames: true, delegate
		{
			wait.Set();
		});
		wait.WaitOne();
		_doEvents.DoEvents(500);
	}

	public void UnfoldAnimation(Action finishedCallback)
	{
		GeneralUserSettingsConfig config = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (config.UnfoldTime <= 0)
		{
			return;
		}
		_mainWindowBlock.BlockUI_Block(_currentDoc);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		Task.Run(delegate
		{
			double step = 0.0;
			long num = 0L;
			List<ICombinedBendDescriptorInternal> source = _currentDoc.CombinedBendDescriptors.Where((ICombinedBendDescriptorInternal cbd) => _currentDoc.CombinedBendDescriptors.All(delegate(ICombinedBendDescriptorInternal x)
			{
				IReadOnlyList<ICombinedBendDescriptor> splitPredecessors = x.SplitPredecessors;
				return splitPredecessors == null || !splitPredecessors.Contains(cbd);
			})).ToList();
			Parallel.ForEach(source, delegate(ICombinedBendDescriptorInternal cbd)
			{
				cbd.UnfoldBendInUnfoldModel(1.0, relative: false);
			});
			foreach (IBendDescriptor bendDescriptor in _currentDoc.BendDescriptors)
			{
				Screen3D.ScreenD3D.UpdateModelGeometry(bendDescriptor.BendParams.UnfoldFaceGroupModel, render: false);
			}
			Screen3D.ScreenD3D.UpdateAllModelTransform();
			while (step < 1.0)
			{
				sw.Stop();
				long elapsedMilliseconds = sw.ElapsedMilliseconds;
				sw.Start();
				long num2 = elapsedMilliseconds - num;
				long num3 = 16 - num2;
				if (num3 > 0)
				{
					Task.Delay((int)num3).Wait();
					sw.Stop();
					elapsedMilliseconds = sw.ElapsedMilliseconds;
					sw.Start();
				}
				step = Math.Min(1.0, (double)elapsedMilliseconds / ((double)config.UnfoldTime * 1000.0));
				Parallel.ForEach(source, delegate(ICombinedBendDescriptorInternal cbd)
				{
					cbd.UnfoldBendInUnfoldModel(step, relative: false);
				});
				foreach (IBendDescriptor bendDescriptor2 in _currentDoc.BendDescriptors)
				{
					Screen3D.ScreenD3D.UpdateModelGeometry(bendDescriptor2.BendParams.UnfoldFaceGroupModel, render: false);
				}
				Screen3D.ScreenD3D.UpdateAllModelTransform();
			}
			Application.Current.Dispatcher.Invoke(delegate
			{
				_mainWindowBlock.BlockUI_Unblock(_currentDoc);
			});
			sw.Stop();
			finishedCallback();
		});
	}

	private void SaveCameraState()
	{
		if (IsActive)
		{
			switch (ModelTypeActive)
			{
			case ModelViewMode.InputModel:
				_cameraStateInputModel = Screen3D.ScreenD3D.Renderer.ExportCameraState();
				break;
			case ModelViewMode.OriginalEntryModel:
				_cameraStateEntryModel = Screen3D.ScreenD3D.Renderer.ExportCameraState();
				break;
			case ModelViewMode.ModifiedEntryModel:
				_cameraStateModifiedModel = Screen3D.ScreenD3D.Renderer.ExportCameraState();
				break;
			case ModelViewMode.UnfoldModel:
				_cameraStateUnfoldModel = Screen3D.ScreenD3D.Renderer.ExportCameraState();
				break;
			}
		}
	}

	private void ModelChanged(Model oldModel, Model newModel)
	{
		CurrentRenderModel = newModel;
		Screen3D.ScreenD3D.RemoveModel(null, render: false);
		Screen3D.ScreenD3D.AddModel(newModel, render: false);
		Screen3D.ScreenD3D.RemoveBillboard(null, render: false);
		_currentDoc.UpdateGeneralInfo(ModelTypeActive);
		Screen3D.ScreenD3D.Render(skipQueuedFrames: false);
	}

	public UnfoldViewModel(Pn3DKernel kernel3D, IMainWindowBlock mainWindowBlock, IPN3DDocPipe pn3DDocPipe, IRibbon ribbon, ITranslator translator, IAutoMode autoMode, IPnPathService pnPathService, IStatus3dDefault status3dDefault, IConfigProvider configProvider, IScreen3DMain screen3DMain, IShortcutSettingsCommon shortcutSettingsCommon, IInfoViewModel infoViewModel, IDoc3d currentDoc, IIssueViewModel issueViewModel, IValidationViewModel validationViewModel, IPaintTool painter, IDoEvents doEvents)
	{
		_globals = kernel3D;
		_kernel3D = kernel3D;
		_ribbon = ribbon;
		_translator = translator;
		_autoMode = autoMode;
		_pnPathService = pnPathService;
		_status3dDefault = status3dDefault;
		_screen3DMain = screen3DMain;
		_configProvider = configProvider;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		ContextMenuViewModel = null;
		ContextMenuStepBendWithoutMachineViewModel = null;
		_mainWindowBlock = mainWindowBlock;
		_pn3DDocPipe = pn3DDocPipe;
		_currentDoc = currentDoc;
		IssueViewModel = issueViewModel;
		_infoViewModel = infoViewModel;
		_infoViewModel.SetActiveModelType(ModelTypeActive);
		ValidationViewModel = validationViewModel;
		_painter = painter;
		_doEvents = doEvents;
		ValidationViewModel.SetActiveModelType(ModelTypeActive);
		ValidationViewModel.RequestRepaint += RepaintModel;
	}

	private void SubViewModelClosed(ISubViewModel sender, Triangle tri, Model model, double x, double y, Vector3d hitPoint, MouseButtonEventArgs mouseButtonEventArgs)
	{
		Screen3D.InteractionMode = new NavigateInteractionMode(Screen3D);
		ContextMenuViewModel = null;
		ContextMenuStepBendWithoutMachineViewModel = null;
		if (sender != null)
		{
			sender.Closed -= SubViewModelClosed;
			sender.RequestRepaint -= RepaintModel;
			if (sender == _activeSubViewModel)
			{
				_activeSubViewModel = null;
			}
		}
	}

	private void ModelTypeChanged(ModelViewMode oldMode, ModelViewMode newMode)
	{
		ValidationViewModel.SetActiveModelType(newMode);
		_infoViewModel.SetActiveModelType(newMode);
		Screen3D.HideScreenInfoText();
		switch (oldMode)
		{
		case ModelViewMode.InputModel:
			_currentDoc.EntryModel3DChanged -= ModelChanged;
			break;
		case ModelViewMode.OriginalEntryModel:
			_currentDoc.EntryModel3DChanged -= ModelChanged;
			break;
		case ModelViewMode.ModifiedEntryModel:
			_currentDoc.ModifiedEntryModel3DChanged -= ModelChanged;
			break;
		case ModelViewMode.UnfoldModel:
			_currentDoc.UnfoldModel3DChanged -= ModelChanged;
			break;
		}
		switch (newMode)
		{
		case ModelViewMode.InputModel:
			_currentDoc.EntryModel3DChanged += ModelChanged;
			break;
		case ModelViewMode.OriginalEntryModel:
			_currentDoc.EntryModel3DChanged += ModelChanged;
			break;
		case ModelViewMode.ModifiedEntryModel:
			_currentDoc.ModifiedEntryModel3DChanged += ModelChanged;
			break;
		case ModelViewMode.UnfoldModel:
			_currentDoc.UnfoldModel3DChanged += ModelChanged;
			break;
		}
	}

	public void RefreshScreen()
	{
		ShowModel(ModelTypeActive, setView: false, zoomExtend: false);
	}

	public void SetActive(bool show)
	{
		if (IsActive != show)
		{
			if (show)
			{
				Screen3D.ScreenD3D.Renderer.LayerBlacklist.Clear();
				ShowModel(ModelTypeActive, setView: true, zoomExtend: false);
				Visible = Visibility.Visible;
				Screen3D.TriangleSelected += MouseSelectTriangle;
				Screen3D.KeyUp += Pn3DScreenOnKeyUp;
				Screen3D screen3D = Screen3D;
				screen3D.ExternalKeyDown = (KeyEventHandler)Delegate.Combine(screen3D.ExternalKeyDown, new KeyEventHandler(Pn3DScreenOnKeyUp));
				Screen3D.ScreenD3D.Renderer.RenderData.AutoAdjustFloorToScreenRotation = true;
				RepaintModel();
				_status3dDefault.ShowDefaultStatusBars();
			}
			else
			{
				SaveCameraState();
				Visible = Visibility.Collapsed;
				Screen3D.TriangleSelected -= MouseSelectTriangle;
				Screen3D.KeyUp -= Pn3DScreenOnKeyUp;
				Screen3D screen3D2 = Screen3D;
				screen3D2.ExternalKeyDown = (KeyEventHandler)Delegate.Remove(screen3D2.ExternalKeyDown, new KeyEventHandler(Pn3DScreenOnKeyUp));
			}
			IsActive = show;
			ActiveSubViewModel?.SetActive(IsActive);
			_infoViewModel?.SetActive(IsActive);
		}
	}

	private void Pn3DScreenOnKeyUp(object sender, KeyEventArgs e)
	{
		PnInputEventArgs pnInputEventArgs = new PnInputEventArgs(e, keyUp: true);
		ActiveSubViewModel?.KeyUp(sender, pnInputEventArgs);
		if (!pnInputEventArgs.Handled && _shortcutSettingsCommon.Cancel.IsShortcut(pnInputEventArgs))
		{
			ActiveSubViewModel?.Close();
			pnInputEventArgs.Handle();
		}
		if (!pnInputEventArgs.Handled)
		{
			Screen3D.NavigationKeyDown(sender, pnInputEventArgs);
		}
	}

	private bool IsValidTriangleForFaceSelection(ITriangleEventArgs e)
	{
		Model model = e.Model?.GetRootParent();
		if (model != null && model == _currentDoc.EntryModel3D)
		{
			Face face = e.Tri?.Face;
			if (face?.FaceGroup != null && (face.FaceGroup.Side0.Contains(face) || face.FaceGroup.Side1.Contains(face)) && !face.Shell.Macros.Any((Macro m) => m.Faces.Contains(face)) && !face.FaceGroup.IsBendingZone)
			{
				return !e.Model.PartInfo.NotConformFaces.Any((KeyValuePair<FaceGroup, List<Face>> f) => f.Value.Contains(face));
			}
			return false;
		}
		return false;
	}

	private void MouseSelectTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		if (e.Args.Handled)
		{
			return;
		}
		if (ActiveSubViewModel != null && e.MouseEventArgs != null)
		{
			ActiveSubViewModel.MouseSelectTriangle(this, e);
			if (e.Args.Handled)
			{
				return;
			}
		}
		if (_currentDoc.EntryModel3D == null)
		{
			return;
		}
		MouseButtonEventArgs? mouseEventArgs = e.MouseEventArgs;
		if (mouseEventArgs != null && mouseEventArgs.ChangedButton == MouseButton.Left)
		{
			ActiveSubViewModel?.Close();
			if (!_mainWindowBlock.BlockUI_IsBlock())
			{
				_infoViewModel.SetActive(active: true);
				_infoViewModel.MouseSelectTriangle(sender, e);
				ValidationViewModel?.MouseSelectTriangle(sender, e);
			}
		}
		else
		{
			MouseButtonEventArgs? mouseEventArgs2 = e.MouseEventArgs;
			if (mouseEventArgs2 != null && mouseEventArgs2.ChangedButton == MouseButton.Right)
			{
				ActiveSubViewModel?.Close();
				_infoViewModel.SetActive(active: false);
				MouseSelectBendingContextMenu(e.Tri, e.Model, e.MouseEventArgs, e.HitPoint);
			}
		}
		RepaintModel();
	}

	private void RepaintModel()
	{
		_painter.FrameStart();
		ValidationViewModel?.ColorModelParts(_painter);
		_infoViewModel?.ColorModelParts(_painter);
		ActiveSubViewModel?.ColorModelParts(_painter);
		_painter.FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);
		Screen3D.ScreenD3D.UpdateModelAppearance(ref modifiedModels, ref modifiedShells);
	}

	public void UnfoldFromSelectFace()
	{
		if (_currentDoc?.EntryModel3D == null || !IsActive || _mainWindowBlock.BlockUI_IsBlock())
		{
			return;
		}
		if (!_currentDoc.HasModel)
		{
			string message = _translator.Translate("BendView.BendPipe.SelectEntryModel");
			string caption = _translator.Translate("BendView.BendPipe.NoEntryModel");
			_currentDoc.MessageDisplay.ShowErrorMessage(message, caption);
			return;
		}
		ShowModel(ModelViewMode.OriginalEntryModel, setView: false, zoomExtend: false);
		Color highlightColor = _configProvider.InjectOrCreate<ModelColors3DConfig>().SelectionHighlightFaceColor.ToBendColor();
		ActiveSubViewModel = new ActionByFaceViewModel(_translator, Screen3D, highlightColor, highlightEdges: false, "UnfoldView.SelectUnfoldFace", IsValidTriangleForFaceSelection, delegate(ITriangleEventArgs e)
		{
			_mainWindowBlock.InitWait(_currentDoc);
			_mainWindowBlock.BlockUI_Block(_currentDoc);
			_pn3DDocPipe.UnfoldFromSelectedFace(e.Tri, _globals, _currentDoc);
			ShowModel(ModelViewMode.UnfoldModel, setView: true, zoomExtend: true, animate: true);
			_mainWindowBlock.BlockUI_Unblock(_currentDoc);
			_mainWindowBlock.CloseWait(_currentDoc);
		});
	}

	public void PipetteFaceColorForVisibleFace()
	{
		if (_currentDoc?.EntryModel3D == null || !IsActive || _mainWindowBlock.BlockUI_IsBlock())
		{
			return;
		}
		if (!_currentDoc.HasModel)
		{
			string message = _translator.Translate("BendView.BendPipe.SelectEntryModel");
			string caption = _translator.Translate("BendView.BendPipe.NoEntryModel");
			_currentDoc.MessageDisplay.ShowErrorMessage(message, caption);
			return;
		}
		ShowModel(ModelViewMode.OriginalEntryModel, setView: false, zoomExtend: false);
		ModelColors3DConfig modelColors3DConfig = _configProvider.InjectOrCreate<ModelColors3DConfig>();
		bool showOriginalColors = Screen3D.ScreenD3D.Renderer.RenderData.ShowOriginalColors;
		if (!showOriginalColors)
		{
			Screen3D.SetColorsToOriginal(value: true);
		}
		Color highlightColor = modelColors3DConfig.SelectionHighlightFaceColor.ToBendColor();
		ActiveSubViewModel = new SelectSpecialVisibleFaceViewModel(_translator, Screen3D, highlightColor, highlightEdges: true, "UnfoldView.SelectVisibleFaceSpecialColor", (ITriangleEventArgs e) => e.Tri != null, delegate(ITriangleEventArgs e)
		{
			_mainWindowBlock.InitWait(_currentDoc);
			_mainWindowBlock.BlockUI_Block(_currentDoc);
			Color? color = e.Tri?.Face?.ColorInitial;
			if (color.HasValue)
			{
				AnalyzeConfig analyzeConfig = _configProvider.InjectOrCreate<AnalyzeConfig>();
				analyzeConfig.SpecialVisibleFaceColor.A = color.Value.A;
				analyzeConfig.SpecialVisibleFaceColor.R = color.Value.R;
				analyzeConfig.SpecialVisibleFaceColor.G = color.Value.G;
				analyzeConfig.SpecialVisibleFaceColor.B = color.Value.B;
				_configProvider.Push(analyzeConfig);
				_configProvider.Save<AnalyzeConfig>();
			}
			_mainWindowBlock.BlockUI_Unblock(_currentDoc);
			_mainWindowBlock.CloseWait(_currentDoc);
		}, !showOriginalColors);
	}

	public void ReconstructFromSelectFace()
	{
		if (_currentDoc?.EntryModel3D == null || !IsActive || _mainWindowBlock.BlockUI_IsBlock())
		{
			return;
		}
		if (!_currentDoc.HasModel)
		{
			string message = _translator.Translate("BendView.BendPipe.SelectEntryModel");
			string caption = _translator.Translate("BendView.BendPipe.NoEntryModel");
			_currentDoc.MessageDisplay.ShowErrorMessage(message, caption);
			return;
		}
		ShowModel(ModelViewMode.OriginalEntryModel, setView: false, zoomExtend: false);
		Color highlightColor = _configProvider.InjectOrCreate<ModelColors3DConfig>().SelectionHighlightFaceColor.ToBendColor();
		ActiveSubViewModel = new ActionByFaceViewModel(_translator, Screen3D, highlightColor, highlightEdges: false, "UnfoldView.SelectReconstruct", IsValidTriangleForFaceSelection, delegate(ITriangleEventArgs e)
		{
			_mainWindowBlock.InitWait(_currentDoc);
			_mainWindowBlock.BlockUI_Block(_currentDoc);
			if (_pn3DDocPipe.ReconstructFromSelectedFace(e.Tri?.Face, e.Model, _currentDoc, _globals))
			{
				ShowModel(ModelViewMode.UnfoldModel, setView: true, zoomExtend: true, animate: true);
			}
			_mainWindowBlock.BlockUI_Unblock(_currentDoc);
			_mainWindowBlock.CloseWait(_currentDoc);
		});
	}

	public void Transfer2DFromSelectModifiedFace(bool removeProjectionHoles)
	{
		if (_currentDoc?.EntryModel3D == null || !IsActive || _mainWindowBlock.BlockUI_IsBlock())
		{
			return;
		}
		if (!_currentDoc.HasModel)
		{
			string message = _translator.Translate("BendView.BendPipe.SelectEntryModel");
			string caption = _translator.Translate("BendView.BendPipe.NoEntryModel");
			_currentDoc.MessageDisplay.ShowErrorMessage(message, caption);
			return;
		}
		if (_currentDoc.SafeModeUnfold)
		{
			_currentDoc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.WarningSafeMode");
			return;
		}
		if (_currentDoc.IsUpdateDocNeeded())
		{
			_currentDoc.UpdateDoc();
		}
		ShowModel(ModelViewMode.ModifiedEntryModel, setView: false, zoomExtend: false);
		Color highlightColor = _configProvider.InjectOrCreate<ModelColors3DConfig>().SelectionHighlightFaceColor.ToBendColor();
		ActiveSubViewModel = new ActionByFaceViewModel(_translator, Screen3D, highlightColor, highlightEdges: false, "UnfoldView.SelectFace", isValidTriangleForFaceSelection, delegate(ITriangleEventArgs e)
		{
			_mainWindowBlock.InitWait(_currentDoc);
			_mainWindowBlock.BlockUI_Block(_currentDoc);
			F2exeReturnCode f2exeReturnCode = _pn3DDocPipe.Generate2D(_currentDoc, _globals, _currentDoc.ModifiedEntryModel3D, e.Tri.Face, e.Model, removeProjectionHoles, includeZeroBorders: false);
			if (_autoMode.HasGui && f2exeReturnCode <= F2exeReturnCode.OK)
			{
				_ribbon.Ribbon_ActivateCadTab();
			}
			_mainWindowBlock.BlockUI_Unblock(_currentDoc);
			_mainWindowBlock.CloseWait(_currentDoc);
		});
		bool isValidTriangleForFaceSelection(ITriangleEventArgs e)
		{
			Model model = e.Model?.GetRootParent();
			if (model != null && model == _currentDoc.ModifiedEntryModel3D)
			{
				return e.Tri.Face.FaceType == FaceType.Flat;
			}
			return false;
		}
	}

	private bool MouseSelectBendingContextMenu(Triangle triangle, Model modelTri, MouseButtonEventArgs e, Vector3d hitPoint)
	{
		if (!_mainWindowBlock.BlockUI_IsBlock() && ModelTypeActive != 0 && ModelTypeActive != ModelViewMode.OriginalEntryModel)
		{
			Model model = modelTri?.GetRootParent();
			if (model == null || model.ModelType != ModelType.Part || CurrentRenderModel != model)
			{
				return false;
			}
			Vector3d v = hitPoint;
			model.WorldMatrix.Inverted.TransformInPlace(ref v);
			Triangle triangle2 = triangle;
			if (triangle2 != null && triangle2.Face?.FaceGroup?.IsBendingZone == true)
			{
				ICombinedBendDescriptorInternal selectedBend = _currentDoc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal c) => c.Enumerable.Any((IBendDescriptor b) => b.BendParams.UnfoldFaceGroup.ID == (triangle.Face.FaceGroup.ParentGroup ?? triangle.Face.FaceGroup).ID));
				int? entryGrpId = triangle?.Face?.FaceGroup.ParentGroup?.BendEntryId ?? triangle?.Face?.FaceGroup.BendEntryId;
				if ((from x in _currentDoc.CombinedBendDescriptors.SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable)
					where x.BendParams.EntryFaceGroup.ID == entryGrpId
					select x).Any((IBendDescriptor x) => x.Type == BendingType.StepBend))
				{
					ContextMenuStepBendWithoutMachineModel modelImpl = new ContextMenuStepBendWithoutMachineModel(triangle?.Face?.FaceGroup, _currentDoc, _globals);
					_configProvider.InjectOrCreate<ModelColors3DConfig>().SelectedObjectHighlightBorderColorPrimary.ToBendColor();
					ContextMenuStepBendWithoutMachineViewModel = new ContextMenuStepBendWithoutMachineViewModel(Screen3D, model, v, modelImpl, _globals, _currentDoc)
					{
						Width = 320.0,
						Height = 180.0
					};
					ActiveSubViewModel = ContextMenuStepBendWithoutMachineViewModel;
				}
				else
				{
					ContextMenuModel model2 = new ContextMenuModel(_currentDoc, selectedBend, _configProvider);
					Color highlightColor = _configProvider.InjectOrCreate<ModelColors3DConfig>().SelectedObjectHighlightBorderColorPrimary.ToBendColor();
					ContextMenuViewModel = new ContextMenuViewModel(model2, Screen3D, _globals, _currentDoc, _mainWindowBlock, model, v, highlightColor, null, CurrentRenderModel, _translator, _pnPathService, _shortcutSettingsCommon)
					{
						Width = 320.0,
						Height = 180.0
					};
					ActiveSubViewModel = ContextMenuViewModel;
				}
				RepaintModel();
				return true;
			}
		}
		return false;
	}

	public void Dispose()
	{
		SetActive(show: false);
		_infoViewModel?.Close();
		ValidationViewModel?.Close();
		IssueViewModel?.Close();
	}
}

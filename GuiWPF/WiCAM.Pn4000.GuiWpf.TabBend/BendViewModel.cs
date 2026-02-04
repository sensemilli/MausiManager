using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.GuiWpf.GeneralSubWindow;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList;
using WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList.BendContext;
using WiCAM.Pn4000.GuiWpf.TabBend.Developer;
using WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;
using WiCAM.Pn4000.GuiWpf.TabBend.EditLcb;
using WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools;
using WiCAM.Pn4000.GuiWpf.TabBend.OpacityControl;
using WiCAM.Pn4000.GuiWpf.TabBend.OrderBillboards;
using WiCAM.Pn4000.GuiWpf.TabBend.PlayScreen;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

internal class BendViewModel : ViewModelBase, IBendViewModel, ITab
{
	private enum ViewModifications
	{
		SetView,
		ZoomExtend,
		RestoreView,
		NoModification
	}

	private bool _bendSequenceExpanded = true;

	private bool _subViewExpanded = true;

	private readonly IEditToolsViewModel _editToolsViewModel;

	private readonly IPN3DBendPipe _bendPipe;

	private readonly IMachineBasePainter _machineBasePainter;

	private bool _sequenceAlternative;

	private bool _isEnabled = true;

	private bool _isIni;

	private CameraState? _lastRenderCameraState;

	private bool _isDisposed;

	private Visibility _visible = Visibility.Collapsed;

	private Visibility _submenuVisibility;

	private Action? _actionRefreshProps;

	private readonly OpacityControlView _opacityControlView;

	private readonly IProfileOpacitySelector _profileOpacitySelector;

	private readonly OrderBillboardsView _orderBillboardsView;

	private readonly IOrderBillboardsViewModel _orderBillboardsViewModel;

	private ISubViewModel? _activeSubViewModel;

	private readonly Pn3DKernel _pN3DKernel;

	private readonly IGlobals _globals;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IDoc3d _currentDoc;

	private readonly IMainWindowBlock _windowBlock;

	private readonly IScopedFactorio _factorio;

	private readonly ITranslator _translator;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IStatus3dDefault _status3dDefault;

	private ICombinedBendDescriptorInternal _selectedCommonBendFace;

	private ICombinedBendDescriptorInternal _hoveredCommonBendFace;

	private Triangle? _hoveredTri;

	private bool _isActivating;

	private IBillboard? _hoveredBillboard;

	private readonly IPaintTool _painter;

	private readonly ICurrentCalculation _currentCalculation;

	private readonly IRibbon _ribbon;

	private readonly IEditBendPositionBillboards _editBendPositionBillboards;

	private readonly IEditToolsVisualizer _editToolsVisualizer;

	private readonly IBendSelection _bendSelection;

	private readonly IDockingService _dockingService;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly ShortcutTabBend _shortcutTabBend;

	public Visibility BendSequenceVisibility { get; set; }

	public bool BendSequenceExpanded
	{
		get
		{
			return _bendSequenceExpanded;
		}
		set
		{
			if (_bendSequenceExpanded != value)
			{
				_bendSequenceExpanded = value;
				NotifyPropertyChanged("BendSequenceExpanded");
			}
		}
	}

	public bool SubViewExpanded
	{
		get
		{
			return _subViewExpanded;
		}
		set
		{
			if (_subViewExpanded != value)
			{
				_subViewExpanded = value;
				NotifyPropertyChanged("SubViewExpanded");
			}
		}
	}

	public Visibility SubViewVisibility
	{
		get
		{
			if (ActiveSubView == null)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public IIssueViewModel IssueViewModel { get; }

	public IBendSequenceListViewModel BendSequenceListViewModel { get; }

	public bool IsActive { get; private set; }

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				NotifyPropertyChanged("IsEnabled");
			}
		}
	}

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

	public Visibility SubmenuVisibility
	{
		get
		{
			return _submenuVisibility;
		}
		set
		{
			if (_submenuVisibility != value)
			{
				_submenuVisibility = value;
				NotifyPropertyChanged("SubmenuVisibility");
			}
		}
	}

	public PlayScreenViewModel PlayScreenViewModel { get; set; }

	public OpacityControlView OpacityControlView => _opacityControlView;

	public OrderBillboardsView OrderBillboardView => _orderBillboardsView;

	public FrameworkElement? ActiveSubView { get; private set; }

	public ISubViewModel? ActiveSubViewModel
	{
		get
		{
			return _activeSubViewModel;
		}
		set
		{
			if (_activeSubViewModel != value)
			{
				if (_activeSubViewModel != null)
				{
					ISubViewModel? activeSubViewModel = _activeSubViewModel;
					_activeSubViewModel.Close();
					activeSubViewModel.RequestRepaint -= RepaintModelAndHighlightInListView;
				}
				_activeSubViewModel = value;
				if (_activeSubViewModel != null)
				{
					_activeSubViewModel.RequestRepaint += RepaintModelAndHighlightInListView;
					_activeSubViewModel.Closed += SubViewModelClosed;
					_activeSubViewModel.SetActive(IsActive);
				}
				NotifyPropertyChanged("ActiveSubViewModel");
				NotifyPropertyChanged("ActiveSubView");
				NotifyPropertyChanged("SubViewVisibility");
			}
		}
	}

	public RelayCommand CmdEditBendSequence { get; set; }

	public RelayCommand CmdEditBendSequenceNew { get; set; }

	public RelayCommand CmdEditTools3D { get; set; }

	public Model CurrentRenderModel { get; set; }

	public bool SequenceAlternative
	{
		get
		{
			return _sequenceAlternative;
		}
		set
		{
			_sequenceAlternative = value;
			NotifyPropertyChanged("SequenceAlternative");
			NotifyPropertyChanged("SequenceVisibilityAlternative1");
			NotifyPropertyChanged("SequenceVisibilityAlternative2");
		}
	}

	public bool FreezeBendSequence
	{
		get
		{
			return _currentDoc.FreezeCombinedBendDescriptors;
		}
		set
		{
			if (_currentDoc.FreezeCombinedBendDescriptors != value)
			{
				_currentDoc.FreezeCombinedBendDescriptors = value;
			}
		}
	}

	public string FreezeBendSequenceText { get; set; } = "\ue114";

	public Visibility SequenceVisibilityAlternative1
	{
		get
		{
			if (!_sequenceAlternative)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility SequenceVisibilityAlternative2
	{
		get
		{
			if (!_sequenceAlternative)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	private Screen3D Screen3d => _screen3DMain.Screen3D;

	private void UpdateFreezeBendSequence()
	{
		FreezeBendSequenceText = (FreezeBendSequence ? "\ue113" : "\ue114");
		NotifyPropertyChanged("FreezeBendSequence");
		NotifyPropertyChanged("FreezeBendSequenceText");
	}

	public BendViewModel(Pn3DKernel pN3DKernel, IMainWindowBlock windowBlock, IScopedFactorio factorio, ITranslator translator, IStatus3dDefault status3dDefault, IProfileOpacitySelector profileOpacitySelector, OpacityControlViewModel opacityControlViewModel, OpacityControlView opacityControlView, IOrderBillboardsViewModel orderBillboardsViewModel, OrderBillboardsView orderBillboardsView, IScreen3DMain screen3DMain, IConfigProvider configProvider, ToolCalculationMode toolCalculationMode, IShortcutSettingsCommon shortcutSettingsCommon, ShortcutTabBend shortcutTabBend, IDoc3d currentDoc, IEditBendPositionBillboards editBendPositionBillboards, IEditToolsVisualizer editToolsVisualizer, IIssueViewModel vmIssue, IBendSelection bendSelection, IToolsToMachineModel toolsToMachineModel, IBendSequenceListViewModel bendSequenceListViewModel, IMainWindowDataProvider mainWindowDataProvider, IDockingService dockingService, IEditToolsViewModel editToolsViewModel, IPN3DBendPipe bendPipe, IMachineBasePainter machineBasePainter, IPaintTool painter, ICurrentCalculation currentCalculation, IRibbon ribbon)
	{
		_windowBlock = windowBlock;
		_pN3DKernel = pN3DKernel;
		_globals = pN3DKernel;
		_factorio = factorio;
		_translator = translator;
		_status3dDefault = status3dDefault;
		_ribbon = ribbon;
		CmdEditTools3D = new RelayCommand(EditTools3D);
		CmdEditBendSequenceNew = new RelayCommand(EditBendSequenceNew, CanEditBendSequence);
		_opacityControlView = opacityControlView;
		opacityControlViewModel.Init(profileOpacitySelector.ChangeOpacity, profileLocked: false, profileOpacitySelector);
		_opacityControlView.DataContext = opacityControlViewModel;
		_profileOpacitySelector = profileOpacitySelector;
		profileOpacitySelector.LoadOpacityProfileFromConfig();
		_orderBillboardsViewModel = orderBillboardsViewModel;
		_orderBillboardsView = orderBillboardsView;
		_screen3DMain = screen3DMain;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_shortcutTabBend = shortcutTabBend;
		_orderBillboardsView.DataContext = _orderBillboardsViewModel;
		_currentDoc = currentDoc;
		_editToolsVisualizer = editToolsVisualizer;
		_editToolsVisualizer.RequestRepaint += RepaintModelAndHighlightInListView;
		_editBendPositionBillboards = editBendPositionBillboards;
		_editBendPositionBillboards.RequestRepaint += RepaintModelAndHighlightInListView;
		BendSequenceListViewModel = bendSequenceListViewModel;
		_orderBillboardsViewModel.CurrentDoc = _currentDoc;
		PlayScreenViewModel = new PlayScreenViewModel(SelectedCommonBendFaceChanged, currentDoc, _screen3DMain, configProvider, RepaintModelAndHighlightInListView, IsSubVmActive, toolCalculationMode, bendSelection, toolsToMachineModel, translator);
		IssueViewModel = vmIssue;
		_bendSelection = bendSelection;
		_mainWindowDataProvider = mainWindowDataProvider;
		_dockingService = dockingService;
		_editToolsViewModel = editToolsViewModel;
		_bendPipe = bendPipe;
		_machineBasePainter = machineBasePainter;
		_painter = painter;
		_currentCalculation = currentCalculation;
		BendSequenceListViewModel.OpenBendContextMenu += EditSelectedBendNew;
		BendSequenceListViewModel.RepaintModels += RepaintModelAndHighlightInListView;
		_bendSelection.CurrentBendChanged += delegate
		{
			RepaintModelAndHighlightInListView();
		};
		_bendSelection.CurrentBendHoveringChanged += delegate
		{
			RepaintModelAndHighlightInListView();
		};
		_bendSelection.DataChanged += RepaintModelAndHighlightInListView;
		_bendSelection.SelectionChanged += RepaintModelAndHighlightInListView;
		_currentDoc.BendModel3DChanged += BendModelChanged;
		_currentDoc.BendSimulationChanged += BendSimulationChanged;
		_currentDoc.BendMachineChanged += BendMachineChanged;
		_currentDoc.CombinedBendDescriptorsChanged += CombinedBendsChanged;
		_bendSelection.CurrentBendChanged += OnBendSelection_CurrentBendChanged;
		_bendSelection.CurrentBendHoveringChanged += OnBendSelection_CurrentBendHoveringChanged;
		_currentDoc.FreezeCombinedBendDescriptorsChanged += _currentDoc_FreezeCombinedBendDescriptorsChanged;
		OnActivated(ViewModifications.ZoomExtend);
		CombinedBendsChanged();
		UpdateFreezeBendSequence();
	}

	private void _currentDoc_FreezeCombinedBendDescriptorsChanged(IDoc3d obj)
	{
		UpdateFreezeBendSequence();
	}

	private void OnBendSelection_CurrentBendHoveringChanged(IBendPositioning? obj)
	{
		HoveredCommonBendFaceChanged(obj?.Bend?.CombinedBendDescriptor as ICombinedBendDescriptorInternal);
	}

	private void OnBendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
		SelectedCommonBendFaceChanged(obj?.Bend?.CombinedBendDescriptor as ICombinedBendDescriptorInternal);
	}

	private void ProfileOpacitySelector_OpacityProfileChanged(int arg1, int arg2, Dictionary<PartRole, double> arg3)
	{
		RepaintModelAndHighlightInListView();
	}

	private void BlockBendViewModelChanged()
	{
		if (IsEnabled == _windowBlock.BlockUI_IsBlock(_currentDoc))
		{
			IsEnabled = !IsEnabled;
		}
	}

	private void BendSimulationChanged(ISimulationThread oldSim, ISimulationThread newSim)
	{
		RepaintModelAndHighlightInListView();
	}

	private void BendModelChanged(Model oldModel, Model newModel)
	{
		if (IsActive)
		{
			OnActivated(ViewModifications.NoModification);
		}
	}

	private void CombinedBendsChanged()
	{
		Application.Current.Dispatcher.Invoke(CombinedBendsChangedUiThread);
	}

	private void CombinedBendsChangedUiThread()
	{
		if (_currentDoc.ToolsAndBends != null)
		{
			_factorio.Resolve<IEditToolsSelection>().SetData(_currentDoc, _currentDoc.ToolsAndBends, _currentDoc.ToolsAndBends.BendPositions.FirstOrDefault()?.Anchor?.Root ?? _currentDoc.ToolsAndBends.ToolSetups.FirstOrDefault());
			_factorio.Resolve<IBendSelection>().SetData(_currentDoc, _currentDoc.ToolsAndBends);
		}
		RefreshRibbonButtonStyle();
	}

	private void RefreshRibbonButtonStyle()
	{
		if (!IsActive)
		{
			return;
		}
		_ribbon?.Ribbon_SelectButton(81, "BNDMACHINE", _currentDoc?.HasBendMachine ?? false);
		IRibbon ribbon = _ribbon;
		if (ribbon != null)
		{
			IDoc3d currentDoc = _currentDoc;
			ribbon.Ribbon_SelectButton(81, "BNDTOOLS", currentDoc != null && currentDoc.HasToolSetups && _currentDoc.ToolsAndBends.BendPositions.All((IBendPositioning x) => x.Anchor != null));
		}
		_ribbon?.Ribbon_SelectButton(81, "BNDFINGERS", _currentDoc?.HasAllFingers ?? false);
		_ribbon?.Ribbon_SelectButton(81, "BNDVALIDATESIM", _currentDoc?.SimulationValidated ?? false);
		_ribbon?.Ribbon_SelectButton(81, "BNDPP", _currentDoc?.NcFileGenerated ?? false);
		_ribbon?.Ribbon_SelectButton(81, "BNDREPORT", _currentDoc?.ReportGenerated ?? false);
		_ribbon?.Ribbon_SelectButton(81, "BNDPPSEND", _currentDoc?.NcDataSend ?? false);
		_ribbon?.Ribbon_SelectButton(81, "BNDPPNO", !string.IsNullOrEmpty(_currentDoc?.NamePPBase));
	}

	private void PipeCommandCalledBegin(IPnCommand obj)
	{
		ActiveSubViewModel?.Close();
	}

	private void PipeCommandCalledEnded(IPnCommand obj)
	{
		RefreshRibbonButtonStyle();
	}

	private void SelectedCommonBendFaceChanged(ICombinedBendDescriptorInternal newCbf)
	{
		if (_selectedCommonBendFace != newCbf)
		{
			ActiveSubViewModel?.Close();
			_selectedCommonBendFace = newCbf;
			_orderBillboardsViewModel.SetSelectedAndHoveredCombinedBend(_selectedCommonBendFace, _hoveredCommonBendFace);
			PlayScreenViewModel.SelectedCommonBendFace = newCbf;
			_bendSelection?.SetCurrentBendBySimulation(newCbf?.Order);
			RepaintModelAndHighlightInListView();
		}
	}

	private void HoveredCommonBendFaceChanged(ICombinedBendDescriptorInternal newCbf)
	{
		if (_hoveredCommonBendFace != newCbf)
		{
			_hoveredCommonBendFace = newCbf;
			_orderBillboardsViewModel.SetSelectedAndHoveredCombinedBend(_selectedCommonBendFace, _hoveredCommonBendFace);
			_bendSelection?.SetCurrentBendHoveredByOrder(newCbf?.Order);
			RepaintModelAndHighlightInListView();
		}
	}

	public void SetActive(bool active)
	{
		if (active == IsActive)
		{
			return;
		}
		IsActive = active;
		if (IsActive)
		{
			if (_isDisposed)
			{
				throw new Exception("Try to activate disposed tab");
			}
			_windowBlock.BlockChanged += BlockBendViewModelChanged;
			_pN3DKernel.Pn3DRootPipe.OnCommandCalledBegin += PipeCommandCalledBegin;
			_pN3DKernel.Pn3DRootPipe.OnCommandCalledEnded += PipeCommandCalledEnded;
			_profileOpacitySelector.OpacityProfileChanged += ProfileOpacitySelector_OpacityProfileChanged;
			Screen3d.ScreenD3D.Renderer.LayerBlacklist.Clear();
			Screen3d.ScreenD3D.Renderer.LayerBlacklist.TryAdd(AuxiliaryShellType.PurchasedPartIgnoreCollision, value: true);
			Visible = Visibility.Visible;
			Screen3d.TriangleSelected += MouseSelectTriangle;
			Screen3d.MouseMove += MouseMove;
			Screen3d.MouseEnterTriangle += MouseEnterTriangle;
			Screen3d.MouseEnterBillboard += MouseEnterBillboard;
			Screen3d.KeyUp += Screen3dOnKeyUp;
			Screen3D screen3d = Screen3d;
			screen3d.ExternalKeyDown = (KeyEventHandler)Delegate.Combine(screen3d.ExternalKeyDown, new KeyEventHandler(Screen3dOnKeyUp));
			Screen3d.ScreenD3D.Renderer.RenderData.AutoAdjustFloorToScreenRotation = false;
			if (_isIni)
			{
				OnActivated(ViewModifications.RestoreView);
			}
			else
			{
				OnActivated(ViewModifications.SetView);
				_isIni = true;
			}
			BlockBendViewModelChanged();
			RepaintModelAndHighlightInListView();
			_status3dDefault.ShowDefaultStatusBars();
			Screen3d.ScreenD3D.UpdateAllModelAppearance(render: true);
			_dockingService.ShowIfExists(ActiveSubViewModel);
			_actionRefreshProps?.Invoke();
			_actionRefreshProps = null;
		}
		else
		{
			_pN3DKernel.Pn3DRootPipe.OnCommandCalledBegin -= PipeCommandCalledBegin;
			_pN3DKernel.Pn3DRootPipe.OnCommandCalledEnded -= PipeCommandCalledEnded;
			_profileOpacitySelector.OpacityProfileChanged -= ProfileOpacitySelector_OpacityProfileChanged;
			_windowBlock.BlockChanged -= BlockBendViewModelChanged;
			_lastRenderCameraState = Screen3d.ScreenD3D.Renderer.ExportCameraState();
			Visible = Visibility.Collapsed;
			Screen3d.TriangleSelected -= MouseSelectTriangle;
			Screen3d.MouseMove -= MouseMove;
			Screen3d.MouseEnterTriangle -= MouseEnterTriangle;
			Screen3d.MouseEnterBillboard -= MouseEnterBillboard;
			Screen3d.KeyUp -= Screen3dOnKeyUp;
			Screen3D screen3d2 = Screen3d;
			screen3d2.ExternalKeyDown = (KeyEventHandler)Delegate.Remove(screen3d2.ExternalKeyDown, new KeyEventHandler(Screen3dOnKeyUp));
			Screen3d.ScreenD3D.UpdateAllModelAppearance(render: true);
			ActiveSubViewModel?.Close();
		}
		if (ActiveSubViewModel != null)
		{
			ActiveSubViewModel.SetActive(active);
		}
		if (ActiveSubViewModel != _editToolsVisualizer)
		{
			_editToolsVisualizer?.SetActive(active);
		}
	}

	private void OnActivated(ViewModifications viewModification)
	{
		if (_currentDoc == null)
		{
			return;
		}
		if (viewModification == ViewModifications.RestoreView && !_lastRenderCameraState.HasValue)
		{
			viewModification = ViewModifications.SetView;
		}
		switch (viewModification)
		{
		case ViewModifications.SetView:
		{
			Matrix4d viewDirection = Matrix4d.RotationZ(2.356194496154785);
			viewDirection *= Matrix4d.RotationX(0.39269909262657166);
			Screen3d.ScreenD3D.SetViewDirectionByMatrix4d(viewDirection, render: false, delegate
			{
				ZoomExtend();
			});
			break;
		}
		case ViewModifications.ZoomExtend:
			ZoomExtend();
			break;
		case ViewModifications.RestoreView:
			Screen3d.ScreenD3D.Renderer.ImportCameraState(_lastRenderCameraState.Value);
			break;
		}
		UpdateRibbonToolCalcRecently();
		_currentDoc.RecalculateSimulation();
	}

	private void ZoomExtend()
	{
		Screen3d.ScreenD3D.ZoomExtend();
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			Screen3d.ScreenD3D.ZoomExtend();
		});
	}

	private void UpdateRibbonToolCalcRecently()
	{
		List<RecentlyUsedRecord> list = new List<RecentlyUsedRecord>();
		List<IToolCalculationOptionOverwrite> list2 = _currentDoc.BendMachine?.ToolCalculationSettings.Options;
		if (list2 != null)
		{
			list.Add(new RecentlyUsedRecord
			{
				FileName = _translator.Translate("ToolCalcSettings.ByUser"),
				FullPath = _translator.Translate("ToolCalcSettings.ByUserTt"),
				ArchiveID = -2,
				Type = "ToolCalc"
			});
			foreach (IToolCalculationOptionOverwrite item in list2)
			{
				list.Add(new RecentlyUsedRecord
				{
					FileName = item.Description,
					FullPath = _translator.Translate("ToolCalcSettings.UseProfileX"),
					ArchiveID = list2.IndexOf(item),
					Type = "ToolCalc"
				});
			}
		}
		_mainWindowDataProvider.SetAllRecentlyUsedRecordForType("ToolCalc", list);
	}

	public void RefreshScreen()
	{
		OnActivated(ViewModifications.NoModification);
		RefreshRibbonButtonStyle();
		RepaintModelAndHighlightInListView();
		_currentDoc.RecalculateSimulation();
	}

	private void Screen3dOnKeyUp(object sender, KeyEventArgs e)
	{
		PnInputEventArgs pnInputEventArgs = new PnInputEventArgs(e, keyUp: true);
		InputEvent(sender, pnInputEventArgs);
		if (!pnInputEventArgs.Handled && e.Key == Key.F4 && pnInputEventArgs.ShiftModifier)
		{
			_factorio.Resolve<IWinDebug>().Init();
			e.Handled = true;
		}
		e.Handled = false;
	}

	private void InputEvent(object sender, IPnInputEventArgs arg)
	{
		ActiveSubViewModel?.KeyUp(sender, arg);
		if (!arg.Handled)
		{
			if (_shortcutSettingsCommon.Cancel.IsShortcut(arg))
			{
				ICalculationArg? currentCalculationOption = _currentCalculation.CurrentCalculationOption;
				if (currentCalculationOption != null && !currentCalculationOption.CancellationToken.IsCancellationRequested)
				{
					ICalculationArg? currentCalculationOption2 = _currentCalculation.CurrentCalculationOption;
					if (currentCalculationOption2 != null && currentCalculationOption2.TryCancelCalculation())
					{
						arg.Handle();
						goto IL_0091;
					}
				}
				if (ActiveSubViewModel != null)
				{
					ActiveSubViewModel.Close();
					arg.Handle();
				}
			}
			goto IL_0091;
		}
		goto IL_00bb;
		IL_00bb:
		if (arg.Handled)
		{
			return;
		}
		if (_shortcutTabBend.ToggleExpandSequence.IsShortcut(arg))
		{
			if (!BendSequenceExpanded)
			{
				BendSequenceExpanded = true;
			}
			else if (!SequenceAlternative)
			{
				SequenceAlternative = true;
			}
			else
			{
				SequenceAlternative = false;
				BendSequenceExpanded = false;
			}
			arg.Handle();
		}
		else if (_shortcutTabBend.ToggleExpandSubView.IsShortcut(arg))
		{
			SubViewExpanded = !SubViewExpanded;
			arg.Handle();
		}
		return;
		IL_0091:
		if (!arg.Handled)
		{
			_editToolsViewModel.KeyUp(sender, arg);
		}
		if (!arg.Handled)
		{
			Screen3d.NavigationKeyDown(sender, arg);
		}
		goto IL_00bb;
	}

	private void MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		IPnInputEventArgs args = e.Args;
		if (args == null || !args.Handled)
		{
			if (e.Tri?.Face == _hoveredTri?.Face)
			{
				_hoveredTri = e.Tri;
				return;
			}
			_hoveredTri = e.Tri;
			UpdateHoveredBend();
		}
	}

	private void MouseSelectTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		if (e.Args.Handled)
		{
			return;
		}
		if (ActiveSubViewModel != null)
		{
			ActiveSubViewModel.MouseSelectTriangle(this, e);
			if (e.Args.Handled)
			{
				return;
			}
		}
		InputEvent(sender, e.Args);
		if (e.Args.Handled)
		{
			return;
		}
		if (ActiveSubViewModel != null)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = e.Args.MouseButtonEventArgs;
			if (mouseButtonEventArgs == null || mouseButtonEventArgs.ChangedButton != 0)
			{
				MouseButtonEventArgs? mouseButtonEventArgs2 = e.Args.MouseButtonEventArgs;
				if (mouseButtonEventArgs2 == null || mouseButtonEventArgs2.ChangedButton != MouseButton.Right)
				{
					return;
				}
			}
			ActiveSubViewModel.Close();
			e.Args.Handle();
		}
		else
		{
			Point position = e.MouseEventArgs.GetPosition(Screen3d);
			SelectObject(sender, e, e.Billboard, e.Tri, e.Model, position.X, position.Y, e.HitPoint, e.MouseEventArgs);
		}
	}

	private void MouseEnterBillboard(IScreen3D sender, IBillboardEventArgs e)
	{
		IPnInputEventArgs args = e.Args;
		if ((args == null || !args.Handled) && e.Billboard != _hoveredBillboard)
		{
			_hoveredBillboard = e.Billboard;
			UpdateHoveredBend();
		}
	}

	private void UpdateHoveredBend()
	{
		IBendPositioning bendPositioning = null;
		if (_hoveredBillboard != null)
		{
			IBendDescriptor bendDescriptor = _orderBillboardsViewModel.GetBend(_hoveredBillboard);
			bendPositioning = BendSequenceListViewModel.Bends.FirstOrDefault((BendSequenceListViewModel.BendViewModel x) => x.Cbd.Enumerable.Contains(bendDescriptor))?.Bend;
		}
		FaceGroup root = _hoveredTri?.Face.FaceGroup?.GetParentRoot();
		FaceGroup faceGroup = root;
		if (faceGroup != null && faceGroup.IsBendingZone && bendPositioning == null)
		{
			bendPositioning = BendSequenceListViewModel.Bends.FirstOrDefault((BendSequenceListViewModel.BendViewModel x) => x.Cbd.Enumerable.Any((IBendDescriptor y) => y.BendParams.FaceGroupId == root.ID))?.Bend;
		}
		_bendSelection.CurrentBendHovering = bendPositioning;
	}

	private void MouseMove(object sender, MouseEventArgs e)
	{
		ActiveSubViewModel?.MouseMove(sender, e);
	}

	private ICombinedBendDescriptorInternal? GetCombinedBend(int fgId)
	{
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _currentDoc.CombinedBendDescriptors)
		{
			foreach (IBendDescriptor item in combinedBendDescriptor.Enumerable)
			{
				if (item.BendParams.EntryFaceGroup.ID == fgId)
				{
					return combinedBendDescriptor;
				}
			}
		}
		return null;
	}

	public void SelectObject(IScreen3D sender, ITriangleEventArgs e, IBillboard? billboard, Triangle? tri, Model? modelTri, double x, double y, Vector3d hitPoint, MouseButtonEventArgs mouseButtonEventArgs)
	{
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = null;
		if (billboard != null)
		{
			combinedBendDescriptorInternal = _orderBillboardsViewModel.GetCombinedBend(billboard);
		}
		Model? model = e.Model;
		if (model != null && model.GetRootParent().PartRole == PartRole.BendModel && e.Tri.Face.FaceGroup.IsBendingZone)
		{
			int iD = e.Tri.Face.FaceGroup.ID;
			if (combinedBendDescriptorInternal == null)
			{
				combinedBendDescriptorInternal = GetCombinedBend(iD);
			}
		}
		if (combinedBendDescriptorInternal != null)
		{
			if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				SelectedCommonBendFaceChanged(combinedBendDescriptorInternal);
			}
			else
			{
				if (combinedBendDescriptorInternal == null || mouseButtonEventArgs.ChangedButton != MouseButton.Right)
				{
					return;
				}
				IBendPositioning bendPositioning = _bendSelection.GetBendPositioning(combinedBendDescriptorInternal);
				if (bendPositioning != null)
				{
					if (!e.Args.ShiftModifier && !e.Args.ControlModifier)
					{
						_bendSelection.UnselectAll();
					}
					_bendSelection.SetSelection(bendPositioning, isSelected: true);
				}
				EditSelectedBendNew();
			}
		}
		else
		{
			if (billboard != null)
			{
				return;
			}
			for (Model model2 = modelTri; model2 != null; model2 = model2.Parent)
			{
				if (model2.PartRole == PartRole.LeftFinger || model2.PartRole == PartRole.RightFinger)
				{
					StartEditFinger(sender, e);
					break;
				}
				if (model2.PartRole == PartRole.Lower_Tool || model2.PartRole == PartRole.Upper_Tool || model2.PartRole == PartRole.Lower_Tools || model2.PartRole == PartRole.Upper_Tools)
				{
					StartEditTools(sender, e);
				}
				if (model2.PartRole == PartRole.BendModel)
				{
					StartEditBendPosition(sender, e);
				}
				if (model2.PartRole == PartRole.LeftBackLiftingAid || model2.PartRole == PartRole.LeftFrontLiftingAid || model2.PartRole == PartRole.RightFrontLiftingAid || model2.PartRole == PartRole.RightBackLiftingAid)
				{
					StartEditLiftingAid(sender, e, model2);
				}
				if (model2.PartRole == PartRole.AngleMeasurement)
				{
					StartEditLcb(sender, e, model2);
				}
			}
		}
	}

	private bool IsSubVmActive()
	{
		return ActiveSubViewModel != null;
	}

	private void SubViewModelClosed(ISubViewModel sender, Triangle tri, Model model, double x, double y, Vector3d hitPoint, MouseButtonEventArgs mouseButtonEventArgs)
	{
		Screen3d.InteractionMode = new NavigateInteractionMode(Screen3d);
		if (sender != null)
		{
			sender.Closed -= SubViewModelClosed;
			sender.RequestRepaint -= RepaintModelAndHighlightInListView;
			if (sender == _activeSubViewModel)
			{
				_activeSubViewModel = null;
				ActiveSubView = null;
				PlayScreenViewModel.Active = _activeSubViewModel == null;
				NotifyPropertyChanged("ActiveSubViewModel");
				NotifyPropertyChanged("ActiveSubView");
				NotifyPropertyChanged("SubViewVisibility");
			}
		}
		_ = _selectedCommonBendFace;
		RepaintModelAndHighlightInListView();
	}

	private void RepaintModelAndHighlightInListView()
	{
		Application.Current.Dispatcher.Invoke(RepaintModelAndHighlightInListViewMainThread);
	}

	private void RepaintModelAndHighlightInListViewMainThread()
	{
		if (IsActive)
		{
			_painter.FrameStart();
			_profileOpacitySelector?.ColorModelParts(_painter);
			_machineBasePainter.PaintMachineBasic(_painter);
			_editToolsVisualizer?.ColorModelParts(_painter);
			BendSequenceListViewModel.ColorModelParts(_painter);
			PlayScreenViewModel?.ColorModelParts(_painter);
			ActiveSubViewModel?.ColorModelParts(_painter);
			_painter.FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);
			_orderBillboardsViewModel.UpdateOrAddBillboards(render: false);
			Screen3d.ScreenD3D.UpdateModelAppearance(ref modifiedModels, ref modifiedShells);
		}
	}

	private void SetActiveSubViewModel<T>(T vm) where T : class, ISubViewModel, IPopupViewModel
	{
		ActiveSubView = null;
		ActiveSubViewModel = vm;
		_dockingService.Show(vm, _factorio);
		RepaintModelAndHighlightInListView();
	}

	private void OpenContextMenuAtCenter(ICombinedBendDescriptorInternal cbf)
	{
		EditSelectedBendNew();
	}

	private void EditSelectedBendNew()
	{
		BendContextViewModel bendContextViewModel = _factorio.Resolve<BendContextViewModel>();
		bendContextViewModel.RefreshBendList();
		SetActiveSubViewModel(bendContextViewModel);
	}

	private bool CanEditBendSequence(object obj)
	{
		if (_currentDoc?.UnfoldModel3D == null)
		{
			return false;
		}
		IDoc3d currentDoc = _currentDoc;
		if (currentDoc != null && currentDoc.CombinedBendDescriptors?.Count < 2)
		{
			return false;
		}
		return true;
	}

	private void EditBendSequenceNew(object obj)
	{
		if (!_windowBlock.BlockUI_IsBlock(_currentDoc))
		{
			EditOrderView editOrderView = _factorio.Resolve<EditOrderView>();
			editOrderView.Init(_factorio.Resolve<IEditOrderViewModel>(), delegate
			{
				_actionRefreshProps = BendSequenceListViewModel.RefreshAllProps;
			});
			_mainWindowDataProvider.SetViewForConfig(editOrderView);
		}
	}

	public void EditTools3D()
	{
		_editToolsViewModel.Activate();
		ActiveSubView = null;
		ActiveSubViewModel = _editToolsVisualizer;
		_dockingService.Show(_editToolsViewModel, _factorio);
		RepaintModelAndHighlightInListView();
	}

	public void ToolCalcEditProfile()
	{
		IToolCalculationOptionViewModel toolCalculationOptionViewModel = _factorio.Resolve<IToolCalculationOptionViewModel>();
		List<IToolCalculationOptionOverwrite> obj = _currentDoc.BendMachine?.ToolCalculationSettings.Options;
		if (obj != null && obj.Count > 0)
		{
			toolCalculationOptionViewModel.ImportOptions();
			SetActiveSubViewModel(toolCalculationOptionViewModel);
		}
	}

	private void StartEditBendPosition(IScreen3D sender, ITriangleEventArgs e)
	{
		if (_editBendPositionBillboards.StartSubMenu(sender, e))
		{
			_editToolsViewModel.Activate();
			ActiveSubViewModel = _editBendPositionBillboards;
			RepaintModelAndHighlightInListView();
		}
	}

	private void StartEditTools(IScreen3D sender, ITriangleEventArgs e)
	{
		if (_editToolsVisualizer.StartSubMenu(sender, e))
		{
			EditTools3D();
		}
	}

	private void StartEditLiftingAid(IScreen3D sender, ITriangleEventArgs e, Model model)
	{
		IEditLiftingAidsViewModel editLiftingAidsViewModel = _factorio.Resolve<IEditLiftingAidsViewModel>();
		if (editLiftingAidsViewModel.StartSubMenu(sender, e))
		{
			editLiftingAidsViewModel.SelectModel(model);
			SetActiveSubViewModel(editLiftingAidsViewModel);
		}
	}

	private void StartEditLcb(IScreen3D sender, ITriangleEventArgs e, Model model)
	{
		IEditLcbViewModel editLcbViewModel = _factorio.Resolve<IEditLcbViewModel>();
		if (editLcbViewModel.StartSubMenu(sender, e))
		{
			editLcbViewModel.SelectModel(model);
			SetActiveSubViewModel(editLcbViewModel);
		}
	}

	private void StartEditFinger(IScreen3D sender, ITriangleEventArgs e)
	{
		IEditFingersVisualizer editFingersVisualizer = _factorio.Resolve<IEditFingersVisualizer>();
		if (editFingersVisualizer.StartSubMenu(sender, e))
		{
			ActiveSubView = null;
			ActiveSubViewModel = editFingersVisualizer;
			_dockingService.Show(editFingersVisualizer.EditFingersViewModel);
			RepaintModelAndHighlightInListView();
		}
	}

	public void Dispose()
	{
		_currentDoc.BendModel3DChanged -= BendModelChanged;
		_currentDoc.BendSimulationChanged -= BendSimulationChanged;
		_currentDoc.BendMachineChanged -= BendMachineChanged;
		_currentDoc.CombinedBendDescriptorsChanged -= CombinedBendsChanged;
		SetActive(active: false);
		if (_editToolsVisualizer != null)
		{
			_editToolsVisualizer.RequestRepaint -= RepaintModelAndHighlightInListView;
		}
		PlayScreenViewModel?.Dispose();
		IssueViewModel?.Close();
		_editToolsViewModel?.Dispose();
		BendSequenceListViewModel?.Dispose();
		_isDisposed = true;
	}

	private void BendMachineChanged()
	{
		_lastRenderCameraState = null;
		if (IsActive)
		{
			OnActivated(ViewModifications.RestoreView);
			_currentDoc.RecalculateSimulation();
		}
	}
}

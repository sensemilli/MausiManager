using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;
using WiCAM.Pn4000.GuiWpf.F7;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.TabBend;
using WiCAM.Pn4000.GuiWpf.TabUnfold;
using WiCAM.Pn4000.GuiWpf.TabViewer3D;
using WiCAM.Pn4000.GuiWpf.Ui3D.DocManagement;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;

namespace WiCAM.Pn4000.GuiWpf;

public class MainWindowViewModel : INotifyPropertyChanged
{
	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IFactorio _factorio;

	private readonly IDockingService _dockingService;

	private IMainWindowDataProvider _mainWindowDataProvider;

	private Pn3DKernel _pn3DKernel;

	private ITab _activeTab;

	private int? _lastTabId;

	private bool _activeTabHidden;

	private IUnfoldViewModel _unfoldViewModel;
	public IUnfoldViewModel UnfoldViewModel 
	{
		get => _unfoldViewModel;
		set
		{
			if (_unfoldViewModel != value)
			{
				_unfoldViewModel = value;
				OnPropertyChanged(nameof(UnfoldViewModel));
			}
		}
	}

	public IBendViewModel BendViewModel;

	public IViewer3dViewModel Viewer3dViewModel;

	private IPnStatusBarHelper _pnStatusBar;

	private bool _isTab3D;

	private IApplicationHeaderView _applicationHeaderControl;

	private IGlobals Globals => _pn3DKernel;

	public Window MainWindow => _pn3DKernel.MainWindow;

	private IPN3DBendPipe BendPipe => _pn3DKernel.Pn3DRootPipe.PN3DBendPipe;

	private IDoc3d CurrentDoc => _currentDocProvider.CurrentDoc;

	public bool IsHelpMode { get; set; }

	public Control RibbonControl { get; set; }

	public IApplicationHeaderView ApplicationHeaderControl
	{
		get
		{
			return _applicationHeaderControl;
		}
		set
		{
			if (!object.Equals(value, _applicationHeaderControl))
			{
				_applicationHeaderControl = value;
				OnPropertyChanged("ApplicationHeaderControl");
			}
		}
	}

	public IPnStatusBarHelper PnStatusBar
	{
		get
		{
			return _pnStatusBar;
		}
		set
		{
			_pnStatusBar = value;
			OnPropertyChanged("PnStatusBar");
		}
	}

	public IDocumentManagerViewModel DocumentManager { get; }

	public bool IsTab3D
	{
		get
		{
			return _isTab3D;
		}
		set
		{
			if (_isTab3D != value)
			{
				_isTab3D = value;
				if (IsHelpMode && !_isTab3D)
				{
					DeactivateHelpMode();
				}
			}
		}
	}

	public WeakReference<UserControl> PopupBackup { get; set; } = new WeakReference<UserControl>(null);

	public event PropertyChangedEventHandler? PropertyChanged;

	public MainWindowViewModel(IFactorio factorio, IPnPathService pnPathService, IPnStatusBarHelper pnStatusBarHelper, ICurrentDocProvider currentDocProvider, IDocumentManagerViewModel documentManager, IDockingService dockingService)
	{
		_currentDocProvider = currentDocProvider;
		_factorio = factorio;
		PnStatusBar = pnStatusBarHelper;
		DocumentManager = documentManager;
		_dockingService = dockingService;
		_currentDocProvider.CurrentDocChanged += OnCurrentDocChanged;
	}

	private void OnCurrentDocChanged(IDoc3d arg1, IDoc3d arg2)
	{
		_dockingService.ChangeScope(_currentDocProvider.CurrentFactorio);
	}

	public void Ini(IMainWindowDataProvider mainWindowDataProvider, Pn3DKernel pn3DKernel, IApplicationHeaderView applicationHeaderView)
	{
		_mainWindowDataProvider = mainWindowDataProvider;
		_pn3DKernel = pn3DKernel;
		ApplicationHeaderControl = applicationHeaderView;
	}

	private void RibbonTabChange(int ID)
	{
		OnActiveTabChanged(ID);
	}

	public void OnActiveTabChanged(int tabId)
	{
		ITab activeTab = null;
		_lastTabId = tabId;
		switch (tabId)
		{
		case 899:
			activeTab = UnfoldViewModel;
			break;
		case 898:
			activeTab = BendViewModel;
			break;
		case 897:
			activeTab = Viewer3dViewModel;
			break;
		}
		SetActiveTab(activeTab);
	}

	public void HideActiveTab()
	{
		_dockingService.ChangeContext(null);
		_activeTab?.SetActive(active: false);
		_activeTabHidden = true;
	}

	public void ShowActiveTab()
	{
		_activeTabHidden = false;
		_dockingService.ChangeContext(_activeTab);
		_activeTab?.SetActive(active: true);
		if (_activeTab == null && _lastTabId.HasValue)
		{
			OnActiveTabChanged(_lastTabId.Value);
		}
	}

	public void SetActiveTab(ITab tab)
	{
		if (_activeTab != null)
		{
			_activeTab.SetActive(active: false);
		}
		_activeTab = tab;
		_dockingService.ChangeContext(_activeTab);
		if (!_activeTabHidden)
		{
			if (_activeTab == BendViewModel)
			{
				BendPipe.SetBendMachineForVisualization(CurrentDoc, Globals, _mainWindowDataProvider);
			}
			if (_activeTab != null)
			{
				_activeTab.SetActive(active: true);
			}
		}
		if (_activeTab == BendViewModel || _activeTab == UnfoldViewModel)
		{
			PopupBackup.TryGetTarget(out UserControl target);
			if (target is MachineConfigurationView machineConfigurationView)
			{
				machineConfigurationView.SetActive(active: true);
			}
			DocumentManager.Visibility = Visibility.Visible;
		}
		else
		{
			DocumentManager.Visibility = Visibility.Collapsed;
		}
	}

	public ITab GetActiveTab()
	{
		return _activeTab;
	}

	public void CreateSnapshot()
	{
		F7View f7View = _factorio.Resolve<F7View>();
		f7View.Init(CurrentDoc);
		f7View.ShowDialog();
	}

	public void RefreshScreenActiveTab()
	{
		_activeTab?.RefreshScreen();
	}

	public void ActivateHelpMode()
	{
		IsHelpMode = true;
		MainWindow.Cursor = Cursors.Help;
	}

	public void DeactivateHelpMode()
	{
		if (IsHelpMode)
		{
			IsHelpMode = false;
			MainWindow.Cursor = Cursors.Arrow;
		}
	}

	public void ActivateHelpForCommand(string command)
	{
		_mainWindowDataProvider.StartHelp(command);
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SharpDX;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.ManualCameraStateView;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend.PPaperSettings;

public class PpScreenshotSettingsViewModel : WiCAM.Pn4000.Common.ViewModelBase
{
	public class ScreenshotConfigItemVm : WiCAM.Pn4000.Common.ViewModelBase
	{
		private bool _isSelected;

		private readonly PpScreenshotSettingsViewModel _vm;

		public ScreenshotType ScreenshotType { get; init; }

		public string Header { get; init; }

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				_isSelected = value;
				NotifyPropertyChanged("IsSelected");
			}
		}

		public Dictionary<MachineParts, double> Opacities { get; set; }

		public bool AdjustManually { get; set; }

		public Matrix4d ViewRotation { get; set; }

		public ProjectionType ProjectionType { get; set; }

		public ICommand SelectThisTabCommand { get; }

		public IPpScreenshotConfigItem Item { get; set; }

		public ScreenshotConfigItemVm(IPpScreenshotConfigItem item, PpScreenshotSettingsViewModel vm)
		{
			Item = item;
			_vm = vm;
			SelectThisTabCommand = new WiCAM.Pn4000.Common.RelayCommand((Action<object>)delegate
			{
				_vm.SelectedItem = this;
			});
			Reset();
		}

		public void Reset()
		{
			Reset(Item);
		}

		private void Reset(IPpScreenshotConfigItem item)
		{
			AdjustManually = item.AdjustManually;
			ProjectionType = item.ProjectionType;
			Opacities = item.Opacities.ToDictionary((KeyValuePair<MachineParts, double> x) => x.Key, (KeyValuePair<MachineParts, double> x) => x.Value);
			ViewRotation = item.ViewRotation;
			if (IsSelected)
			{
				_vm.ViewChanged(item.ViewRotation);
			}
		}

		public void ResetDefault()
		{
			Reset(_vm.GetDefault(ScreenshotType));
		}

		public void Save()
		{
			Item.AdjustManually = AdjustManually;
			Item.ProjectionType = ProjectionType;
			Item.ViewRotation = ViewRotation;
			Item.Opacities.Clear();
			Item.Opacities.AddRange(Opacities, (KeyValuePair<MachineParts, double> x) => x.Key, (KeyValuePair<MachineParts, double> x) => x.Value);
		}
	}

	public class ScreenshotConfigVm : WiCAM.Pn4000.Common.ViewModelBase
	{
		private string _configDisplayName;

		public string ConfigDisplayName
		{
			get
			{
				return _configDisplayName;
			}
			set
			{
				_configDisplayName = value;
				NotifyPropertyChanged("ConfigDisplayName");
			}
		}

		public List<ScreenshotConfigItemVm> Items { get; set; } = new List<ScreenshotConfigItemVm>();

		public IPpScreenshotConfig? Config { get; set; }

		public ScreenshotConfigVm(IPpScreenshotConfig? config)
		{
			Config = config;
		}

		public override string ToString()
		{
			return ConfigDisplayName;
		}
	}

	private Action<PpScreenshotSettingsViewModel> _closeAction;

	private ScreenshotConfigVm? _selectedConfig;

	private ScreenshotConfigItemVm? _selectedItem;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IPaintTool _paintTool;

	private readonly IMachinePainter _machinePainter;

	private readonly ITranslator _translator;

	private readonly IGlobalToolStorage _globalToolStorage;

	private readonly IGlobalPpScreenshotConfig _globalPpScreenshotConfig;

	private readonly IMachineBendFactory _machineBendFactory;

	private readonly IMessageLogGlobal _messageLog;

	private double _roll;

	private double _pitch;

	private double _yaw;

	private int _newConfigCount = 1;

	private IBendMachine BendMachine => _currentDocProvider.CurrentDoc.BendSimulation.State.MachineConfig;

	public Screen3D Screen3D { get; }

	private ScreenshotConfigVm _dummyConfig { get; }

	public RadObservableCollection<ScreenshotConfigVm> Configs { get; } = new RadObservableCollection<ScreenshotConfigVm>();

	public RadObservableCollection<ScreenshotConfigItemVm> ConfigItems { get; set; } = new RadObservableCollection<ScreenshotConfigItemVm>();

	public ICommand CloseCommand { get; }

	public ICommand SaveCommand { get; }

	public ICommand LoadDefaultCommand { get; }

	public ICommand LoadCurrentCommand { get; }

	public ICommand DeleteCurrentCommand { get; }

	public Model FinishedProductModel { get; set; }

	public Model UnfoldModel { get; set; }

	public Model MachineModel { get; set; }

	public Model SimulationModel { get; set; }

	public Thickness CellMargin => new Thickness(0.0, 0.0, 10.0, 20.0);

	public double Roll
	{
		get
		{
			return _roll;
		}
		set
		{
			_roll = value;
			ManualViewChanged();
		}
	}

	public double Pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = value;
			ManualViewChanged();
		}
	}

	public double Yaw
	{
		get
		{
			return _yaw;
		}
		set
		{
			_yaw = value;
			ManualViewChanged();
		}
	}

	public bool AdjustManually
	{
		get
		{
			return _selectedItem?.AdjustManually ?? false;
		}
		set
		{
			SelectedItem.AdjustManually = value;
			NotifyPropertyChanged("AdjustManually");
		}
	}

	public int PerspectiveSelection
	{
		get
		{
			return ((int?)_selectedItem?.ProjectionType).GetValueOrDefault();
		}
		set
		{
			SelectedItem.ProjectionType = (ProjectionType)value;
			UpdateView();
			NotifyPropertyChanged("PerspectiveSelection");
		}
	}

	public Visibility IsOpacitiesVisible
	{
		get
		{
			if (_selectedItem != null && _selectedItem.ScreenshotType != 0 && _selectedItem.ScreenshotType != ScreenshotType.UnfoldModel)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public double OpacityFrame
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.MainFrame, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.MainFrame, value);
		}
	}

	public double OpacityLowerTools
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.LowerTools, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.LowerTools, value);
		}
	}

	public double OpacityUpperTools
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.UpperTools, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.UpperTools, value);
		}
	}

	public double OpacityFinger
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.Fingers, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.Fingers, value);
		}
	}

	public double OpacityLiftingAid
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.LiftingAids, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.LiftingAids, value);
		}
	}

	public double OpacityAngleMeasurement
	{
		get
		{
			return _selectedItem?.Opacities.GetValueOrDefault(MachineParts.AngleMeasurement, 0.0) ?? 0.0;
		}
		set
		{
			ChangeOpacity(MachineParts.AngleMeasurement, value);
		}
	}

	public string ConfigDisplayName
	{
		get
		{
			return _selectedConfig?.ConfigDisplayName ?? string.Empty;
		}
		set
		{
			_selectedConfig.ConfigDisplayName = value;
		}
	}

	public ScreenshotConfigVm SelectedConfig
	{
		get
		{
			return _selectedConfig;
		}
		set
		{
			if (_selectedConfig != value)
			{
				_selectedConfig = value;
				if (value == _dummyConfig)
				{
					ScreenshotConfigVm screenshotConfigVm = Convert(null);
					Configs.Add(screenshotConfigVm);
					_selectedConfig = screenshotConfigVm;
				}
				ConfigItems.Clear();
				ConfigItems.AddRange(SelectedConfig.Items);
				SelectedItem = ConfigItems.First();
				NotifyPropertyChanged("SelectedConfig");
				NotifyPropertyChanged("ConfigDisplayName");
			}
		}
	}

	public ScreenshotConfigItemVm SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			if (_selectedItem != value)
			{
				if (_selectedItem != null)
				{
					_selectedItem.IsSelected = false;
				}
				_selectedItem = value;
				_selectedItem.IsSelected = true;
				SelectedItemChanged();
				NotifyPropertyChanged("SelectedItem");
				NotifyPropertyChanged("Yaw");
				NotifyPropertyChanged("Pitch");
				NotifyPropertyChanged("Roll");
				NotifyPropertyChanged("AdjustManually");
				NotifyPropertyChanged("PerspectiveSelection");
				NotifyPropertyChanged("OpacityAngleMeasurement");
				NotifyPropertyChanged("OpacityLiftingAid");
				NotifyPropertyChanged("OpacityFinger");
				NotifyPropertyChanged("OpacityUpperTools");
				NotifyPropertyChanged("OpacityLowerTools");
				NotifyPropertyChanged("OpacityFrame");
				NotifyPropertyChanged("IsOpacitiesVisible");
			}
		}
	}

	public PpScreenshotSettingsViewModel(IConfigProvider configProvider, ICurrentDocProvider currentDocProvider, IPaintTool paintTool, IMachinePainter machinePainter, ITranslator translator, IGlobalPpScreenshotConfig globalPpScreenshotConfig, IGlobalToolStorage globalToolStorage, IMachineBendFactory machineBendFactory, IMessageLogGlobal messageLog)
	{
		CloseCommand = new WiCAM.Pn4000.Common.RelayCommand(Close);
		SaveCommand = new WiCAM.Pn4000.Common.RelayCommand(Save);
		LoadDefaultCommand = new WiCAM.Pn4000.Common.RelayCommand(LoadDefault);
		LoadCurrentCommand = new WiCAM.Pn4000.Common.RelayCommand(Load);
		DeleteCurrentCommand = new WiCAM.Pn4000.Common.RelayCommand(DeleteTheCustomization);
		_currentDocProvider = currentDocProvider;
		_paintTool = paintTool;
		_machinePainter = machinePainter;
		_translator = translator;
		_globalPpScreenshotConfig = globalPpScreenshotConfig;
		_globalToolStorage = globalToolStorage;
		_machineBendFactory = machineBendFactory;
		_messageLog = messageLog;
		_dummyConfig = new ScreenshotConfigVm(null)
		{
			ConfigDisplayName = _translator.Translate("PPSettings.New")
		};
		Configs.Add(_dummyConfig);
		foreach (IPpScreenshotConfig allPpCameraConfig in _globalToolStorage.GetAllPpCameraConfigs())
		{
			ScreenshotConfigVm item = Convert(allPpCameraConfig);
			Configs.Add(item);
		}
		Screen3D = new Screen3D();
		Screen3D.ShowNavigation(show: false);
		Screen3D.SetConfigProviderAndApplySettings(configProvider);
		Screen3D.SetDrag3DMode(Drag3DMode.None);
		Screen3D.SetZoomMode(ZoomMode.None);
		Screen3D.Loaded += delegate
		{
			Screen3DLoaded();
		};
	}

	public void Init(Action<PpScreenshotSettingsViewModel> closeAction)
	{
		_closeAction = closeAction;
	}

	public IPpScreenshotConfigItem GetDefault(ScreenshotType type)
	{
		return _globalPpScreenshotConfig.GetDefaultCustomizeValues(type);
	}

	public void Screen3DLoaded()
	{
		if (BendMachine == null)
		{
			Close();
			return;
		}
		if (Screen3D.InteractionMode is NavigateInteractionMode navigateInteractionMode)
		{
			navigateInteractionMode.ViewChanged += delegate
			{
				ViewChanged();
			};
		}
		UnfoldModel = _currentDocProvider.CurrentDoc.UnfoldModel3D;
		FinishedProductModel = _currentDocProvider.CurrentDoc.ModifiedEntryModel3D;
		ISimulationThread bendSimulation = _currentDocProvider.CurrentDoc.BendSimulation;
		MachineModel = bendSimulation.State.Machine;
		SimulationModel = bendSimulation.State.Part;
		Screen3D.ScreenD3D.AddModel(UnfoldModel);
		Screen3D.ScreenD3D.AddModel(MachineModel);
		Screen3D.ScreenD3D.AddModel(FinishedProductModel);
		Screen3D.ScreenD3D.AddModel(SimulationModel);
		Screen3D.ScreenD3D.Renderer.RenderData.ShadowMode = RenderData.Shadows.None;
		IPpScreenshotConfig machineConfig = BendMachine.PpScreenshotConfig;
		SelectedConfig = Configs.FirstOrDefault((ScreenshotConfigVm x) => x.Config == machineConfig) ?? Configs.First();
	}

	public void ManualViewChanged()
	{
		Matrix4d viewRotation = Matrix4d.RotationY(DegToRad(Roll));
		viewRotation *= Matrix4d.RotationX(DegToRad(Pitch));
		viewRotation *= Matrix4d.RotationZ(DegToRad(Yaw));
		SelectedItem.ViewRotation = viewRotation;
		UpdateView();
	}

	public void ViewChanged(Matrix4d? forcedView = null)
	{
		Matrix4d? matrix4d = forcedView ?? MtoM4d(Screen3D.ScreenD3D.Renderer.Root.Transform);
		if (matrix4d.HasValue)
		{
			_pitch = RadToDeg(Math.Asin(matrix4d.Value.M12));
			double num = RadToDeg(Math.Atan2(matrix4d.Value.M02, matrix4d.Value.M22));
			double num2 = RadToDeg(Math.Atan2(matrix4d.Value.M10, matrix4d.Value.M11));
			_roll = (360.0 - num + 360.0) % 360.0;
			_yaw = (360.0 - num2 + 360.0) % 360.0;
			NotifyPropertyChanged("Yaw");
			NotifyPropertyChanged("Pitch");
			NotifyPropertyChanged("Roll");
			if (Screen3D != null)
			{
				SelectedItem.ViewRotation = matrix4d.Value;
				Screen3D.ScreenD3D.ZoomExtend(800, 600, 1.0);
			}
		}
	}

	public void ChangeOpacity(MachineParts role, double opacity)
	{
		if (SelectedItem.Opacities.ContainsKey(role))
		{
			SelectedItem.Opacities[role] = opacity;
		}
		RepaintModels();
	}

	public void SelectedItemChanged()
	{
		ViewChanged(SelectedItem.ViewRotation);
		UpdateView();
	}

	private void RepaintModels()
	{
		_paintTool.FrameStart();
		_paintTool.SetModelVisibility(FinishedProductModel, visible: false, applyToSubModels: true);
		_paintTool.SetModelVisibility(MachineModel, visible: false, applyToSubModels: true);
		_paintTool.SetModelVisibility(UnfoldModel, visible: false, applyToSubModels: true);
		_paintTool.SetModelVisibility(SimulationModel, visible: false, applyToSubModels: true);
		switch (SelectedItem.ScreenshotType)
		{
		case ScreenshotType.FinishedProduct:
			_paintTool.SetModelVisibility(FinishedProductModel, visible: true, applyToSubModels: true);
			break;
		case ScreenshotType.UnfoldModel:
			_paintTool.SetModelVisibility(UnfoldModel, visible: true, applyToSubModels: true);
			break;
		case ScreenshotType.ToolSetup:
			_paintTool.SetModelVisibility(MachineModel, visible: true, applyToSubModels: true);
			_machinePainter.SetOpacities(_paintTool, BendMachine.Geometry, SelectedItem.Opacities);
			break;
		case ScreenshotType.EachBend:
			_paintTool.SetModelVisibility(SimulationModel, visible: true, applyToSubModels: true);
			_paintTool.SetModelVisibility(MachineModel, visible: true, applyToSubModels: true);
			_machinePainter.SetOpacities(_paintTool, BendMachine.Geometry, SelectedItem.Opacities);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_paintTool.FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);
		Screen3D.ScreenD3D.UpdateModelAppearance(ref modifiedModels, ref modifiedShells);
		Screen3D.ScreenD3D.ZoomExtend(800, 600, 1.0);
	}

	public void UpdateView()
	{
		RepaintModels();
		Screen3D.ProjectionType = SelectedItem.ProjectionType;
		Screen3D.ScreenD3D.SetViewDirectionByMatrix4d(SelectedItem.ViewRotation, render: false);
		Screen3D.ScreenD3D.ZoomExtend(800, 600, 1.0);
		Screen3D.ScreenD3D.Render(skipQueuedFrames: false);
	}

	public void Close()
	{
		_paintTool.FrameStart();
		_paintTool.FrameApply();
		_closeAction?.Invoke(this);
	}

	public void Save()
	{
		SaveInternal(SelectedConfig);
	}

	private IPpScreenshotConfig? SaveInternal(ScreenshotConfigVm vm)
	{
		if (vm.Config == null)
		{
			string configDisplayName = vm.ConfigDisplayName;
			if (_globalToolStorage.TryGetPpScreenshotConfig(configDisplayName) != null)
			{
				_messageLog.ShowErrorMessage("Config with the name '" + configDisplayName + "' already exists!");
				return null;
			}
			vm.Config = _globalToolStorage.CreatePpScreenshotConfig(configDisplayName);
		}
		foreach (ScreenshotConfigItemVm configItem in ConfigItems)
		{
			configItem.Save();
			vm.Config.Items[configItem.ScreenshotType] = configItem.Item;
		}
		_globalToolStorage.Save();
		return vm.Config;
	}

	public void LoadDefault()
	{
		foreach (ScreenshotConfigItemVm configItem in ConfigItems)
		{
			configItem.ResetDefault();
		}
		UpdateView();
	}

	public void Load()
	{
		IPpScreenshotConfig ppScreenshotConfig = SelectedConfig.Config;
		if (ppScreenshotConfig == null)
		{
			ppScreenshotConfig = SaveInternal(SelectedConfig);
		}
		if (ppScreenshotConfig != null)
		{
			BendMachine.PpScreenshotConfig = ppScreenshotConfig;
			_machineBendFactory.SaveMachine(BendMachine);
		}
	}

	public void DeleteTheCustomization()
	{
		ScreenshotConfigVm config = SelectedConfig;
		SelectedConfig = Configs.FirstOrDefault((ScreenshotConfigVm x) => x != _dummyConfig && x != config) ?? _dummyConfig;
		Configs.Remove(config);
		if (config.Config != null)
		{
			if (BendMachine.PpScreenshotConfig == config.Config)
			{
				BendMachine.PpScreenshotConfig = _globalPpScreenshotConfig.GetGlobalConfig();
				_machineBendFactory.SaveMachine(BendMachine);
			}
			_globalToolStorage.DeletePpScreenshotConfig(config.Config);
			_globalToolStorage.Save();
		}
	}

	public virtual void Dispose()
	{
		Screen3D?.Dispose();
	}

	private static double RadToDeg(double rad)
	{
		return (rad * 180.0 / Math.PI + 360.0) % 360.0;
	}

	private static float DegToRad(double deg)
	{
		return (float)((deg * Math.PI / 180.0 + Math.PI * 2.0) % (Math.PI * 2.0));
	}

	private static Matrix4d? MtoM4d(Matrix? input)
	{
		if (!input.HasValue)
		{
			return null;
		}
		Matrix value = input.Value;
		Matrix4d value2 = default(Matrix4d);
		value2.M00 = value.M11;
		value2.M01 = value.M12;
		value2.M02 = value.M13;
		value2.M03 = value.M14;
		value2.M10 = value.M21;
		value2.M11 = value.M22;
		value2.M12 = value.M23;
		value2.M13 = value.M24;
		value2.M20 = value.M31;
		value2.M21 = value.M32;
		value2.M22 = value.M33;
		value2.M23 = value.M34;
		value2.M30 = value.M41;
		value2.M31 = value.M42;
		value2.M32 = value.M43;
		value2.M33 = value.M44;
		return value2;
	}

	private ScreenshotConfigVm Convert(IPpScreenshotConfig? config)
	{
		string text = string.Empty;
		if (config == null)
		{
			text = $"{_translator.Translate("PPSettings.NewConfigName")} {_newConfigCount++}";
		}
		ScreenshotConfigVm screenshotConfigVm = new ScreenshotConfigVm(config)
		{
			ConfigDisplayName = (config?.DisplayName ?? text)
		};
		foreach (ScreenshotType value2 in System.Enum.GetValues(typeof(ScreenshotType)))
		{
			string header = _translator.Translate("PPSettings.Tab_" + value2);
			if (config == null || !config.Items.TryGetValue(value2, out IPpScreenshotConfigItem value))
			{
				value = _globalPpScreenshotConfig.GetDefaultCustomizeValues(value2);
			}
			ScreenshotConfigItemVm item = new ScreenshotConfigItemVm(value, this)
			{
				Header = header,
				ScreenshotType = value2
			};
			screenshotConfigVm.Items.Add(item);
		}
		return screenshotConfigVm;
	}
}

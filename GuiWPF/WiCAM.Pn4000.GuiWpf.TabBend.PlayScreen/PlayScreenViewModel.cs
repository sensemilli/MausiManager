using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend.PlayScreen;

public class PlayScreenViewModel : EditPanel3DContentBase
{
	private ICommand _acceptThisCollision;

	private ICommand _goToStart;

	private ICommand _previous;

	private ICommand _play;

	private ICommand _pause;

	private ICommand _next;

	private ICommand _goToEnd;

	private ICommand _settings;

	private ICommand _speedUp;

	private ICommand _speedDown;

	private Visibility _playButtonVisibility;

	private Visibility _pauseButtonVisibility = Visibility.Collapsed;

	private Visibility _settingsVisibility = Visibility.Collapsed;

	private Visibility _visibility = Visibility.Collapsed;

	private bool _active = true;

	private bool _radioButtonsEnabled = true;

	private bool _showMachine = true;

	private double _sliderValue;

	private double _sliderMaxValue = 1.0;

	private bool _triggerFromSim;

	private bool _collisionControlOn;

	private bool _collisionControlEveryFrame = true;

	private bool _ignoreClampCollisions;

	private bool _ignoreOverbendCollisions;

	private bool _ignoreOpeningCollisions;

	private bool _ignoreLiftingAidCollisions;

	private bool _ignoreClosingCollisions;

	private bool _sheetMetalHandlingVisible = true;

	private double _speedFactor = 1.0;

	private Action<ICombinedBendDescriptorInternal> _selectedCommonBendFaceChanged;

	private ICombinedBendDescriptorInternal _selectedCommonBendFace;

	private IDoc3d _currentDoc;

	private readonly IScreen3DMain _screen3d;

	private readonly IConfigProvider _configProvider;

	private ISimulationThread _currentSimulation;

	private readonly Func<bool> _isSubVmActive;

	private readonly ToolCalculationMode _toolCalculationMode;

	private readonly IBendSelection _bendSelection;

	private readonly IToolsToMachineModel _toolsToMachineModel;

	private readonly ITranslator _translator;

	private bool _stopStepsLoop;

	private bool _stopExcludedStep;

	private bool _stopNotPositionedStep;

	private bool _stopPositionDieStep;

	private bool _stopPositionFingerStep;

	private bool _stopPositionPartInToolStationStep;

	private bool _stopCloseStep;

	private bool _stopTouchStep;

	private bool _stopSpringPressDownStep;

	private bool _stopClampStep;

	private bool _stopRetractFingerStep;

	private bool _stopBendStep;

	private bool _stopOverbendStep;

	private bool _stopRelaxStep;

	private bool _stopOpenStep;

	private bool _stopRemovePartStep;

	private bool _stopPositionAcbLaserStep;

	private bool _stopPositionLiftingAidStep;

	private bool _stopOthersStep;

	private HashSet<(Face face, Model model)> _collidingFaces = new HashSet<(Face, Model)>();

	private Action _requestRepaint;

	public Visibility PlayButtonVisibility
	{
		get
		{
			return _playButtonVisibility;
		}
		set
		{
			_playButtonVisibility = value;
			NotifyPropertyChanged("PlayButtonVisibility");
		}
	}

	public Visibility PauseButtonVisibility
	{
		get
		{
			return _pauseButtonVisibility;
		}
		set
		{
			_pauseButtonVisibility = value;
			NotifyPropertyChanged("PauseButtonVisibility");
		}
	}

	public Visibility SettingsVisibility
	{
		get
		{
			return _settingsVisibility;
		}
		set
		{
			_settingsVisibility = value;
			NotifyPropertyChanged("SettingsVisibility");
		}
	}

	public Visibility Visibility
	{
		get
		{
			if (_currentSimulation == null || !Active)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
				NotifyPropertyChanged("Active");
				NotifyPropertyChanged("Visibility");
			}
		}
	}

	public bool SheetHandlingOn
	{
		get
		{
			return _sheetMetalHandlingVisible;
		}
		set
		{
			_sheetMetalHandlingVisible = value;
			if (_currentDoc != null)
			{
				ISimulationThread bendSimulation = _currentDoc.BendSimulation;
				bendSimulation.State.SheetHandlingVisible = value;
				SetSliderValues(bendSimulation);
				_sliderValue = bendSimulation.StepToTime(bendSimulation.State.CurrentStep);
				NotifyPropertyChanged("SliderValue");
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.SheetHandling != value)
			{
				simulationConfig.SheetHandling = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("SheetHandlingOn");
		}
	}

	public bool CollisionControlOn
	{
		get
		{
			return _collisionControlOn;
		}
		set
		{
			_collisionControlOn = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.CheckCollisions = value;
				_currentDoc.BendSimulation.State.CheckSelfCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.CollisionCheck != value)
			{
				simulationConfig.CollisionCheck = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			RadioButtonsEnabled = value;
			NotifyPropertyChanged("CollisionControlOn");
		}
	}

	public bool IgnoreClampCollisions
	{
		get
		{
			return _ignoreClampCollisions;
		}
		set
		{
			_ignoreClampCollisions = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.IgnoreClampCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.IgnoreClampCollisions != value)
			{
				simulationConfig.IgnoreClampCollisions = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("IgnoreClampCollisions");
		}
	}

	public bool IgnoreOverbendCollisions
	{
		get
		{
			return _ignoreOverbendCollisions;
		}
		set
		{
			_ignoreOverbendCollisions = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.IgnoreOverbendCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.IgnoreOverbendCollisions != value)
			{
				simulationConfig.IgnoreOverbendCollisions = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("IgnoreOverbendCollisions");
		}
	}

	public bool IgnoreOpeningCollisions
	{
		get
		{
			return _ignoreOpeningCollisions;
		}
		set
		{
			_ignoreOpeningCollisions = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.IgnoreOpeningCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.IgnoreOpeningCollisions != value)
			{
				simulationConfig.IgnoreOpeningCollisions = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("IgnoreOpeningCollisions");
		}
	}

	public bool IgnoreClosingCollisions
	{
		get
		{
			return _ignoreClosingCollisions;
		}
		set
		{
			_ignoreClosingCollisions = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.IgnoreClosingCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.IgnoreClosingCollisions != value)
			{
				simulationConfig.IgnoreClosingCollisions = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("IgnoreClosingCollisions");
		}
	}

	public bool IgnoreLiftingAidCollisions
	{
		get
		{
			return _ignoreLiftingAidCollisions;
		}
		set
		{
			_ignoreLiftingAidCollisions = value;
			if (_currentDoc != null)
			{
				_currentDoc.BendSimulation.State.IgnoreLiftingAidCollisions = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.IgnoreLiftingAidCollisions != value)
			{
				simulationConfig.IgnoreLiftingAidCollisions = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("IgnoreLiftingAidCollisions");
		}
	}

	public bool CheckEveryFrame
	{
		get
		{
			return _collisionControlEveryFrame;
		}
		set
		{
			_collisionControlEveryFrame = value;
			if (_currentSimulation != null)
			{
				_currentSimulation.State.CheckCollisionsKeyFrames = !value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.CheckCollisionsKeyFrames == value)
			{
				simulationConfig.CheckCollisionsKeyFrames = !value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("CheckEveryFrame");
		}
	}

	public bool CheckKeyFrames
	{
		get
		{
			return !_collisionControlEveryFrame;
		}
		set
		{
			_collisionControlEveryFrame = !value;
			if (_currentSimulation != null)
			{
				_currentSimulation.State.CheckCollisionsKeyFrames = value;
			}
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null && simulationConfig.CheckCollisionsKeyFrames != value)
			{
				simulationConfig.CheckCollisionsKeyFrames = value;
				_configProvider?.Push(simulationConfig);
				_configProvider?.Save<SimulationConfig>();
			}
			NotifyPropertyChanged("CheckKeyFrames");
		}
	}

	public bool RadioButtonsEnabled
	{
		get
		{
			return _radioButtonsEnabled;
		}
		set
		{
			_radioButtonsEnabled = value;
			NotifyPropertyChanged("RadioButtonsEnabled");
		}
	}

	public double SpeedFactor
	{
		get
		{
			return _speedFactor;
		}
		set
		{
			double num = Math.Min(SpeedMax, Math.Max(SpeedMin, value));
			if (_speedFactor != num)
			{
				_speedFactor = num;
				if (_currentDoc?.BendSimulation != null)
				{
					_currentDoc.BendSimulation.State.SpeedFactor = num;
				}
				SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
				if (simulationConfig != null && simulationConfig.Speed != num)
				{
					simulationConfig.Speed = num;
					_configProvider?.Push(simulationConfig);
					_configProvider?.Save<SimulationConfig>();
				}
				NotifyPropertyChanged("SpeedFactor");
			}
		}
	}

	public double SpeedMax { get; set; } = 10.0;

	public double SpeedMin { get; set; } = 0.25;

	public bool ShowMachine
	{
		get
		{
			return _showMachine;
		}
		set
		{
		}
	}

	public ICombinedBendDescriptorInternal SelectedCommonBendFace
	{
		get
		{
			return _selectedCommonBendFace;
		}
		set
		{
			if (_selectedCommonBendFace != value)
			{
				_selectedCommonBendFace = value;
				NotifyPropertyChanged("SelectedCommonBendFace");
				_selectedCommonBendFaceChanged?.Invoke(_selectedCommonBendFace);
				ApplySelectedBendToSimulation();
			}
		}
	}

	public string CurrentStepDesc { get; set; }

	public bool StopStepsLoop
	{
		get
		{
			return _stopStepsLoop;
		}
		set
		{
			if (_stopStepsLoop != value)
			{
				_stopStepsLoop = value;
				NotifyPropertyChanged("StopStepsLoop");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopNotPositionedStep
	{
		get
		{
			return _stopNotPositionedStep;
		}
		set
		{
			if (_stopNotPositionedStep != value)
			{
				_stopNotPositionedStep = value;
				NotifyPropertyChanged("StopNotPositionedStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopPositionDieStep
	{
		get
		{
			return _stopPositionDieStep;
		}
		set
		{
			if (_stopPositionDieStep != value)
			{
				_stopPositionDieStep = value;
				NotifyPropertyChanged("StopPositionDieStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopPositionFingerStep
	{
		get
		{
			return _stopPositionFingerStep;
		}
		set
		{
			if (_stopPositionFingerStep != value)
			{
				_stopPositionFingerStep = value;
				NotifyPropertyChanged("StopPositionFingerStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopPositionPartInToolStationStep
	{
		get
		{
			return _stopPositionPartInToolStationStep;
		}
		set
		{
			if (_stopPositionPartInToolStationStep != value)
			{
				_stopPositionPartInToolStationStep = value;
				NotifyPropertyChanged("StopPositionPartInToolStationStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopCloseStep
	{
		get
		{
			return _stopCloseStep;
		}
		set
		{
			if (_stopCloseStep != value)
			{
				_stopCloseStep = value;
				NotifyPropertyChanged("StopCloseStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopTouchStep
	{
		get
		{
			return _stopTouchStep;
		}
		set
		{
			if (_stopTouchStep != value)
			{
				_stopTouchStep = value;
				NotifyPropertyChanged("StopTouchStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopSpringPressDownStep
	{
		get
		{
			return _stopSpringPressDownStep;
		}
		set
		{
			if (_stopSpringPressDownStep != value)
			{
				_stopSpringPressDownStep = value;
				NotifyPropertyChanged("StopSpringPressDownStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopClampStep
	{
		get
		{
			return _stopClampStep;
		}
		set
		{
			if (_stopClampStep != value)
			{
				_stopClampStep = value;
				NotifyPropertyChanged("StopClampStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopRetractFingerStep
	{
		get
		{
			return _stopRetractFingerStep;
		}
		set
		{
			if (_stopRetractFingerStep != value)
			{
				_stopRetractFingerStep = value;
				NotifyPropertyChanged("StopRetractFingerStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopBendStep
	{
		get
		{
			return _stopBendStep;
		}
		set
		{
			if (_stopBendStep != value)
			{
				_stopBendStep = value;
				NotifyPropertyChanged("StopBendStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopOverbendStep
	{
		get
		{
			return _stopOverbendStep;
		}
		set
		{
			if (_stopOverbendStep != value)
			{
				_stopOverbendStep = value;
				NotifyPropertyChanged("StopOverbendStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopRelaxStep
	{
		get
		{
			return _stopRelaxStep;
		}
		set
		{
			if (_stopRelaxStep != value)
			{
				_stopRelaxStep = value;
				NotifyPropertyChanged("StopRelaxStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopOpenStep
	{
		get
		{
			return _stopOpenStep;
		}
		set
		{
			if (_stopOpenStep != value)
			{
				_stopOpenStep = value;
				NotifyPropertyChanged("StopOpenStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopRemovePartStep
	{
		get
		{
			return _stopRemovePartStep;
		}
		set
		{
			if (_stopRemovePartStep != value)
			{
				_stopRemovePartStep = value;
				NotifyPropertyChanged("StopRemovePartStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopPositionAcbLaserStep
	{
		get
		{
			return _stopPositionAcbLaserStep;
		}
		set
		{
			if (_stopPositionAcbLaserStep != value)
			{
				_stopPositionAcbLaserStep = value;
				NotifyPropertyChanged("StopPositionAcbLaserStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopPositionLiftingAidStep
	{
		get
		{
			return _stopPositionLiftingAidStep;
		}
		set
		{
			if (_stopPositionLiftingAidStep != value)
			{
				_stopPositionLiftingAidStep = value;
				NotifyPropertyChanged("StopPositionLiftingAidStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool StopOthersStep
	{
		get
		{
			return _stopOthersStep;
		}
		set
		{
			if (_stopOthersStep != value)
			{
				_stopOthersStep = value;
				NotifyPropertyChanged("StopOthersStep");
				SaveStopStepConfig();
			}
		}
	}

	public bool? StopAllStep
	{
		get
		{
			if (StopNotPositionedStep != StopPositionDieStep || StopNotPositionedStep != StopPositionFingerStep || StopNotPositionedStep != StopPositionPartInToolStationStep || StopNotPositionedStep != StopCloseStep || StopNotPositionedStep != StopTouchStep || StopNotPositionedStep != StopSpringPressDownStep || StopNotPositionedStep != StopClampStep || StopNotPositionedStep != StopRetractFingerStep || StopNotPositionedStep != StopBendStep || StopNotPositionedStep != StopOverbendStep || StopNotPositionedStep != StopRelaxStep || StopNotPositionedStep != StopOpenStep || StopNotPositionedStep != StopRemovePartStep || StopNotPositionedStep != StopPositionAcbLaserStep || StopNotPositionedStep != StopPositionLiftingAidStep || StopNotPositionedStep != StopOthersStep)
			{
				return null;
			}
			return StopNotPositionedStep;
		}
		set
		{
			if (value.HasValue)
			{
				StopNotPositionedStep = value.Value;
				StopPositionDieStep = value.Value;
				StopPositionFingerStep = value.Value;
				StopPositionPartInToolStationStep = value.Value;
				StopCloseStep = value.Value;
				StopTouchStep = value.Value;
				StopSpringPressDownStep = value.Value;
				StopClampStep = value.Value;
				StopRetractFingerStep = value.Value;
				StopBendStep = value.Value;
				StopOverbendStep = value.Value;
				StopRelaxStep = value.Value;
				StopOpenStep = value.Value;
				StopRemovePartStep = value.Value;
				StopPositionAcbLaserStep = value.Value;
				StopPositionLiftingAidStep = value.Value;
				StopOthersStep = value.Value;
			}
		}
	}

	public ICommand AcceptThisCollision => _acceptThisCollision ?? (_acceptThisCollision = new RelayCommand(delegate
	{
		AcceptThisCollisionClick();
	}, (object param) => CanUseCommands()));

	public ICommand GoToStart => _goToStart ?? (_goToStart = new RelayCommand(delegate
	{
		GoToStartClick();
	}, (object param) => CanUseCommands()));

	public ICommand Previous => _previous ?? (_previous = new RelayCommand(delegate
	{
		PreviousClick();
	}, (object param) => CanUseCommands()));

	public ICommand Play => _play ?? (_play = new RelayCommand(delegate
	{
		PlayClick();
	}, (object param) => CanUseCommands()));

	public ICommand Pause => _pause ?? (_pause = new RelayCommand(delegate
	{
		PauseClick();
	}, (object param) => CanUseCommands()));

	public ICommand Next => _next ?? (_next = new RelayCommand(delegate
	{
		NextClick();
	}, (object param) => CanUseCommands()));

	public ICommand GoToEnd => _goToEnd ?? (_goToEnd = new RelayCommand(delegate
	{
		GoToEndClick();
	}, (object param) => CanUseCommands()));

	public ICommand Settings => _settings ?? (_settings = new RelayCommand(delegate
	{
		SettingsCommand();
	}, (object param) => CanUseCommands()));

	public ICommand SpeedUp => _speedUp ?? (_speedUp = new RelayCommand(delegate
	{
		SpeedUpCommand();
	}, (object param) => CanUseCommands()));

	public ICommand SpeedDown => _speedDown ?? (_speedDown = new RelayCommand(delegate
	{
		SpeedDownCommand();
	}, (object param) => CanUseCommands()));

	public double SliderValue
	{
		get
		{
			return _sliderValue;
		}
		set
		{
			if (_sliderValue != value)
			{
				_sliderValue = value;
				if (!_triggerFromSim)
				{
					_currentDoc.BendSimulation.GotoTime(value);
				}
				NotifyPropertyChanged("SliderValue");
				TakeOverSelectedBendFromSimulation();
			}
		}
	}

	public double SliderMaxValue
	{
		get
		{
			return _sliderMaxValue;
		}
		set
		{
			_sliderMaxValue = value;
			NotifyPropertyChanged("SliderMaxValue");
		}
	}

	public DoubleCollection SliderTicks { get; set; }

	public Action<double, IEnumerable<(double start, double end, bool userAccepted)>> SetTimelineIntervals { get; set; }

	public PlayScreenViewModel(Action<ICombinedBendDescriptorInternal> selectedCommonBendFaceChanged, IDoc3d doc, IScreen3DMain screen3d, IConfigProvider configProvider, Action requestRepaint, Func<bool> isSubVmActive, ToolCalculationMode toolCalculationMode, IBendSelection bendSelection, IToolsToMachineModel toolsToMachineModel, ITranslator translator)
	{
		_selectedCommonBendFaceChanged = selectedCommonBendFaceChanged;
		_currentDoc = doc;
		_screen3d = screen3d;
		_configProvider = configProvider;
		_requestRepaint = requestRepaint;
		_isSubVmActive = isSubVmActive;
		_toolCalculationMode = toolCalculationMode;
		_bendSelection = bendSelection;
		_toolsToMachineModel = toolsToMachineModel;
		_translator = translator;
		_bendSelection.CurrentBendChanged += OnBendSelection_CurrentBendChanged;
		RegisterAtDoc();
		LoadStopStepConfig();
	}

	private void OnBendSelection_CurrentBendChanged(IBendPositioning? bend)
	{
		RefreshSlider(_currentSimulation);
	}

	private void RegisterAtDoc()
	{
		if (_currentDoc != null)
		{
			_currentDoc.BendSimulationChanged += DocOnBendSimulationChanged;
		}
		DocOnBendSimulationChanged(null, _currentDoc?.BendSimulation);
	}

	private void UnregisterAtDoc()
	{
		if (_currentDoc != null)
		{
			_currentDoc.BendSimulationChanged -= DocOnBendSimulationChanged;
		}
	}

	private void DocOnBendSimulationChanged(ISimulationThread oldSim, ISimulationThread newSim)
	{
		if (_currentSimulation == newSim)
		{
			return;
		}
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (_currentSimulation != null)
			{
				_currentSimulation.Pause();
				_currentSimulation.NewToolSetupsEvent -= OnNewToolSetups;
				_currentSimulation.PlayEvent -= OnPlayEvent;
				_currentSimulation.PauseEvent -= OnPauseEvent;
				_currentSimulation.TickEvent -= OnTickEvent;
				_currentSimulation.StopEvent -= OnStopEvent;
				_currentSimulation.Screen = null;
			}
			_currentSimulation = newSim;
			if (newSim != null)
			{
				_currentSimulation.NewToolSetupsEvent += OnNewToolSetups;
				newSim.PlayEvent += OnPlayEvent;
				newSim.PauseEvent += OnPauseEvent;
				newSim.TickEvent += OnTickEvent;
				newSim.StopEvent += OnStopEvent;
				newSim.State.CheckCollisions = CollisionControlOn;
				newSim.State.CheckCollisionsKeyFrames = CheckKeyFrames;
				newSim.State.IgnoreClampCollisions = IgnoreClampCollisions;
				newSim.State.IgnoreOverbendCollisions = IgnoreOverbendCollisions;
				newSim.State.IgnoreOpeningCollisions = IgnoreOpeningCollisions;
				newSim.State.IgnoreClosingCollisions = IgnoreClosingCollisions;
				newSim.State.IgnoreLiftingAidCollisions = IgnoreLiftingAidCollisions;
				newSim.State.SheetHandlingVisible = SheetHandlingOn;
				newSim.State.SpeedFactor = SpeedFactor;
				newSim.Screen = _screen3d.ScreenD3D;
				SetSliderValues(newSim);
				ApplySettings();
			}
			NotifyPropertyChanged("Visibility");
		});
	}

	private void ApplySettings()
	{
		if (_currentSimulation != null && !_currentSimulation.HiddenForUser)
		{
			SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
			if (simulationConfig != null)
			{
				SpeedFactor = simulationConfig.Speed;
				SheetHandlingOn = simulationConfig.SheetHandling;
				CollisionControlOn = simulationConfig.CollisionCheck;
				IgnoreClampCollisions = simulationConfig.IgnoreClampCollisions;
				IgnoreOverbendCollisions = simulationConfig.IgnoreOverbendCollisions;
				IgnoreOpeningCollisions = simulationConfig.IgnoreOpeningCollisions;
				IgnoreClosingCollisions = simulationConfig.IgnoreClosingCollisions;
				IgnoreLiftingAidCollisions = simulationConfig.IgnoreLiftingAidCollisions;
				_collisionControlEveryFrame = !simulationConfig.CheckCollisionsKeyFrames;
				_currentSimulation.State.CheckCollisionsKeyFrames = simulationConfig.CheckCollisionsKeyFrames;
				NotifyPropertyChanged("CheckEveryFrame");
				NotifyPropertyChanged("CheckKeyFrames");
			}
		}
	}

	private bool CanUseCommands()
	{
		return !_isSubVmActive();
	}

	private void OnNewToolSetups(IToolSetups? setups)
	{
	}

	private void OnPlayEvent(ISimulationThread sim)
	{
		SetPlayButton(visible: false);
	}

	private void OnPauseEvent(ISimulationThread sim)
	{
		SetPlayButton(visible: true);
	}

	private void OnTickEvent(ISimulationThread sim, bool visible)
	{
		if (visible)
		{
			RefreshSlider(sim);
		}
	}

	private void RefreshSlider(ISimulationThread? sim)
	{
		if (sim == null)
		{
			return;
		}
		ISimulationStep simulationStep = sim.SimulationsSteps[Math.Min((int)sim.CurrentStep, sim.SimulationsSteps.Count - 1)];
		string stepDescription = simulationStep.BendInfo.Order + 1 + " " + _translator.Translate("SimulationSteps." + simulationStep.GetType().Name);
		double currentTime = sim.CurrentTime;
		List<(double start, double, bool userAccepted)> collisions = (from x in sim.GetCollisionIntervals()
			select (start: sim.StepToTime(x.start), sim.StepToTime(x.end), userAccepted: x.userAccepted)).ToList();
		bool repaintNeeded = PaintChanged(sim.State);
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			CurrentStepDesc = stepDescription;
			NotifyPropertyChanged("CurrentStepDesc");
			_triggerFromSim = true;
			SliderValue = currentTime;
			SetTimelineIntervals?.Invoke(SliderMaxValue, collisions);
			if (repaintNeeded)
			{
				_requestRepaint?.Invoke();
			}
			_triggerFromSim = false;
		});
	}

	private void OnStopEvent(ISimulationThread sim)
	{
		SetPlayButton(visible: true);
	}

	public void SetPlayButton(bool visible)
	{
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			if (visible)
			{
				PlayButtonVisibility = Visibility.Visible;
				PauseButtonVisibility = Visibility.Collapsed;
			}
			else
			{
				PlayButtonVisibility = Visibility.Collapsed;
				PauseButtonVisibility = Visibility.Visible;
			}
		});
	}

	private void PlayClick()
	{
		_currentDoc.BendSimulation.State.PauseOnCollision = true;
		_currentDoc.BendSimulation.Play();
	}

	private void PauseClick()
	{
		_currentDoc.BendSimulation.Pause();
	}

	private void NextClick()
	{
		WindSimulation(forward: true);
	}

	private void GoToEndClick()
	{
		_currentDoc.BendSimulation.GoToEnd();
	}

	private void GoToStartClick()
	{
		_currentDoc.BendSimulation.GotoStep(0.0);
	}

	private void PreviousClick()
	{
		WindSimulation(forward: false);
	}

	private void WindSimulation(bool forward)
	{
		ISimulationThread bendSimulation = _currentDoc.BendSimulation;
		double currentStep = bendSimulation.CurrentStep;
		bool flag = StopStepsLoop;
		while (true)
		{
			if (bendSimulation.WindSimulation(forward, visible: true, WindSimulationValidateStep) != null)
			{
				return;
			}
			if (!flag)
			{
				break;
			}
			flag = false;
			if (forward)
			{
				bendSimulation.GotoStep(0.0, visible: false);
			}
			else
			{
				bendSimulation.GoToEnd(visible: false);
			}
		}
		bendSimulation.GotoStep(currentStep);
	}

	private bool WindSimulationValidateStep(ISimulationStep step)
	{
		if (!(step is IDoNotFoldStep))
		{
			if (!(step is IPositionDieStep))
			{
				if (!(step is IPositionFingersStep))
				{
					if (!(step is IPositionPartInToolStationStep))
					{
						if (!(step is ICloseStep))
						{
							if (!(step is ITouchStep))
							{
								if (!(step is ISpringPressDownStep))
								{
									if (!(step is IClampStep))
									{
										if (!(step is IRetractFingersStep))
										{
											if (!(step is IBendStep))
											{
												if (!(step is IOverBendStep))
												{
													if (!(step is IRelaxStep))
													{
														if (!(step is IOpenStep))
														{
															if (!(step is IRemovePartStep))
															{
																if (!(step is IAngleMeasurementStep))
																{
																	if (step is IPositionLiftingAidStep)
																	{
																		return StopPositionLiftingAidStep;
																	}
																	return StopOthersStep;
																}
																return StopPositionAcbLaserStep;
															}
															return StopRemovePartStep;
														}
														return StopOpenStep;
													}
													return StopRelaxStep;
												}
												return StopOverbendStep;
											}
											return StopBendStep;
										}
										return StopRetractFingerStep;
									}
									return StopClampStep;
								}
								return StopSpringPressDownStep;
							}
							return StopTouchStep;
						}
						return StopCloseStep;
					}
					return StopPositionPartInToolStationStep;
				}
				return StopPositionFingerStep;
			}
			return StopPositionDieStep;
		}
		return StopNotPositionedStep;
	}

	private void SaveStopStepConfig()
	{
		SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
		if (simulationConfig != null)
		{
			simulationConfig.StopStepsLoop = StopStepsLoop;
			simulationConfig.StopNotPositionedStep = StopNotPositionedStep;
			simulationConfig.StopPositionDieStep = StopPositionDieStep;
			simulationConfig.StopPositionFingerStep = StopPositionFingerStep;
			simulationConfig.StopPositionPartInToolStationStep = StopPositionPartInToolStationStep;
			simulationConfig.StopCloseStep = StopCloseStep;
			simulationConfig.StopTouchStep = StopTouchStep;
			simulationConfig.StopSpringPressDownStep = StopSpringPressDownStep;
			simulationConfig.StopClampStep = StopClampStep;
			simulationConfig.StopRetractFingerStep = StopRetractFingerStep;
			simulationConfig.StopBendStep = StopBendStep;
			simulationConfig.StopOverbendStep = StopOverbendStep;
			simulationConfig.StopRelaxStep = StopRelaxStep;
			simulationConfig.StopOpenStep = StopOpenStep;
			simulationConfig.StopRemovePartStep = StopRemovePartStep;
			simulationConfig.StopPositionAcbLaserStep = StopPositionAcbLaserStep;
			simulationConfig.StopPositionLiftingAidStep = StopPositionLiftingAidStep;
			simulationConfig.StopOthersStep = StopOthersStep;
			_configProvider?.Push(simulationConfig);
			_configProvider?.Save<SimulationConfig>();
		}
	}

	private void LoadStopStepConfig()
	{
		SimulationConfig simulationConfig = _configProvider?.InjectOrCreate<SimulationConfig>();
		if (simulationConfig != null)
		{
			StopStepsLoop = simulationConfig.StopStepsLoop;
			StopNotPositionedStep = simulationConfig.StopNotPositionedStep;
			StopPositionDieStep = simulationConfig.StopPositionDieStep;
			StopPositionFingerStep = simulationConfig.StopPositionFingerStep;
			StopPositionPartInToolStationStep = simulationConfig.StopPositionPartInToolStationStep;
			StopCloseStep = simulationConfig.StopCloseStep;
			StopTouchStep = simulationConfig.StopTouchStep;
			StopSpringPressDownStep = simulationConfig.StopSpringPressDownStep;
			StopClampStep = simulationConfig.StopClampStep;
			StopRetractFingerStep = simulationConfig.StopRetractFingerStep;
			StopBendStep = simulationConfig.StopBendStep;
			StopOverbendStep = simulationConfig.StopOverbendStep;
			StopRelaxStep = simulationConfig.StopRelaxStep;
			StopOpenStep = simulationConfig.StopOpenStep;
			StopRemovePartStep = simulationConfig.StopRemovePartStep;
			StopPositionAcbLaserStep = simulationConfig.StopPositionAcbLaserStep;
			StopPositionLiftingAidStep = simulationConfig.StopPositionLiftingAidStep;
			StopOthersStep = simulationConfig.StopOthersStep;
		}
	}

	private void AcceptThisCollisionClick()
	{
		_currentDoc.BendSimulation.State.SimulationCollisionManager.AcceptCollision(_currentDoc.BendSimulation.State.CurrentStep);
	}

	public void SetSliderValues(ISimulationThread simulation)
	{
		SliderMaxValue = simulation.MaxTime();
		DoubleCollection doubleCollection = new DoubleCollection();
		int num = -1;
		for (int i = 0; i < simulation.SimulationsSteps.Count; i++)
		{
			int order = simulation.SimulationsSteps[i].BendInfo.Order;
			if (order != num)
			{
				num = order;
				doubleCollection.Add(simulation.StepToTime(i));
			}
		}
		SliderTicks = doubleCollection;
		NotifyPropertyChanged("SliderTicks");
	}

	public void MouseEnterCommand()
	{
		base.Opacity = 1.0;
	}

	public void MouseLeaveCommand()
	{
		base.Opacity = 0.6;
	}

	private void SettingsCommand()
	{
		SettingsVisibility = ((SettingsVisibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
	}

	private void SpeedUpCommand()
	{
		if (SpeedFactor < SpeedMax)
		{
			SpeedFactor += GetInCreaseFactor();
		}
	}

	private void SpeedDownCommand()
	{
		if (SpeedFactor > SpeedMin)
		{
			SpeedFactor -= GetInCreaseFactor();
		}
	}

	private double GetInCreaseFactor()
	{
		if (!(SpeedFactor < 2.0))
		{
			if (!(SpeedFactor < 5.0))
			{
				if (!(SpeedFactor <= 10.0))
				{
					return 0.25;
				}
				return 1.0;
			}
			return 0.5;
		}
		return 0.25;
	}

	private void TakeOverSelectedBendFromSimulation()
	{
		if (_currentSimulation != null)
		{
			int num = -1;
			if (SelectedCommonBendFace != null)
			{
				num = SelectedCommonBendFace.Order;
			}
			int order = _currentSimulation.State.ActiveStep.BendInfo.Order;
			if (order != num)
			{
				SelectedCommonBendFace = _currentDoc.CombinedBendDescriptors[order];
			}
		}
	}

	private void ApplySelectedBendToSimulation()
	{
		if (_currentSimulation != null && !_currentSimulation.IsRunning)
		{
			int num = -1;
			if (SelectedCommonBendFace != null)
			{
				num = SelectedCommonBendFace.Order;
			}
			if (_currentSimulation.State.ActiveStep.BendInfo.Order != num && num >= 0)
			{
				_currentSimulation.GoToBend(num);
			}
		}
	}

	public void Dispose()
	{
		UnregisterAtDoc();
		DocOnBendSimulationChanged(_currentSimulation, null);
		_selectedCommonBendFaceChanged = null;
	}

	public bool PaintChanged(ISimulationState simState)
	{
		lock (_collidingFaces)
		{
			if (simState.IsRunning)
			{
				ConcurrentDictionary<(Triangle, Model), HashSet<(Triangle, Model)>> detectedCollisionsCurrentStep = simState.DetectedCollisionsCurrentStep;
				if (detectedCollisionsCurrentStep != null)
				{
					HashSet<(Face, Model)> hashSet = new HashSet<(Face, Model)>(from tri in detectedCollisionsCurrentStep.SelectMany<KeyValuePair<(Triangle, Model), HashSet<(Triangle, Model)>>, (Triangle, Model)>((KeyValuePair<(Triangle tri, Model model), HashSet<(Triangle tri, Model model)>> x) => x.Value).Concat(detectedCollisionsCurrentStep.Keys)
						select (Face: tri.Item1.Face, model: tri.Item2));
					if (!hashSet.IsSubsetOf(_collidingFaces))
					{
						foreach (var item in hashSet)
						{
							_collidingFaces.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			HashSet<(Face, Model)> collidingFaces = new HashSet<(Face, Model)>(simState.SimulationCollisionManager.GetCollisionFaces(0.0, simState.CurrentStep));
			_collidingFaces = collidingFaces;
			return true;
		}
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		lock (_collidingFaces)
		{
			if (_collidingFaces.Count <= 0)
			{
				return;
			}
			WiCAM.Pn4000.BendModel.Base.Color value = new WiCAM.Pn4000.BendModel.Base.Color(1f, 0f, 0f, 1f);
			foreach (var (face, model) in _collidingFaces)
			{
				paintTool.SetFaceColor(face, model, value);
			}
		}
	}
}

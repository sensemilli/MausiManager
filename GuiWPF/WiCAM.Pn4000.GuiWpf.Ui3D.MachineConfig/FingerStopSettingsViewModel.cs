using System.Linq;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class FingerStopSettingsViewModel : ViewModelBase
{
	private double _minFingerDistance;

	private double _minEdgeLength;

	private bool _ignoreNonParallelEdges;

	private bool _ignoreNoneFlatFacesAsStopPosition;

	private bool _ignoreHolesFacesAsStopPosition;

	private bool _ignoreBentUpFacesAsStopPosition;

	private bool _ignoreFingerStopsNextToTools;

	private int _minimizeZAxesMovements;

	private bool _ignoreZPositions;

	private StopSide _prefConfigurationSide;

	private bool _ignoreStability;

	private double _fixedLeftZPosition;

	private double _fixedRightZPosition;

	private double _prefDistanceFromCorner;

	private double _prefRetractDistance;

	private double _prefRetractSafetyDistance;

	private bool _retractWithSameValue;

	private bool _useHemFingerX;

	private double _hemFingerX1;

	private double _hemFingerX2;

	private double _minRetractDistance;

	private double _minXDistanceForHemming;

	private double _minDistanceForVerticalSupport;

	private double _prefSafetyDistanceAboveDie;

	private Vertical _prefSheetFingerstopFaceAlignment;

	private double _prefSheetFingerstopFaceAlignmentCorrection;

	private int _verticalSupport;

	private bool _allowCornerStops;

	private bool _allowCylinderStops;

	private int _prefNumberOfCornerStops;

	private int _prefCornerStop;

	public ChangedConfigType Changed { get; set; }

	public double MinFingerDistance
	{
		get
		{
			return _minFingerDistance;
		}
		set
		{
			_minFingerDistance = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinFingerDistance");
		}
	}

	public double MinEdgeLength
	{
		get
		{
			return _minEdgeLength;
		}
		set
		{
			_minEdgeLength = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinEdgeLength");
		}
	}

	public bool IgnoreNonParallelEdges
	{
		get
		{
			return _ignoreNonParallelEdges;
		}
		set
		{
			_ignoreNonParallelEdges = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreNonParallelEdges");
		}
	}

	public bool IgnoreNoneFlatFacesAsStopPosition
	{
		get
		{
			return _ignoreNoneFlatFacesAsStopPosition;
		}
		set
		{
			_ignoreNoneFlatFacesAsStopPosition = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreNoneFlatFacesAsStopPosition");
		}
	}

	public bool IgnoreHolesFacesAsStopPosition
	{
		get
		{
			return _ignoreHolesFacesAsStopPosition;
		}
		set
		{
			_ignoreHolesFacesAsStopPosition = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreHolesFacesAsStopPosition");
		}
	}

	public bool IgnoreBentUpFacesAsStopPosition
	{
		get
		{
			return _ignoreBentUpFacesAsStopPosition;
		}
		set
		{
			_ignoreBentUpFacesAsStopPosition = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreBentUpFacesAsStopPosition");
		}
	}

	public bool IgnoreFingerStopsNextToTools
	{
		get
		{
			return _ignoreFingerStopsNextToTools;
		}
		set
		{
			_ignoreFingerStopsNextToTools = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreFingerStopsNextToTools");
		}
	}

	public int MinimizeZAxesMovements
	{
		get
		{
			return _minimizeZAxesMovements;
		}
		set
		{
			_minimizeZAxesMovements = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinimizeZAxesMovements");
		}
	}

	public bool IgnoreZPositions
	{
		get
		{
			return _ignoreZPositions;
		}
		set
		{
			_ignoreZPositions = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreZPositions");
		}
	}

	public StopSide PrefConfigurationSide
	{
		get
		{
			return _prefConfigurationSide;
		}
		set
		{
			_prefConfigurationSide = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("PrefConfigurationSide");
		}
	}

	public bool IgnoreStability
	{
		get
		{
			return _ignoreStability;
		}
		set
		{
			_ignoreStability = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("IgnoreStability");
		}
	}

	public double FixedLeftZPosition
	{
		get
		{
			return _fixedLeftZPosition;
		}
		set
		{
			_fixedLeftZPosition = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("FixedLeftZPosition");
		}
	}

	public double FixedRightZPosition
	{
		get
		{
			return _fixedRightZPosition;
		}
		set
		{
			_fixedRightZPosition = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("FixedRightZPosition");
		}
	}

	public double PrefDistanceFromCorner
	{
		get
		{
			return _prefDistanceFromCorner;
		}
		set
		{
			_prefDistanceFromCorner = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("PrefDistanceFromCorner");
		}
	}

	public double PrefRetractDistance
	{
		get
		{
			return _prefRetractDistance;
		}
		set
		{
			_prefRetractDistance = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("PrefRetractDistance");
		}
	}

	public double PrefRetractSafetyDistance
	{
		get
		{
			return _prefRetractSafetyDistance;
		}
		set
		{
			_prefRetractSafetyDistance = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("PrefRetractSafetyDistance");
		}
	}

	public bool RetractWithSameValue
	{
		get
		{
			return _retractWithSameValue;
		}
		set
		{
			_retractWithSameValue = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("RetractWithSameValue");
		}
	}

	public bool UseHemFingerX
	{
		get
		{
			return _useHemFingerX;
		}
		set
		{
			_useHemFingerX = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("UseHemFingerX");
		}
	}

	public double HemFingerX1
	{
		get
		{
			return _hemFingerX1;
		}
		set
		{
			_hemFingerX1 = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("HemFingerX1");
		}
	}

	public double HemFingerX2
	{
		get
		{
			return _hemFingerX2;
		}
		set
		{
			_hemFingerX2 = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("HemFingerX2");
		}
	}

	public double MinRetractDistance
	{
		get
		{
			return _minRetractDistance;
		}
		set
		{
			_minRetractDistance = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinRetractDistance");
		}
	}

	public RadObservableCollection<RetractThresholdByThicknessViewModel> RetractThresholdByThickness { get; set; } = new RadObservableCollection<RetractThresholdByThicknessViewModel>();

	public double MinXDistanceForHemming
	{
		get
		{
			return _minXDistanceForHemming;
		}
		set
		{
			_minXDistanceForHemming = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinXDistanceForHemming");
		}
	}

	public double MinDistanceForVerticalSupport
	{
		get
		{
			return _minDistanceForVerticalSupport;
		}
		set
		{
			_minDistanceForVerticalSupport = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("MinDistanceForVerticalSupport");
		}
	}

	public double PrefSafetyDistanceAboveDie
	{
		get
		{
			return _prefSafetyDistanceAboveDie;
		}
		set
		{
			_prefSafetyDistanceAboveDie = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("PrefSafetyDistanceAboveDie");
		}
	}

	public Vertical PrefSheetFingerstopFaceAlignment
	{
		get
		{
			return _prefSheetFingerstopFaceAlignment;
		}
		set
		{
			if (_prefSheetFingerstopFaceAlignment != value)
			{
				_prefSheetFingerstopFaceAlignment = value;
				Changed = ChangedConfigType.Fingers;
				NotifyPropertyChanged("PrefSheetFingerstopFaceAlignment");
			}
		}
	}

	public double PrefSheetFingerstopFaceAlignmentCorrection
	{
		get
		{
			return _prefSheetFingerstopFaceAlignmentCorrection;
		}
		set
		{
			if (_prefSheetFingerstopFaceAlignmentCorrection != value)
			{
				_prefSheetFingerstopFaceAlignmentCorrection = value;
				Changed = ChangedConfigType.Fingers;
				NotifyPropertyChanged("PrefSheetFingerstopFaceAlignmentCorrection");
			}
		}
	}

	public int VerticalSupport
	{
		get
		{
			return _verticalSupport;
		}
		set
		{
			if (_verticalSupport != value)
			{
				_verticalSupport = value;
				Changed = ChangedConfigType.Fingers;
				NotifyPropertyChanged("VerticalSupport");
			}
		}
	}

	public bool AllowCornerStops
	{
		get
		{
			return _allowCornerStops;
		}
		set
		{
			_allowCornerStops = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("AllowCornerStops");
		}
	}

	public bool AllowCylinderStops
	{
		get
		{
			return _allowCylinderStops;
		}
		set
		{
			_allowCylinderStops = value;
			Changed = ChangedConfigType.Fingers;
			NotifyPropertyChanged("AllowCylinderStops");
		}
	}

	public int PrefNumberOfCornerStops
	{
		get
		{
			return _prefNumberOfCornerStops;
		}
		set
		{
			if (_prefNumberOfCornerStops != value)
			{
				_prefNumberOfCornerStops = value;
				Changed = ChangedConfigType.Fingers;
				NotifyPropertyChanged("PrefNumberOfCornerStops");
			}
		}
	}

	public int PrefCornerStop
	{
		get
		{
			return _prefCornerStop;
		}
		set
		{
			if (_prefCornerStop != value)
			{
				_prefCornerStop = value;
				Changed = ChangedConfigType.Fingers;
				NotifyPropertyChanged("PrefCornerStop");
			}
		}
	}

	public FingerStopSettingsViewModel(IFingerStopSettings fingerStopSettings)
	{
		_minFingerDistance = fingerStopSettings.MinFingerDistance;
		_minEdgeLength = fingerStopSettings.MinEdgeLength;
		_ignoreNonParallelEdges = fingerStopSettings.IgnoreNonParallelEdges;
		_ignoreNoneFlatFacesAsStopPosition = fingerStopSettings.IgnoreNoneFlatFacesAsStopPosition;
		_ignoreHolesFacesAsStopPosition = fingerStopSettings.IgnoreHolesFacesAsStopPosition;
		_ignoreBentUpFacesAsStopPosition = fingerStopSettings.IgnoreBentUpFacesAsStopPosition;
		_ignoreFingerStopsNextToTools = fingerStopSettings.IgnoreFingerStopsNextToTools;
		_minimizeZAxesMovements = fingerStopSettings.MinimizeZAxesMovements;
		_ignoreZPositions = fingerStopSettings.IgnoreZPositions;
		_prefConfigurationSide = fingerStopSettings.PrefConfigurationSide;
		_ignoreStability = fingerStopSettings.IgnoreStability;
		_fixedLeftZPosition = fingerStopSettings.FixedLeftZPosition;
		_fixedRightZPosition = fingerStopSettings.FixedRightZPosition;
		_prefDistanceFromCorner = fingerStopSettings.PrefDistanceFromCorner;
		_prefRetractDistance = fingerStopSettings.PrefRetractDistance;
		_prefRetractSafetyDistance = fingerStopSettings.PrefRetractSafetyDistance;
		_retractWithSameValue = fingerStopSettings.RetractWithSameValue;
		_useHemFingerX = fingerStopSettings.UseHemFingerX;
		_hemFingerX1 = fingerStopSettings.HemFingerX1;
		_hemFingerX2 = fingerStopSettings.HemFingerX2;
		_minRetractDistance = fingerStopSettings.MinRetractDistance;
		_minXDistanceForHemming = fingerStopSettings.MinXDistanceForHemming;
		_minDistanceForVerticalSupport = fingerStopSettings.MinDistanceForVerticalSupport;
		_prefSafetyDistanceAboveDie = fingerStopSettings.PrefSafetyDistanceAboveDie;
		_prefSheetFingerstopFaceAlignment = fingerStopSettings.PrefSheetFingerstopFaceAlignment;
		_prefSheetFingerstopFaceAlignmentCorrection = fingerStopSettings.PrefSheetFingerstopFaceAlignmentCorrection;
		_verticalSupport = fingerStopSettings.VerticalSupport;
		_allowCornerStops = fingerStopSettings.AllowCornerStops;
		_allowCylinderStops = fingerStopSettings.AllowCylinderStops;
		_prefNumberOfCornerStops = fingerStopSettings.PrefNumberOfCornerStops;
		_prefCornerStop = fingerStopSettings.PrefCornerStop;
		RetractThresholdByThickness.AddRange(fingerStopSettings.RetractThresholdByThickness.Select((Pair<double, double> x) => new RetractThresholdByThicknessViewModel
		{
			Thickness = x.Item1,
			RetractThreshold = x.Item2
		}));
	}

	public void Save(IFingerStopSettings saveTarget)
	{
		saveTarget.MinFingerDistance = _minFingerDistance;
		saveTarget.MinEdgeLength = _minEdgeLength;
		saveTarget.IgnoreNonParallelEdges = _ignoreNonParallelEdges;
		saveTarget.IgnoreNoneFlatFacesAsStopPosition = _ignoreNoneFlatFacesAsStopPosition;
		saveTarget.IgnoreHolesFacesAsStopPosition = _ignoreHolesFacesAsStopPosition;
		saveTarget.IgnoreBentUpFacesAsStopPosition = _ignoreBentUpFacesAsStopPosition;
		saveTarget.IgnoreFingerStopsNextToTools = _ignoreFingerStopsNextToTools;
		saveTarget.MinimizeZAxesMovements = _minimizeZAxesMovements;
		saveTarget.IgnoreZPositions = _ignoreZPositions;
		saveTarget.PrefConfigurationSide = _prefConfigurationSide;
		saveTarget.IgnoreStability = _ignoreStability;
		saveTarget.FixedLeftZPosition = _fixedLeftZPosition;
		saveTarget.FixedRightZPosition = _fixedRightZPosition;
		saveTarget.PrefDistanceFromCorner = _prefDistanceFromCorner;
		saveTarget.PrefRetractSafetyDistance = _prefRetractSafetyDistance;
		saveTarget.PrefRetractDistance = _prefRetractDistance;
		saveTarget.RetractWithSameValue = _retractWithSameValue;
		saveTarget.UseHemFingerX = _useHemFingerX;
		saveTarget.HemFingerX1 = _hemFingerX1;
		saveTarget.HemFingerX2 = _hemFingerX2;
		saveTarget.MinRetractDistance = _minRetractDistance;
		saveTarget.MinXDistanceForHemming = _minXDistanceForHemming;
		saveTarget.MinDistanceForVerticalSupport = _minDistanceForVerticalSupport;
		saveTarget.PrefSafetyDistanceAboveDie = _prefSafetyDistanceAboveDie;
		saveTarget.PrefSheetFingerstopFaceAlignment = _prefSheetFingerstopFaceAlignment;
		saveTarget.PrefSheetFingerstopFaceAlignmentCorrection = _prefSheetFingerstopFaceAlignmentCorrection;
		saveTarget.VerticalSupport = _verticalSupport;
		saveTarget.AllowCornerStops = _allowCornerStops;
		saveTarget.AllowCylinderStops = _allowCylinderStops;
		saveTarget.PrefNumberOfCornerStops = _prefNumberOfCornerStops;
		saveTarget.PrefCornerStop = _prefCornerStop;
		saveTarget.RetractThresholdByThickness = RetractThresholdByThickness.Select((RetractThresholdByThicknessViewModel x) => new Pair<double, double>(x.Thickness, x.RetractThreshold)).ToList();
	}
}

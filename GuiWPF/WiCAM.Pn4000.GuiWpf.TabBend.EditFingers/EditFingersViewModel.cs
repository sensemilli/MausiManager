using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.FingerCalculation;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.FingerStopCalculationMediator;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class EditFingersViewModel : SubViewModelBase, IEditFingersViewModel, ISubViewModel, IPopupViewModel
{
	private IFingerStop? _selectedFinger;

	private FingerStopCombinationViewModel _selectedFingerCombination;

	private double _r;

	private double _x;

	private double _z;

	private double? _retractX;

	private double? _retractR;

	private double? _retractZ;

	private ICommand _nextTopPosition;

	private ICommand _nextBottomPosition;

	private ICommand _nextLeftPosition;

	private ICommand _nextRightPosition;

	private IPnBndDoc _doc;

	private IBendSelection _bendSelection;

	private IFingerStopModifier _fingerModifier;

	private IFingerStopCalculationMediator _fingerCalculation;

	private readonly IUnitConverter _unitConverter;

	private readonly ITranslator _translator;

	private readonly IUndo3dDocService _undo3d;

	private RadObservableCollection<FingerStopViewModel> _availableFingers = new RadObservableCollection<FingerStopViewModel>();

	private bool _isClosing;

	private bool _isVisible;

	private bool _recalculatePosition = true;

	private List<IFingerStopPointInternal> _fingerStopPositionsTop;

	private List<IFingerStopPointInternal> _fingerStopPositionsBottom;

	private List<IFingerStopPointInternal> _fingerStopPositionsLeft;

	private List<IFingerStopPointInternal> _fingerStopPositionsRight;

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		private set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				NotifyPropertyChanged("IsVisible");
			}
		}
	}

	public RadObservableCollection<FingerStopViewModel> AvailableFingers
	{
		get
		{
			return _availableFingers;
		}
		set
		{
			_availableFingers = value;
		}
	}

	public IFingerStop? SelectedFinger
	{
		get
		{
			return _selectedFinger;
		}
		set
		{
			if (_selectedFinger != value)
			{
				_selectedFinger = value;
				UpdateValues();
				UpdateFingerStopCombinations();
				NotifyPropertyChanged("SelectedFinger");
			}
		}
	}

	public bool AutoRetract
	{
		get
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
			IFingerStop? selectedFinger = _selectedFinger;
			if (selectedFinger != null && selectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				if (combinedBendDescriptorInternal.XLeftRetractAuto.HasValue)
				{
					return !combinedBendDescriptorInternal.XLeftRetractUser.HasValue;
				}
				return false;
			}
			IFingerStop? selectedFinger2 = _selectedFinger;
			if (selectedFinger2 != null && selectedFinger2.FingerModel.PartRole == PartRole.RightFinger)
			{
				if (combinedBendDescriptorInternal.XRightRetractAuto.HasValue)
				{
					return !combinedBendDescriptorInternal.XRightRetractUser.HasValue;
				}
				return false;
			}
			return false;
		}
		set
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
			IFingerStop? selectedFinger = _selectedFinger;
			if (selectedFinger != null && selectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				if (value)
				{
					combinedBendDescriptorInternal.XLeftRetractUser = null;
				}
				UpdateValues();
			}
			else
			{
				IFingerStop? selectedFinger2 = _selectedFinger;
				if (selectedFinger2 != null && selectedFinger2.FingerModel.PartRole == PartRole.RightFinger)
				{
					if (value)
					{
						combinedBendDescriptorInternal.XRightRetractUser = null;
					}
					UpdateValues();
				}
			}
			NotifyPropertyChanged("AutoRetract");
		}
	}

	public bool SnapActive
	{
		get
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
			IFingerStop? selectedFinger = _selectedFinger;
			if (selectedFinger != null && selectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				return combinedBendDescriptorInternal.LeftFingerSnap;
			}
			IFingerStop? selectedFinger2 = _selectedFinger;
			if (selectedFinger2 != null && selectedFinger2.FingerModel.PartRole == PartRole.RightFinger)
			{
				return combinedBendDescriptorInternal.RightFingerSnap;
			}
			return false;
		}
		set
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
			IFingerStop? selectedFinger = _selectedFinger;
			if (selectedFinger != null && selectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				combinedBendDescriptorInternal.LeftFingerSnap = value;
				if (value)
				{
					Snap();
				}
				UpdateValues();
			}
			else
			{
				IFingerStop? selectedFinger2 = _selectedFinger;
				if (selectedFinger2 != null && selectedFinger2.FingerModel.PartRole == PartRole.RightFinger)
				{
					combinedBendDescriptorInternal.RightFingerSnap = value;
					if (value)
					{
						Snap();
					}
					UpdateValues();
				}
			}
			NotifyPropertyChanged("SnapActive");
		}
	}

	public RadObservableCollection<FingerStopCombinationViewModel> AvailableFingerStopCombinations { get; set; } = new RadObservableCollection<FingerStopCombinationViewModel>();

	public FingerStopCombinationViewModel SelectedFingerCombination
	{
		get
		{
			return _selectedFingerCombination;
		}
		set
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
			if (value != null)
			{
				_selectedFingerCombination = value;
				if (!_recalculatePosition)
				{
					IFingerStopPointInternal stopPointLeft = combinedBendDescriptorInternal.SelectedStopPointLeft;
					IFingerStopPointInternal stopPointRight = combinedBendDescriptorInternal.SelectedStopPointRight;
					_ = combinedBendDescriptorInternal.StopPointsLeft;
					_ = combinedBendDescriptorInternal.StopPointsRight;
					FingerPositioningMode fingerPositioningMode = combinedBendDescriptorInternal.FingerPositioningMode;
					FingerStability fingerStability = combinedBendDescriptorInternal.FingerStability;
					double? xLeftRetractAuto = combinedBendDescriptorInternal.XLeftRetractAuto;
					double? xRightRetractAuto = combinedBendDescriptorInternal.XRightRetractAuto;
					if (SelectedFingerCombination != null)
					{
						_ = SelectedFingerCombination.Combination;
						_fingerCalculation.CalculateStopPointsForFinger(_doc.BendSimulation.State.Part, _doc.Material, _doc.Thickness, combinedBendDescriptorInternal.Order, SelectedFinger.FingerModel.PartRole, SelectedFingerCombination.Combination, otherFingerFixed: false, _doc.CombinedBendDescriptors.ToList(), _doc.ToolsAndBends.BendPositions, _doc.BendMachine, _doc.ToolsAndBends.ToolSetups, FingerPositioningMode.User);
					}
					combinedBendDescriptorInternal.SelectedStopPointLeft = stopPointLeft;
					combinedBendDescriptorInternal.SelectedStopPointRight = stopPointRight;
					if (combinedBendDescriptorInternal.StopPointsLeft != null && !combinedBendDescriptorInternal.StopPointsLeft.Any((IFingerStopPointInternal x) => (x.StopPoint - stopPointLeft.StopPoint).Length < 0.01))
					{
						combinedBendDescriptorInternal.StopPointsLeft.Add(stopPointLeft);
					}
					if (combinedBendDescriptorInternal.StopPointsRight != null && !combinedBendDescriptorInternal.StopPointsRight.Any((IFingerStopPointInternal x) => (x.StopPoint - stopPointRight.StopPoint).Length < 0.01))
					{
						combinedBendDescriptorInternal.StopPointsRight.Add(stopPointRight);
					}
					combinedBendDescriptorInternal.FingerPositioningMode = fingerPositioningMode;
					combinedBendDescriptorInternal.FingerStability = fingerStability;
					combinedBendDescriptorInternal.XLeftRetractAuto = xLeftRetractAuto;
					combinedBendDescriptorInternal.XRightRetractAuto = xRightRetractAuto;
					UpdateFingerStopPositions();
				}
				else
				{
					RecalculateFingerStopPositions();
				}
				UpdateValues();
				NotifyPropertyChanged("SelectedFingerCombination");
			}
			else
			{
				combinedBendDescriptorInternal.StopPointsLeft = null;
				combinedBendDescriptorInternal.StopPointsRight = null;
				UpdateFingerStopPositions();
				NotifyPropertyChanged("SelectedFingerCombination");
			}
		}
	}

	IFingerStopCombination IEditFingersViewModel.SelectedFingerCombination
	{
		get
		{
			return SelectedFingerCombination.Combination;
		}
		set
		{
			AvailableFingerStopCombinations.FirstOrDefault((FingerStopCombinationViewModel x) => x.Combination.Equals(value));
		}
	}

	public double RUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_r, 2);
		}
		set
		{
			double num = _unitConverter.Length.FromUi(value);
			if (_r != num)
			{
				_r = num;
				UpdateFromValues();
				NotifyPropertyChanged("RUi");
			}
		}
	}

	public double XUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_x, 2);
		}
		set
		{
			double num = _unitConverter.Length.FromUi(value);
			if (_x != num)
			{
				_x = num;
				UpdateFromValues();
				NotifyPropertyChanged("XUi");
			}
		}
	}

	public double ZUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_z, 2);
		}
		set
		{
			double num = _unitConverter.Length.FromUi(value);
			if (_z != num)
			{
				_z = num;
				UpdateFromValues();
				NotifyPropertyChanged("ZUi");
			}
		}
	}

	public double? RetractXUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_retractX, 2);
		}
		set
		{
			double? num = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value)) : null);
			if (_retractX != num)
			{
				_retractX = num;
				ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
				IBendMachine? bendMachine = _doc.BendMachine;
				if (bendMachine != null && bendMachine.FingerStopSettings.RetractWithSameValue)
				{
					combinedBendDescriptorInternal.XLeftRetractUser = num;
					combinedBendDescriptorInternal.XRightRetractUser = num;
					combinedBendDescriptorInternal.RLeftRetractUser = null;
					combinedBendDescriptorInternal.RRightRetractUser = null;
					combinedBendDescriptorInternal.ZLeftRetractUser = null;
					combinedBendDescriptorInternal.ZRightRetractUser = null;
				}
				else if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
				{
					combinedBendDescriptorInternal.XLeftRetractUser = num;
					combinedBendDescriptorInternal.RLeftRetractUser = null;
					combinedBendDescriptorInternal.ZLeftRetractUser = null;
				}
				else
				{
					combinedBendDescriptorInternal.XRightRetractUser = num;
					combinedBendDescriptorInternal.RRightRetractUser = null;
					combinedBendDescriptorInternal.ZRightRetractUser = null;
				}
				_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
				_doc.RecalculateSimulation();
				RaiseRequestRepaint();
				NotifyPropertyChanged("RetractXUi");
				NotifyPropertyChanged("RetractRUi");
				NotifyPropertyChanged("RetractZUi");
			}
		}
	}

	public double? RetractRUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_retractR, 2);
		}
		set
		{
			double? num = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value)) : null);
			if (_retractR != num)
			{
				_retractR = num;
				ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
				IBendMachine? bendMachine = _doc.BendMachine;
				if (bendMachine != null && bendMachine.FingerStopSettings.RetractWithSameValue)
				{
					combinedBendDescriptorInternal.RLeftRetractUser = num;
					combinedBendDescriptorInternal.RRightRetractUser = num;
					combinedBendDescriptorInternal.XLeftRetractUser = null;
					combinedBendDescriptorInternal.XRightRetractUser = null;
					combinedBendDescriptorInternal.ZLeftRetractUser = null;
					combinedBendDescriptorInternal.ZRightRetractUser = null;
				}
				else if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
				{
					combinedBendDescriptorInternal.RLeftRetractUser = num;
					combinedBendDescriptorInternal.XLeftRetractUser = null;
					combinedBendDescriptorInternal.ZLeftRetractUser = null;
				}
				else
				{
					combinedBendDescriptorInternal.RRightRetractUser = num;
					combinedBendDescriptorInternal.XRightRetractUser = null;
					combinedBendDescriptorInternal.ZRightRetractUser = null;
				}
				_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
				_doc.RecalculateSimulation();
				RaiseRequestRepaint();
				NotifyPropertyChanged("RetractRUi");
				NotifyPropertyChanged("RetractXUi");
				NotifyPropertyChanged("RetractZUi");
			}
		}
	}

	public double? RetractZUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_retractZ, 2);
		}
		set
		{
			double? num = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value)) : null);
			if (_retractZ == num)
			{
				return;
			}
			_retractZ = num;
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
			IBendMachine? bendMachine = _doc.BendMachine;
			if (bendMachine != null && bendMachine.FingerStopSettings.RetractWithSameValue)
			{
				if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
				{
					combinedBendDescriptorInternal.ZLeftRetractUser = num;
					combinedBendDescriptorInternal.ZRightRetractUser = 0.0 - num;
				}
				else
				{
					combinedBendDescriptorInternal.ZLeftRetractUser = 0.0 - num;
					combinedBendDescriptorInternal.ZRightRetractUser = num;
				}
				combinedBendDescriptorInternal.XLeftRetractUser = null;
				combinedBendDescriptorInternal.RLeftRetractUser = null;
				combinedBendDescriptorInternal.XRightRetractUser = null;
				combinedBendDescriptorInternal.RRightRetractUser = null;
			}
			else if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				combinedBendDescriptorInternal.ZLeftRetractUser = num;
				combinedBendDescriptorInternal.XLeftRetractUser = null;
				combinedBendDescriptorInternal.RLeftRetractUser = null;
			}
			else
			{
				combinedBendDescriptorInternal.ZRightRetractUser = num;
				combinedBendDescriptorInternal.XRightRetractUser = null;
				combinedBendDescriptorInternal.RRightRetractUser = null;
			}
			_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
			_doc.RecalculateSimulation();
			RaiseRequestRepaint();
			NotifyPropertyChanged("RetractZUi");
			NotifyPropertyChanged("RetractXUi");
			NotifyPropertyChanged("RetractRUi");
		}
	}

	public double? RetractXAutoUi
	{
		get
		{
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
			if (combinedBendDescriptorInternal == null || SelectedFinger == null)
			{
				return null;
			}
			IBendMachine? bendMachine = _doc.BendMachine;
			if (bendMachine != null && bendMachine.FingerStopSettings.RetractWithSameValue)
			{
				if (combinedBendDescriptorInternal.XLeftRetractAuto.HasValue && combinedBendDescriptorInternal.XRightRetractAuto.HasValue)
				{
					return _unitConverter.Length.ToUi(Math.Max(combinedBendDescriptorInternal.XLeftRetractAuto.Value, combinedBendDescriptorInternal.XRightRetractAuto.Value), 2);
				}
			}
			else if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				if (combinedBendDescriptorInternal.XLeftRetractAuto.HasValue)
				{
					return _unitConverter.Length.ToUi(combinedBendDescriptorInternal.XLeftRetractAuto.Value, 2);
				}
			}
			else if (combinedBendDescriptorInternal.XRightRetractAuto.HasValue)
			{
				return _unitConverter.Length.ToUi(combinedBendDescriptorInternal.XRightRetractAuto.Value, 2);
			}
			return null;
		}
		set
		{
			NotifyPropertyChanged("RetractXAutoUi");
		}
	}

	public string LengthUnit => _unitConverter.Length.Unit;

	public ICommand CmdSnap { get; }

	public ICommand NextTopPosition => _nextTopPosition ?? (_nextTopPosition = new RelayCommand((Action<object>)delegate
	{
		GoToNextTopPosition();
	}));

	public ICommand NextBottomPosition => _nextBottomPosition ?? (_nextBottomPosition = new RelayCommand((Action<object>)delegate
	{
		GoToNextBottomPosition();
	}));

	public ICommand NextLeftPosition => _nextLeftPosition ?? (_nextLeftPosition = new RelayCommand((Action<object>)delegate
	{
		GoToNextLeftPosition();
	}));

	public ICommand NextRightPosition => _nextRightPosition ?? (_nextRightPosition = new RelayCommand((Action<object>)delegate
	{
		GoToNextRightPosition();
	}));

	public bool NextTopEnabled
	{
		get
		{
			List<IFingerStopPointInternal> fingerStopPositionsTop = _fingerStopPositionsTop;
			if (fingerStopPositionsTop == null)
			{
				return false;
			}
			return fingerStopPositionsTop.Count > 0;
		}
	}

	public bool NextBottomEnabled
	{
		get
		{
			List<IFingerStopPointInternal> fingerStopPositionsBottom = _fingerStopPositionsBottom;
			if (fingerStopPositionsBottom == null)
			{
				return false;
			}
			return fingerStopPositionsBottom.Count > 0;
		}
	}

	public bool NextLeftEnabled
	{
		get
		{
			List<IFingerStopPointInternal> fingerStopPositionsLeft = _fingerStopPositionsLeft;
			if (fingerStopPositionsLeft == null)
			{
				return false;
			}
			return fingerStopPositionsLeft.Count > 0;
		}
	}

	public bool NextRightEnabled
	{
		get
		{
			List<IFingerStopPointInternal> fingerStopPositionsRight = _fingerStopPositionsRight;
			if (fingerStopPositionsRight == null)
			{
				return false;
			}
			return fingerStopPositionsRight.Count > 0;
		}
	}

	private IFingerStop _leftFinger => _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger;

	private IFingerStop _rightFinger => _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger;

	public EditFingersViewModel(IPnBndDoc doc, IBendSelection bendSelection, IFingerStopModifier fingerModifier, IFingerStopCalculationMediator fingerCalculation, IUnitConverter unitConverter, ITranslator translator, IUndo3dDocService undo3d)
	{
		_doc = doc;
		_bendSelection = bendSelection;
		_fingerModifier = fingerModifier;
		_fingerCalculation = fingerCalculation;
		_unitConverter = unitConverter;
		_translator = translator;
		_undo3d = undo3d;
		_bendSelection.CurrentBendChanged += _bendSelection_CurrentBendChanged;
		CmdSnap = new RelayCommand(Snap, () => SelectedFinger != null);
		Init();
	}

	private void _bendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
		Init();
	}

	public void Init()
	{
		if (_doc.BendSimulation?.State?.MachineConfig != null)
		{
			AvailableFingers.SuspendNotifications();
			AvailableFingers.Clear();
			AvailableFingers.Add(new FingerStopViewModel
			{
				Finger = _doc.BendSimulation.State.MachineConfig.Geometry.LeftFinger,
				Name = "Left Finger"
			});
			AvailableFingers.Add(new FingerStopViewModel
			{
				Finger = _doc.BendSimulation.State.MachineConfig.Geometry.RightFinger,
				Name = "Right Finger"
			});
			AvailableFingers.ResumeNotifications();
			AvailableFingerStopCombinations.Clear();
		}
	}

	public void SelectFinger(Model model)
	{
		SelectedFinger = AvailableFingers.FirstOrDefault((FingerStopViewModel x) => x.Finger.FingerModel == model)?.Finger;
	}

	private void UpdateFingerStopCombinations()
	{
		AvailableFingerStopCombinations.SuspendNotifications();
		AvailableFingerStopCombinations.Clear();
		if (SelectedFinger != null)
		{
			_recalculatePosition = false;
			AvailableFingerStopCombinations.AddRange((from pair in _doc.BendSimulation.State.MachineConfig.FingerStopPriorities.Priorities0Corners.List.FirstOrDefault()?.Item2.List
				where !pair.Item1.NoValidPositionFound && !pair.Item1.BoundingBoxPosition
				select new FingerStopCombinationViewModel(_doc.BendSimulation.State.MachineConfig, SelectedFinger, pair.Item1) into x
				where File.Exists(x.Img)
				select x));
			if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
			{
				SelectedFingerCombination = AvailableFingerStopCombinations.FirstOrDefault((FingerStopCombinationViewModel x) => x.Combination.FaceNames.SequenceEqual(_doc.BendSimulation.State.ActiveStep.BendInfo.FingerPosInfo?.LeftFingerStopPoint?.StopCombination?.FaceNames ?? new List<string>()));
			}
			else
			{
				SelectedFingerCombination = AvailableFingerStopCombinations.FirstOrDefault((FingerStopCombinationViewModel x) => x.Combination.FaceNames.SequenceEqual(_doc.BendSimulation.State.ActiveStep.BendInfo.FingerPosInfo?.RightFingerStopPoint?.StopCombination?.FaceNames ?? new List<string>()));
			}
			_recalculatePosition = true;
		}
		AvailableFingerStopCombinations.ResumeNotifications();
		NotifyPropertyChanged("AvailableFingerStopCombinations");
		NotifyPropertyChanged("SelectedFinger");
		UpdateValues();
	}

	public void Snap()
	{
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
		_fingerModifier.SnapFinger(_selectedFinger.FingerModel.PartRole, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos, _selectedFinger.FingerModel.PartRole == PartRole.LeftFinger || (combinedBendDescriptorInternal?.LeftFingerSnap ?? false), _selectedFinger.FingerModel.PartRole == PartRole.RightFinger || (combinedBendDescriptorInternal?.RightFingerSnap ?? false));
		_doc.Doc3d().SetFingerPos(combinedBendDescriptorInternal, leftFingerPos, rightFingerPos, FingerPositioningMode.User);
		UpdateFingerStopPositions();
		UpdateValues();
		_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
		_doc.RecalculateSimulation();
		RaiseRequestRepaint();
	}

	private void GoToNextTopPosition()
	{
		if (_fingerStopPositionsTop != null)
		{
			IFingerStopPointInternal fingerStopPointInternal = _fingerStopPositionsTop.FirstOrDefault();
			ApplyPosition(fingerStopPointInternal.StopPoint);
		}
	}

	private void GoToNextBottomPosition()
	{
		if (_fingerStopPositionsBottom != null)
		{
			IFingerStopPointInternal fingerStopPointInternal = _fingerStopPositionsBottom.LastOrDefault();
			ApplyPosition(fingerStopPointInternal.StopPoint);
		}
	}

	private void GoToNextLeftPosition()
	{
		if (_fingerStopPositionsLeft != null)
		{
			IFingerStopPointInternal fingerStopPointInternal = _fingerStopPositionsLeft.LastOrDefault();
			ApplyPosition(fingerStopPointInternal.StopPoint);
		}
	}

	private void GoToNextRightPosition()
	{
		if (_fingerStopPositionsRight != null)
		{
			IFingerStopPointInternal fingerStopPointInternal = _fingerStopPositionsRight.FirstOrDefault();
			ApplyPosition(fingerStopPointInternal.StopPoint);
		}
	}

	private void ApplyPosition(Vector3d stopPoint, bool snapOther = true)
	{
		ICombinedBendDescriptorInternal cbd = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
		if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
		{
			_fingerModifier.ApplyPositions(PartRole.LeftFinger, stopPoint, SelectedFinger.OtherFinger.FingerModel.WorldMatrix.TranslationVector, snapLeft: false, snapOther, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);
			UpdateStopFaceCombinations(leftFingerPos, rightFingerPos);
			_fingerCalculation.CalculateRetractForFingers(_doc.BendSimulation, cbd);
			_doc.Doc3d().SetFingerPos(cbd, leftFingerPos, rightFingerPos, FingerPositioningMode.User);
		}
		else if (SelectedFinger.FingerModel.PartRole == PartRole.RightFinger)
		{
			_fingerModifier.ApplyPositions(PartRole.RightFinger, SelectedFinger.OtherFinger.FingerModel.WorldMatrix.TranslationVector, stopPoint, snapOther, snapRight: false, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos2, out IFingerStopPointInternal rightFingerPos2);
			UpdateStopFaceCombinations(leftFingerPos2, rightFingerPos2);
			_fingerCalculation.CalculateRetractForFingers(_doc.BendSimulation, cbd);
			_doc.Doc3d().SetFingerPos(cbd, leftFingerPos2, rightFingerPos2, FingerPositioningMode.User);
		}
		UpdateFingerStopPositions();
		UpdateValues();
		_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
		_doc.RecalculateSimulation();
		RaiseRequestRepaint();
	}

	private void UpdateStopFaceCombinations(IFingerStopPointInternal posLeft, IFingerStopPointInternal posRight)
	{
		_fingerModifier.UpdateStopFaceCombinations(_doc, posLeft, posRight);
	}

	private void UpdateFingerStopPositions()
	{
		if (SelectedFinger?.StopPoints == null)
		{
			_fingerStopPositionsTop = new List<IFingerStopPointInternal>();
			_fingerStopPositionsBottom = new List<IFingerStopPointInternal>();
			_fingerStopPositionsLeft = new List<IFingerStopPointInternal>();
			_fingerStopPositionsRight = new List<IFingerStopPointInternal>();
			NotifyPropertyChanged("NextTopEnabled");
			NotifyPropertyChanged("NextBottomEnabled");
			NotifyPropertyChanged("NextLeftEnabled");
			NotifyPropertyChanged("NextRightEnabled");
			return;
		}
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
		List<IFingerStopPointInternal> list = ((SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger) ? combinedBendDescriptorInternal.StopPointsLeft : combinedBendDescriptorInternal.StopPointsRight);
		if (list != null)
		{
			List<IFingerStopPointInternal> source = (from p in list
				orderby p.StopPoint.Z, p.StopPoint.X, p.StopPoint.Y
				select p).ToList();
			List<IFingerStopPointInternal> source2 = (from p in list
				orderby p.StopPoint.X, p.StopPoint.Z, p.StopPoint.Y
				select p).ToList();
			IFingerStopPointInternal selectedPos = ((SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger) ? combinedBendDescriptorInternal.SelectedStopPointLeft : combinedBendDescriptorInternal.SelectedStopPointRight);
			_fingerStopPositionsTop = (from p in source
				where p.StopPoint.Z - selectedPos.StopPoint.Z > 0.01 && CheckPosition(p.StopPoint)
				orderby (selectedPos.StopPoint - p.StopPoint).LengthSquared
				select p).ToList();
			_fingerStopPositionsBottom = (from p in source
				where p.StopPoint.Z - selectedPos.StopPoint.Z < -0.01 && CheckPosition(p.StopPoint)
				orderby (selectedPos.StopPoint - p.StopPoint).LengthSquared descending
				select p).ToList();
			_fingerStopPositionsLeft = (from p in source2
				where p.StopPoint.X - selectedPos.StopPoint.X < -0.01 && CheckPosition(p.StopPoint)
				orderby (selectedPos.StopPoint - p.StopPoint).LengthSquared descending
				select p).ToList();
			_fingerStopPositionsRight = (from p in source2
				where p.StopPoint.X - selectedPos.StopPoint.X > 0.01 && CheckPosition(p.StopPoint)
				orderby (selectedPos.StopPoint - p.StopPoint).LengthSquared
				select p).ToList();
			NotifyPropertyChanged("NextTopEnabled");
			NotifyPropertyChanged("NextBottomEnabled");
			NotifyPropertyChanged("NextLeftEnabled");
			NotifyPropertyChanged("NextRightEnabled");
		}
	}

	private bool CheckPosition(Vector3d pos)
	{
		Pair<Vector3d, Vector3d> boundary = SelectedFinger.FingerModel.GetBoundary(SelectedFinger.FingerModel.Transform.Inverted);
		if (SelectedFinger.FingerModel.PartRole == PartRole.LeftFinger)
		{
			if (_doc.BendSimulation.State.RightFingerFixed)
			{
				Pair<Vector3d, Vector3d> boundary2 = _rightFinger.FingerModel.GetBoundary(_rightFinger.FingerModel.Parent.WorldMatrix);
				if (pos.X + boundary.Item2.X + _doc.BendMachine.FingerStopSettings.MinFingerDistance > boundary2.Item1.X)
				{
					return false;
				}
			}
		}
		else if (_doc.BendSimulation.State.LeftFingerFixed)
		{
			Pair<Vector3d, Vector3d> boundary3 = _leftFinger.FingerModel.GetBoundary(_leftFinger.FingerModel.Parent.WorldMatrix);
			if (pos.X + boundary.Item1.X - _doc.BendMachine.FingerStopSettings.MinFingerDistance < boundary3.Item2.X)
			{
				return false;
			}
		}
		return true;
	}

	private void RecalculateFingerStopPositions()
	{
		if (SelectedFingerCombination != null)
		{
			_ = SelectedFingerCombination.Combination;
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
			if (_fingerCalculation.CalculateStopPointsForFinger(_doc.BendSimulation.State.Part, _doc.Material, _doc.Thickness, combinedBendDescriptorInternal.Order, SelectedFinger.FingerModel.PartRole, SelectedFingerCombination.Combination, otherFingerFixed: false, _doc.CombinedBendDescriptors.ToList(), _doc.ToolsAndBends.BendPositions, _doc.BendMachine, _doc.ToolsAndBends.ToolSetups, FingerPositioningMode.User))
			{
				UpdateFingerStopPositions();
				_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
				_doc.RecalculateSimulation();
				RaiseRequestRepaint();
				_doc.FingerChangedInvoke();
			}
			else
			{
				_doc.MessageDisplay.ShowWarningMessage(_translator.Translate("l_popup.EditFingers.FailedFingerCalculationTt"), _translator.Translate("l_popup.EditFingers.FailedFingerCalculation"), notificationStyle: true);
			}
		}
	}

	private void UpdateValues()
	{
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend.Order);
		Vector3d? vector3d = SelectedFinger?.FingerModel.PartRole switch
		{
			PartRole.LeftFinger => combinedBendDescriptorInternal.SelectedStopPointLeft?.StopPoint, 
			PartRole.RightFinger => combinedBendDescriptorInternal.SelectedStopPointRight?.StopPoint, 
			_ => null, 
		};
		if (vector3d.HasValue)
		{
			_z = vector3d.Value.X;
			_x = vector3d.Value.Y;
			_r = vector3d.Value.Z - ((double)_doc.BendMachine.LowerToolSystem.WorkingHeight + _doc.BendSimulation.State.ActiveStep.BendInfo.WorkingHeightDie);
		}
		else
		{
			_z = 0.0;
			_x = 0.0;
			_r = 0.0;
		}
		_retractX = SelectedFinger?.FingerModel.PartRole switch
		{
			PartRole.LeftFinger => combinedBendDescriptorInternal.XLeftRetractUser, 
			PartRole.RightFinger => combinedBendDescriptorInternal.XRightRetractUser, 
			_ => null, 
		};
		_retractR = SelectedFinger?.FingerModel.PartRole switch
		{
			PartRole.LeftFinger => combinedBendDescriptorInternal.RLeftRetractUser, 
			PartRole.RightFinger => combinedBendDescriptorInternal.RRightRetractUser, 
			_ => null, 
		};
		_retractZ = SelectedFinger?.FingerModel.PartRole switch
		{
			PartRole.LeftFinger => combinedBendDescriptorInternal.ZLeftRetractUser, 
			PartRole.RightFinger => combinedBendDescriptorInternal.ZRightRetractUser, 
			_ => null, 
		};
		NotifyPropertyChanged("RUi");
		NotifyPropertyChanged("XUi");
		NotifyPropertyChanged("ZUi");
		NotifyPropertyChanged("RetractXUi");
		NotifyPropertyChanged("RetractRUi");
		NotifyPropertyChanged("RetractZUi");
		NotifyPropertyChanged("RetractXAutoUi");
		NotifyPropertyChanged("SnapActive");
	}

	private void UpdateFromValues()
	{
		if (SelectedFinger != null)
		{
			ApplyPosition(new Vector3d(_z, _x, _r + ((double)_doc.BendMachine.LowerToolSystem.WorkingHeight + _doc.BendSimulation.State.ActiveStep.BendInfo.WorkingHeightDie)), snapOther: false);
		}
	}

	private void UpdateRetract()
	{
	}

	private Pair<List<string>, int> GetStopCombinationId(List<Pair<List<string>, int>> list, IFingerStopCombination combination)
	{
		Pair<List<string>, int> result = new Pair<List<string>, int>(new List<string>(), -1);
		if (combination != null)
		{
			int num = 0;
			foreach (Pair<List<string>, int> item in list)
			{
				int num2 = combination.FaceNames.Count(item.Item1.Contains);
				if (num2 > num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	public void Dispose()
	{
		_bendSelection.CurrentBendChanged -= _bendSelection_CurrentBendChanged;
	}

	public void Activate()
	{
		IsVisible = true;
		_isClosing = false;
	}

	public void CommitChanges()
	{
		Close();
	}

	public void Cancel()
	{
		Close();
	}

	public override bool Close()
	{
		_bendSelection.CurrentBendChanged -= _bendSelection_CurrentBendChanged;
		IsVisible = false;
		if (!_isClosing)
		{
			_isClosing = true;
			return base.Close();
		}
		return true;
	}
}

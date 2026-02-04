using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Telerik.Windows.Data;
using Telerik.Windows.DragDrop;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.WpfControls.DragDrop;
using DragEventArgs = Telerik.Windows.DragDrop.DragEventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.BendSequenceList;

internal class BendSequenceListViewModel : IBendSequenceListViewModel
{
	public class BendViewModel : ViewModelBase
	{
		private static Brush _brushFingerStable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, byte.MaxValue, 0));

		private static Brush _brushFingerUnstable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0));

		private static Brush _brushFingerSemiStable = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0));

		private static Brush _brushFingerUser = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, 178, byte.MaxValue));

		private static Brush _brushFingerNone = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 128, 128, 128));

		private IBendPositioning _bend;

		private readonly ICombinedBendDescriptorInternal _cbd;

		private readonly IUnitConverter _unitConverter;

		private readonly IBendSelection _bendSelection;

		private readonly IEditToolsSelection _toolsSelection;

		private readonly IToolOperator _toolOperator;

		private readonly BendSequenceListViewModel _parentVm;

		private readonly ITranslator _translator;

		private readonly IPnBndDoc _doc;

		private readonly IScopedFactorio _scopedFactorio;

		private readonly IEditToolsViewModel _editToolsVm;

		private bool _isHovering;

		private bool _isSelected;

		private Brush _backgroundCol;

		private Brush _borderCol;

		private Brush _fingerStabilityColor;

		private string _fingerStabilityTooltip;

		private Dictionary<IAcbPunchPiece, AcbActivationResult> _displayedAcb = new Dictionary<IAcbPunchPiece, AcbActivationResult>();

		public ICombinedBendDescriptorInternal Cbd => _cbd;

		public IBendPositioning Bend => _bend;

		public int OrderUi => _bend.Order + 1;

		public string DragDesc => "Bend " + OrderUi;

		private string _angleSign
		{
			get
			{
				IBendDescriptor? bendDescriptor = _cbd.Enumerable.FirstOrDefault();
				if (bendDescriptor == null || bendDescriptor.BendParams.AngleSign >= 0)
				{
					return "+";
				}
				return "-";
			}
		}

		public string AngleChange
		{
			get
			{
				if (Math.Abs(_bend.Bend.StartAngle) < 1E-06)
				{
					return _angleSign + _unitConverter.Angle.ToUiUnit(_bend.Bend.DestAngle, 3);
				}
				return $"{_angleSign}{_unitConverter.Angle.ToUiUnit(_bend.Bend.StartAngle, 3)} -> {_angleSign}{_unitConverter.Angle.ToUiUnit(_bend.Bend.DestAngle, 3)}";
			}
		}

		public double LengthTotalWithGaps => _unitConverter.Length.ToUi(_bend.Bend.BendingZones.Last().End - _bend.Bend.BendingZones.First().Start, 1);

		public double LengthTotalWithoutGaps => _unitConverter.Length.ToUi(_bend.Bend.BendingZones.Sum((IRange x) => x.Length), 1);

		public bool? Collision { get; private set; }

		public bool HasTools
		{
			get
			{
				if (_bend.Anchor != null && _bend.PunchProfile != null)
				{
					return _bend.DieProfile != null;
				}
				return false;
			}
		}

		public string? UpperToolProfile => _bend.PunchProfile?.Name ?? "-";

		public string? LowerToolProfile => _bend.DieProfile?.Name ?? "-";

		public bool? Lcb
		{
			get
			{
				return _cbd.UseAngleMeasurement;
			}
			set
			{
				if (value.HasValue && value.Value != _cbd.UseAngleMeasurement)
				{
					_cbd.ActivateAndAutoPositionAngleMeasurementSystem(value.Value, recalcSim: true);
				}
			}
		}

		public string LiftingAidToolTip { get; set; }

		public UIElement LiftingAidIcon => CreateLiftingAidIcon().icon;

		public string AcbToolTip { get; set; }

		public UIElement AcbIcon => CalculateAcbIcon().icon;

		public int ToolSetupNo
		{
			get
			{
				return _bend.Anchor?.Root.Number ?? (-1);
			}
			set
			{
				if (ToolSetupNo != value)
				{
					IToolSetups toolSetups = _toolsSelection.ToolsAndBends.ToolSetups.FirstOrDefault((IToolSetups x) => x.Number == value);
					if (toolSetups == null)
					{
						_toolOperator.UnAssignBendToSections(_bend);
					}
					else
					{
						_editToolsVm.DoCmdChangeToolSetups(_bend, toolSetups, null);
					}
					NotifyPropertyChanged("ToolSetupNo");
				}
			}
		}

		public int? AnchorClusterNo => _bend.Anchor?.Number;

		public string Comment
		{
			get
			{
				return _bend.Bend.CombinedBendDescriptor.Comment;
			}
			set
			{
				_bend.Bend.CombinedBendDescriptor.Comment = value;
				NotifyPropertyChanged("Comment");
			}
		}

		public int? UserStepChangeMode
		{
			get
			{
				return _bend.Bend.CombinedBendDescriptor.UserStepChangeMode;
			}
			set
			{
				if (value.HasValue && !(_doc?.BendMachine?.PostProcessor?.GetAllStepChangeModes()).EmptyIfNull().Contains(value.Value))
				{
					NotifyPropertyChanged("UserStepChangeMode");
					return;
				}
				_bend.Bend.CombinedBendDescriptor.UserStepChangeMode = value;
				NotifyPropertyChanged("UserStepChangeMode");
			}
		}

		public double? UserReleasePointUiUnits
		{
			get
			{
				return _unitConverter.Length.ToUi(UserReleasePoint, 4) + _doc.Thickness;
			}
			set
			{
				UserReleasePoint = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value - _doc.Thickness)) : null);
			}
		}

		public double? UserReleasePoint
		{
			get
			{
				return _bend.Bend.CombinedBendDescriptor.ReleasePointUser;
			}
			set
			{
				_bend.Bend.CombinedBendDescriptor.ReleasePointUser = value;
				NotifyPropertyChanged("UserReleasePoint");
				NotifyPropertyChanged("UserReleasePointUiUnits");
			}
		}

		public double? ReleasePointUiUnits => _unitConverter.Length.ToUi(NcBendValuesOut.ReleasePoint, 4) + _doc.Thickness;

		public bool FlipGeometry
		{
			get
			{
				return _bend.MachineInsertDirection == MachinePartInsertionDirection.PartCentroidBehindMachine;
			}
			set
			{
				if (value)
				{
					_toolOperator.SetInsertDirection(_bend, MachinePartInsertionDirection.PartCentroidBehindMachine);
				}
				else
				{
					_toolOperator.SetInsertDirection(_bend, MachinePartInsertionDirection.PartCentroidInFrontOfMachine);
				}
				Cbd.MachinePartInsertionDirection = _bend.MachineInsertDirection;
				_doc.RecalculateSimulation();
				NotifyPropertyChanged("FlipGeometry");
			}
		}

		public bool IsHovering
		{
			get
			{
				return _isHovering;
			}
			set
			{
				if (_isHovering != value)
				{
					_isHovering = value;
					NotifyPropertyChanged("IsHovering");
					if (_bendSelection.CurrentBendHovering == _bend && !value)
					{
						_bendSelection.CurrentBendHovering = null;
					}
					else if (value)
					{
						_bendSelection.CurrentBendHovering = _bend;
					}
					RecalcBackground(out var _);
				}
			}
		}

		public bool IsSelectedByBinding
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					NotifyPropertyChanged("IsSelectedByBinding");
					_bendSelection.SetSelection(_bend, value);
					RecalcBackground(out var _);
				}
			}
		}

		public bool IsIncluded
		{
			get
			{
				return _cbd.IsIncluded;
			}
			set
			{
				if (_cbd.IsIncluded != value)
				{
					_cbd.IsIncluded = value;
					NotifyPropertyChanged("IsIncluded");
				}
			}
		}

		public Brush BackgroundCol
		{
			get
			{
				return _backgroundCol;
			}
			set
			{
				if (_backgroundCol != value)
				{
					_backgroundCol = value;
					NotifyPropertyChanged("BackgroundCol");
				}
			}
		}

		public Brush BorderCol
		{
			get
			{
				return _borderCol;
			}
			set
			{
				if (_borderCol != value)
				{
					_borderCol = value;
					NotifyPropertyChanged("BorderCol");
				}
			}
		}

		public Brush FingerStabilityColor
		{
			get
			{
				return _fingerStabilityColor;
			}
			set
			{
				_fingerStabilityColor = value;
				NotifyPropertyChanged("FingerStabilityColor");
			}
		}

		public string FingerStabilityTooltip
		{
			get
			{
				return _fingerStabilityTooltip;
			}
			set
			{
				_fingerStabilityTooltip = value;
				NotifyPropertyChanged("FingerStabilityTooltip");
			}
		}

		public double RadiusUiUnits
		{
			get
			{
				return _unitConverter.Length.ToUi(RadiusMm, 4);
			}
			set
			{
				RadiusMm = _unitConverter.Length.FromUi(value);
			}
		}

		public double RadiusMm
		{
			get
			{
				return _bend.Bend.CombinedBendDescriptor[0].BendParams.FinalRadius;
			}
			set
			{
				_parentVm.ChangeRadius(this.ToIEnumerable().ToList(), value);
				NotifyPropertyChanged("RadiusMm");
				NotifyPropertyChanged("RadiusUiUnits");
			}
		}

		public double BendDeductionUiUnits
		{
			get
			{
				return _unitConverter.Length.ToUi(BendDeductionMm, 4);
			}
			set
			{
				BendDeductionMm = _unitConverter.Length.FromUi(value);
			}
		}

		public double BendDeductionMm
		{
			get
			{
				return _bend.Bend.CombinedBendDescriptor[0].BendParams.FinalBendDeduction;
			}
			set
			{
				_parentVm.ChangeDeduction(this.ToIEnumerable().ToList(), value);
				NotifyPropertyChanged("BendDeductionMm");
				NotifyPropertyChanged("BendDeductionUiUnits");
			}
		}

		public LiftingAidEnum LiftingAidLeftFront
		{
			get
			{
				return _cbd.UseLeftFrontLiftingAid;
			}
			set
			{
				if (_cbd.UseLeftFrontLiftingAid != value)
				{
					_cbd.UseLeftFrontLiftingAid = value;
					UpdateLiftingAid();
				}
			}
		}

		public LiftingAidEnum LiftingAidLeftBack
		{
			get
			{
				return _cbd.UseLeftBackLiftingAid;
			}
			set
			{
				if (_cbd.UseLeftBackLiftingAid != value)
				{
					_cbd.UseLeftBackLiftingAid = value;
					UpdateLiftingAid();
				}
			}
		}

		public LiftingAidEnum LiftingAidRightFront
		{
			get
			{
				return _cbd.UseRightFrontLiftingAid;
			}
			set
			{
				if (_cbd.UseRightFrontLiftingAid != value)
				{
					_cbd.UseRightFrontLiftingAid = value;
					UpdateLiftingAid();
				}
			}
		}

		public LiftingAidEnum LiftingAidRightBack
		{
			get
			{
				return _cbd.UseRightBackLiftingAid;
			}
			set
			{
				if (_cbd.UseRightBackLiftingAid != value)
				{
					_cbd.UseRightBackLiftingAid = value;
					UpdateLiftingAid();
				}
			}
		}

		public ICommand CmdSimGotoBend { get; }

		public ICommand CmdCalcFinger { get; }

		public ICommand CmdCalcTools { get; }

		public ICommand CmdValidateCollisions { get; }

		public IAcbPunchPiece hoveredAcb { get; private set; }

		private INcBendValues? NcBendValuesOut => _doc.BendSimulation?.State?.NcValuesOut.GetNcBendValues(_bend.Order);

		public BendViewModel(IBendPositioning bend, ICombinedBendDescriptorInternal cbd, IUnitConverter unitConverter, IBendSelection bendSelection, IEditToolsSelection toolsSelection, IToolOperator toolOperator, BendSequenceListViewModel parentVm, ITranslator translator, IPnBndDoc doc, IScopedFactorio scopedFactorio)
		{
			_bend = bend;
			_cbd = cbd;
			_unitConverter = unitConverter;
			_bendSelection = bendSelection;
			_toolsSelection = toolsSelection;
			_toolOperator = toolOperator;
			_parentVm = parentVm;
			_translator = translator;
			_doc = doc;
			_scopedFactorio = scopedFactorio;
			_editToolsVm = scopedFactorio.Resolve<IEditToolsViewModel>();
			CmdSimGotoBend = new RelayCommand(SetCurrentBend);
			CmdCalcFinger = new RelayCommand(CalcFinger);
			CmdCalcTools = new RelayCommand(CalcTools);
			CmdValidateCollisions = new RelayCommand(ValidateCollisions);
			RecalcBackground(out var _);
			UpdateFingerStatus();
			UpdateLiftingAid();
			UpdateAcb();
		}

		private void CalcTools()
		{
		}

		private void CalcFinger()
		{
			_doc.CalculateFingers(_bend.Order.ToIEnumerable().ToList());
			_scopedFactorio.Resolve<IUndo3dService>().Save(_doc, _translator.Translate("Undo3d.FingerCalculationForBend", _bend.Order + 1));
		}

		private void ValidateCollisions()
		{
		}

		public void RefreshSelectionBinding()
		{
			bool flag = _bendSelection.IsSelected(_bend);
			if (_isSelected != flag)
			{
				_isSelected = flag;
				NotifyPropertyChanged("IsSelectedByBinding");
			}
			RecalcBackground(out var _);
		}

		public void RefreshAllProps()
		{
			NotifyPropertyChanged("HasTools");
			NotifyPropertyChanged("Collision");
			NotifyPropertyChanged("ToolSetupNo");
			NotifyPropertyChanged("AngleChange");
			NotifyPropertyChanged("UpperToolProfile");
			NotifyPropertyChanged("LowerToolProfile");
			NotifyPropertyChanged("Comment");
			NotifyPropertyChanged("FlipGeometry");
			NotifyPropertyChanged("FlipGeometry");
			NotifyPropertyChanged("Lcb");
			NotifyPropertyChanged("IsIncluded");
			NotifyPropertyChanged("ReleasePointUiUnits");
			UpdateFingerStatus();
			UpdateLiftingAid();
			UpdateAcb();
		}

		public WiCAM.Pn4000.BendModel.Base.Color? GetModelColor()
		{
			RecalcBackground(out var colorModel);
			return colorModel?.ToBendColor();
		}

		private void RecalcBackground(out System.Windows.Media.Color? colorModel)
		{
			System.Windows.Media.Color? color = null;
			bool flag = _bendSelection.CurrentBend == _bend;
			bool flag2 = _bendSelection.CurrentBendHovering == _bend;
			byte b = (byte)(flag ? 128u : 0u);
			bool flag3 = IsHovering || flag2;
			color = (_isSelected ? ((!flag3) ? new System.Windows.Media.Color?(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, 100, b)) : new System.Windows.Media.Color?(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, 150, b))) : (flag3 ? new System.Windows.Media.Color?(System.Windows.Media.Color.FromArgb(byte.MaxValue, 0, 200, b)) : ((!flag) ? null : new System.Windows.Media.Color?(System.Windows.Media.Color.FromArgb(byte.MaxValue, 128, 128, byte.MaxValue)))));
			System.Windows.Media.Color color2 = color ?? System.Windows.Media.Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			BorderCol = new SolidColorBrush(color2);
			System.Windows.Media.Color color3 = System.Windows.Media.Color.FromArgb(color2.A, c(color2.R), c(color2.G), c(color2.B));
			BackgroundCol = new SolidColorBrush(color3);
			colorModel = color;
			static byte c(byte val)
			{
				return (byte)(255f - (255f - (float)(int)val) * 0.3f);
			}
		}

		public void UpdateFingerStatus()
		{
			ICombinedBendDescriptorInternal cbd = _cbd;
			if (cbd == null)
			{
				return;
			}
			switch (cbd.FingerPositioningMode)
			{
			case FingerPositioningMode.Auto:
				switch (cbd.FingerStability)
				{
				case FingerStability.SemiStable:
					FingerStabilityColor = _brushFingerSemiStable;
					FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerSemiStableTt");
					break;
				case FingerStability.Stable:
					FingerStabilityColor = _brushFingerStable;
					FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerStableTt");
					break;
				case FingerStability.Unstable:
					FingerStabilityColor = _brushFingerUnstable;
					FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerUnStableTt");
					break;
				default:
					throw new NotImplementedException();
				}
				break;
			case FingerPositioningMode.User:
				FingerStabilityColor = _brushFingerUser;
				FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerUserTt");
				break;
			case FingerPositioningMode.None:
				FingerStabilityColor = _brushFingerNone;
				FingerStabilityTooltip = _translator.Translate("l_popup.PopupBendSequence.FingerNoneTt");
				break;
			default:
				throw new NotImplementedException();
			}
		}

		private void UpdateLiftingAid()
		{
			if (_cbd != null)
			{
				LiftingAidToolTip = CreateLiftingAidIcon().tooltip;
				NotifyPropertyChanged("LiftingAidToolTip");
				NotifyPropertyChanged("LiftingAidIcon");
				_doc.RecalculateSimulation();
			}
		}

		private (UIElement icon, string tooltip) CreateLiftingAidIcon()
		{
			double width = 15.0;
			double num = 5.0;
			double height = 10.0;
			double y2 = 0.0;
			double x2 = 0.0;
			string text = "";
			Grid canvas = new Grid();
			IBendMachineGeometry bendMachineGeometry = _doc.BendMachine?.Geometry;
			if (bendMachineGeometry != null)
			{
				if (bendMachineGeometry.LeftFrontLiftingAid != null)
				{
					text = text + "links vorne: " + LiftingStatus(_cbd.UseLeftFrontLiftingAid) + Environment.NewLine;
					x2 = width + num;
				}
				if (bendMachineGeometry.LeftBackLiftingAid != null)
				{
					text = text + "links hinten: " + LiftingStatus(_cbd.UseLeftBackLiftingAid) + Environment.NewLine;
					y2 = height;
					x2 = width + num;
				}
				if (bendMachineGeometry.RightFrontLiftingAid != null)
				{
					text = text + "rechts vorne: " + LiftingStatus(_cbd.UseRightFrontLiftingAid) + Environment.NewLine;
				}
				if (bendMachineGeometry.RightBackLiftingAid != null)
				{
					text = text + "rechts hinten: " + LiftingStatus(_cbd.UseRightBackLiftingAid) + Environment.NewLine;
					y2 = height;
				}
				text = text.Trim(Environment.NewLine.ToCharArray());
				if (bendMachineGeometry.LeftFrontLiftingAid != null)
				{
					AddPath(0.0, y2, _cbd.UseLeftFrontLiftingAid).MouseLeftButtonDown += ToggleLiftingAidLeftFront;
				}
				if (bendMachineGeometry.LeftBackLiftingAid != null)
				{
					AddPath(0.0, 0.0, _cbd.UseLeftBackLiftingAid).MouseLeftButtonDown += ToggleLiftingAidLeftBack;
				}
				if (bendMachineGeometry.RightFrontLiftingAid != null)
				{
					AddPath(x2, y2, _cbd.UseRightFrontLiftingAid).MouseLeftButtonDown += ToggleLiftingAidRightFront;
				}
				if (bendMachineGeometry.RightBackLiftingAid != null)
				{
					AddPath(x2, 0.0, _cbd.UseRightBackLiftingAid).MouseLeftButtonDown += ToggleLiftingAidRightBack;
				}
			}
			return (icon: canvas, tooltip: text);
			Path AddPath(double x, double y, LiftingAidEnum liftingAidStatus)
			{
				Path path = new Path();
				switch (liftingAidStatus)
				{
				case LiftingAidEnum.NoLiftingAid:
					path.Fill = Brushes.Red;
					break;
				case LiftingAidEnum.UseSupportOnly:
					path.Fill = Brushes.DodgerBlue;
					break;
				case LiftingAidEnum.UseActive:
					path.Fill = Brushes.LimeGreen;
					break;
				}
				path.Data = new RectangleGeometry(new Rect(new Point(x, y), new Size(width, height)));
				canvas.Children.Add(path);
				return path;
			}
		}

		public void UpdateAcb()
		{
			Dictionary<IAcbPunchPiece, AcbActivationResult> dictionary = _bend.AcbStatus.ToDictionary<KeyValuePair<IAcbPunchPiece, AcbActivationResult>, IAcbPunchPiece, AcbActivationResult>((KeyValuePair<IAcbPunchPiece, AcbActivationResult> x) => x.Key, (KeyValuePair<IAcbPunchPiece, AcbActivationResult> x) => x.Value);
			if (dictionary != null && !dictionary.SequenceEqual<KeyValuePair<IAcbPunchPiece, AcbActivationResult>>(_displayedAcb))
			{
				_displayedAcb = dictionary;
				AcbToolTip = CalculateAcbIcon().tooltip.Trim();
				NotifyPropertyChanged("AcbToolTip");
				NotifyPropertyChanged("AcbIcon");
			}
		}

		private (UIElement icon, string tooltip) CalculateAcbIcon()
		{
			object obj = _bend.Anchor?.Root.Acbs.OrderBy((IAcbPunchPiece x) => x.OffsetWorld.X).ToList();
			Dictionary<IAcbPunchPiece, AcbActivationResult> dictionary = _bend.AcbStatus.ToDictionary<KeyValuePair<IAcbPunchPiece, AcbActivationResult>, IAcbPunchPiece, AcbActivationResult>((KeyValuePair<IAcbPunchPiece, AcbActivationResult> x) => x.Key, (KeyValuePair<IAcbPunchPiece, AcbActivationResult> x) => x.Value);
			double num = 3.0;
			double width = 10.0;
			double height = 15.0;
			double num2 = 0.0;
			string tooltip = "";
			Grid canvas = new Grid();
			if (obj == null)
			{
				obj = new List<IAcbPunchPiece>();
			}
			foreach (IAcbPunchPiece acbTool in (List<IAcbPunchPiece>)obj)
			{
				AcbActivationResult valueOrDefault = dictionary.GetValueOrDefault(acbTool, AcbActivationResult.NoDisks);
				Path path = AddPath(num2, 0.0, valueOrDefault);
				path.MouseLeftButtonDown += delegate
				{
					ToggleAcb(acbTool);
				};
				path.MouseEnter += delegate
				{
					HoverAcb(acbTool, isHovering: true);
				};
				path.MouseLeave += delegate
				{
					HoverAcb(acbTool, isHovering: false);
				};
				num2 += width + num;
			}
			return (icon: canvas, tooltip: tooltip);
			Path AddPath(double x, double y, AcbActivationResult active)
			{
				Path path2 = new Path();
				SolidColorBrush solidColorBrush;
				if (active.HasFlag(AcbActivationResult.UserActivated))
				{
					solidColorBrush = Brushes.Green;
				}
				else
				{
					AcbActivationResult acbActivationResult = active;
					if (acbActivationResult.HasFlag(AcbActivationResult.UserDeactivated))
					{
						solidColorBrush = Brushes.Purple;
					}
					else
					{
						AcbActivationResult acbActivationResult2 = active;
						if (acbActivationResult2.HasFlag(AcbActivationResult.ProductAngleToSmall))
						{
							solidColorBrush = Brushes.Red;
						}
						else
						{
							AcbActivationResult acbActivationResult3 = active;
							solidColorBrush = (acbActivationResult3.HasFlag(AcbActivationResult.InvalidThickness) ? Brushes.Red : ((active != AcbActivationResult.Coverd) ? Brushes.Yellow : Brushes.Lime));
						}
					}
				}
				SolidColorBrush fill = solidColorBrush;
				tooltip = tooltip + AcbStatus(active) + Environment.NewLine;
				path2.Fill = fill;
				path2.Data = new RectangleGeometry(new Rect(new Point(x, y), new Size(width, height)));
				canvas.Children.Add(path2);
				return path2;
			}
		}

		private void ToggleLiftingAidLeftFront(object sender, MouseButtonEventArgs e)
		{
			LiftingAidLeftFront = (LiftingAidEnum)((int)(LiftingAidLeftFront + 1) % 3);
		}

		private void ToggleLiftingAidLeftBack(object sender, MouseButtonEventArgs e)
		{
			LiftingAidLeftBack = (LiftingAidEnum)((int)(LiftingAidLeftBack + 1) % 3);
		}

		private void ToggleLiftingAidRightFront(object sender, MouseButtonEventArgs e)
		{
			LiftingAidRightFront = (LiftingAidEnum)((int)(LiftingAidRightFront + 1) % 3);
		}

		private void ToggleLiftingAidRightBack(object sender, MouseButtonEventArgs e)
		{
			LiftingAidRightBack = (LiftingAidEnum)((int)(LiftingAidRightBack + 1) % 3);
		}

		private void ToggleAcb(IAcbPunchPiece acb)
		{
			AcbActivationResult valueOrDefault = _bend.AcbStatus.GetValueOrDefault(acb, AcbActivationResult.None);
			if (!valueOrDefault.HasFlag(AcbActivationResult.UserActivated))
			{
				valueOrDefault = ((!valueOrDefault.HasFlag(AcbActivationResult.UserDeactivated)) ? (valueOrDefault | AcbActivationResult.UserActivated) : (valueOrDefault ^ AcbActivationResult.UserDeactivated));
			}
			else
			{
				valueOrDefault ^= AcbActivationResult.UserActivated;
				valueOrDefault |= AcbActivationResult.UserDeactivated;
			}
			_bend.AcbStatus[acb] = valueOrDefault;
			_parentVm.RaiseRepaint();
			UpdateAcb();
		}

		private void HoverAcb(IAcbPunchPiece acb, bool isHovering)
		{
			if (isHovering)
			{
				hoveredAcb = acb;
			}
			else if (hoveredAcb == acb)
			{
				hoveredAcb = null;
			}
			_parentVm.RaiseRepaint();
		}

		private string LiftingStatus(LiftingAidEnum status)
		{
			ITranslator translator = _translator;
			string msgKey = default(string);
			switch (status)
			{
			case LiftingAidEnum.NoLiftingAid:
				msgKey = "l_enum.LiftingAidEnum.NoLiftingAid";
				break;
			case LiftingAidEnum.UseSupportOnly:
				msgKey = "l_enum.LiftingAidEnum.UseSupportOnly";
				break;
			case LiftingAidEnum.UseActive:
				msgKey = "l_enum.LiftingAidEnum.UseActive";
				break;
			default:
				ThrowSwitchExpressionException(status);
				break;
			}
			return translator.Translate(msgKey);
		}

        private void ThrowSwitchExpressionException(LiftingAidEnum status)
        {
            throw new NotImplementedException();
        }

        private string AcbStatus(AcbActivationResult status)
		{
			ITranslator translator = _translator;
			string msgKey;
			if (status.HasFlag(AcbActivationResult.UserActivated))
			{
				msgKey = "l_enum.AcbActivationResult.UserActivated";
			}
			else
			{
				AcbActivationResult acbActivationResult = status;
				if (acbActivationResult.HasFlag(AcbActivationResult.UserDeactivated))
				{
					msgKey = "l_enum.AcbActivationResult.UserDeactivated";
				}
				else
				{
					AcbActivationResult acbActivationResult2 = status;
					if (acbActivationResult2.HasFlag(AcbActivationResult.ProductAngleToSmall))
					{
						msgKey = "l_enum.AcbActivationResult.ProductAngleToSmall";
					}
					else
					{
						AcbActivationResult acbActivationResult3 = status;
						msgKey = (acbActivationResult3.HasFlag(AcbActivationResult.InvalidThickness) ? "l_enum.AcbActivationResult.InvalidThickness" : ((status != AcbActivationResult.Coverd) ? "l_enum.AcbActivationResult.Fallback" : "l_enum.AcbActivationResult.Coverd"));
					}
				}
			}
			return translator.Translate(msgKey);
		}

		public void SetCurrentBend()
		{
			if (_bendSelection.CurrentBend == _bend)
			{
				_bendSelection.RefreshCurrentBend();
			}
			else
			{
				_bendSelection.CurrentBend = _bend;
			}
		}
	}

	private readonly IBendSelection _bendSelection;

	private readonly IUnitConverter _unitConverter;

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IToolOperator _toolOperator;

	private readonly IScopedFactorio _scopedFactorio;

	private readonly ITranslator _translator;

	private readonly IPnBndDoc _doc;

	private readonly IGlobals _globals;

	private readonly IUndo3dService _undo3dService;

	private readonly IEditToolsViewModel _editToolsViewModel;

	private bool _disposedValue;

	public RadObservableCollection<ComboboxEntry<int>> ToolSetups { get; } = new RadObservableCollection<ComboboxEntry<int>>();

	public RadObservableCollection<ComboboxEntry<int?>> StepChangeModes { get; } = new RadObservableCollection<ComboboxEntry<int?>>();

	public RadObservableCollection<BendViewModel> Bends { get; } = new RadObservableCollection<BendViewModel>();

	public event Action OpenBendContextMenu;

	public event Action RepaintModels;

	public void RefreshAllProps()
	{
		EditToolsSelection_DataRefreshed();
	}

	public BendSequenceListViewModel(IBendSelection bendSelection, IUnitConverter unitConverter, IEditToolsSelection toolsSelection, IToolOperator toolOperator, IScopedFactorio scopedFactorio, ITranslator translator, IPnBndDoc doc, IGlobals globals, IEditToolsViewModel editToolsViewModel, IUndo3dService undo3dService)
	{
		_bendSelection = bendSelection;
		_unitConverter = unitConverter;
		_toolsSelection = toolsSelection;
		_toolOperator = toolOperator;
		_scopedFactorio = scopedFactorio;
		_translator = translator;
		_doc = doc;
		_globals = globals;
		_undo3dService = undo3dService;
		_bendSelection.DataChanged += BendSelection_DataChanged;
		_bendSelection.SelectionChanged += BendSelection_SelectionChanged;
		_bendSelection.CurrentBendChanged += BendSelection_CurrentBendChanged;
		_bendSelection.CurrentBendHoveringChanged += BendSelection_CurrentBendChanged;
		_toolsSelection.DataRefreshed += EditToolsSelection_DataRefreshed;
		_doc.FingerChanged += Doc_FingerChanged;
		editToolsViewModel.ToolSetups.CollectionChanged += ToolSetups_CollectionChanged;
		_doc.BendMachineChanged += Doc_BendMachineChanged;
		BendSelection_DataChanged();
		Doc_BendMachineChanged();
	}

	private void Doc_BendMachineChanged()
	{
		StepChangeModes.SuspendNotifications();
		StepChangeModes.Clear();
		int? defaultVal = _doc.BendMachine?.PressBrakeData.StepChangeDefault;
		List<int> list = _doc.BendMachine?.PostProcessor?.GetAllStepChangeModes().ToList();
		if (list != null && list.Count > 0)
		{
			foreach (int item in list)
			{
				StepChangeModes.Add(new ComboboxEntry<int?>(_translator.Translate($"l_enum.PpSpecific.{_doc.BendMachine.PostProcessor.TranslationBaseKey}.StepChange.{item}"), item));
			}
			ComboboxEntry<int?> comboboxEntry = StepChangeModes.FirstOrDefault((ComboboxEntry<int?> x) => x.Value == defaultVal);
			StepChangeModes.Insert(0, new ComboboxEntry<int?>(_translator.Translate("l_enum.PpSpecific.Default.StepChange.DefaultPrefix") + " " + comboboxEntry?.Desc, null));
		}
		else
		{
			StepChangeModes.Add(new ComboboxEntry<int?>("-", null));
		}
		StepChangeModes.ResumeNotifications();
	}

	~BendSequenceListViewModel()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_bendSelection.DataChanged -= BendSelection_DataChanged;
				_bendSelection.SelectionChanged -= BendSelection_SelectionChanged;
				_bendSelection.CurrentBendChanged -= BendSelection_CurrentBendChanged;
				_bendSelection.CurrentBendHoveringChanged -= BendSelection_CurrentBendChanged;
				_toolsSelection.DataRefreshed -= EditToolsSelection_DataRefreshed;
			}
			_disposedValue = true;
		}
	}

	private void ToolSetups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		BendSelection_DataChanged();
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		foreach (BendViewModel bend in Bends)
		{
			WiCAM.Pn4000.BendModel.Base.Color? modelColor = bend.GetModelColor();
			if (modelColor.HasValue)
			{
				foreach (IBendParameters item in bend.Cbd.Enumerable.Select((IBendDescriptor bz) => bz.BendParams))
				{
					paintTool.SetModelEdgeColor(item.BendFaceGroupModel, modelColor, 5f);
				}
			}
			bend.UpdateAcb();
		}
		Dictionary<IAcbPunchPiece, AcbActivationResult> dictionary = _bendSelection.CurrentBend?.AcbStatus;
		BendViewModel bendViewModel = Bends.FirstOrDefault((BendViewModel x) => x.hoveredAcb != null);
		IAcbPunchPiece acbPunchPiece = null;
		if (bendViewModel?.Bend.Anchor == _bendSelection.CurrentBend?.Anchor)
		{
			acbPunchPiece = bendViewModel?.hoveredAcb;
		}
		if (dictionary == null)
		{
			return;
		}
		IToolCluster toolCluster = _bendSelection.CurrentBend?.Anchor;
		if (toolCluster == null)
		{
			return;
		}
		foreach (IAcbPunchPiece acb in toolCluster.Root.Acbs)
		{
			if (!_toolsSelection.PiecesToModel.TryGetValue(acb, out Model value))
			{
				continue;
			}
			WiCAM.Pn4000.BendModel.Base.Color color2;
			if (dictionary.ContainsKey(acb))
			{
				AcbActivationResult acbActivationResult = dictionary[acb];
				AcbActivationResult acbActivationResult2 = acbActivationResult;
				WiCAM.Pn4000.BendModel.Base.Color color;
				if (acbActivationResult2.HasFlag(AcbActivationResult.UserActivated))
				{
					color = WiCAM.Pn4000.BendModel.Base.Color.Green;
				}
				else
				{
					AcbActivationResult acbActivationResult3 = acbActivationResult;
					if (acbActivationResult3.HasFlag(AcbActivationResult.UserDeactivated))
					{
						color = WiCAM.Pn4000.BendModel.Base.Color.Purple;
					}
					else
					{
						AcbActivationResult acbActivationResult4 = acbActivationResult;
						if (acbActivationResult4.HasFlag(AcbActivationResult.ProductAngleToSmall))
						{
							color = WiCAM.Pn4000.BendModel.Base.Color.Red;
						}
						else
						{
							AcbActivationResult acbActivationResult5 = acbActivationResult;
							color = (acbActivationResult5.HasFlag(AcbActivationResult.InvalidThickness) ? WiCAM.Pn4000.BendModel.Base.Color.Red : ((acbActivationResult != AcbActivationResult.Coverd) ? WiCAM.Pn4000.BendModel.Base.Color.Yellow : WiCAM.Pn4000.BendModel.Base.Color.Lime));
						}
					}
				}
				color2 = color;
			}
			else
			{
				color2 = WiCAM.Pn4000.BendModel.Base.Color.Yellow;
			}
			WiCAM.Pn4000.BendModel.Base.Color value2 = color2;
			paintTool.SetModelFaceColor(value, value2);
			if (acbPunchPiece == acb)
			{
				paintTool.SetModelEdgeColor(value, WiCAM.Pn4000.BendModel.Base.Color.Yellow, 5f);
			}
		}
	}

	public void MoveBend(DropIndicationDetails details, DragEventArgs e)
	{
		List<ICombinedBendDescriptorInternal> list = Bends.Select((BendViewModel x) => x.Cbd).ToList();
		ICombinedBendDescriptorInternal combinedBendDescriptorInternal = (details.CurrentDraggedItem as BendViewModel)?.Cbd;
		if (combinedBendDescriptorInternal == null)
		{
			return;
		}
		int num = list.IndexOf(combinedBendDescriptorInternal);
		list.Remove(combinedBendDescriptorInternal);
		list.Insert(details.DropIndex, combinedBendDescriptorInternal);
		int num2 = list.IndexOf(combinedBendDescriptorInternal);
		if (list.Count != _doc.CombinedBendDescriptors.Count)
		{
			return;
		}
		HashSet<ICombinedBendDescriptorInternal> hashSet = _doc.CombinedBendDescriptors.ToHashSet();
		foreach (ICombinedBendDescriptorInternal item in list)
		{
			if (!hashSet.Remove(item))
			{
				return;
			}
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != _doc.CombinedBendDescriptors[i])
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			_doc.ApplyBendOrder(list);
			BendSelection_DataChanged();
			_doc.FreezeCombinedBendDescriptors = true;
			_undo3dService.Save(_doc, _translator.Translate("Undo3d.MoveBendOrder", num + 1, num2 + 1));
		}
	}

	private void EditToolsSelection_DataRefreshed()
	{
		foreach (BendViewModel bend in Bends)
		{
			bend.RefreshAllProps();
		}
		if (_bendSelection.CurrentBend?.Anchor?.Root != _toolsSelection.CurrentSetups)
		{
			_bendSelection.CurrentBend = null;
		}
	}

	private void BendSelection_DataChanged()
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			ToolSetups.SuspendNotifications();
			ToolSetups.Clear();
			ToolSetups.Add(new ComboboxEntry<int>("-", -1));
			foreach (IToolSetups item in _toolsSelection?.ToolsAndBends?.ToolSetups ?? new List<IToolSetups>())
			{
				ToolSetups.Add(new ComboboxEntry<int>($"{item.Number} {item.Desc}", item.Number));
			}
			ToolSetups.ResumeNotifications();
			Bends.Clear();
			IReadOnlyList<IBendPositioning> readOnlyList = _bendSelection.ToolsAndBends?.BendPositions;
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				Bends.AddRange(readOnlyList.Select((IBendPositioning x) => new BendViewModel(x, _bendSelection.GetCbd(x), _unitConverter, _bendSelection, _toolsSelection, _toolOperator, this, _translator, _doc, _scopedFactorio)));
			}
		});
	}

	private void BendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
		foreach (BendViewModel bend in Bends)
		{
			bend.RefreshSelectionBinding();
		}
	}

	private void BendSelection_SelectionChanged()
	{
		foreach (BendViewModel bend in Bends)
		{
			bend.RefreshSelectionBinding();
		}
	}

	private void Doc_FingerChanged(IPnBndDoc doc)
	{
		foreach (BendViewModel bend in Bends)
		{
			bend.UpdateFingerStatus();
		}
	}

	public void OpenContextMenu(BendViewModel bend)
	{
		if (!_bendSelection.IsSelected(bend.Bend))
		{
			_bendSelection.UnselectAll();
			_bendSelection.SetSelection(bend.Bend, isSelected: true);
		}
		this.OpenBendContextMenu?.Invoke();
	}

	public void RaiseRepaint()
	{
		this.RepaintModels?.Invoke();
	}

	public void ChangeRadius(List<BendViewModel> cbd, double newRadius)
	{
		List<int> bendIndex = cbd.Select((BendViewModel c) => _doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(c.Cbd)).ToList();
		_doc.ChangeManualRadius(bendIndex, newRadius);
		Unfold.WriteModifiedModelToObj(_doc as IDoc3d, _globals);
		Unfold.WriteModifiedModelToGlb(_doc as IDoc3d, _globals);
	}

	public void ChangeDeduction(List<BendViewModel> cbd, double newDeduction)
	{
		List<int> bendIndex = cbd.Select((BendViewModel c) => _doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(c.Cbd)).ToList();
		_doc.ChangeManualBendDeduction(bendIndex, newDeduction);
		Unfold.WriteModifiedModelToObj(_doc as IDoc3d, _globals);
		Unfold.WriteModifiedModelToGlb(_doc as IDoc3d, _globals);
	}
}

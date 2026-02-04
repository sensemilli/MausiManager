using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common.Wpf.UnitConversion;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Unfold.UI.Model;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

public class ContextMenuViewModel : ScreenControlBaseViewModel, ISubViewModel
{
	[CompilerGenerated]
	private sealed class _003CSelectedBends_003Ed__148 : IEnumerable<ICombinedBendDescriptorInternal>, IEnumerable, IEnumerator<ICombinedBendDescriptorInternal>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private ICombinedBendDescriptorInternal _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public ContextMenuViewModel _003C_003E4__this;

		private IEnumerator<ICombinedBendDescriptorInternal> _003C_003E7__wrap1;

		ICombinedBendDescriptorInternal IEnumerator<ICombinedBendDescriptorInternal>.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSelectedBends_003Ed__148(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = null;
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = this._003C_003E1__state;
				ContextMenuViewModel contextMenuViewModel = this._003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					if (contextMenuViewModel.ChangeForAllBends)
					{
						this._003C_003E7__wrap1 = contextMenuViewModel._doc.CombinedBendDescriptors.GetEnumerator();
						this._003C_003E1__state = -3;
						goto IL_007c;
					}
					this._003C_003E2__current = contextMenuViewModel.SelectedCommonBend;
					this._003C_003E1__state = 2;
					return true;
				case 1:
					this._003C_003E1__state = -3;
					goto IL_007c;
				case 2:
					{
						this._003C_003E1__state = -1;
						break;
					}
					IL_007c:
					if (this._003C_003E7__wrap1.MoveNext())
					{
						this._003C_003E2__current = this._003C_003E7__wrap1.Current;
						this._003C_003E1__state = 1;
						return true;
					}
					this._003C_003Em__Finally1();
					this._003C_003E7__wrap1 = null;
					break;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			this._003C_003E1__state = -1;
			if (this._003C_003E7__wrap1 != null)
			{
				this._003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<ICombinedBendDescriptorInternal> IEnumerable<ICombinedBendDescriptorInternal>.GetEnumerator()
		{
			_003CSelectedBends_003Ed__148 result;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CSelectedBends_003Ed__148(0)
				{
					_003C_003E4__this = this._003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ICombinedBendDescriptorInternal>)this).GetEnumerator();
		}
	}

	private List<object> _items;

	private readonly Action _showStepBendContextMenue;

	private readonly Color _highlightColor;

	private HashSet<FaceHalfEdge> _selectedEdges;

	private ICombinedBendDescriptorInternal _selectedCommonBend;

	private ICommand _changeRadius;

	private ICommand _changeDeduction;

	private ICommand _createStepBend;

	private ICommand _editStepBend;

	private Visibility _visible;

	private Visibility _editToolsVisibility = Visibility.Collapsed;

	private Visibility _splitBendVisibility = Visibility.Collapsed;

	private Visibility _combineBendsVisibility = Visibility.Collapsed;

	private Visibility _stepBendVisibility = Visibility.Collapsed;

	private Visibility _selectSetupVisible = Visibility.Collapsed;

	private Visibility _selectFrontLiftingAid = Visibility.Collapsed;

	private Visibility _selectBackLiftingAid = Visibility.Collapsed;

	private Visibility _selectAngleMeasurement = Visibility.Collapsed;

	private readonly ContextMenuModel _model;

	private readonly global::WiCAM.Pn4000.BendModel.Model _currentRenderModel;

	private readonly ITranslator _translator;

	private readonly IPnPathService _pnPathService;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IDoc3d _doc;

	private readonly IGlobals _globals;

	private readonly Screen3D _screen3d;

	private readonly IMainWindowBlock _mainWindowBlock;

	private string _radius;

	private readonly InchConversion _radiusDouble;

	private string _deduction;

	private readonly InchConversion _deductionDouble;

	private ISubViewModel _subViewModel;

	private double _oldLeft;

	private double _oldTop;

	private double _oldWidth;

	private double _oldHeight;

	private const double _editToolsWidth = 470.0;

	private const double _editToolsHeight = 700.0;

	private const double _splitBendWidth = 470.0;

	private const double _splitBendHeight = 700.0;

	private bool _changeForAllBends;

	private string _bendNumbersDescription;

	private bool IsStepBend => (from x in this._doc.CombinedBendDescriptors.SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable)
		where x.BendParams.EntryFaceGroup.ID == this._selectedCommonBend.Enumerable.FirstOrDefault()?.BendParams.EntryFaceGroup.ID
		select x).Any((IBendDescriptor x) => x.Type == BendingType.StepBend);

	public List<object> Items
	{
		get
		{
			return this._items;
		}
		set
		{
			this._items = value;
			base.NotifyPropertyChanged("Items");
		}
	}

	public ICombinedBendDescriptorInternal SelectedCommonBend
	{
		get
		{
			return this._selectedCommonBend;
		}
		set
		{
			this._selectedCommonBend = value;
			base.NotifyPropertyChanged("SelectedCommonBend");
		}
	}

	public ICommand ChangeRadiusCommand => this._changeRadius ?? (this._changeRadius = new RelayCommand(ChangeRadius));

	public ICommand ChangeDeductionCommand => this._changeDeduction ?? (this._changeDeduction = new RelayCommand(ChangeDeduction));

	public ICommand CreateStepBendCommand => this._createStepBend ?? (this._createStepBend = new RelayCommand(CreateStepBend));

	public ICommand EditStepBendCommand => this._editStepBend ?? (this._editStepBend = new RelayCommand(EditStepBend));

	public Visibility Visible
	{
		get
		{
			return this._visible;
		}
		set
		{
			if (this._visible != value)
			{
				this._visible = value;
				if (this.SelectedCommonBend != null)
				{
					this.Radius = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalRadius).Converted;
					this.Deduction = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalBendDeduction).Converted;
				}
				base.NotifyPropertyChanged("Visible");
			}
		}
	}

	public Visibility HiddenIfMultipleBends
	{
		get
		{
			if (!this.ChangeForAllBends)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility EditToolsVisibility
	{
		get
		{
			if (this._editToolsVisibility == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._editToolsVisibility != value)
			{
				this._editToolsVisibility = value;
				base.NotifyPropertyChanged("EditToolsVisibility");
			}
		}
	}

	public Visibility SplitBendVisibility
	{
		get
		{
			if (this._splitBendVisibility == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._splitBendVisibility != value)
			{
				this._splitBendVisibility = value;
				base.NotifyPropertyChanged("SplitBendVisibility");
			}
		}
	}

	public Visibility CombineBendsVisibility
	{
		get
		{
			if (this._combineBendsVisibility == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._combineBendsVisibility != value)
			{
				this._combineBendsVisibility = value;
				base.NotifyPropertyChanged("CombineBendsVisibility");
			}
		}
	}

	public Visibility StepBendVisibility
	{
		get
		{
			if (this._stepBendVisibility == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._stepBendVisibility != value)
			{
				this._stepBendVisibility = value;
				base.NotifyPropertyChanged("StepBendVisibility");
			}
		}
	}

	public Visibility SelectSetupVisible
	{
		get
		{
			return this._selectSetupVisible;
		}
		set
		{
			if (this._selectSetupVisible != value)
			{
				this._selectSetupVisible = value;
				base.NotifyPropertyChanged("SelectSetupVisible");
			}
		}
	}

	public Visibility SelectFrontLiftingAid
	{
		get
		{
			if (this._selectFrontLiftingAid == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._selectFrontLiftingAid != value)
			{
				this._selectFrontLiftingAid = value;
				base.NotifyPropertyChanged("SelectFrontLiftingAid");
			}
		}
	}

	public Visibility SelectBackLiftingAid
	{
		get
		{
			if (this._selectBackLiftingAid == Visibility.Visible && this.HiddenIfMultipleBends == Visibility.Visible)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
		set
		{
			if (this._selectBackLiftingAid != value)
			{
				this._selectBackLiftingAid = value;
				base.NotifyPropertyChanged("SelectBackLiftingAid");
			}
		}
	}

	public Visibility SelectAngleMeasurement
	{
		get
		{
			return this._selectAngleMeasurement;
		}
		set
		{
			if (this._selectAngleMeasurement != value)
			{
				this._selectAngleMeasurement = value;
				base.NotifyPropertyChanged("SelectAngleMeasurement");
			}
		}
	}

	public Visibility CreateStepBendVisibility
	{
		get
		{
			if (!this.IsStepBend && !this.ChangeForAllBends)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility EditStepBendVisibility
	{
		get
		{
			if (this.IsStepBend && !this.ChangeForAllBends)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public string Radius
	{
		get
		{
			return this._radius;
		}
		set
		{
			if (this.SelectedCommonBend != null)
			{
				this._radius = value;
				if (double.TryParse(this._radius, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					this._radiusDouble.Converted = result.ToString(CultureInfo.InvariantCulture);
				}
				base.NotifyPropertyChanged("Radius");
			}
		}
	}

	public string Deduction
	{
		get
		{
			return this._deduction;
		}
		set
		{
			if (this.SelectedCommonBend != null)
			{
				this._deduction = value;
				if (double.TryParse(this._deduction, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					this._deductionDouble.Converted = result.ToString(CultureInfo.InvariantCulture);
				}
				base.NotifyPropertyChanged("Deduction");
			}
		}
	}

	public bool? PartTurned180Degree
	{
		get
		{
			return this.SelectedBends().Select((Func<ICombinedBendDescriptorInternal, bool?>)((ICombinedBendDescriptorInternal x) => x.MachinePartInsertionDirection == MachinePartInsertionDirection.PartCentroidBehindMachine)).DefaultIfEmpty(null)
				.Aggregate((bool? x, bool? y) => (x != y) ? ((bool?)null) : x);
		}
		set
		{
			if (!value.HasValue || value == this.PartTurned180Degree)
			{
				return;
			}
			foreach (ICombinedBendDescriptorInternal item in this.SelectedBends())
			{
				this._doc.SetPartInsertionDirectionForBend(this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(item), value.Value ? MachinePartInsertionDirection.PartCentroidBehindMachine : MachinePartInsertionDirection.PartCentroidInFrontOfMachine);
			}
			base.NotifyPropertyChanged("PartTurned180Degree");
		}
	}

	public string BendNumbersDescription
	{
		get
		{
			return this._bendNumbersDescription;
		}
		set
		{
			if (this._bendNumbersDescription != value)
			{
				this._bendNumbersDescription = value;
				base.NotifyPropertyChanged("BendNumbersDescription");
			}
		}
	}

	public bool ChangeForAllBends
	{
		get
		{
			return this._changeForAllBends;
		}
		set
		{
			this._changeForAllBends = value;
			base.NotifyPropertyChanged("ChangeForAllBends");
			base.NotifyPropertyChanged("PartTurned180Degree");
			base.NotifyPropertyChanged("UseAngleMeasurement");
			base.NotifyPropertyChanged("EditToolsVisibility");
			base.NotifyPropertyChanged("SplitBendVisibility");
			base.NotifyPropertyChanged("CombineBendsVisibility");
			base.NotifyPropertyChanged("StepBendVisibility");
			base.NotifyPropertyChanged("SelectFrontLiftingAid");
			base.NotifyPropertyChanged("SelectBackLiftingAid");
			base.NotifyPropertyChanged("CreateStepBendVisibility");
			base.NotifyPropertyChanged("EditStepBendVisibility");
			base.NotifyPropertyChanged("HiddenIfMultipleBends");
			if (value)
			{
				this.BendNumbersDescription = this._translator.Translate("l_popup.EditBendContextMenu.ForAllBends");
				double? num = this.SelectedBends().Select((Func<ICombinedBendDescriptorInternal, double?>)((ICombinedBendDescriptorInternal x) => Math.Round(x[0].BendParams.FinalRadius, 4))).DefaultIfEmpty(null)
					.Aggregate((double? x, double? y) => (x != y) ? ((double?)null) : x);
				if (num.HasValue)
				{
					this.Radius = new InchConversion(num.Value).Converted;
				}
				else
				{
					this.Radius = "";
				}
				double? num2 = this.SelectedBends().Select((Func<ICombinedBendDescriptorInternal, double?>)((ICombinedBendDescriptorInternal x) => Math.Round(x[0].BendParams.FinalBendDeduction, 4))).DefaultIfEmpty(null)
					.Aggregate((double? x, double? y) => (x != y) ? ((double?)null) : x);
				if (num2.HasValue)
				{
					this.Deduction = new InchConversion(num2.Value).Converted;
				}
				else
				{
					this.Deduction = "";
				}
			}
			else if (this.SelectedCommonBend != null)
			{
				this.BendNumbersDescription = this._translator.Translate("l_popup.EditBendContextMenu.OnlyOneBend", this.SelectedCommonBend.Order + 1);
				this.Radius = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalRadius).Converted;
				this.Deduction = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalBendDeduction).Converted;
			}
			else
			{
				this.BendNumbersDescription = "";
				this.Radius = "";
				this.Deduction = "";
			}
		}
	}

	public ISubViewModel SubMenuViewModel
	{
		get
		{
			return this._subViewModel;
		}
		set
		{
			if (this._subViewModel != value)
			{
				if (this._subViewModel != null)
				{
					this._subViewModel.Close();
				}
				this._subViewModel = value;
				if (this._subViewModel != null)
				{
					this._subViewModel.Closed += SubViewModelClosed;
				}
				base.NotifyPropertyChanged("SubMenuViewModel");
			}
		}
	}

	public List<EnumValueName<LiftingAidEnum>> LiftingAidOptions { get; set; }

	public LiftingAidEnum ActualLeftFrontLiftingAid
	{
		get
		{
			return this.SelectedCommonBend.UseLeftFrontLiftingAid;
		}
		set
		{
			this.SelectedCommonBend.UseLeftFrontLiftingAid = value;
			base.NotifyPropertyChanged("ActualLeftFrontLiftingAid");
			if (!this.SelectedCommonBend.LeftFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				this.SelectedCommonBend.LeftFrontLiftingAidHorizontalCoordinar = 0.0;
			}
			this._doc.BendSimulation?.UpdateLiftingAidPos(this.SelectedCommonBend.LeftFrontLiftingAidHorizontalCoordinar.Value, AxisType.FrontLiftingAidLeftHorizontal, this._doc.BendMachine.Geometry.LeftFrontLiftingAid, this.SelectedCommonBend);
		}
	}

	public LiftingAidEnum ActualRightFrontLiftingAid
	{
		get
		{
			return this.SelectedCommonBend.UseRightFrontLiftingAid;
		}
		set
		{
			this.SelectedCommonBend.UseRightFrontLiftingAid = value;
			base.NotifyPropertyChanged("ActualRightFrontLiftingAid");
			if (!this.SelectedCommonBend.RightFrontLiftingAidHorizontalCoordinar.HasValue)
			{
				this.SelectedCommonBend.RightFrontLiftingAidHorizontalCoordinar = 0.0;
			}
			this._doc.BendSimulation?.UpdateLiftingAidPos(this.SelectedCommonBend.RightFrontLiftingAidHorizontalCoordinar.Value, AxisType.FrontLiftingAidRightHorizontal, this._doc.BendMachine.Geometry.RightFrontLiftingAid, this.SelectedCommonBend);
		}
	}

	public LiftingAidEnum ActualLeftBackLiftingAid
	{
		get
		{
			return this.SelectedCommonBend.UseLeftBackLiftingAid;
		}
		set
		{
			this.SelectedCommonBend.UseLeftBackLiftingAid = value;
			base.NotifyPropertyChanged("ActualLeftBackLiftingAid");
			if (!this.SelectedCommonBend.LeftBackLiftingAidHorizontalCoordinar.HasValue)
			{
				this.SelectedCommonBend.LeftBackLiftingAidHorizontalCoordinar = 0.0;
			}
			this._doc.BendSimulation?.UpdateLiftingAidPos(this.SelectedCommonBend.LeftBackLiftingAidHorizontalCoordinar.Value, AxisType.BackLiftingAidLeftHorizontal, this._doc.BendMachine.Geometry.LeftBackLiftingAid, this.SelectedCommonBend);
		}
	}

	public LiftingAidEnum ActualRightBackLiftingAid
	{
		get
		{
			return this.SelectedCommonBend.UseRightBackLiftingAid;
		}
		set
		{
			this.SelectedCommonBend.UseRightBackLiftingAid = value;
			base.NotifyPropertyChanged("ActualRightBackLiftingAid");
			if (!this.SelectedCommonBend.RightBackLiftingAidHorizontalCoordinar.HasValue)
			{
				this.SelectedCommonBend.RightBackLiftingAidHorizontalCoordinar = 0.0;
			}
			this._doc.BendSimulation?.UpdateLiftingAidPos(this.SelectedCommonBend.RightBackLiftingAidHorizontalCoordinar.Value, AxisType.BackLiftingAidRightHorizontal, this._doc.BendMachine.Geometry.RightBackLiftingAid, this.SelectedCommonBend);
		}
	}

	public bool? UseAngleMeasurement
	{
		get
		{
			return this.SelectedBends().Select((Func<ICombinedBendDescriptorInternal, bool?>)((ICombinedBendDescriptorInternal x) => x.UseAngleMeasurement)).DefaultIfEmpty(null)
				.Aggregate((bool? x, bool? y) => (x != y) ? ((bool?)null) : x);
		}
		set
		{
			if (!value.HasValue || value == this.UseAngleMeasurement)
			{
				return;
			}
			foreach (ICombinedBendDescriptorInternal item in this.SelectedBends())
			{
				item.ActivateAndAutoPositionAngleMeasurementSystem(value.Value, recalcSim: false);
			}
			this._doc.RecalculateSimulation();
			base.NotifyPropertyChanged("UseAngleMeasurement");
		}
	}

	public string Comment
	{
		get
		{
			return this.SelectedCommonBend.Comment;
		}
		set
		{
			if (value != this.SelectedCommonBend.Comment)
			{
				this.SelectedCommonBend.Comment = value;
			}
			base.NotifyPropertyChanged("Comment");
		}
	}

	public double? ReleasePointUser
	{
		get
		{
			return this.SelectedCommonBend.ReleasePointUser;
		}
		set
		{
			this.SelectedCommonBend.ReleasePointUser = value;
		}
	}

	public event Action<ISubViewModel, Triangle, global::WiCAM.Pn4000.BendModel.Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action RequestRepaint;

	public ContextMenuViewModel(ContextMenuModel model, Screen3D screen3d, IGlobals globals, IDoc3d doc, IMainWindowBlock mainBlock, global::WiCAM.Pn4000.BendModel.Model bendModel, Vector3d anchorPoint, Color highlightColor, Action showStepBendContextMenue, global::WiCAM.Pn4000.BendModel.Model currentRenderModel, ITranslator translator, IPnPathService pnPathService, IShortcutSettingsCommon shortcutSettingsCommon)
		: base(screen3d, bendModel, anchorPoint)
	{
		this._showStepBendContextMenue = showStepBendContextMenue;
		this._currentRenderModel = currentRenderModel;
		this._translator = translator;
		this._pnPathService = pnPathService;
		this._shortcutSettingsCommon = shortcutSettingsCommon;
		this._highlightColor = highlightColor;
		this._screen3d = screen3d;
		this._globals = globals;
		this._mainWindowBlock = mainBlock;
		this._doc = doc;
		this._model = model;
		this.LiftingAidOptions = EnumValueNameBase.GetValues<LiftingAidEnum>(this._translator).ToList();
		this.CalculateHighlightedEdges();
		this.SelectedCommonBend = model.SelectedBend;
		this.Items = this.GetItems();
		base.SetOpacityMin(this._globals.ConfigProvider.InjectOrCreate<GeneralUserSettingsConfig>().DialogOpacity);
		this._radiusDouble = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalRadius);
		this._deductionDouble = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalBendDeduction);
		this.Radius = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalRadius).Converted;
		this.Deduction = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalBendDeduction).Converted;
		this.OnViewChanged();
		this.ChangeForAllBends = false;
	}

	private List<object> GetItems()
	{
		string text = this._pnPathService.PNDRIVE + "\\u\\pn\\pixmap\\32\\";
		bool num = this.SelectedCommonBend.IsSplitableBend();
		bool flag = this._doc.CombinedBendDescriptors.Any((ICombinedBendDescriptorInternal c) => c != this.SelectedCommonBend && ((base.Model == this._doc.UnfoldModel3D && c.IsCompatibleBendUnfoldModel(this.SelectedCommonBend)) || (base.Model == this._doc.BendModel3D && c.IsCompatibleBendBendModel(this.SelectedCommonBend))));
		List<object> list = new List<object>();
		if (this._doc.BendMachineConfig != null && this._doc.CombinedBendDescriptors.Count > 0)
		{
			list.Add(new ContextMenuItemViewModel(buttonContent: this._translator.Translate("l_popup.EditBendContextMenu.SelectTools"), command: EditTools2D, imagePath: text + "BNDTOOLS.png"));
		}
		if (num || flag)
		{
			list.Add(new ContextMenuItemViewModel(buttonContent: this._translator.Translate("l_popup.EditBendContextMenu.SplitCommonBend"), command: SplitOrCombineBends, imagePath: ""));
		}
		if (this.SelectedCommonBend.SplitBendCount > 0)
		{
			foreach (ICombinedBendDescriptorInternal cbf in from x in this._doc.CombinedBendDescriptors
				where x.Enumerable.Intersect(this.SelectedCommonBend.Enumerable).Any()
				orderby x.SplitBendOrder
				select x)
			{
				bool flag2 = cbf.SplitBendOrder >= cbf.SplitBendCount;
				string value = "";
				if (Math.Sign(cbf[0].BendParams.AngleSign) < 0)
				{
					value = "-";
				}
				string value2 = this._translator.Translate("l_popup.EditBendContextMenu.Bend");
				list.Add(new ContextMenuItemBendViewModel((!flag2) ? ((Action<double>)delegate(double x)
				{
					this.ChangeBendAngle(cbf, x);
				}) : null, this.CanMergeBendAngle(cbf) ? ((Action)delegate
				{
					this.MergeBendAngle(cbf);
				}) : null, delegate
				{
					this.SplitBendAngle(cbf);
				}, "", $"{value2} {cbf.SplitBendOrder + 1}/{cbf.SplitBendCount + 1}: {value}{cbf.StartProductAngleAbs * 180.0 / Math.PI:0.##}° - {value}", Math.Round(Math.Abs(cbf.StopProductAngleAbs * 180.0 / Math.PI), 2), "°"));
			}
		}
		else
		{
			list.Add(new ContextMenuItemViewModel(buttonContent: this._translator.Translate("l_popup.EditBendContextMenu.SplitBend"), command: delegate
			{
				this.SplitBendAngle(this.SelectedCommonBend);
			}, imagePath: ""));
		}
		if (this._doc.MachineFullyLoaded && this._doc.BendMachine.PressBrakeData.MeasurementAngle == 1)
		{
			this.SelectAngleMeasurement = Visibility.Visible;
		}
		if (this._doc.MachineFullyLoaded && this._doc.BendMachine.PressBrakeData.FrontLiftingAid == 1)
		{
			this.SelectFrontLiftingAid = Visibility.Visible;
		}
		if (this._doc.MachineFullyLoaded && this._doc.BendMachine.PressBrakeData.BackLiftingAid == 1)
		{
			this.SelectBackLiftingAid = Visibility.Visible;
		}
		return list;
	}

	private void RepaintModel()
	{
		this.RequestRepaint?.Invoke();
	}

	private void CalculateHighlightedEdges()
	{
		this._selectedEdges = new HashSet<FaceHalfEdge>((from bz in this._model.SelectedBend.Enumerable
			select bz.BendParams.UnfoldFaceGroup into fg
			select fg.ParentGroup ?? fg).Distinct().SelectMany((FaceGroup fg) => fg.GetAllFaces().SelectMany((Face f) => f.GetAllEdges())));
	}

	private void ChangeBendAngle(ICombinedBendDescriptorInternal cbf, double angleStop)
	{
		if (cbf.SplitBendCount > 0 && cbf.SplitBendOrder < cbf.SplitBendCount)
		{
			double splitValue = 1.0 - Math.Abs(angleStop / 180.0 * Math.PI) / cbf[0].BendParams.AngleAbs;
			this._doc.ChangeSplitValue(this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf), splitValue);
		}
		this.Items = this.GetItems();
	}

	private void MergeBendAngle(ICombinedBendDescriptorInternal cbf)
	{
		if (cbf.SplitBendCount > 0)
		{
			List<int> list = new List<int> { this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf) };
			list.AddRange(cbf.SplitPredecessors.Select((ICombinedBendDescriptor x) => this._doc.CombinedBendDescriptors.IndexOf(x)));
			this._doc.MergeSplitBends(list);
			this.Items = this.GetItems();
		}
	}

	private bool CanMergeBendAngle(ICombinedBendDescriptorInternal cbf)
	{
		if (cbf.SplitBendCount > 0 && cbf.SplitPredecessors != null)
		{
			List<int> list = new List<int> { this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf) };
			list.AddRange(cbf.SplitPredecessors.Select((ICombinedBendDescriptor x) => this._doc.CombinedBendDescriptors.IndexOf(x)));
			return this._doc.CanSplitBendsMerge(list);
		}
		return false;
	}

	private void SplitBendAngle(ICombinedBendDescriptorInternal selectedCommonBend)
	{
		this._doc.SplitBend(this._doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(selectedCommonBend), 0.5);
		this.Items = this.GetItems();
	}

	[IteratorStateMachine(typeof(_003CSelectedBends_003Ed__148))]
	private IEnumerable<ICombinedBendDescriptorInternal> SelectedBends()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSelectedBends_003Ed__148(-2)
		{
			_003C_003E4__this = this
		};
	}

	private void ChangeRadius(object param)
	{
		if (this.SelectedCommonBend != null)
		{
			this._mainWindowBlock.InitWait(this._doc);
			if (this._model.ChangeRadius(this.SelectedBends(), this._radiusDouble.Value, this._globals))
			{
				this.RefreshModel(this.SelectedCommonBend);
			}
			this._mainWindowBlock.CloseWait(this._doc);
		}
	}

	private void ChangeDeduction(object param)
	{
		if (this.SelectedCommonBend != null)
		{
			this._mainWindowBlock.InitWait(this._doc);
			List<ICombinedBendDescriptorInternal> cbf = (from x in this.SelectedBends()
				where Math.Abs(x.Enumerable.First().BendParams.AngleAbs - this.SelectedCommonBend.Enumerable.First().BendParams.AngleAbs) < 0.0001
				select x).ToList();
			if (this._model.ChangeDeduction(cbf, this._deductionDouble.Value, this._globals))
			{
				this.RefreshModel(this.SelectedCommonBend);
			}
			this._mainWindowBlock.CloseWait(this._doc);
		}
	}

	private void CreateStepBend(object param)
	{
		if (this.SelectedCommonBend != null)
		{
			this._mainWindowBlock.InitWait(this._doc);
			if (this._model.Doc.ConvertBendToStepBend(this._model.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(this.SelectedCommonBend)))
			{
				this.RefreshModel(this.SelectedCommonBend);
			}
			this._mainWindowBlock.CloseWait(this._doc);
		}
	}

	private void EditStepBend(object param)
	{
		if (this.SelectedCommonBend != null)
		{
			this._showStepBendContextMenue?.Invoke();
		}
	}

	private void RefreshModel(ICombinedBendDescriptorInternal cbd)
	{
		foreach (IBendDescriptor item in cbd.Enumerable)
		{
			this._screen3d.ScreenD3D.UpdateModelGeometry(item.BendParams.UnfoldFaceGroupModel, null, render: false);
			this._screen3d.ScreenD3D.UpdateModelGeometry(item.BendParams.ModifiedEntryFaceGroupModel, null, render: false);
			this._screen3d.ScreenD3D.UpdateModelGeometry(item.BendParams.BendFaceGroupModel, null, render: false);
		}
		this._screen3d.ScreenD3D.UpdateAllModelTransform();
		this.CalculateHighlightedEdges();
		if (this.SelectedCommonBend == null)
		{
			this.Close();
			return;
		}
		this.Radius = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalRadius).Converted;
		this.Deduction = new InchConversion(this.SelectedCommonBend[0].BendParams.FinalBendDeduction).Converted;
		this.RequestRepaint?.Invoke();
	}

	private void EditTools2D()
	{
	}

	private void SelectSetup(int id)
	{
	}

	private void SplitOrCombineBends()
	{
		this._oldWidth = base.Width;
		this._oldHeight = base.Height;
		this._oldLeft = base.Left;
		this._oldTop = base.Top;
		SplitAndCombineBendViewModel splitAndCombineBendViewModel = new SplitAndCombineBendViewModel(new SplitAndCombineBendModel(this.SelectedCommonBend), this._screen3d, this._doc, base.Model, base.AnchorPoint3d, this._currentRenderModel, this._shortcutSettingsCommon)
		{
			Width = 470.0,
			Height = 700.0
		};
		base.Width = 470.0;
		base.Height = 700.0;
		splitAndCombineBendViewModel.KeyDown += delegate(object sender, KeyEventArgs args)
		{
			if (args.Key == Key.Escape)
			{
				base.Width = this._oldWidth;
				base.Height = this._oldHeight;
				base.Left = this._oldLeft;
				base.Top = this._oldTop;
				this.SplitBendVisibility = Visibility.Collapsed;
				this.SubMenuViewModel = null;
				this.Visible = Visibility.Visible;
				this.OnViewChanged();
			}
		};
		this.Visible = Visibility.Collapsed;
		this.SubMenuViewModel = splitAndCombineBendViewModel;
		this.SplitBendVisibility = Visibility.Visible;
		this.OnViewChanged();
	}

	public void SetActive(bool active)
	{
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
		this.SubMenuViewModel?.KeyUp(sender, e);
		if (!e.Handled && this._shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			this.Close();
			e.Handle();
		}
	}

	public override bool Close()
	{
		if (this.SubMenuViewModel != null)
		{
			this.SubMenuViewModel.Close();
		}
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		base.Close();
		return true;
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		if (this.SubMenuViewModel != null)
		{
			this.SubMenuViewModel.Close();
			e.MouseEventArgs.Handled = true;
		}
	}

	public void MouseEnterCommand()
	{
		base.Opacity = base.OpacityMax;
	}

	public void MouseLeaveCommand()
	{
		base.Opacity = base.OpacityMin;
	}

	private void SubViewModelClosed(ISubViewModel sender, Triangle tri, global::WiCAM.Pn4000.BendModel.Model model, double x, double y, Vector3d hitPoint, MouseButtonEventArgs mouseButtonEventArgs)
	{
		base.Width = this._oldWidth;
		base.Height = this._oldHeight;
		base.Left = this._oldLeft;
		base.Top = this._oldTop;
		this.EditToolsVisibility = Visibility.Collapsed;
		this.SplitBendVisibility = Visibility.Collapsed;
		this.CombineBendsVisibility = Visibility.Collapsed;
		this.SelectSetupVisible = Visibility.Collapsed;
		this.SelectFrontLiftingAid = Visibility.Collapsed;
		this.SelectBackLiftingAid = Visibility.Collapsed;
		this.SelectAngleMeasurement = Visibility.Collapsed;
		this._subViewModel = null;
		this.Visible = Visibility.Visible;
		this.OnViewChanged();
		if (sender != null)
		{
			sender.Closed -= SubViewModelClosed;
			if (sender == this._subViewModel)
			{
				this._subViewModel = null;
			}
		}
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		if (this._selectedEdges != null)
		{
			foreach (FaceHalfEdge selectedEdge in this._selectedEdges)
			{
				paintTool.SetEdgeColorInShell(selectedEdge, this._highlightColor, 5f);
			}
		}
		if (this.SubMenuViewModel is SplitAndCombineBendViewModel splitAndCombineBendViewModel)
		{
			splitAndCombineBendViewModel.ColorModelParts(paintTool);
		}
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}
}

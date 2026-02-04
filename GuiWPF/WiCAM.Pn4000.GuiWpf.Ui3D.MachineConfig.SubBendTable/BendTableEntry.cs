using System;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.BendTable.DataClasses;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public class BendTableEntry : ViewModelBase
{
	private IBendDataCalculator _bendDataCalculator;

	private IUnitConverter _unitConverter;

	private int? _material3DGroupID;

	private double? _thickness;

	private double? _angle;

	private double? _r;

	private double? _kFactor;

	private double? _springBack;

	private double? _vWidth;

	private double? _cornerRadius;

	private CornerType _cornerType;

	private double? _punchRadius;

	private double? _punchAngle;

	private double? _bendAllowance;

	private double? _bendDeduction;

	private double? _din;

	private bool _isSelected;

	public bool IsValid { get; }

	public int Material3DGroupID
	{
		get
		{
			return _material3DGroupID ?? (-1);
		}
		set
		{
			int? num = ((value < 0) ? null : new int?(value));
			if (_material3DGroupID != num)
			{
				_material3DGroupID = num;
				NotifyPropertyChanged("Material3DGroupID");
			}
		}
	}

	public double? Thickness
	{
		get
		{
			return LenToUi(_thickness);
		}
		set
		{
			double? num = LenFromUi(value);
			if (_thickness != num)
			{
				double? currentFixedValue = GetCurrentFixedValue();
				_thickness = num;
				SetCurrentFixedValue(currentFixedValue);
				NotifyPropertyChanged("Thickness");
			}
		}
	}

	public double? Angle
	{
		get
		{
			return AngleToUi(_angle);
		}
		set
		{
			double? num = AngleFromUi(value);
			if (_angle != num)
			{
				double? currentFixedValue = GetCurrentFixedValue();
				_angle = num;
				SetCurrentFixedValue(currentFixedValue);
				NotifyPropertyChanged("Angle");
			}
		}
	}

	public double? R
	{
		get
		{
			return LenToUi(_r);
		}
		set
		{
			double? num = LenFromUi(value);
			if (_r != num)
			{
				double? currentFixedValue = GetCurrentFixedValue();
				_r = value;
				SetCurrentFixedValue(currentFixedValue);
				NotifyPropertyChanged("R");
				NotifyPropertyChanged("Radius");
			}
		}
	}

	public double? Radius
	{
		get
		{
			return R;
		}
		set
		{
			R = value;
		}
	}

	public double? KFactor
	{
		get
		{
			return _kFactor;
		}
		set
		{
			if (_kFactor != value)
			{
				_kFactor = value;
				_bendAllowance = null;
				_bendDeduction = null;
				_din = null;
				NotifyPropertyChanged("KFactor");
				NotifyPropertyChanged("BendAllowance");
				NotifyPropertyChanged("BendDeduction");
				NotifyPropertyChanged("Din");
			}
		}
	}

	public string Tag { get; set; }

	public double? SpringBack
	{
		get
		{
			return AngleToUi(_springBack);
		}
		set
		{
			double? num = AngleFromUi(value);
			if (_springBack != num)
			{
				_springBack = num;
				NotifyPropertyChanged("SpringBack");
			}
		}
	}

	public double? MinRadius { get; set; }

	public double? MaxRadius { get; set; }

	public double? VWidth
	{
		get
		{
			return LenToUi(_vWidth);
		}
		set
		{
			double? num = LenFromUi(value);
			if (_vWidth != num)
			{
				_vWidth = num;
				NotifyPropertyChanged("VWidth");
			}
		}
	}

	public double? CornerRadius
	{
		get
		{
			return LenToUi(_cornerRadius);
		}
		set
		{
			double? num = LenFromUi(value);
			if (_cornerRadius != num)
			{
				_cornerRadius = num;
				NotifyPropertyChanged("CornerRadius");
			}
		}
	}

	public CornerType CornerType
	{
		get
		{
			return _cornerType;
		}
		set
		{
			if (_cornerType != value)
			{
				_cornerType = value;
				NotifyPropertyChanged("CornerType");
			}
		}
	}

	public double? PunchRadius
	{
		get
		{
			return LenToUi(_punchRadius);
		}
		set
		{
			double? num = LenFromUi(value);
			if (_punchRadius != num)
			{
				_punchRadius = num;
				NotifyPropertyChanged("PunchRadius");
			}
		}
	}

	public double? PunchAngle
	{
		get
		{
			return AngleToUi(_punchAngle);
		}
		set
		{
			double? num = AngleFromUi(value);
			if (_punchAngle != num)
			{
				_punchAngle = num;
				NotifyPropertyChanged("PunchAngle");
			}
		}
	}

	public double? BendLengthMin { get; set; }

	public double? BendLengthMax { get; set; }

	public double? BendAllowance
	{
		get
		{
			return _bendAllowance ?? (_bendAllowance = ((!_kFactor.HasValue) ? null : _bendDataCalculator.BendAllowanceFromKFactor(_thickness, _angle, _r, KFactor)));
		}
		set
		{
			KFactor = _bendDataCalculator.KFactorFromBendAllowance(_thickness, _angle, _r, value);
		}
	}

	public double? BendDeduction
	{
		get
		{
			return _bendDeduction ?? (_bendDeduction = ((!_kFactor.HasValue) ? null : _bendDataCalculator.BendDeductionFromKFactor(_thickness, _angle, _r, KFactor)));
		}
		set
		{
			KFactor = _bendDataCalculator.KFactorFromBendDeduction(_thickness, _angle, _r, value);
		}
	}

	public double? Din
	{
		get
		{
			return _din ?? (_din = ((!_kFactor.HasValue) ? null : _bendDataCalculator.DinLengthFromKFactor(_thickness, _angle, _r, KFactor)));
		}
		set
		{
			KFactor = _bendDataCalculator.KFactorFromDinLength(_thickness, _angle, _r, value);
		}
	}

	public bool IsSelected
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
				NotifyPropertyChanged("IsSelected");
			}
		}
	}

	public double PlotAngle => Angle ?? double.NaN;

	public double PlotRadius => R ?? double.NaN;

	public double PlotCurrentFixedValue => GetCurrentFixedValue() ?? double.NaN;

	private BendTableViewModel _bendTableVm { get; set; }

	public string LengthUnit => _bendTableVm.LengthUnit;

	public string AngleUnit => _bendTableVm.AngleUnit;

	public string CurrentFixedValueDesc => _bendTableVm.CurrentFixedValueTypeDesc;

	public bool IsSpecific
	{
		get
		{
			if (_material3DGroupID.HasValue)
			{
				return _thickness.HasValue;
			}
			return false;
		}
	}

	public BendTableEntry()
	{
	}

	public BendTableEntry(IBendDataCalculator bendDataCalculator, IUnitConverter unitConverter)
	{
		_bendDataCalculator = bendDataCalculator;
		_unitConverter = unitConverter;
	}

	internal void Init(IBendDataCalculator bendDataCalculator, IUnitConverter unitConverter, BendTableViewModel bendTableViewModel)
	{
		if (_bendDataCalculator == null)
		{
			_bendDataCalculator = bendDataCalculator;
			_unitConverter = unitConverter;
			_bendTableVm = bendTableViewModel;
		}
	}

	internal BendTableEntry Init(BendTableViewModel bendTableViewModel)
	{
		_bendTableVm = bendTableViewModel;
		return this;
	}

	internal BendTableEntry Init(BendTableViewModel bendTableViewModel, IBendTableItem item)
	{
		_bendTableVm = bendTableViewModel;
		_material3DGroupID = item.Material3DGroupID;
		_thickness = item.Thickness;
		_angle = item.Angle * Math.PI / 180.0;
		_r = item.R;
		_kFactor = item.KFactor;
		Tag = item.Tag;
		_springBack = item.SpringBack;
		MinRadius = item.MinRadius;
		MaxRadius = item.MaxRadius;
		_vWidth = item.VWidth;
		_cornerRadius = item.CornerRadius;
		_cornerType = item.CornerType;
		_punchRadius = item.PunchRadius;
		_punchAngle = item.PunchAngle * Math.PI / 180.0;
		BendLengthMin = item.BendLengthMin;
		BendLengthMax = item.BendLengthMax;
		return this;
	}

	public IBendTableItem ExportBendTableItem()
	{
		return new BendTableItem
		{
			Material3DGroupID = _material3DGroupID,
			Thickness = _thickness,
			Angle = _angle * 180.0 / Math.PI,
			R = _r,
			KFactor = _kFactor,
			Tag = Tag,
			SpringBack = _springBack,
			MinRadius = MinRadius,
			MaxRadius = MaxRadius,
			VWidth = _vWidth,
			CornerRadius = _cornerRadius,
			CornerType = _cornerType,
			PunchRadius = _punchRadius,
			PunchAngle = _punchAngle * 180.0 / Math.PI,
			BendLengthMin = BendLengthMin,
			BendLengthMax = BendLengthMax
		};
	}

	private double? GetCurrentFixedValue()
	{
		return _bendTableVm.FixedValueType switch
		{
			BendTableFixedValueTypes.BendAllowance => BendAllowance, 
			BendTableFixedValueTypes.BendDeduction => BendDeduction, 
			BendTableFixedValueTypes.Din => Din, 
			_ => KFactor, 
		};
	}

	private void SetCurrentFixedValue(double? value)
	{
		switch (_bendTableVm.FixedValueType)
		{
		case BendTableFixedValueTypes.BendAllowance:
			BendAllowance = value;
			return;
		case BendTableFixedValueTypes.BendDeduction:
			BendDeduction = value;
			return;
		case BendTableFixedValueTypes.Din:
			Din = value;
			return;
		}
		_bendAllowance = null;
		_bendDeduction = null;
		_din = null;
		NotifyPropertyChanged("BendAllowance");
		NotifyPropertyChanged("BendDeduction");
		NotifyPropertyChanged("Din");
		KFactor = value;
	}

	private double? LenToUi(double? mm)
	{
		return _unitConverter.Length.ToUi(mm);
	}

	private double? LenFromUi(double? len)
	{
		if (!len.HasValue)
		{
			return null;
		}
		return _unitConverter.Length.FromUi(len.Value);
	}

	private double? AngleToUi(double? rad)
	{
		return _unitConverter.Angle.ToUi(rad, 10);
	}

	private double? AngleFromUi(double? angle)
	{
		if (!angle.HasValue)
		{
			return null;
		}
		return _unitConverter.Angle.FromUi(angle.Value);
	}
}

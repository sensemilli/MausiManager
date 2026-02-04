using System;
using System.Globalization;
using System.Windows;
using BendDataBase.Enums;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public class VisualBendZoneDataBaseItem : ViewModelBase
{
	private bool _flagKChanging;

	private bool _flagMustRecalc;

	private bool _isChanged;

	private string _material3DGroup = "*";

	private string _thickness = "*";

	private string _angle = "*";

	private string _radius = "*";

	private string _minRadius = "*";

	private string _maxRadius = "*";

	private string _vWidth = "*";

	private string _vAngle = "*";

	private string _cornerRadius = "*";

	private string _punchRadius = "*";

	private string _punchAngle = "*";

	private string _tag = "";

	private string _bendLengthMin = "*";

	private string _bendLengthMax = "*";

	private string _kFactor = "---";

	private string _ba = "---";

	private string _bd = "---";

	private string _dinLength = "---";

	private string _wGamma = "---";

	private string _visualThickness = "*";

	private string _visualAngle = "*";

	private string _visualRadius = "*";

	private string _visualKFactor = "---";

	private string _visualBa = "---";

	private string _visualBd = "---";

	private string _visualDin = "---";

	private string _visualWGamma = "---";

	private double _kFactorDouble;

	private double _thicknessDouble;

	private double _angleDouble;

	private double _radiusDouble;

	private double _baDouble;

	private double _bdDouble;

	private double _dinDouble;

	private double _wGammaDouble;

	private double _vWidthDouble;

	private double _vAngleDouble;

	private double _cornerRadiusDouble;

	private double _punchRadiusDouble;

	private double _punchAngleDouble;

	private double _bendLengthMinDouble;

	private double _bendLengthMaxDouble;

	private double _minRadiusDouble;

	private double _maxRadiusDouble;

	private double? _springBack;

	private bool _isSelected;

	private Func<BendParamType> _getFixedValue;

	public string Material3DGroup
	{
		get
		{
			return _material3DGroup;
		}
		set
		{
			if (!(_material3DGroup == value))
			{
				_material3DGroup = value;
				IsChanged = true;
				NotifyPropertyChanged("Material3DGroup");
			}
		}
	}

	public string Thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			if (_thickness == value)
			{
				VisualThickness = _thickness;
				return;
			}
			_thickness = value;
			IsChanged = true;
			VisualThickness = _thickness;
			if (IsDoubleValue(_thickness, out var value2))
			{
				ThicknessDouble = value2;
			}
			else
			{
				ThicknessDouble = double.NaN;
			}
			NotifyPropertyChanged("Thickness");
			switch (_getFixedValue?.Invoke())
			{
			case BendParamType.KFactor:
			{
				string kFactor = KFactor;
				_flagMustRecalc = true;
				KFactor = kFactor;
				break;
			}
			case BendParamType.BA:
			{
				string bA = BA;
				_flagMustRecalc = true;
				BA = bA;
				break;
			}
			case BendParamType.BD:
			{
				string bD = BD;
				_flagMustRecalc = true;
				BD = bD;
				break;
			}
			case BendParamType.Din:
			{
				string dinLength = DinLength;
				_flagMustRecalc = true;
				DinLength = dinLength;
				break;
			}
			}
			if (IsDoubleValue(Thickness, out var value3) && IsDoubleValue(Angle, out var value4) && IsDoubleValue(Radius, out var value5))
			{
				WGamma = BendDataCalculator.WGammaFromRadius(value3, value4, value5).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				WGamma = "---";
			}
		}
	}

	public string Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			if (_angle == value)
			{
				VisualAngle = _angle;
				return;
			}
			_angle = value;
			IsChanged = true;
			VisualAngle = _angle;
			if (IsDoubleValue(_angle, out var value2))
			{
				AngleDouble = value2;
			}
			else
			{
				AngleDouble = double.NaN;
			}
			NotifyPropertyChanged("Angle");
			switch (_getFixedValue?.Invoke())
			{
			case BendParamType.KFactor:
			{
				string kFactor = KFactor;
				_flagMustRecalc = true;
				KFactor = kFactor;
				break;
			}
			case BendParamType.BA:
			{
				string bA = BA;
				_flagMustRecalc = true;
				BA = bA;
				break;
			}
			case BendParamType.BD:
			{
				string bD = BD;
				_flagMustRecalc = true;
				BD = bD;
				break;
			}
			case BendParamType.Din:
			{
				string dinLength = DinLength;
				_flagMustRecalc = true;
				DinLength = dinLength;
				break;
			}
			}
			if (IsDoubleValue(Thickness, out var value3) && IsDoubleValue(Angle, out var value4) && IsDoubleValue(Radius, out var value5))
			{
				WGamma = BendDataCalculator.WGammaFromRadius(value3, value4, value5).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				WGamma = "---";
			}
		}
	}

	public string Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			if (_radius == value)
			{
				VisualRadius = _radius;
				return;
			}
			_radius = value;
			IsChanged = true;
			VisualRadius = _radius;
			if (IsDoubleValue(_radius, out var value2))
			{
				RadiusDouble = value2;
			}
			else
			{
				RadiusDouble = double.NaN;
			}
			NotifyPropertyChanged("Radius");
			BendParamType? bendParamType = _getFixedValue?.Invoke();
			if (bendParamType.HasValue)
			{
				switch (bendParamType.GetValueOrDefault())
				{
				case BendParamType.KFactor:
				{
					string kFactor = KFactor;
					_flagMustRecalc = true;
					KFactor = kFactor;
					break;
				}
				case BendParamType.BA:
				{
					string bA = BA;
					_flagMustRecalc = true;
					BA = bA;
					break;
				}
				case BendParamType.BD:
				{
					string bD = BD;
					_flagMustRecalc = true;
					BD = bD;
					break;
				}
				case BendParamType.Din:
				{
					string dinLength = DinLength;
					_flagMustRecalc = true;
					DinLength = dinLength;
					break;
				}
				}
			}
		}
	}

	public string Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			if (!(_tag == value))
			{
				_tag = value;
				IsChanged = true;
				NotifyPropertyChanged("Tag");
			}
		}
	}

	public string BendLengthMin
	{
		get
		{
			return _bendLengthMin;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_bendLengthMin == value))
			{
				_bendLengthMin = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					BendLengthMinDouble = value2;
				}
				else
				{
					BendLengthMinDouble = double.NaN;
				}
				NotifyPropertyChanged("BendLengthMin");
			}
		}
	}

	public string BendLengthMax
	{
		get
		{
			return _bendLengthMax;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_bendLengthMax == value))
			{
				_bendLengthMax = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					BendLengthMaxDouble = value2;
				}
				else
				{
					BendLengthMaxDouble = double.NaN;
				}
				NotifyPropertyChanged("BendLengthMax");
			}
		}
	}

	public string MinRadius
	{
		get
		{
			return _minRadius;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_minRadius == value))
			{
				_minRadius = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					MinRadiusDouble = value2;
				}
				else
				{
					MinRadiusDouble = double.NaN;
				}
				NotifyPropertyChanged("MinRadius");
			}
		}
	}

	public string MaxRadius
	{
		get
		{
			return _maxRadius;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_maxRadius == value))
			{
				_maxRadius = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					MaxRadiusDouble = value2;
				}
				else
				{
					MaxRadiusDouble = double.NaN;
				}
				NotifyPropertyChanged("MaxRadius");
			}
		}
	}

	public string VWidth
	{
		get
		{
			return _vWidth;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_vWidth == value))
			{
				_vWidth = value;
				IsChanged = true;
				if (flag)
				{
					VWidthDouble = value2;
				}
				else
				{
					VWidthDouble = double.NaN;
				}
				NotifyPropertyChanged("VWidth");
			}
		}
	}

	public string VAngle
	{
		get
		{
			return _vAngle;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_vAngle == value))
			{
				_vAngle = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					VAngleDouble = value2;
				}
				else
				{
					VAngleDouble = double.NaN;
				}
				NotifyPropertyChanged("VAngle");
			}
		}
	}

	public string CornerRadius
	{
		get
		{
			return _cornerRadius;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*" && value != "")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_cornerRadius == value))
			{
				_cornerRadius = ((value == "") ? "*" : value);
				IsChanged = true;
				if (flag)
				{
					CornerRadiusDouble = value2;
				}
				else
				{
					CornerRadiusDouble = double.NaN;
				}
				NotifyPropertyChanged("CornerRadius");
			}
		}
	}

	public string PunchRadius
	{
		get
		{
			return _punchRadius;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_punchRadius == value))
			{
				_punchRadius = value;
				IsChanged = true;
				if (flag)
				{
					PunchRadiusDouble = value2;
				}
				else
				{
					PunchRadiusDouble = double.NaN;
				}
				NotifyPropertyChanged("PunchRadius");
			}
		}
	}

	public string PunchAngle
	{
		get
		{
			return _punchAngle;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			if (!(_punchAngle == value))
			{
				_punchAngle = value;
				IsChanged = true;
				if (flag)
				{
					PunchAngleDouble = value2;
				}
				else
				{
					PunchAngleDouble = double.NaN;
				}
				NotifyPropertyChanged("PunchAngle");
			}
		}
	}

	public string KFactor
	{
		get
		{
			return _kFactor;
		}
		set
		{
			if (_flagKChanging)
			{
				return;
			}
			if (!IsDoubleValue(value, out var value2))
			{
				value = "---";
			}
			if (IsDoubleValue(_kFactor, out value2) && !IsDoubleValue(value, out value2))
			{
				return;
			}
			if (_kFactor == value && !_flagMustRecalc)
			{
				VisualKFactor = _kFactor;
				return;
			}
			_kFactor = value;
			IsChanged = true;
			VisualKFactor = _kFactor;
			if (IsDoubleValue(_kFactor, out var value3))
			{
				KFactorDouble = value3;
			}
			else
			{
				KFactorDouble = double.NaN;
			}
			NotifyPropertyChanged("KFactor");
			_flagKChanging = true;
			if (IsDoubleValue(Thickness, out var value4) && IsDoubleValue(Angle, out var value5) && IsDoubleValue(Radius, out var value6) && IsDoubleValue(KFactor, out var value7))
			{
				double ba = BendDataCalculator.BendAllowanceFromKFactor(value4, value5 * Math.PI / 180.0, value6, value7);
				double num = BendDataCalculator.BendDeductionFromBendAllowance(value4, value5, value6, ba);
				double num2 = BendDataCalculator.DinLengthFromKFactor(value4, value5 * Math.PI / 180.0, value6, value7);
				BA = ba.ToString(CultureInfo.InvariantCulture);
				BD = num.ToString(CultureInfo.InvariantCulture);
				DinLength = num2.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				BA = "---";
				BD = "---";
				DinLength = "---";
			}
			_flagKChanging = false;
			_flagMustRecalc = false;
		}
	}

	public string BA
	{
		get
		{
			return _ba;
		}
		set
		{
			if (IsDoubleValue(_ba, out var value2) && !IsDoubleValue(value, out value2))
			{
				return;
			}
			if (_ba == value && !_flagMustRecalc)
			{
				VisualBa = _ba;
				return;
			}
			_ba = value;
			IsChanged = true;
			VisualBa = _ba;
			if (IsDoubleValue(_ba, out var value3))
			{
				BaDouble = value3;
			}
			else
			{
				BaDouble = double.NaN;
			}
			NotifyPropertyChanged("BA");
			if (IsDoubleValue(Thickness, out var value4) && IsDoubleValue(Angle, out var value5) && IsDoubleValue(Radius, out var value6) && IsDoubleValue(BA, out var value7))
			{
				KFactor = BendDataCalculator.KFactorFromBendAllowance(value4, value5 * Math.PI / 180.0, value6, value7).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				KFactor = "---";
			}
		}
	}

	public string BD
	{
		get
		{
			return _bd;
		}
		set
		{
			if (IsDoubleValue(_bd, out var value2) && !IsDoubleValue(value, out value2))
			{
				return;
			}
			if (_bd == value && !_flagMustRecalc)
			{
				VisualBd = _bd;
				return;
			}
			_bd = value;
			IsChanged = true;
			VisualBd = _bd;
			if (IsDoubleValue(_bd, out var value3))
			{
				BdDouble = value3;
			}
			else
			{
				BdDouble = double.NaN;
			}
			NotifyPropertyChanged("BD");
			if (IsDoubleValue(Thickness, out var value4) && IsDoubleValue(Angle, out var value5) && IsDoubleValue(Radius, out var value6) && IsDoubleValue(BD, out var value7))
			{
				KFactor = BendDataCalculator.KFactorFromBendDeduction(value4, value5 * Math.PI / 180.0, value6, value7).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				KFactor = "---";
			}
		}
	}

	public string DinLength
	{
		get
		{
			return _dinLength;
		}
		set
		{
			if (IsDoubleValue(_dinLength, out var value2) && !IsDoubleValue(value, out value2))
			{
				return;
			}
			if (_dinLength == value && !_flagMustRecalc)
			{
				VisualDin = _dinLength;
				return;
			}
			_dinLength = value;
			IsChanged = true;
			VisualDin = _dinLength;
			if (IsDoubleValue(_dinLength, out var value3))
			{
				DinDouble = value3;
			}
			else
			{
				DinDouble = double.NaN;
			}
			NotifyPropertyChanged("DinLength");
			if (IsDoubleValue(Thickness, out var value4) && IsDoubleValue(Angle, out var value5) && IsDoubleValue(Radius, out var value6) && IsDoubleValue(DinLength, out var value7))
			{
				KFactor = BendDataCalculator.KFactorFromDinLength(value4, value5 * Math.PI / 180.0, value6, value7).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				KFactor = "---";
			}
		}
	}

	public string WGamma
	{
		get
		{
			return _wGamma;
		}
		set
		{
			if (_wGamma == value)
			{
				VisualWGamma = _wGamma;
				return;
			}
			_wGamma = value;
			IsChanged = true;
			VisualWGamma = _wGamma;
			if (IsDoubleValue(_wGamma, out var value2))
			{
				WGammaDouble = value2;
			}
			else
			{
				WGammaDouble = double.NaN;
			}
			NotifyPropertyChanged("WGamma");
		}
	}

	public string VisualThickness
	{
		get
		{
			return _visualThickness;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (ChecksActive && flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			_visualThickness = (flag ? Math.Round(value2, 5).ToString("0.#####", CultureInfo.InvariantCulture) : value);
			if (Thickness != value)
			{
				Thickness = value;
			}
			NotifyPropertyChanged("VisualThickness");
		}
	}

	public string VisualAngle
	{
		get
		{
			return _visualAngle;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (ChecksActive && flag && (value2 <= 0.0 || value2 >= 270.0))
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0) + " " + string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueLessEqual") as string, 270.0));
			}
			_visualAngle = (flag ? Math.Round(value2, 5).ToString("0.#####", CultureInfo.InvariantCulture) : value);
			if (Angle != value)
			{
				Angle = value;
			}
			NotifyPropertyChanged("VisualAngle");
		}
	}

	public string VisualRadius
	{
		get
		{
			return _visualRadius;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "*")
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (ChecksActive && flag && value2 <= 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			_visualRadius = (flag ? Math.Round(value2, 5).ToString("0.#####", CultureInfo.InvariantCulture) : value);
			if (Radius != value)
			{
				Radius = value;
			}
			NotifyPropertyChanged("VisualRadius");
		}
	}

	public string VisualKFactor
	{
		get
		{
			return _visualKFactor;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "---" && value != "*" && !string.IsNullOrEmpty(value))
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			_visualKFactor = (flag ? Math.Round(value2, 5).ToString("0.#####", CultureInfo.InvariantCulture) : value);
			if (KFactor != value)
			{
				KFactor = value;
			}
			NotifyPropertyChanged("VisualKFactor");
		}
	}

	public string VisualBa
	{
		get
		{
			return _visualBa;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "---" && value != "*" && !string.IsNullOrEmpty(value))
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			if (ChecksActive && flag && value2 < 0.0)
			{
				throw new Exception(string.Format(Application.Current.FindResource("l_popup.GridViewAggregate.ValueBiggerEqual") as string, 0.0));
			}
			_visualBa = (flag ? Math.Round(value2, 3).ToString("0.###", CultureInfo.InvariantCulture) : value);
			if (BA != value)
			{
				BA = value;
			}
			NotifyPropertyChanged("VisualBa");
		}
	}

	public string VisualBd
	{
		get
		{
			return _visualBd;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (ChecksActive && !flag && value != "---" && value != "*" && !string.IsNullOrEmpty(value))
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			_visualBd = (flag ? Math.Round(value2, 3).ToString("0.###", CultureInfo.InvariantCulture) : value);
			if (BD != value)
			{
				BD = value;
			}
			NotifyPropertyChanged("VisualBd");
		}
	}

	public string VisualDin
	{
		get
		{
			return _visualDin;
		}
		set
		{
			double value2;
			bool flag = IsDoubleValue(value, out value2);
			if (!flag && value != "---" && value != "*" && !string.IsNullOrEmpty(value))
			{
				throw new Exception(Application.Current.FindResource("l_popup.GridViewAggregate.ValueNumerical") as string);
			}
			_visualDin = (flag ? Math.Round(value2, 3).ToString("0.###", CultureInfo.InvariantCulture) : value);
			if (DinLength != value)
			{
				DinLength = value;
			}
			NotifyPropertyChanged("VisualDin");
		}
	}

	public string VisualWGamma
	{
		get
		{
			return _visualWGamma;
		}
		set
		{
		}
	}

	public double KFactorDouble
	{
		get
		{
			return _kFactorDouble;
		}
		set
		{
			_kFactorDouble = value;
			NotifyPropertyChanged("KFactorDouble");
		}
	}

	public double ThicknessDouble
	{
		get
		{
			return _thicknessDouble;
		}
		set
		{
			_thicknessDouble = value;
			NotifyPropertyChanged("ThicknessDouble");
		}
	}

	public double AngleDouble
	{
		get
		{
			return _angleDouble;
		}
		set
		{
			_angleDouble = value;
			NotifyPropertyChanged("AngleDouble");
		}
	}

	public double RadiusDouble
	{
		get
		{
			return _radiusDouble;
		}
		set
		{
			_radiusDouble = value;
			NotifyPropertyChanged("RadiusDouble");
		}
	}

	public double BaDouble
	{
		get
		{
			return _baDouble;
		}
		set
		{
			_baDouble = value;
			NotifyPropertyChanged("BaDouble");
		}
	}

	public double BdDouble
	{
		get
		{
			return _bdDouble;
		}
		set
		{
			_bdDouble = value;
			NotifyPropertyChanged("BdDouble");
		}
	}

	public double DinDouble
	{
		get
		{
			return _dinDouble;
		}
		set
		{
			_dinDouble = value;
			NotifyPropertyChanged("DinDouble");
		}
	}

	public double WGammaDouble
	{
		get
		{
			return _wGammaDouble;
		}
		set
		{
			_wGammaDouble = value;
			NotifyPropertyChanged("WGammaDouble");
		}
	}

	public double VWidthDouble
	{
		get
		{
			return _vWidthDouble;
		}
		set
		{
			_vWidthDouble = value;
			NotifyPropertyChanged("VWidthDouble");
		}
	}

	public double VAngleDouble
	{
		get
		{
			return _vAngleDouble;
		}
		set
		{
			_vAngleDouble = value;
			NotifyPropertyChanged("VAngleDouble");
		}
	}

	public double CornerRadiusDouble
	{
		get
		{
			return _cornerRadiusDouble;
		}
		set
		{
			_cornerRadiusDouble = value;
			NotifyPropertyChanged("CornerRadiusDouble");
		}
	}

	public double PunchRadiusDouble
	{
		get
		{
			return _punchRadiusDouble;
		}
		set
		{
			_punchRadiusDouble = value;
			NotifyPropertyChanged("PunchRadiusDouble");
		}
	}

	public double PunchAngleDouble
	{
		get
		{
			return _punchAngleDouble;
		}
		set
		{
			_punchAngleDouble = value;
			NotifyPropertyChanged("PunchAngleDouble");
		}
	}

	public double BendLengthMinDouble
	{
		get
		{
			return _bendLengthMinDouble;
		}
		set
		{
			_bendLengthMinDouble = value;
			NotifyPropertyChanged("BendLengthMinDouble");
		}
	}

	public double BendLengthMaxDouble
	{
		get
		{
			return _bendLengthMaxDouble;
		}
		set
		{
			_bendLengthMaxDouble = value;
			NotifyPropertyChanged("BendLengthMaxDouble");
		}
	}

	public double MinRadiusDouble
	{
		get
		{
			return _minRadiusDouble;
		}
		set
		{
			_minRadiusDouble = value;
			NotifyPropertyChanged("MinRadiusDouble");
		}
	}

	public double MaxRadiusDouble
	{
		get
		{
			return _maxRadiusDouble;
		}
		set
		{
			_maxRadiusDouble = value;
			NotifyPropertyChanged("MaxRadiusDouble");
		}
	}

	public double? SpringBackDeg
	{
		get
		{
			if (_springBack.HasValue)
			{
				return Math.Round(_springBack.Value * 180.0 / Math.PI, 3);
			}
			return null;
		}
		set
		{
			SpringBack = value * Math.PI / 180.0;
		}
	}

	public double? SpringBack
	{
		get
		{
			return _springBack;
		}
		set
		{
			if (_springBack != value)
			{
				_springBack = value;
				NotifyPropertyChanged("SpringBack");
				NotifyPropertyChanged("SpringBackDeg");
			}
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
			_isSelected = value;
			NotifyPropertyChanged("IsSelected");
		}
	}

	public bool IsChanged
	{
		get
		{
			return _isChanged;
		}
		set
		{
			_isChanged = value;
			NotifyPropertyChanged("IsChanged");
		}
	}

	public bool ChecksActive { get; set; }

	public VisualBendZoneDataBaseItem(Func<BendParamType> getFixedValue)
	{
		_getFixedValue = getFixedValue;
	}

	public VisualBendZoneDataBaseItem()
	{
	}

	public void SetFixedValueFunc(Func<BendParamType> getFixedValue)
	{
		_getFixedValue = getFixedValue;
	}

	private static bool IsDoubleValue(string txt, out double value)
	{
		value = 0.0;
		if (string.IsNullOrEmpty(txt))
		{
			return false;
		}
		txt = txt.Trim().Replace(',', '.');
		if (txt == string.Empty || txt == "*" || txt == "---")
		{
			return false;
		}
		return double.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
	}

	public bool IsEqualInKeyProperties(VisualBendZoneDataBaseItem o)
	{
		if (o.Material3DGroup == Material3DGroup && o.Thickness == Thickness && o.Angle == Angle)
		{
			return o.Radius == Radius;
		}
		return false;
	}
}

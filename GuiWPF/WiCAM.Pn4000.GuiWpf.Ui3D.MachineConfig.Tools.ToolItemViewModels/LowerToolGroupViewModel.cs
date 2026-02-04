using System.ComponentModel;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class LowerToolGroupViewModel : ToolItemViewModel
{
	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private bool _isChanged;

	private int? _primaryToolID;

	private string _primaryToolName;

	private int? _id;

	private string _name;

	private double _radius;

	private double _vWidth;

	private double _vAngleRad;

	private CornerType _cornerType;

	[Browsable(false)]
	public bool IsChanged => _isChanged;

	[ReadOnly(true)]
	public int? PrimaryToolId
	{
		get
		{
			return _primaryToolID;
		}
		set
		{
			if (_primaryToolID != value)
			{
				_primaryToolID = value;
				_isChanged = true;
				NotifyPropertyChanged("PrimaryToolId");
			}
		}
	}

	[ReadOnly(true)]
	public string PrimaryToolName
	{
		get
		{
			return _primaryToolName;
		}
		set
		{
			if (_primaryToolName != value)
			{
				_primaryToolName = value;
				_isChanged = true;
				NotifyPropertyChanged("PrimaryToolName");
			}
		}
	}

	public int? ID
	{
		get
		{
			return _id;
		}
		set
		{
			if (_id != value)
			{
				_id = value;
				_isChanged = true;
				NotifyPropertyChanged("ID");
			}
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (_name != value)
			{
				_name = value;
				_isChanged = true;
				NotifyPropertyChanged("Name");
			}
		}
	}

	public double RadiusMm
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			_isChanged = true;
		}
	}

	public double Radius
	{
		get
		{
			return _unitConverter.Length.ToUi(_radius, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_radius != value)
			{
				_radius = value;
				_isChanged = true;
				NotifyPropertyChanged("Radius");
			}
		}
	}

	public double VWidthMm
	{
		get
		{
			return _vWidth;
		}
		set
		{
			_vWidth = value;
			_isChanged = true;
		}
	}

	public double VWidth
	{
		get
		{
			return _unitConverter.Length.ToUi(_vWidth, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_vWidth != value)
			{
				_vWidth = value;
				_isChanged = true;
				NotifyPropertyChanged("VWidth");
			}
		}
	}

	public double VAngleRad
	{
		get
		{
			return _vAngleRad;
		}
		set
		{
			_vAngleRad = value;
			_isChanged = true;
		}
	}

	public double VAngle
	{
		get
		{
			return _unitConverter.Angle.ToUi(_vAngleRad, 4);
		}
		set
		{
			value = _unitConverter.Angle.FromUi(value, 4);
			if (_vAngleRad != value)
			{
				_vAngleRad = value;
				_isChanged = true;
				NotifyPropertyChanged("VAngle");
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
			_cornerType = value;
			_isChanged = true;
		}
	}

	public IDieGroup? DieGroup { get; private set; }

	public string Desc => Name;

	public LowerToolGroupViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, double vWidth = 0.0, double vAngle = 0.0, double radius = 0.0)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
	}

	public LowerToolGroupViewModel(IDieGroup group, IUnitConverter unitConverter, IGlobalToolStorage toolStorage)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_primaryToolID = group.PrimaryToolId;
		_primaryToolName = group.PrimaryToolName;
		_id = group.ID;
		_name = group.Name;
		_radius = group.Radius;
		_vWidth = group.VWidth;
		_vAngleRad = group.VAngleRad;
		_cornerType = group.CornerType;
		DieGroup = group;
	}

	public IDieGroup Save(IDieGroup dieGroup)
	{
		if (dieGroup == null)
		{
			dieGroup = _toolStorage.CreateDieGroup();
		}
		dieGroup.Name = _name;
		dieGroup.VWidth = _vWidth;
		dieGroup.Radius = _radius;
		dieGroup.VAngleRad = _vAngleRad;
		dieGroup.PrimaryToolId = _primaryToolID ?? (-1);
		dieGroup.PrimaryToolName = _primaryToolName;
		dieGroup.CornerType = _cornerType;
		DieGroup = dieGroup;
		return dieGroup;
	}

	public void ClearID()
	{
		_id = null;
	}
}

using System.ComponentModel;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class UpperToolGroupViewModel : ToolItemViewModel
{
	private readonly IGlobalToolStorage _toolStorage;

	private readonly IUnitConverter _unitConverter;

	private bool _isChanged;

	private string _name;

	private int? _primaryToolID;

	private int? _id;

	private string _primaryToolName;

	private double _radius;

	[Browsable(false)]
	public bool IsChanged => _isChanged;

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

	[ReadOnly(true)]
	public int? ID => _id;

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

	public IPunchGroup? PunchGroup { get; private set; }

	public string Desc => Name;

	public UpperToolGroupViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, double radius = 0.0)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_radius = radius;
	}

	public UpperToolGroupViewModel(IPunchGroup group, IUnitConverter unitConverter, IGlobalToolStorage toolStorage)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_name = group.Name;
		_primaryToolID = group.PrimaryToolId;
		_primaryToolName = group.PrimaryToolName;
		_id = group.ID;
		_radius = group.Radius;
		PunchGroup = group;
	}

	public IPunchGroup Save(IPunchGroup punchGroup)
	{
		if (punchGroup == null)
		{
			punchGroup = _toolStorage.CreatePunchGroup();
		}
		punchGroup.Name = _name;
		punchGroup.Radius = _radius;
		punchGroup.PrimaryToolId = _primaryToolID ?? (-1);
		punchGroup.PrimaryToolName = _primaryToolName;
		PunchGroup = punchGroup;
		return punchGroup;
	}

	public void ClearID()
	{
		_id = null;
	}
}

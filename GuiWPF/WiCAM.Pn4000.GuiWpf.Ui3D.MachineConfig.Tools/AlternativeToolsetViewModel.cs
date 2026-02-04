using System;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.MachineComponents;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class AlternativeToolsetViewModel : ViewModelBase
{
	private readonly PreferredProfilesViewModel _baseVm;

	private UpperToolGroupViewModel? _upperGroup;

	private LowerToolGroupViewModel? _lowerGroup;

	private ToolViewModel? _upperTool;

	private ToolViewModel? _lowerTool;

	private double? _prebendAngleRad;

	private UpperToolGroupViewModel? _prebendUpperGroup;

	private LowerToolGroupViewModel? _prebendLowerGroup;

	private ToolViewModel? _prebendUpperTool;

	private ToolViewModel? _prebendLowerTool;

	public BendMethod BendMethod { get; set; }

	public UpperToolGroupViewModel? UpperGroup
	{
		get
		{
			return _upperGroup;
		}
		set
		{
			if (_upperGroup != value)
			{
				_upperGroup = value;
				NotifyPropertyChanged("UpperGroup");
				if (_upperGroup != null)
				{
					UpperTool = null;
				}
			}
		}
	}

	public LowerToolGroupViewModel? LowerGroup
	{
		get
		{
			return _lowerGroup;
		}
		set
		{
			if (_lowerGroup != value)
			{
				_lowerGroup = value;
				NotifyPropertyChanged("LowerGroup");
				if (_lowerGroup != null)
				{
					LowerTool = null;
				}
			}
		}
	}

	public ToolViewModel? UpperTool
	{
		get
		{
			return _upperTool;
		}
		set
		{
			if (_upperTool != value)
			{
				_upperTool = value;
				NotifyPropertyChanged("UpperTool");
				if (_upperTool != null)
				{
					UpperGroup = null;
				}
			}
		}
	}

	public ToolViewModel? LowerTool
	{
		get
		{
			return _lowerTool;
		}
		set
		{
			if (_lowerTool != value)
			{
				_lowerTool = value;
				NotifyPropertyChanged("LowerTool");
				if (_lowerTool != null)
				{
					LowerGroup = null;
				}
			}
		}
	}

	public ToolViewModel? FoldDieExtension { get; set; }

	public double? PrebendAngleRad
	{
		get
		{
			return _prebendAngleRad;
		}
		set
		{
			if (BendMethod == BendMethod.AirBend)
			{
				value = null;
			}
			if (_prebendAngleRad != value)
			{
				_prebendAngleRad = value;
				NotifyPropertyChanged("PrebendAngleRad");
				NotifyPropertyChanged("PrebendAngleDeg");
			}
		}
	}

	public double? PrebendAngleDeg
	{
		get
		{
			return _prebendAngleRad * 180.0 / Math.PI;
		}
		set
		{
			PrebendAngleRad = value * Math.PI / 180.0;
		}
	}

	public UpperToolGroupViewModel? PrebendUpperGroup
	{
		get
		{
			return _prebendUpperGroup;
		}
		set
		{
			if (BendMethod == BendMethod.AirBend)
			{
				value = null;
			}
			if (_prebendUpperGroup != value)
			{
				_prebendUpperGroup = value;
				NotifyPropertyChanged("PrebendUpperGroup");
				if (_prebendUpperGroup != null)
				{
					PrebendUpperTool = null;
				}
			}
		}
	}

	public LowerToolGroupViewModel? PrebendLowerGroup
	{
		get
		{
			return _prebendLowerGroup;
		}
		set
		{
			if (BendMethod == BendMethod.AirBend)
			{
				value = null;
			}
			if (_prebendLowerGroup != value)
			{
				_prebendLowerGroup = value;
				NotifyPropertyChanged("PrebendLowerGroup");
				if (_prebendLowerGroup != null)
				{
					PrebendLowerTool = null;
				}
			}
		}
	}

	public ToolViewModel? PrebendUpperTool
	{
		get
		{
			return _prebendUpperTool;
		}
		set
		{
			if (BendMethod == BendMethod.AirBend)
			{
				value = null;
			}
			if (_prebendUpperTool != value)
			{
				_prebendUpperTool = value;
				NotifyPropertyChanged("PrebendUpperTool");
				if (_prebendUpperTool != null)
				{
					PrebendUpperGroup = null;
				}
			}
		}
	}

	public ToolViewModel? PrebendLowerTool
	{
		get
		{
			return _prebendLowerTool;
		}
		set
		{
			if (BendMethod == BendMethod.AirBend)
			{
				value = null;
			}
			if (_prebendLowerTool != value)
			{
				_prebendLowerTool = value;
				NotifyPropertyChanged("PrebendLowerTool");
				if (_prebendLowerTool != null)
				{
					PrebendLowerGroup = null;
				}
			}
		}
	}

	public AlternativeToolsetViewModel(PreferredProfilesViewModel baseVm)
	{
		_baseVm = baseVm;
	}

	public AlternativeToolsetViewModel(IPreferredProfileToolSet tool, PreferredProfilesViewModel baseVm)
		: this(baseVm)
	{
		LoadFromToolSet(tool);
	}

	private void LoadFromToolSet(IPreferredProfileToolSet toolset)
	{
		BendMethod = toolset.BendMethod;
		UpperGroup = GetStartGroupUpper(toolset.UpperGroup?.ID);
		LowerGroup = GetStartGroupLower(toolset.LowerGroup?.ID);
		UpperTool = GetStartTool(toolset.UpperTool?.ID);
		LowerTool = GetStartTool(toolset.LowerTool?.ID);
		FoldDieExtension = GetStartTool(toolset.FoldDieExtension?.ID);
		PrebendAngleRad = toolset.PrebendAngle;
		PrebendUpperGroup = GetStartGroupUpper(toolset.PrebendUpperGroup?.ID);
		PrebendLowerGroup = GetStartGroupLower(toolset.PrebendLowerGroup?.ID);
		PrebendUpperTool = GetStartTool(toolset.PrebendUpperTool?.ID);
		PrebendLowerTool = GetStartTool(toolset.PrebendLowerTool?.ID);
	}

	private UpperToolGroupViewModel? GetStartGroupUpper(int? groupId)
	{
		if (groupId.HasValue && _baseVm.MachineVm.StartUpperGroupsById.TryGetValue(groupId.Value, out UpperToolGroupViewModel value))
		{
			return value;
		}
		return null;
	}

	private LowerToolGroupViewModel? GetStartGroupLower(int? groupId)
	{
		if (groupId.HasValue && _baseVm.MachineVm.StartLowerGroupsById.TryGetValue(groupId.Value, out LowerToolGroupViewModel value))
		{
			return value;
		}
		return null;
	}

	private ToolViewModel? GetStartTool(int? toolId)
	{
		if (toolId.HasValue && _baseVm.MachineVm.StartToolsById.TryGetValue(toolId.Value, out ToolViewModel value))
		{
			return value;
		}
		return null;
	}

	public IPreferredProfileToolSet CreatePreferredProfileToolSet(int priority, IPreferredProfile profile)
	{
		return new PreferredProfileToolSet
		{
			Priority = priority,
			BendMethod = BendMethod,
			PrebendAngle = PrebendAngleRad,
			UpperGroup = UpperGroup?.PunchGroup,
			UpperTool = UpperTool?.ToolProfile,
			LowerGroup = LowerGroup?.DieGroup,
			LowerTool = LowerTool?.ToolProfile,
			FoldDieExtension = FoldDieExtension?.ToolProfile,
			PrebendUpperGroup = PrebendUpperGroup?.PunchGroup,
			PrebendUpperTool = PrebendUpperTool?.ToolProfile,
			PrebendLowerGroup = PrebendLowerGroup?.DieGroup,
			PrebendLowerTool = PrebendLowerTool?.ToolProfile,
			PreferredProfiles = profile
		};
	}

	public bool CanSave()
	{
		if (((UpperGroup != null) ^ (UpperTool != null)) && ((LowerGroup != null) ^ (LowerTool != null)))
		{
			return true;
		}
		return false;
	}

	public AlternativeToolsetViewModel Dupplicate()
	{
		return new AlternativeToolsetViewModel(_baseVm)
		{
			BendMethod = BendMethod,
			_upperGroup = _upperGroup,
			_lowerGroup = _lowerGroup,
			_upperTool = _upperTool,
			_lowerTool = _lowerTool,
			FoldDieExtension = FoldDieExtension,
			_prebendAngleRad = _prebendAngleRad,
			_prebendUpperGroup = _prebendUpperGroup,
			_prebendLowerGroup = _prebendLowerGroup,
			_prebendUpperTool = _prebendUpperTool,
			_prebendLowerTool = _prebendLowerTool
		};
	}
}

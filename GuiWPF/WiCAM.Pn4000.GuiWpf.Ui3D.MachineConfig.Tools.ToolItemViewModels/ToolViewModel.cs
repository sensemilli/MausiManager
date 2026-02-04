using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public abstract class ToolViewModel : ToolItemViewModel
{
	protected readonly IUnitConverter _unitConverter;

	protected readonly IGlobalToolStorage _toolStorage;

	protected FrameworkElement _imageProfile;

	protected bool _isChanged;

	protected int? _id;

	protected string _name;

	protected int _priority;

	protected double _workingHeight;

	protected double _maxAllowableForcePerLengthUnit;

	protected bool _implemented;

	protected bool _isFoldTool;

	protected MultiToolViewModel _multiTool;

	private bool _flippedByDefault;

	private AllowedFlippedStates _flippingAllowed;

	protected InstallationDirection _plugInstallationDirection;

	protected Vector3d _plugInstallationOffset;

	[Browsable(false)]
	public bool IsChanged => _isChanged;

	[Browsable(false)]
	public MultiToolViewModel MultiTool
	{
		get
		{
			return _multiTool;
		}
		set
		{
			if (_multiTool != value)
			{
				_isChanged = true;
				_multiTool = value;
				NotifyPropertyChanged("MultiTool");
			}
		}
	}

	[Browsable(false)]
	public FrameworkElement ImageProfile
	{
		get
		{
			return _imageProfile;
		}
		set
		{
			if (_imageProfile != value)
			{
				_imageProfile = value;
				NotifyPropertyChanged("ImageProfile");
			}
		}
	}

	public int? ID => _id;

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

	public string Desc => $"{ID} - {Name}";

	public string MultiToolName
	{
		get
		{
			return MultiTool.Name;
		}
		set
		{
			MultiTool.Name = value;
		}
	}

	[Browsable(false)]
	public int Priority
	{
		get
		{
			return _priority;
		}
		set
		{
			if (_priority != value)
			{
				_priority = value;
				_isChanged = true;
				NotifyPropertyChanged("Priority");
			}
		}
	}

	public double WorkingHeight
	{
		get
		{
			return _unitConverter.Length.ToUi(_workingHeight, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_workingHeight != value)
			{
				_workingHeight = value;
				_isChanged = true;
				NotifyPropertyChanged("WorkingHeight");
			}
		}
	}

	public double MaxAllowableToolForcePerLengthUnit
	{
		get
		{
			return _maxAllowableForcePerLengthUnit;
		}
		set
		{
			if (_maxAllowableForcePerLengthUnit != value)
			{
				_maxAllowableForcePerLengthUnit = value;
				_isChanged = true;
				NotifyPropertyChanged("MaxAllowableToolForcePerLengthUnit");
			}
		}
	}

	public bool Implemented
	{
		get
		{
			return _implemented;
		}
		set
		{
			if (_implemented != value)
			{
				_implemented = value;
				_isChanged = true;
				NotifyPropertyChanged("Implemented");
			}
		}
	}

	[TranslationKey("l_popup.ToolsView.FlippedByDefault")]
	public bool FlippedByDefault
	{
		get
		{
			return _flippedByDefault;
		}
		set
		{
			_flippedByDefault = value;
			_isChanged = true;
		}
	}

	[TranslationKey("l_popup.ToolsView.FlippingAllowed")]
	public AllowedFlippedStates FlippingAllowed
	{
		get
		{
			return _flippingAllowed;
		}
		set
		{
			_flippingAllowed = value;
			_isChanged = true;
		}
	}

	[TranslationKey("l_popup.ToolsView.MountTypeID")]
	public int? MountTypeID
	{
		get
		{
			return MultiTool.MountTypeID;
		}
		set
		{
			if (MultiTool.MountTypeID != value)
			{
				MultiTool.MountTypeID = value;
				_isChanged = true;
				NotifyPropertyChanged("MountTypeID");
			}
		}
	}

	public double OffsetFront
	{
		get
		{
			return MultiTool.OffsetFrontMm;
		}
		set
		{
			if (MultiTool.OffsetFrontMm != value)
			{
				MultiTool.OffsetFrontMm = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetFront");
			}
		}
	}

	public double OffsetBack
	{
		get
		{
			return MultiTool.OffsetBackMm;
		}
		set
		{
			if (MultiTool.OffsetBackMm != value)
			{
				MultiTool.OffsetBackMm = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetBack");
			}
		}
	}

	[Browsable(false)]
	public double OffsetFrontMm
	{
		get
		{
			return MultiTool.OffsetFrontMm;
		}
		set
		{
			MultiTool.OffsetFrontMm = value;
			_isChanged = true;
		}
	}

	[Browsable(false)]
	public double OffsetBackMm
	{
		get
		{
			return MultiTool.OffsetBackMm;
		}
		set
		{
			MultiTool.OffsetBackMm = value;
			_isChanged = true;
		}
	}

	public bool IsFoldTool
	{
		get
		{
			return _isFoldTool;
		}
		set
		{
			if (_isFoldTool != value)
			{
				_isFoldTool = value;
				_isChanged = true;
				NotifyPropertyChanged("IsFoldTool");
			}
		}
	}

	public string ToolType => GetType().Name.ToString();

	public abstract IToolProfile? ToolProfile { get; }

	public InstallationDirection PlugInstallationDirection
	{
		get
		{
			return _plugInstallationDirection;
		}
		set
		{
			if (_plugInstallationDirection != value)
			{
				_plugInstallationDirection = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugInstallationDirection");
			}
		}
	}

	public double PlugInstallationOffsetXMm
	{
		get
		{
			return _plugInstallationOffset.X;
		}
		set
		{
			if (_plugInstallationOffset.X != value)
			{
				_plugInstallationOffset = new Vector3d(value, _plugInstallationOffset.Y, _plugInstallationOffset.Z);
				_isChanged = true;
				NotifyPropertyChanged("PlugInstallationOffsetXMm");
				NotifyPropertyChanged("PlugInstallationOffsetXUi");
			}
		}
	}

	public double PlugInstallationOffsetXUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_plugInstallationOffset.X, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_plugInstallationOffset.X != value)
			{
				_plugInstallationOffset = new Vector3d(value, _plugInstallationOffset.Y, _plugInstallationOffset.Z);
				_isChanged = true;
				NotifyPropertyChanged("PlugInstallationOffsetXUi");
				NotifyPropertyChanged("PlugInstallationOffsetXMm");
			}
		}
	}

	protected ToolViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolsStorage, MultiToolViewModel multiTool, double workingHeight)
	{
		_toolStorage = toolsStorage;
		_unitConverter = unitConverter;
		MultiTool = multiTool;
		_workingHeight = workingHeight;
	}

	protected ToolViewModel(IToolProfile profile, IUnitConverter unitConverter, IGlobalToolStorage toolsStorage, MultiToolViewModel multiTool)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolsStorage;
		_id = profile.ID;
		_name = profile.Name;
		_priority = profile.Priority;
		_workingHeight = profile.WorkingHeight;
		_maxAllowableForcePerLengthUnit = profile.MaxToolLoad;
		_implemented = profile.Implemented;
		MultiTool = multiTool;
		_isFoldTool = profile.ProfileType.HasFlag(ToolProfileTypes.Fold);
		_flippedByDefault = profile.FlippedByDefault;
		_flippingAllowed = profile.FlippingAllowed;
		_plugInstallationDirection = profile.MultiToolProfile.PlugInstallationDirection;
		_plugInstallationOffset = profile.MultiToolProfile.Plug.Position;
	}

	public virtual bool CanSave(IMessageDisplay messageDisplay)
	{
		return true;
	}

	public void Save(IToolProfile profile)
	{
		IMultiToolProfile multiToolProfile = MultiTool.MultiToolProfile;
		if (profile.MultiToolProfile != multiToolProfile)
		{
			if (profile.MultiToolProfile != null)
			{
				profile.MultiToolProfile.RemoveToolProfile(profile);
			}
			multiToolProfile.AddToolProfile(profile);
			profile.MultiToolProfile = multiToolProfile;
		}
		profile.Name = _name;
		profile.Priority = _priority;
		profile.WorkingHeight = _workingHeight;
		profile.MaxToolLoad = _maxAllowableForcePerLengthUnit;
		profile.FlippedByDefault = _flippedByDefault;
		profile.FlippingAllowed = _flippingAllowed;
		profile.MultiToolProfile.PlugInstallationDirection = _plugInstallationDirection;
		profile.MultiToolProfile.Plug.Position = _plugInstallationOffset;
		if (IsFoldTool)
		{
			profile.ProfileType |= ToolProfileTypes.Fold;
		}
		else if (profile.ProfileType.HasFlag(ToolProfileTypes.Fold))
		{
			profile.ProfileType &= ~ToolProfileTypes.Fold;
		}
	}

	public void AfterCopy(MultiToolViewModel multiTool)
	{
		_id = null;
		MultiTool = multiTool;
	}

	public abstract void Load2DPreview(Canvas previewImage2D, IDrawToolProfiles drawToolProfiles);
}

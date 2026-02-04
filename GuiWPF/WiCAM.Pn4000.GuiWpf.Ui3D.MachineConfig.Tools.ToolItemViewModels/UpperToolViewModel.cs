using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class UpperToolViewModel : ToolViewModel
{
	private ICommand _increasePriorityCommand;

	private ICommand _decreasePriorityCommand;

	private double? _angleRad;

	private double? _radius;

	private double? _hemOffsetX;

	private double? _widthHemmingFace;

	private UpperToolGroupViewModel _group;

	private bool _isRadiusTool;

	[Browsable(false)]
	public UpperToolGroupViewModel Group
	{
		get
		{
			return _group;
		}
		set
		{
			if (_group != value)
			{
				_group = value;
				_isChanged = true;
				NotifyPropertyChanged("Group");
			}
		}
	}

	[ReadOnly(true)]
	public string GroupName => Group.Name;

	[ReadOnly(true)]
	public int? GroupID => Group.ID;

	public double? Angle
	{
		get
		{
			return _angleRad;
		}
		set
		{
			if (_angleRad != value)
			{
				_angleRad = value;
				_isChanged = true;
				NotifyPropertyChanged("Angle");
				NotifyPropertyChanged("AngleUi");
			}
		}
	}

	[TranslationKey("l_popup.PunchesView.Angle")]
	public double? AngleUi
	{
		get
		{
			return _unitConverter.Angle.ToUi(_angleRad, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Angle.FromUi(value.Value, 4);
			}
			if (_angleRad != value)
			{
				_angleRad = value;
				_isChanged = true;
				NotifyPropertyChanged("AngleUi");
				NotifyPropertyChanged("Angle");
			}
		}
	}

	public double? Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			if (_radius != value)
			{
				_radius = value;
				_isChanged = true;
				NotifyPropertyChanged("Radius");
				NotifyPropertyChanged("RadiusUi");
			}
		}
	}

	[TranslationKey("l_popup.PunchesView.Radius")]
	public double? RadiusUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_radius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_radius != value)
			{
				_radius = value;
				_isChanged = true;
				NotifyPropertyChanged("RadiusUi");
				NotifyPropertyChanged("Radius");
			}
		}
	}

	[Browsable(false)]
	public double? HemOffsetXMm
	{
		get
		{
			return _hemOffsetX;
		}
		set
		{
			_hemOffsetX = value;
			_isChanged = true;
		}
	}

	[TranslationKey("l_popup.PunchesView.HemOffsetX")]
	public double? HemOffsetX
	{
		get
		{
			return _unitConverter.Length.ToUi(_hemOffsetX, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_hemOffsetX != value)
			{
				_hemOffsetX = value;
				_isChanged = true;
				NotifyPropertyChanged("HemOffsetX");
			}
		}
	}

	[Browsable(false)]
	public double? WidthHemmingFaceMm
	{
		get
		{
			return _widthHemmingFace;
		}
		set
		{
			_widthHemmingFace = value;
			_isChanged = true;
		}
	}

	[TranslationKey("l_popup.PunchesView.WidthHemmingFace")]
	public double? WidthHemmingFace
	{
		get
		{
			return _unitConverter.Length.ToUi(_widthHemmingFace, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_widthHemmingFace != value)
			{
				_widthHemmingFace = value;
				_isChanged = true;
				NotifyPropertyChanged("WidthHemmingFace");
			}
		}
	}

	public bool IsRadiusTool
	{
		get
		{
			return _isRadiusTool;
		}
		set
		{
			if (value != _isRadiusTool)
			{
				_isRadiusTool = value;
				_isChanged = true;
				NotifyPropertyChanged("IsRadiusTool");
			}
		}
	}

	[Browsable(false)]
	public ICommand IncreasePriorityCommand => _increasePriorityCommand ?? (_increasePriorityCommand = new RelayCommand((Action<object>)delegate
	{
		IncreasePriority();
	}));

	[Browsable(false)]
	public ICommand DecreasePriorityCommand => _decreasePriorityCommand ?? (_decreasePriorityCommand = new RelayCommand((Action<object>)delegate
	{
		DecreasePriority();
	}));

	public IPunchProfile? PunchProfile { get; private set; }

	public override IToolProfile? ToolProfile => PunchProfile;

	public UpperToolViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool, UpperToolGroupViewModel group, double? angleRad = 0.0, double? radius = 0.0, double? hemOffsetX = 0.0, double? widthHemmingFace = 0.0, double workingHeight = 0.0, bool isRadiusTool = false)
		: base(unitConverter, toolStorage, multiTool, workingHeight)
	{
		_group = group;
		_angleRad = angleRad;
		_radius = radius;
		_hemOffsetX = hemOffsetX;
		_widthHemmingFace = widthHemmingFace;
		_isChanged = true;
		_isRadiusTool = isRadiusTool;
	}

	public UpperToolViewModel(IPunchProfile profile, UpperToolGroupViewModel group, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool)
		: base(profile, unitConverter, toolStorage, multiTool)
	{
		_angleRad = profile.AngleRad;
		_radius = profile.Radius;
		_hemOffsetX = profile.HemOffsetX;
		_widthHemmingFace = profile.WidthHemmingFace;
		_group = group;
		_isRadiusTool = profile.IsRadiusTool;
		PunchProfile = profile;
	}

	public IPunchProfile Save(IPunchProfile punchProfile, IPunchGroup punchGroup)
	{
		IMultiToolProfile multiToolProfile = _toolStorage.GetMultiToolProfile(base.MultiTool.ID.Value);
		if (punchProfile == null)
		{
			punchProfile = _toolStorage.CreatePunchProfile();
		}
		if (punchProfile.MultiToolProfile != multiToolProfile)
		{
			if (punchProfile.MultiToolProfile != null)
			{
				punchProfile.MultiToolProfile.RemoveToolProfile(punchProfile);
			}
			multiToolProfile.AddToolProfile(punchProfile);
			punchProfile.MultiToolProfile = multiToolProfile;
		}
		PunchProfile = punchProfile;
		Save(punchProfile);
		punchProfile.Radius = _radius;
		punchProfile.HemOffsetX = _hemOffsetX;
		punchProfile.WidthHemmingFace = _widthHemmingFace;
		punchProfile.AngleRad = _angleRad;
		punchProfile.IsRadiusTool = _isRadiusTool;
		punchProfile.Group = punchGroup;
		return punchProfile;
	}

	public override void Load2DPreview(Canvas previewImage2D, IDrawToolProfiles drawToolProfiles)
	{
		ICadGeo activeGeometry = base.MultiTool.GetActiveGeometry();
		if (activeGeometry != null)
		{
			CadGeoHelper cadGeoHelper = new CadGeoHelper();
			CadGeoInfo cadGeoInfo = new CadGeoInfo
			{
				GeoElements = activeGeometry.GeoElements.ToList()
			};
			cadGeoHelper.FixCadGeo(cadGeoInfo);
			drawToolProfiles.LoadPreview2D(cadGeoInfo, 0.0, base.WorkingHeight, 0.0, previewImage2D);
		}
	}

	private void IncreasePriority()
	{
		if (base.Priority < 5)
		{
			base.Priority++;
		}
	}

	private void DecreasePriority()
	{
		if (base.Priority > 1)
		{
			base.Priority--;
		}
	}
}

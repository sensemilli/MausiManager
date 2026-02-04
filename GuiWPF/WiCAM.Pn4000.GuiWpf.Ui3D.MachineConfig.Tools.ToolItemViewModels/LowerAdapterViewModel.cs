using System.Linq;
using System.Windows.Controls;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class LowerAdapterViewModel : ToolViewModel
{
	private double? _springHeight;

	private double? _offsetInX;

	private AdapterXPositions _adapterXPosition;

	private AdapterDirections _adapterDirection;

	private int? _socketId;

	private string _socketCoupling;

	private double? _socketAngle;

	private double? _socketMinRadius;

	private double? _socketMaxRadius;

	private Vector3d _socketPos;

	private InstallationDirection _SocketInstallationDirection;

	public int? SocketId
	{
		get
		{
			return _socketId;
		}
		set
		{
			if (_socketId != value)
			{
				_socketId = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketId");
			}
		}
	}

	public string SocketCoupling
	{
		get
		{
			return _socketCoupling;
		}
		set
		{
			if (_socketCoupling != value)
			{
				_socketCoupling = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketCoupling");
			}
		}
	}

	public double? SocketAngleRad
	{
		get
		{
			return _socketAngle;
		}
		set
		{
			if (_socketAngle != value)
			{
				_socketAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketAngleRad");
				NotifyPropertyChanged("SocketAngleUi");
			}
		}
	}

	public double? SocketAngleUi
	{
		get
		{
			return _unitConverter.Angle.ToUi(_socketAngle, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Angle.FromUi(value.Value, 4);
			}
			if (_socketAngle != value)
			{
				_socketAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketAngleUi");
				NotifyPropertyChanged("SocketAngleRad");
			}
		}
	}

	public double? SocketMinRadiusMm
	{
		get
		{
			return _socketMinRadius;
		}
		set
		{
			if (_socketMinRadius != value)
			{
				_socketAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketMinRadiusMm");
				NotifyPropertyChanged("SocketMinRadiusUi");
			}
		}
	}

	public double? SocketMinRadiusUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_socketMinRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_socketMinRadius != value)
			{
				_socketAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketMinRadiusUi");
				NotifyPropertyChanged("SocketMinRadiusMm");
			}
		}
	}

	public double? SocketMaxRadiusMm
	{
		get
		{
			return _socketMaxRadius;
		}
		set
		{
			if (_socketMaxRadius != value)
			{
				_socketMaxRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketMaxRadiusMm");
				NotifyPropertyChanged("SocketMaxRadiusUi");
			}
		}
	}

	public double? SocketMaxRadiusUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_socketMaxRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_socketMaxRadius != value)
			{
				_socketMaxRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketMaxRadiusUi");
				NotifyPropertyChanged("SocketMaxRadiusMm");
			}
		}
	}

	public AdapterDirections AdapterDirection
	{
		get
		{
			return _adapterDirection;
		}
		set
		{
			if (_adapterDirection != value)
			{
				_adapterDirection = value;
				_isChanged = true;
				NotifyPropertyChanged("AdapterDirection");
			}
		}
	}

	public AdapterXPositions AdapterXPosition
	{
		get
		{
			return _adapterXPosition;
		}
		set
		{
			if (_adapterXPosition != value)
			{
				_adapterXPosition = value;
				_isChanged = true;
				NotifyPropertyChanged("AdapterXPosition");
			}
		}
	}

	public double? SpringHeightUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_springHeight, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_springHeight != value)
			{
				_springHeight = value;
				_isChanged = true;
				NotifyPropertyChanged("SpringHeightUi");
				NotifyPropertyChanged("SpringHeight");
			}
		}
	}

	public double? SpringHeight
	{
		get
		{
			return _springHeight;
		}
		set
		{
			if (_springHeight != value)
			{
				_springHeight = value;
				_isChanged = true;
				NotifyPropertyChanged("SpringHeight");
				NotifyPropertyChanged("SpringHeightUi");
			}
		}
	}

	public double? OffsetInX
	{
		get
		{
			return _offsetInX;
		}
		set
		{
			if (_offsetInX != value)
			{
				_offsetInX = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetInX");
			}
		}
	}

	public double? OffsetInXUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_offsetInX, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_offsetInX != value)
			{
				_offsetInX = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetInXUi");
				NotifyPropertyChanged("OffsetInX");
			}
		}
	}

	public InstallationDirection SocketInstallationDirection
	{
		get
		{
			return _SocketInstallationDirection;
		}
		set
		{
			if (_SocketInstallationDirection != value)
			{
				_SocketInstallationDirection = value;
				_isChanged = true;
				NotifyPropertyChanged("SocketInstallationDirection");
			}
		}
	}

	public double SocketPosXMm
	{
		get
		{
			return _socketPos.X;
		}
		set
		{
			if (_socketPos.X != value)
			{
				_socketPos = new Vector3d(value, _socketPos.Y, _socketPos.Z);
				_isChanged = true;
				NotifyPropertyChanged("SocketPosXMm");
			}
		}
	}

	public double SocketPosXUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_socketPos.X, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_socketPos.X != value)
			{
				_socketPos = new Vector3d(value, _socketPos.Y, _socketPos.Z);
				_isChanged = true;
				NotifyPropertyChanged("SocketPosXUi");
				NotifyPropertyChanged("SocketPosXMm");
			}
		}
	}

	public IToolProfile? AdapterProfile { get; private set; }

	public override IToolProfile? ToolProfile => AdapterProfile;

	public LowerAdapterViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool, double workingHeight = 0.0, double? springHeight = null, double? offsetInX = null)
		: base(unitConverter, toolStorage, multiTool, workingHeight)
	{
		_springHeight = springHeight;
		_adapterXPosition = AdapterXPositions.Free;
		_offsetInX = offsetInX;
	}

	public LowerAdapterViewModel(IAdapterProfile profile, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool)
		: base(profile, unitConverter, toolStorage, multiTool)
	{
		_adapterDirection = profile.AdapterDirection;
		_adapterXPosition = profile.AdapterXPosition;
		AdapterProfile = profile;
		_springHeight = profile.SpringHeight;
		_offsetInX = profile.OffsetInX;
		_socketId = profile.Socket.Id;
		_socketCoupling = profile.Socket.Coupling;
		_socketAngle = profile.Socket.Angle;
		_socketMinRadius = profile.Socket.MinRadius;
		_socketMaxRadius = profile.Socket.MaxRadius;
		_socketPos = profile.Socket.Position;
		_SocketInstallationDirection = profile.SocketInstallationDirection;
	}

	public void Save(IAdapterProfile adapterProfile)
	{
		if (adapterProfile == null)
		{
			adapterProfile = _toolStorage.CreateLowerAdapterProfile();
			base.MultiTool.MultiToolProfile.AddToolProfile(adapterProfile);
			adapterProfile.MultiToolProfile = base.MultiTool.MultiToolProfile;
		}
		AdapterProfile = adapterProfile;
		Save((IToolProfile)adapterProfile);
		adapterProfile.Socket.Id = _socketId;
		adapterProfile.Socket.Coupling = _socketCoupling;
		adapterProfile.Socket.Angle = _socketAngle;
		adapterProfile.Socket.MinRadius = _socketMinRadius;
		adapterProfile.Socket.MaxRadius = _socketMaxRadius;
		adapterProfile.Socket.Position = _socketPos;
		adapterProfile.SocketInstallationDirection = _SocketInstallationDirection;
		adapterProfile.AdapterDirection = _adapterDirection;
		adapterProfile.AdapterXPosition = _adapterXPosition;
		adapterProfile.SpringHeight = _springHeight;
		adapterProfile.OffsetInX = _offsetInX;
	}

	public override bool CanSave(IMessageDisplay messageDisplay)
	{
		if (!base.CanSave(messageDisplay))
		{
			return false;
		}
		if (!_socketId.HasValue)
		{
			messageDisplay.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.SaveErrorLowerAdapterNoSocketId", base.ID, base.Name);
			return false;
		}
		return true;
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
			drawToolProfiles.LoadPreview2D(cadGeoInfo, 0.0 - base.WorkingHeight, 0.0, 0.0, previewImage2D);
		}
	}
}

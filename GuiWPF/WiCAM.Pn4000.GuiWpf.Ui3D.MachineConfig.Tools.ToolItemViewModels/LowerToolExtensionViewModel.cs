using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class LowerToolExtensionViewModel : ToolViewModel
{
	private ICommand _increasePriorityCommand;

	private ICommand _decreasePriorityCommand;

	private readonly LowerToolGroupViewModel _group;

	private double _offsetInXForHemming;

	private double _workingHeightForHemming;

	private double? _depthForHemming;

	private double _foldGap;

	public double OffsetInXForHemmingMm
	{
		get
		{
			return _offsetInXForHemming;
		}
		set
		{
			if (_offsetInXForHemming != value)
			{
				_offsetInXForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetInXForHemmingMm");
				NotifyPropertyChanged("OffsetInXForHemmingUi");
			}
		}
	}

	[TranslationKey("l_popup.DiesView.HemOffsetX")]
	public double OffsetInXForHemmingUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_offsetInXForHemming, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_offsetInXForHemming != value)
			{
				_offsetInXForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetInXForHemmingUi");
				NotifyPropertyChanged("OffsetInXForHemmingMm");
			}
		}
	}

	public double HemWorkingHeightMm
	{
		get
		{
			return _workingHeightForHemming;
		}
		set
		{
			if (_workingHeightForHemming != value)
			{
				_workingHeightForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("HemWorkingHeightMm");
				NotifyPropertyChanged("HemWorkingHeightUi");
			}
		}
	}

	public double HemWorkingHeightUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_workingHeightForHemming, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_workingHeightForHemming != value)
			{
				_workingHeightForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("HemWorkingHeightUi");
				NotifyPropertyChanged("HemWorkingHeightMm");
			}
		}
	}

	public double DepthForHemmingMm
	{
		get
		{
			return _depthForHemming.GetValueOrDefault();
		}
		set
		{
			if (_depthForHemming != value)
			{
				_depthForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("DepthForHemmingMm");
				NotifyPropertyChanged("DepthForHemmingUi");
			}
		}
	}

	public double DepthForHemmingUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_depthForHemming.GetValueOrDefault(), 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_depthForHemming != value)
			{
				_depthForHemming = value;
				_isChanged = true;
				NotifyPropertyChanged("DepthForHemmingUi");
				NotifyPropertyChanged("DepthForHemmingMm");
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

	public IDieFoldExtentionProfile? DieFoldExtensionProfile { get; private set; }

	public override IToolProfile? ToolProfile => DieFoldExtensionProfile;

	public LowerToolExtensionViewModel(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool, LowerToolGroupViewModel group, double offsetInXForHemming = 0.0, double hemWorkingHeight = 0.0, double depthForHemming = 0.0, double workingHeight = 0.0)
		: base(unitConverter, toolStorage, multiTool, workingHeight)
	{
		_group = group;
		_offsetInXForHemming = offsetInXForHemming;
		_workingHeightForHemming = hemWorkingHeight;
		_depthForHemming = depthForHemming;
	}

	public LowerToolExtensionViewModel(IDieFoldExtentionProfile profile, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool, LowerToolGroupViewModel? group)
		: base(profile, unitConverter, toolStorage, multiTool)
	{
		_group = group;
		_offsetInXForHemming = profile.OffsetInX;
		_workingHeightForHemming = profile.WorkingHeight;
		_depthForHemming = profile.FoldDepth;
	}

	public IDieFoldExtentionProfile Save(IDieFoldExtentionProfile dieProfile)
	{
		if (dieProfile == null)
		{
			dieProfile = _toolStorage.CreateDieExtensionProfile();
			base.MultiTool.MultiToolProfile.AddToolProfile(dieProfile);
			dieProfile.MultiToolProfile = base.MultiTool.MultiToolProfile;
		}
		DieFoldExtensionProfile = dieProfile;
		Save((IToolProfile)dieProfile);
		dieProfile.OffsetInX = _offsetInXForHemming;
		dieProfile.FoldDepth = _depthForHemming;
		dieProfile.Group = _group?.DieGroup;
		return dieProfile;
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

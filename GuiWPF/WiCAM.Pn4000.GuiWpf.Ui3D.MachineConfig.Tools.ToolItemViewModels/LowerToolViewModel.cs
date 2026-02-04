using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class LowerToolViewModel : ToolViewModel
{
	private ICommand _increasePriorityCommand;

	private ICommand _decreasePriorityCommand;

	private double? _vWidth;

	private double? _vAngle;

	private double? _vDepth;

	private double? _vRadius;

	private double? _vGroundWidth;

	private double? _cornerRadius;

	private double? _tractrixRadiusRadius;

	private double? _transitionAngle;

	private double _offsetInX;

	private double? _foldGap;

	private double? _springHeight;

	private double? _foldDepth;

	private double? _stoneFactor;

	private double? _partFoldOffset;

	private LowerToolGroupViewModel _group;

	private VWidthTypes? _vWidthType;

	private int? _mountTypeExtensionFrontId;

	private int? _mountTypeExtensionBackId;

	private double? _widthHemmingFace;

	private double? _legLengthMin;

	private bool _isRollBend;

	private bool? _isValidGeometry;

	private double? _maxGeometryError;

	private double? _predictedVWidth;

	private double? _predictedVAngle;

	private double? _predictedCornerRadius;

	public MachineToolsViewModel MachineToolsViewModel { get; set; }

	[Browsable(false)]
	public LowerToolGroupViewModel Group
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

	public double? VWidth
	{
		get
		{
			return _unitConverter.Length.ToUi(_vWidth, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_vWidth != value)
			{
				_vWidth = value;
				_isChanged = true;
				NotifyPropertyChanged("VWidth");
			}
		}
	}

	public double? VAngle
	{
		get
		{
			return _unitConverter.Angle.ToUi(_vAngle, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Angle.FromUi(value.Value, 4);
			}
			if (_vAngle != value)
			{
				_vAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("VAngle");
			}
		}
	}

	public double? VDepth
	{
		get
		{
			return _unitConverter.Length.ToUi(_vDepth, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_vDepth != value)
			{
				_vDepth = value;
				_isChanged = true;
				NotifyPropertyChanged("VDepth");
			}
		}
	}

	public double? VRadius
	{
		get
		{
			return _unitConverter.Length.ToUi(_vRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_vRadius != value)
			{
				_vRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("VRadius");
			}
		}
	}

	public double? VGroundWidth
	{
		get
		{
			return _unitConverter.Length.ToUi(_vGroundWidth, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_vGroundWidth != value)
			{
				_vGroundWidth = value;
				_isChanged = true;
				NotifyPropertyChanged("VGroundWidth");
			}
		}
	}

	public double? CornerRadius
	{
		get
		{
			return _unitConverter.Length.ToUi(_cornerRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_cornerRadius != value)
			{
				_cornerRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("CornerRadius");
			}
		}
	}

	public double? TractrixRadius
	{
		get
		{
			return _unitConverter.Length.ToUi(_tractrixRadiusRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_tractrixRadiusRadius != value)
			{
				_tractrixRadiusRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("TractrixRadius");
			}
		}
	}

	public double? TransitionAngle
	{
		get
		{
			return _unitConverter.Angle.ToUi(_transitionAngle, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Angle.FromUi(value.Value, 4);
			}
			if (_transitionAngle != value)
			{
				_transitionAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("TransitionAngle");
			}
		}
	}

	[TranslationKey("l_popup.DiesView.OffsetX")]
	public double OffsetInX
	{
		get
		{
			return _unitConverter.Length.ToUi(_offsetInX, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_offsetInX != value)
			{
				_offsetInX = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetInX");
			}
		}
	}

	public double? FoldGap
	{
		get
		{
			return _unitConverter.Length.ToUi(_foldGap, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_foldGap != value)
			{
				_foldGap = value;
				_isChanged = true;
				NotifyPropertyChanged("FoldGap");
			}
		}
	}

	public double? FoldDepth
	{
		get
		{
			return _unitConverter.Length.ToUi(_foldDepth, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_foldDepth != value)
			{
				_foldDepth = value;
				_isChanged = true;
				NotifyPropertyChanged("FoldDepth");
			}
		}
	}

	public double? SpringHeight
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
				NotifyPropertyChanged("SpringHeight");
			}
		}
	}

	public double? PartFoldOffset
	{
		get
		{
			return _unitConverter.Length.ToUi(_partFoldOffset, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_partFoldOffset != value)
			{
				_partFoldOffset = value;
				_isChanged = true;
				NotifyPropertyChanged("PartFoldOffset");
			}
		}
	}

	public double? StoneFactor
	{
		get
		{
			return _stoneFactor;
		}
		set
		{
			if (_stoneFactor != value)
			{
				_stoneFactor = value;
				_isChanged = true;
				NotifyPropertyChanged("StoneFactor");
			}
		}
	}

	public VWidthTypes? VWidthType
	{
		get
		{
			return _vWidthType;
		}
		set
		{
			if (_vWidthType != value)
			{
				_vWidthType = value;
				_isChanged = true;
				NotifyPropertyChanged("VWidthType");
			}
		}
	}

	public int? MountTypeExtensionFrontId
	{
		get
		{
			return _mountTypeExtensionFrontId;
		}
		set
		{
			if (_mountTypeExtensionFrontId != value)
			{
				_mountTypeExtensionFrontId = value;
				_isChanged = true;
				NotifyPropertyChanged("MountTypeExtensionFrontId");
			}
		}
	}

	public int? MountTypeExtensionBackId
	{
		get
		{
			return _mountTypeExtensionBackId;
		}
		set
		{
			if (_mountTypeExtensionBackId != value)
			{
				_mountTypeExtensionBackId = value;
				_isChanged = true;
				NotifyPropertyChanged("MountTypeExtensionBackId");
			}
		}
	}

	public int? FrontAdapterID { get; set; }

	public int? BackAdapterID { get; set; }

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

	[Browsable(false)]
	public ICommand CalculateVDepthCommand => _decreasePriorityCommand ?? (_decreasePriorityCommand = new RelayCommand((Action<object>)delegate
	{
		CalculateVDepth();
	}));

	[Browsable(false)]
	public ICommand CheckValidToolGeometryCommand => new RelayCommand((Action<object>)delegate
	{
		ValidateGeometry();
	});

	public IDieProfile? DieProfile { get; private set; }

	public override IToolProfile? ToolProfile => DieProfile;

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

	public bool IsRollBend
	{
		get
		{
			return _isRollBend;
		}
		set
		{
			if (value != _isRollBend)
			{
				_isRollBend = value;
				_isChanged = true;
				NotifyPropertyChanged("IsRollBend");
			}
		}
	}

	public double? LegLengthMinUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_legLengthMin, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_legLengthMin != value)
			{
				_legLengthMin = value;
				_isChanged = true;
				NotifyPropertyChanged("LegLengthMinUi");
			}
		}
	}

	public bool? IsValidGeometry
	{
		get
		{
			return _isValidGeometry;
		}
		set
		{
			if (value != _isValidGeometry)
			{
				_isValidGeometry = value;
				_isChanged = true;
				NotifyPropertyChanged("IsValidGeometry");
			}
		}
	}

	public double? MaxGeometryError
	{
		get
		{
			return _maxGeometryError;
		}
		set
		{
			if (value != _maxGeometryError)
			{
				_maxGeometryError = value;
				_isChanged = true;
				NotifyPropertyChanged("MaxGeometryError");
			}
		}
	}

	public double? PredictedVWidth
	{
		get
		{
			return _unitConverter.Length.ToUi(_predictedVWidth, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_predictedVWidth != value)
			{
				_predictedVWidth = value;
				_isChanged = true;
				NotifyPropertyChanged("PredictedVWidth");
			}
		}
	}

	public double? PredictedVAngle
	{
		get
		{
			return _unitConverter.Length.ToUi(_predictedVAngle, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_predictedVAngle != value)
			{
				_predictedVAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("PredictedVAngle");
			}
		}
	}

	public double? PredictedCornerRadius
	{
		get
		{
			return _unitConverter.Length.ToUi(_predictedCornerRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_predictedCornerRadius != value)
			{
				_predictedCornerRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("PredictedCornerRadius");
			}
		}
	}

	public LowerToolViewModel(VWidthTypes vWidthType, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool, LowerToolGroupViewModel group, double offsetInX = 0.0, double workingHeight = 0.0, double? vWidth = null, double? vAngle = null, double? vDepth = null, double? cornerRadius = null, double? foldGap = null, double? widthHemmingFace = null, double? springHeight = null, double? partFoldOffset = null, double? vRadius = null, double? vGroundWidth = null, bool isRollBend = false, double? tractrixRadius = null, double? transitionAngleRad = null)
		: base(unitConverter, toolStorage, multiTool, workingHeight)
	{
		_vWidthType = vWidthType;
		_group = group;
		_vWidth = vWidth;
		_vAngle = vAngle;
		_cornerRadius = cornerRadius;
		_offsetInX = offsetInX;
		_foldGap = foldGap;
		_widthHemmingFace = widthHemmingFace;
		_springHeight = springHeight;
		_partFoldOffset = partFoldOffset;
		_isChanged = true;
		_vDepth = vDepth;
		_vRadius = vRadius;
		_vGroundWidth = vGroundWidth;
		_isRollBend = isRollBend;
		_transitionAngle = transitionAngleRad;
		_tractrixRadiusRadius = tractrixRadius;
		_transitionAngle = transitionAngleRad;
	}

	public LowerToolViewModel(IDieProfile profile, LowerToolGroupViewModel group, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, MultiToolViewModel multiTool)
		: base(profile, unitConverter, toolStorage, multiTool)
	{
		IToolProfile toolProfile = multiTool.MultiToolProfile?.ToolProfiles.FirstOrDefault((IToolProfile x) => x.IsAdapter && x.AdapterDirection == AdapterDirections.Front);
		IToolProfile toolProfile2 = multiTool.MultiToolProfile?.ToolProfiles.FirstOrDefault((IToolProfile x) => x.IsAdapter && x.AdapterDirection == AdapterDirections.Back);
		_vWidth = profile.VWidth;
		_vAngle = profile.VAngleRad;
		_vDepth = profile.VDepth;
		_vRadius = profile.VRadius;
		_vGroundWidth = profile.VGroundWidth;
		_cornerRadius = profile.CornerRadius;
		_offsetInX = profile.OffsetInX;
		_foldGap = profile.FoldGap;
		_stoneFactor = profile.StoneFactor;
		_vWidthType = profile.VWidthType;
		_springHeight = profile.SpringHeight;
		_partFoldOffset = profile.PartFoldOffset;
		_group = group;
		_mountTypeExtensionBackId = toolProfile2?.MountTypeChildId;
		_mountTypeExtensionFrontId = toolProfile?.MountTypeChildId;
		_isFoldTool = profile.IsFoldTool;
		_widthHemmingFace = profile.WidthHemmingFace;
		_isRollBend = profile.IsRollBend;
		_legLengthMin = profile.LegLengthMin;
		_tractrixRadiusRadius = profile.TractrixRadius;
		_transitionAngle = profile.TransitionAngle;
		FrontAdapterID = toolProfile?.ID;
		BackAdapterID = toolProfile2?.ID;
		DieProfile = profile;
	}

	public IDieProfile Save(IDieProfile dieProfile, IToolProfile frontAdapterProfile, IToolProfile backAdapterProfile, IDieGroup dieGroup)
	{
		IMultiToolProfile multiToolProfile = _toolStorage.GetMultiToolProfile(base.MultiTool.ID.Value);
		if (dieProfile == null)
		{
			dieProfile = _toolStorage.CreateDieProfile();
			multiToolProfile.AddToolProfile(dieProfile);
			dieProfile.MultiToolProfile = base.MultiTool.MultiToolProfile;
		}
		DieProfile = dieProfile;
		if (frontAdapterProfile == null && MountTypeExtensionFrontId.HasValue)
		{
			frontAdapterProfile = _toolStorage.CreateLowerAdapterProfile();
			frontAdapterProfile.MountTypeChildId = MountTypeExtensionFrontId;
			multiToolProfile.AddToolProfile(frontAdapterProfile);
			frontAdapterProfile.MultiToolProfile = multiToolProfile;
			frontAdapterProfile.AdapterDirection = AdapterDirections.Front;
			frontAdapterProfile.AdapterXPosition = AdapterXPositions.Center;
			frontAdapterProfile.Name = _name + "_front_adapter";
			frontAdapterProfile.WorkingHeight = _workingHeight;
			FrontAdapterID = frontAdapterProfile.ID;
		}
		else if (frontAdapterProfile != null && MountTypeExtensionFrontId != frontAdapterProfile.MountTypeChildId)
		{
			frontAdapterProfile.MountTypeChildId = MountTypeExtensionFrontId;
			frontAdapterProfile.WorkingHeight = _workingHeight;
		}
		else if (frontAdapterProfile != null && !MountTypeExtensionFrontId.HasValue)
		{
			multiToolProfile.RemoveToolProfile(frontAdapterProfile);
			_toolStorage.DeleteLowerAdapterProfile(frontAdapterProfile.ID);
		}
		if (backAdapterProfile == null && MountTypeExtensionBackId.HasValue)
		{
			backAdapterProfile = _toolStorage.CreateLowerAdapterProfile();
			backAdapterProfile.MountTypeChildId = MountTypeExtensionBackId;
			multiToolProfile.AddToolProfile(backAdapterProfile);
			backAdapterProfile.MultiToolProfile = multiToolProfile;
			backAdapterProfile.AdapterDirection = AdapterDirections.Back;
			backAdapterProfile.AdapterXPosition = AdapterXPositions.Center;
			backAdapterProfile.Name = _name + "_back_adapter";
			backAdapterProfile.WorkingHeight = _workingHeight;
			BackAdapterID = backAdapterProfile.ID;
		}
		else if (backAdapterProfile != null && MountTypeExtensionBackId != backAdapterProfile.MountTypeChildId)
		{
			backAdapterProfile.MountTypeChildId = MountTypeExtensionBackId;
			backAdapterProfile.WorkingHeight = _workingHeight;
		}
		else if (backAdapterProfile != null && !MountTypeExtensionBackId.HasValue)
		{
			multiToolProfile.RemoveToolProfile(backAdapterProfile);
			_toolStorage.DeleteLowerAdapterProfile(backAdapterProfile.ID);
		}
		Save(dieProfile);
		dieProfile.VWidth = _vWidth;
		dieProfile.VAngleRad = _vAngle;
		dieProfile.VDepth = _vDepth;
		dieProfile.VRadius = _vRadius;
		dieProfile.VGroundWidth = _vGroundWidth;
		dieProfile.CornerRadius = _cornerRadius;
		dieProfile.StoneFactor = _stoneFactor;
		dieProfile.FoldGap = _foldGap;
		dieProfile.SpringHeight = _springHeight;
		dieProfile.OffsetInX = _offsetInX;
		dieProfile.PartFoldOffset = _partFoldOffset;
		dieProfile.WidthHemmingFace = _widthHemmingFace;
		dieProfile.VWidthType = _vWidthType;
		dieProfile.IsRollBend = _isRollBend;
		dieProfile.LegLengthMin = _legLengthMin;
		dieProfile.TractrixRadius = _tractrixRadiusRadius;
		dieProfile.TransitionAngle = _transitionAngle;
		dieProfile.Group = dieGroup;
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
			drawToolProfiles.LoadPreview2D(cadGeoInfo, 0.0 - base.WorkingHeight, 0.0, OffsetInX, previewImage2D);
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

	private void CalculateVDepth()
	{
		VDepth = 0.0;
		List<Polygon2D> list = CadGeoLoader.LoadCadGeo2DContours(base.MultiTool.GeometryFileFull);
		if (list.Count == 1)
		{
			(double, double, SpacialRelation) tuple = (from x in list[0].Intersect(new Line2D(new Vector2d(_offsetInX, 0.0), new Vector2d(_offsetInX, 0.0 - _workingHeight)))
				where x.relation == SpacialRelation.Inside
				orderby x.start
				select x).FirstOrDefault();
			if (tuple.Item3 == SpacialRelation.Inside)
			{
				VDepth = tuple.Item1 * _workingHeight;
			}
		}
	}

	public override void ValidateGeometry()
	{
		if (DieProfile == null)
		{
			IsValidGeometry = false;
			return;
		}
		try
		{
			GeometryVerifierDieParameters profile = new GeometryVerifierDieParameters
			{
				VWidth = VWidth,
				VAngleRad = VAngle * Math.PI / 180.0,
				VAngleDeg = VAngle,
				CornerRadius = CornerRadius,
				VWidthType = VWidthType,
				StoneFactor = StoneFactor,
				IsFoldTool = _isFoldTool
			};
			double? maxError;
			double? predictedVWidth;
			double? predictedVAngleRad;
			double? predictedCornerRadius;
			List<GeoSegment2D> svgSegments;
			bool? isValidGeometry = MachineToolsViewModel.ToolGeometryVerifier.Verify(profile, base.MultiTool.GeometryFileFull, out maxError, out predictedVWidth, out predictedVAngleRad, out predictedCornerRadius, out svgSegments);
			MaxGeometryError = maxError;
			IsValidGeometry = isValidGeometry;
			PredictedCornerRadius = predictedCornerRadius;
			PredictedVAngle = predictedVAngleRad * 180.0 / Math.PI;
			PredictedVWidth = predictedVWidth;
		}
		catch (Exception)
		{
			IsValidGeometry = false;
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public class MultiToolViewModel : ViewModelBase
{
	private readonly IGlobalToolStorage _toolStorage;

	private AliasMultiToolViewModel _aliasMultiToolViewModel;

	private readonly MachineToolsViewModel _machineToolsViewModel;

	protected readonly IUnitConverter _unitConverter;

	private IMultiToolProfile? _multiTool;

	private bool _isChanged;

	private ICadGeo? _geometry;

	private double _offsetBack;

	private double _offsetFront;

	private string name;

	private int? _plugId;

	private string _plugCoupling;

	private double? _plugAngle;

	private double? _plugRadius;

	private Vector3d _plugPos;

	private byte[]? _geometryNewContent;

	public IMultiToolProfile? MultiToolProfile => _multiTool;

	public AliasMultiToolViewModel AliasMultiToolViewModel
	{
		get
		{
			return _aliasMultiToolViewModel;
		}
		set
		{
			_aliasMultiToolViewModel = value;
		}
	}

	public int ToolProfileAmountWithAlias { get; }

	public List<ToolPieceViewModel> ToolPieces { get; } = new List<ToolPieceViewModel>();

	public int? ID { get; private set; }

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (name != value)
			{
				name = value;
				_isChanged = true;
			}
		}
	}

	[ReadOnly(true)]
	public string GeometryFile { get; private set; }

	[ReadOnly(true)]
	public string GeometryFileFull { get; private set; }

	public ICadGeo? Geometry
	{
		get
		{
			return _geometry;
		}
		set
		{
			if (_geometry != value)
			{
				_geometry = value;
				_isChanged = true;
			}
		}
	}

	public int? PlugId
	{
		get
		{
			return _plugId;
		}
		set
		{
			if (_plugId != value)
			{
				_plugId = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugId");
			}
		}
	}

	public string PlugCoupling
	{
		get
		{
			return _plugCoupling;
		}
		set
		{
			if (_plugCoupling != value)
			{
				_plugCoupling = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugCoupling");
			}
		}
	}

	public double? PlugAngleRad
	{
		get
		{
			return _plugAngle;
		}
		set
		{
			if (_plugAngle != value)
			{
				_plugAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugAngleRad");
				NotifyPropertyChanged("PlugAngleUi");
			}
		}
	}

	public double? PlugAngleUi
	{
		get
		{
			return _unitConverter.Angle.ToUi(_plugAngle, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Angle.FromUi(value.Value, 4);
			}
			if (_plugAngle != value)
			{
				_plugAngle = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugAngleUi");
				NotifyPropertyChanged("PlugAngleRad");
			}
		}
	}

	public double? PlugRadiusMm
	{
		get
		{
			return _plugRadius;
		}
		set
		{
			if (_plugRadius != value)
			{
				_plugRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugRadiusMm");
				NotifyPropertyChanged("PlugRadiusUi");
			}
		}
	}

	public double? PlugRadiusUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_plugRadius, 4);
		}
		set
		{
			if (value.HasValue)
			{
				value = _unitConverter.Length.FromUi(value.Value, 4);
			}
			if (_plugRadius != value)
			{
				_plugRadius = value;
				_isChanged = true;
				NotifyPropertyChanged("PlugRadiusUi");
				NotifyPropertyChanged("PlugRadiusMm");
			}
		}
	}

	public double OffsetFrontMm
	{
		get
		{
			return _offsetFront;
		}
		set
		{
			if (_offsetFront != value)
			{
				_offsetFront = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetFrontMm");
				NotifyPropertyChanged("OffsetFrontUi");
			}
		}
	}

	public double OffsetFrontUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_offsetFront, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_offsetFront != value)
			{
				_offsetFront = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetFrontUi");
				NotifyPropertyChanged("OffsetFrontMm");
			}
		}
	}

	public double OffsetBackMm
	{
		get
		{
			return _offsetBack;
		}
		set
		{
			if (_offsetBack != value)
			{
				_offsetBack = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetBackMm");
				NotifyPropertyChanged("OffsetBackUi");
			}
		}
	}

	public double OffsetBackUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_offsetBack, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_offsetBack != value)
			{
				_offsetBack = value;
				_isChanged = true;
				NotifyPropertyChanged("OffsetBackUi");
				NotifyPropertyChanged("OffsetBackMm");
			}
		}
	}

	[TranslationKey("l_popup.ToolsView.MountTypeID")]
	public int? MountTypeID
	{
		get
		{
			return _plugId;
		}
		set
		{
			if (_plugId != value)
			{
				_plugId = value;
				_isChanged = true;
				NotifyPropertyChanged("MountTypeID");
				NotifyPropertyChanged("PlugId");
			}
		}
	}

	[Browsable(false)]
	public bool IsChanged => _isChanged;

	public MultiToolViewModel(IGlobalToolStorage toolStorage, IUnitConverter unitConverter, string fileName, MachineToolsViewModel machineToolsViewModel, AliasMultiToolViewModel alias)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_machineToolsViewModel = machineToolsViewModel;
		_aliasMultiToolViewModel = alias;
		GeometryFile = SanitizeNewFilename(fileName);
		ID = null;
	}

	internal MultiToolViewModel(IMultiToolProfile multiTool, IUnitConverter unitConverter, IGlobalToolStorage toolStorage, AliasMultiToolViewModel aliasMultiToolViewModel, MachineToolsViewModel machineToolsViewModel)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_aliasMultiToolViewModel = aliasMultiToolViewModel;
		_machineToolsViewModel = machineToolsViewModel;
		_multiTool = multiTool;
		ID = multiTool.ID;
		Name = multiTool.CustomMultiToolName;
		GeometryFile = multiTool.GeometryFile;
		GeometryFileFull = multiTool.GeometryFileFull;
		OffsetFrontMm = multiTool.OffsetFront;
		OffsetBackMm = multiTool.OffsetBack;
		_plugId = multiTool.Plug.Id;
		_plugCoupling = multiTool.Plug.Coupling;
		_plugAngle = multiTool.Plug.Angle;
		_plugRadius = multiTool.Plug.Radius;
		_plugPos = multiTool.Plug.Position;
	}

	public void Save(IMultiToolProfile multiTool)
	{
		_multiTool = multiTool ?? _toolStorage.CreateMultiToolProfile(createAlias: true);
		ID = _multiTool.ID;
		if (Geometry != null)
		{
			CadGeoHelper cadGeoHelper = new CadGeoHelper();
			byte[] bytes = Encoding.ASCII.GetBytes(cadGeoHelper.ConvertToString(Geometry));
			_geometryNewContent = bytes;
		}
		if (_geometryNewContent != null)
		{
			string text = GeometryFile;
			if (string.IsNullOrEmpty(text))
			{
				text = "Tool";
			}
			_multiTool.GeometryFile = _toolStorage.SaveGeometry(text, _geometryNewContent, (ToolProfileTypes)0);
		}
		_multiTool.CustomMultiToolName = Name;
		_multiTool.OffsetFront = OffsetFrontMm;
		_multiTool.OffsetBack = OffsetBackMm;
		_multiTool.Plug.Id = _plugId;
		_multiTool.Plug.Coupling = _plugCoupling;
		_multiTool.Plug.Angle = _plugAngle;
		_multiTool.Plug.Radius = _plugRadius;
		_multiTool.Plug.Position = _plugPos;
		if (_multiTool.Aliases?.ID != _aliasMultiToolViewModel.Id)
		{
			_multiTool.Aliases?.RemoveAliasMultiToolProfile(_multiTool);
			_multiTool.Aliases = _aliasMultiToolViewModel.Profile;
			_multiTool.Aliases.AddAliasMultiToolProfile(_multiTool);
		}
	}

	public ICadGeo GetActiveGeometry()
	{
		if (Geometry != null)
		{
			return Geometry;
		}
		if (!string.IsNullOrEmpty(GeometryFileFull))
		{
			return new CadGeoHelper().ReadCadgeo(GeometryFileFull);
		}
		return null;
	}

	private string SanitizeNewFilename(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (string current, char c) => current.Replace(c, '_'));
			fileName = fileName.Replace(".WZG", "");
			fileName = fileName.Replace(".wzg", "");
			fileName = fileName.Replace(".c3mo", "");
			fileName = fileName.Trim();
			return fileName;
		}
		return null;
	}

	public bool SetGeometryFile(string newGeometryFileFull)
	{
		if (File.Exists(newGeometryFileFull) && !newGeometryFileFull.ToLower().EndsWith(".c3mo"))
		{
			_geometryNewContent = File.ReadAllBytes(newGeometryFileFull);
			GeometryFileFull = newGeometryFileFull;
			GeometryFile = new FileInfo(newGeometryFileFull).Name;
			NotifyPropertyChanged("GeometryFile");
			_isChanged = true;
			return true;
		}
		return false;
	}

	public bool SetGeometryData(byte[] geometryNewContent, string name)
	{
		_geometryNewContent = geometryNewContent;
		GeometryFile = name;
		NotifyPropertyChanged("GeometryFile");
		_isChanged = true;
		return true;
	}

	public bool SetGeometryData(ICadGeo geometry, string name)
	{
		Geometry = geometry;
		GeometryFile = name;
		NotifyPropertyChanged("GeometryFile");
		_isChanged = true;
		return true;
	}
}

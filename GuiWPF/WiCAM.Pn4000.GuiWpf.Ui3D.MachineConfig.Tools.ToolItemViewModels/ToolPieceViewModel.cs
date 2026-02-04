using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Serialization;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;

public abstract class ToolPieceViewModel : ToolItemViewModel
{
	private readonly MachineToolsViewModel _machineToolsViewModel;

	private ToolListViewModel? _currentToolList;

	protected bool _isChanged;

	public bool IsDeleted;

	private int? _toolID;

	private string _geometryFile;

	private string _geometryFileFull;

	private bool _isSpecialTool;

	private double _length;

	private double? _maxAllowableForcePerLengthUnit;

	private int _amount;

	private int _totalAmount;

	private Model? _geometry3D;

	private MultiToolViewModel _multiTool;

	private byte[]? _geometryNewContent;

	private IConfigProvider _configProvider => _machineToolsViewModel.ConfigProvider;

	protected IGlobalToolStorage _toolStorage => _machineToolsViewModel.ToolStorage;

	private IUnitConverter _unitConverter => _machineToolsViewModel.UnitConverter;

	[Browsable(false)]
	public Model? Geometry3D
	{
		get
		{
			return _geometry3D;
		}
		set
		{
			if (_geometry3D != value)
			{
				_geometry3D = value;
				NotifyPropertyChanged("Geometry3D");
				_isChanged = true;
			}
		}
	}

	[Browsable(false)]
	public ToolListViewModel CurrentToolList
	{
		get
		{
			return _currentToolList;
		}
		set
		{
			if (_currentToolList != value)
			{
				_currentToolList = value;
				_amount = _currentToolList?.GetAmount(this) ?? 0;
				NotifyPropertyChanged("CurrentToolList");
				NotifyPropertyChanged("Amount");
			}
		}
	}

	[Browsable(false)]
	public MultiToolViewModel MultiTool
	{
		get
		{
			return _multiTool;
		}
		private set
		{
			_multiTool?.ToolPieces.Remove(this);
			_multiTool = value;
			_multiTool?.ToolPieces.Add(this);
		}
	}

	public List<AliasPieceViewModel> AliasPieces { get; } = new List<AliasPieceViewModel>();

	[Browsable(false)]
	public bool IsChanged => _isChanged;

	public double? MaxAllowableToolForce
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
				NotifyPropertyChanged("MaxAllowableToolForce");
			}
		}
	}

	[ReadOnly(true)]
	public int? ID => _toolID;

	public int? AliasId { get; private set; }

	public string AliasIds { get; private set; }

	public int AliasConnectedPieceAmount { get; private set; }

	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			if (_amount != value && _currentToolList != null)
			{
				_currentToolList.SetAmount(this, value);
				_amount = value;
				_isChanged = true;
				NotifyPropertyChanged("Amount");
			}
		}
	}

	[Browsable(false)]
	public int TotalAmount
	{
		get
		{
			return _totalAmount;
		}
		set
		{
			if (_totalAmount != value)
			{
				_totalAmount = value;
				NotifyPropertyChanged("TotalAmount");
			}
		}
	}

	[Browsable(false)]
	public string GeometryFileFull => _geometryFileFull;

	[ReadOnly(false)]
	public string GeometryFile => _geometryFile;

	public double Length
	{
		get
		{
			return _unitConverter.Length.ToUi(_length, 4);
		}
		set
		{
			value = _unitConverter.Length.FromUi(value, 4);
			if (_length != value)
			{
				_length = value;
				_isChanged = true;
				NotifyPropertyChanged("Length");
			}
		}
	}

	protected ToolPieceViewModel(MachineToolsViewModel machineToolsViewModel, ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length, int amount)
	{
		_machineToolsViewModel = machineToolsViewModel;
		_currentToolList = currentToolList;
		MultiTool = multiTool;
		_length = length;
		Amount = amount;
		_geometryFile = SanitizeNewFilename(fileName);
		SetAliasPieces(_machineToolsViewModel.CreateAliasPiece().ToIEnumerable());
	}

	protected ToolPieceViewModel(MachineToolsViewModel machineToolsViewModel, ToolListViewModel currentToolList, MultiToolViewModel multiTool, IEnumerable<AliasPieceViewModel> aliasPieces, int totalAmount, IToolPieceProfile profile)
	{
		_machineToolsViewModel = machineToolsViewModel;
		_toolID = profile.ToolId;
		if (profile.Aliases.Count != 1)
		{
			throw new Exception("machine config ui only supports pieces with 1 alias!");
		}
		AliasId = profile.Aliases.First().Id;
		_geometryFileFull = profile.GeometryFileFull;
		_geometryFile = profile.GeometryFile;
		_isSpecialTool = profile.IsSpecialTool;
		_length = profile.Length;
		_maxAllowableForcePerLengthUnit = profile.MaxToolLoadOfPiece;
		MultiTool = multiTool;
		_totalAmount = totalAmount;
		CurrentToolList = currentToolList;
		SetAliasPieces(aliasPieces);
	}

	public void Save(IToolPieceProfile toolPieceProfile)
	{
		_toolID = toolPieceProfile.ToolId;
		if (toolPieceProfile.Aliases.Count != 1)
		{
			throw new Exception("machine config ui only supports pieces with 1 alias!");
		}
		AliasId = toolPieceProfile.Aliases.First().Id;
		toolPieceProfile.Length = _length;
		toolPieceProfile.MaxToolLoadOfPiece = _maxAllowableForcePerLengthUnit;
		HashSet<IAliasPieceProfile> hashSet = toolPieceProfile.Aliases.ToHashSet();
		foreach (IAliasPieceProfile item in AliasPieces.Select((AliasPieceViewModel x) => x.Alias).ToHashSet())
		{
			if (!hashSet.Remove(item))
			{
				toolPieceProfile.AddAliasToolPieceProfile(item);
				item.AddToolPieceProfile(toolPieceProfile);
			}
		}
		foreach (IAliasPieceProfile item2 in hashSet)
		{
			toolPieceProfile.RemoveAliasToolPieceProfile(item2);
			item2.RemoveToolPieceProfile(toolPieceProfile);
		}
		if (GeometryFile == string.Empty)
		{
			toolPieceProfile.GeometryFile = null;
			toolPieceProfile.GeometryFileFull = null;
			return;
		}
		if (Geometry3D != null)
		{
			ModelConverter modelConverter = new ModelConverter();
			byte[] geometryNewContent = modelConverter.Serialize(modelConverter.Convert(Geometry3D));
			_geometryNewContent = geometryNewContent;
		}
		if (_geometryNewContent != null)
		{
			string text = GeometryFile;
			if (string.IsNullOrEmpty(text))
			{
				text = "ToolPiece.c3mo";
			}
			toolPieceProfile.GeometryFile = _toolStorage.SaveGeometry(text, _geometryNewContent, (ToolProfileTypes)0);
		}
	}

	public void AfterCopy()
	{
		_toolID = null;
		AliasId = null;
		_amount = 0;
		_totalAmount = 0;
	}

	public Model GetActiveModel()
	{
		Model model = null;
		double length = Length;
		double num = 0.0;
		if (Geometry3D != null)
		{
			model = Geometry3D;
		}
		if (!string.IsNullOrEmpty(GeometryFileFull) && GeometryFileFull.EndsWith(".c3mo"))
		{
			model = ModelSerializer.Deserialize(GeometryFileFull);
			if (model != null)
			{
				model.Transform = Matrix4d.Identity;
				model.Transform *= Matrix4d.RotationX(-Math.PI / 2.0);
				model.Transform *= Matrix4d.RotationZ(-Math.PI / 2.0);
				model.Transform *= Matrix4d.Translation((0.0 - length) / 2.0, 0.0, num / 2.0);
			}
		}
		else if (model == null)
		{
			ICadGeo activeGeometry = MultiTool.GetActiveGeometry();
			if (activeGeometry != null)
			{
				model = CadGeoLoader.LoadCadGeo3D(activeGeometry, null, length, 200.0);
				if (model != null)
				{
					model.Transform = Matrix4d.Identity;
					model.Transform *= Matrix4d.RotationZ(-Math.PI / 4.0);
					model.Transform *= Matrix4d.Translation(0.0, 0.0, num / 2.0);
				}
			}
		}
		return model;
	}

	public void Load3DPreview(FrameworkElement previewImage3D)
	{
		Model activeModel = GetActiveModel();
		if (previewImage3D is Screen3D screen3D)
		{
			screen3D.ShowNavigation(show: false);
			screen3D.SetConfigProviderAndApplySettings(_configProvider);
			screen3D.ScreenD3D?.RemoveModel(null);
			screen3D.ScreenD3D?.RemoveBillboard(null);
		}
		if (activeModel != null)
		{
			((Screen3D)previewImage3D).ScreenD3D?.AddModel(activeModel, render: false);
			Matrix4d identity = Matrix4d.Identity;
			identity *= Matrix4d.RotationZ(1.5707963705062866);
			identity *= Matrix4d.RotationX(0.39269909262657166);
			((Screen3D)previewImage3D).ScreenD3D?.SetViewDirectionByMatrix4d(identity, render: false, delegate
			{
				((Screen3D)previewImage3D).ScreenD3D.ZoomExtend();
			});
		}
	}

	private string SanitizeNewFilename(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (string current, char c) => current.Replace(c, '_'));
			fileName = fileName.Replace(".WZG", "");
			fileName = fileName.Replace(".wzg", "");
			fileName = fileName.Replace(".c3mo", "");
			fileName += ".c3mo";
			fileName = fileName.Trim();
			return fileName;
		}
		return null;
	}

	public void SetAliasPieces(IEnumerable<AliasPieceViewModel> newAliases)
	{
		HashSet<ToolPieceViewModel> hashSet = new HashSet<ToolPieceViewModel>();
		foreach (AliasPieceViewModel aliasPiece in AliasPieces)
		{
			aliasPiece.Pieces.Remove(this);
			hashSet.AddRange(aliasPiece.Pieces);
		}
		AliasPieces.Clear();
		AliasPieces.AddRange(newAliases);
		foreach (AliasPieceViewModel aliasPiece2 in AliasPieces)
		{
			hashSet.AddRange(aliasPiece2.Pieces);
			aliasPiece2.Pieces.Add(this);
		}
		AliasIds = string.Join(", ", from x in AliasPieces
			orderby x.Id
			select x.Id);
		NotifyPropertyChanged("AliasIds");
		ReCalcAliasConnectedPieceAmount();
		foreach (ToolPieceViewModel item in hashSet)
		{
			item.ReCalcAliasConnectedPieceAmount();
		}
	}

	private void ReCalcAliasConnectedPieceAmount()
	{
		int num = AliasPieces.SelectMany((AliasPieceViewModel x) => x.Pieces).Distinct().Count();
		if (num != AliasConnectedPieceAmount)
		{
			AliasConnectedPieceAmount = num;
			NotifyPropertyChanged("AliasConnectedPieceAmount");
		}
	}

	public bool SetGeometryFile(string? newGeometryFileFull)
	{
		if (newGeometryFileFull == null)
		{
			if (_geometryNewContent != null || Geometry3D != null || GeometryFile != null)
			{
				Geometry3D = null;
				_geometryNewContent = null;
				_geometryFile = "";
				_geometryFileFull = "";
				NotifyPropertyChanged("GeometryFile");
				_isChanged = true;
				return true;
			}
		}
		else if (File.Exists(newGeometryFileFull) && newGeometryFileFull.ToLower().EndsWith(".c3mo"))
		{
			_geometryNewContent = File.ReadAllBytes(newGeometryFileFull);
			_geometryFileFull = newGeometryFileFull;
			_geometryFile = new FileInfo(newGeometryFileFull).Name;
			NotifyPropertyChanged("GeometryFile");
			_isChanged = true;
			return true;
		}
		return false;
	}
}

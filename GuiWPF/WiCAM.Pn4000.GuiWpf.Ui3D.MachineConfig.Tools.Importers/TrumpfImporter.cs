using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.HealCadGeo;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.TrumpfConfig;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public abstract class TrumpfImporter
{
	protected readonly IUnitConverter _unitConverter;

	protected readonly IGlobalToolStorage _toolStorage;

	protected readonly IModelFactory _modelFactory;

	protected readonly ImportHelper _importHelper;

	protected readonly IConfigProvider _configProvider;

	protected MachineToolsViewModel _machineTools;

	public TrumpfImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
	{
		_unitConverter = unitConverter;
		_toolStorage = toolStorage;
		_modelFactory = modelFactory;
		_importHelper = importHelper;
		_configProvider = configProvider;
	}

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	protected void ImportDies(List<TrumpfToolProfile>? dieProfiles)
	{
		if (dieProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfToolProfile dieProfile in dieProfiles)
		{
			Dictionary<string, string> group = dieProfile.Group;
			if (group != null && group.Count > 0)
			{
				string groupName = dieProfile.Group["WZG_MA_GRUPPE_ID"].Replace("'", "");
				if (!_machineTools.LowerGroups.Any((LowerToolGroupViewModel x) => x.Name == groupName))
				{
					ConvertDieGroup(dieProfile.Group);
				}
			}
			string text = dieProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			double vWidth = (dieProfile.Profile.ContainsKey("Gesenkweite") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Gesenkweite"]) : 0.0);
			double vAngleDeg = (dieProfile.Profile.ContainsKey("Winkel") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Winkel"]) : 0.0);
			LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(dieProfile.Profile["GruppeID"].Replace("'", ""), vWidth, vAngleDeg);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			if (ConvertDie(dieProfile, multiTool, matchingLowerGroup) == null)
			{
				continue;
			}
			if (dieProfile.Profile.ContainsKey("Falzspalte") && _importHelper.SpecialConvertToDouble(dieProfile.Profile["Falzspalte"]) != 0.0)
			{
				LowerToolGroupViewModel group2 = _machineTools.CreateLowerToolGroup();
				ConvertDieFold(dieProfile, multiTool, group2);
			}
			foreach (Dictionary<string, string> segment in dieProfile.Segments)
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiTool));
			}
		}
	}

	protected void ImportPunches(List<TrumpfToolProfile>? punchProfiles)
	{
		if (punchProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfToolProfile punchProfile in punchProfiles)
		{
			Dictionary<string, string> group = punchProfile.Group;
			if (group != null && group.Count > 0)
			{
				string groupName = punchProfile.Group["WZG_OW_GRUPPE_ID"].Replace("'", "");
				if (_machineTools.UpperGroups.All((UpperToolGroupViewModel g) => g.Name != groupName))
				{
					_machineTools.CreateUpperToolGroup(groupName, _importHelper.SpecialConvertToDouble(punchProfile.Group["WZG_OW_RADIUS"]));
				}
			}
			string text = punchProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			if (punchProfile.Attachements.Count <= 0)
			{
				MessageBox.Show("Punch Tool: " + text + " error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				continue;
			}
			double radius = (punchProfile.Profile.ContainsKey("Radius") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["Radius"]) : 0.0);
			UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup(punchProfile.Profile["GruppeID"].Replace("'", ""), radius);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			UpperToolViewModel upperTool = ConvertPunch(punchProfile, multiTool, matchingUpperGroup);
			foreach (Dictionary<string, string> segment in punchProfile.Segments)
			{
				_machineTools.UpperPieces.Add(ConvertPunchPiece(segment, multiTool, punchProfile, upperTool));
			}
			if (punchProfile.Profile.ContainsKey("ZudrueckBreite") && _importHelper.SpecialConvertToDouble(punchProfile.Profile["ZudrueckBreite"]) != 0.0)
			{
				UpperToolGroupViewModel group2 = _machineTools.CreateUpperToolGroup();
				ConvertPunchFold(punchProfile, multiTool, group2);
			}
		}
	}

	protected void ImportHems(List<TrumpfToolProfile> hemProfiles)
	{
		if (hemProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfToolProfile hemProfile in hemProfiles)
		{
			string text = hemProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			_machineTools.LowerToolsExtensions.Add(ConvertHem(hemProfile, multiTool));
			foreach (Dictionary<string, string> segment in hemProfile.Segments)
			{
				_machineTools.LowerToolExtentionPieces.Add(ConvertHemPiece(segment, multiTool));
			}
		}
	}

	protected void ImportLowerAdapters(List<TrumpfAdapterProfile> adapterProfiles)
	{
		if (adapterProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfAdapterProfile adapterProfile in adapterProfiles)
		{
			string text = adapterProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			_machineTools.LowerAdapters.Add(ConvertLowerAdapter(adapterProfile, multiTool));
			foreach (Dictionary<string, string> segment in adapterProfile.Segments)
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiTool));
			}
		}
	}

	protected void ImportUpperAdapters(List<TrumpfAdapterProfile> adapterProfiles)
	{
		if (adapterProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfAdapterProfile adapterProfile in adapterProfiles)
		{
			string text = adapterProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			_machineTools.UpperAdapters.Add(ConvertUpperAdapter(adapterProfile, multiTool));
			foreach (Dictionary<string, string> segment in adapterProfile.Segments)
			{
				_machineTools.UpperPieces.Add(ConvertUpperAdapterPiece(segment, multiTool));
			}
		}
	}

	protected void ImportLowerHolders(List<TrumpfHolderProfile> holderProfiles)
	{
		if (holderProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfHolderProfile holderProfile in holderProfiles)
		{
			string text = holderProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			double num = (holderProfile.Profile.ContainsKey("Einbauhoehe") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["Einbauhoehe"]) : 0.0);
			double num2 = (holderProfile.Profile.ContainsKey("MittenVersatz") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["MittenVersatz"]) : 0.0);
			if (holderProfile.Attachements.Count > 0)
			{
				string text2 = holderProfile.Attachements.Last()["data"];
				if (!string.IsNullOrEmpty(text2))
				{
					text2 = text2.Trim();
					multiToolViewModel.Geometry = GetTrumpfGeometry(text2);
					if (num != 0.0 || num2 != 0.0)
					{
						multiToolViewModel.Geometry.MoveAll(0.0 - num2, 0.0);
					}
				}
			}
			_machineTools.LowerAdapters.Add(ConvertLowerHolder(holderProfile, multiToolViewModel, num2));
			if (holderProfile.Profile.ContainsKey("ZudrueckHoehe") && _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckHoehe"]) != 0.0)
			{
				ConvertLowerFoldHolder(holderProfile, multiToolViewModel, num2);
			}
			foreach (Dictionary<string, string> segment in holderProfile.Segments)
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiToolViewModel));
			}
		}
	}

	protected void ImportUpperHolders(List<TrumpfHolderProfile> holderProfiles)
	{
		if (holderProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (TrumpfHolderProfile holderProfile in holderProfiles)
		{
			string text = holderProfile.Profile["Typ"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			if (holderProfile.Attachements.Count > 0)
			{
				string text2 = holderProfile.Attachements.Last()["data"];
				if (!string.IsNullOrEmpty(text2))
				{
					text2 = text2.Trim();
					multiToolViewModel.Geometry = GetTrumpfGeometry(text2);
				}
			}
			_machineTools.UpperAdapters.Add(ConvertUpperHolder(holderProfile, multiToolViewModel));
			if (holderProfile.Profile.ContainsKey("ZudrueckHoehe") && _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckHoehe"]) != 0.0)
			{
				ConvertUpperFoldHolder(holderProfile, multiToolViewModel);
			}
			foreach (Dictionary<string, string> segment in holderProfile.Segments)
			{
				_machineTools.UpperPieces.Add(ConvertUpperAdapterPiece(segment, multiToolViewModel));
			}
		}
	}

	private UpperAdapterViewModel ConvertUpperHolder(TrumpfHolderProfile? holderProfile, MultiToolViewModel multiTool)
	{
		string text = holderProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		double num = (holderProfile.Profile.ContainsKey("Einbauhoehe") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["Einbauhoehe"]) : 0.0);
		return new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, _importHelper.SpecialConvertToDouble(holderProfile.Profile["Arbeitshoehe"]) - num)
		{
			Name = text,
			MountTypeID = Convert.ToInt32(holderProfile.Profile["AufnahmeID"].Replace("'", "").Trim()),
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(holderProfile.Profile["BelastungMax"]),
			Implemented = true
		};
	}

	private UpperToolViewModel ConvertUpperFoldHolder(TrumpfHolderProfile? holderProfile, MultiToolViewModel multiTool)
	{
		string text = holderProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		UpperToolGroupViewModel upperGroup = _machineTools.CreateUpperToolGroup();
		MachineToolsViewModel machineTools = _machineTools;
		double? radius = 0.0;
		double? angleRad = 0.0;
		double? widthHemmingFace = (holderProfile.Profile.ContainsKey("ZudrueckBreite") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckBreite"]) : 0.0);
		double? hemOffsetX = (holderProfile.Profile.ContainsKey("ZudrueckVersatz") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckVersatz"]) : 0.0);
		UpperToolViewModel upperToolViewModel = machineTools.CreateUpperToolProfile(multiTool, upperGroup, radius, angleRad, _importHelper.SpecialConvertToDouble(holderProfile.Profile["Arbeitshoehe"]), text, _importHelper.SpecialConvertToDouble(holderProfile.Profile["BelastungMax"]), hemOffsetX, widthHemmingFace);
		upperToolViewModel.MountTypeID = Convert.ToInt32(holderProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		upperToolViewModel.IsFoldTool = true;
		upperToolViewModel.Radius = null;
		upperToolViewModel.Angle = null;
		return upperToolViewModel;
	}

	private LowerAdapterViewModel ConvertLowerHolder(TrumpfHolderProfile? holderProfile, MultiToolViewModel multiTool, double middleOffset)
	{
		string text = holderProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		CadGeoLoader.GetContours(multiTool.Geometry).Max((Polygon2D x) => x.Vertices.Max((Vector2d y) => y.Y));
		double num = (holderProfile.Profile.ContainsKey("Einbauhoehe") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["Einbauhoehe"]) : 0.0);
		double num2 = _importHelper.SpecialConvertToDouble(holderProfile.Profile["Arbeitshoehe"]);
		return new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, num2)
		{
			Name = text,
			MountTypeID = Convert.ToInt32(holderProfile.Profile["AufnahmeID"].Replace("'", "").Trim()),
			SocketId = (holderProfile.Profile.ContainsKey("WzgAufnahmeID") ? Convert.ToInt32(holderProfile.Profile["WzgAufnahmeID"].Replace("'", "").Trim()) : 0),
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(holderProfile.Profile["BelastungMax"]),
			Implemented = true,
			SpringHeight = ((holderProfile.Profile.ContainsKey("ZudrueckHoehe") && _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckHoehe"]) != 0.0) ? new double?(num2 - num) : null),
			OffsetInX = ((holderProfile.Profile.ContainsKey("MittenVersatz") && _importHelper.SpecialConvertToDouble(holderProfile.Profile["MittenVersatz"]) != 0.0) ? new double?(0.0 - _importHelper.SpecialConvertToDouble(holderProfile.Profile["MittenVersatz"])) : null)
		};
	}

	private LowerToolViewModel ConvertLowerFoldHolder(TrumpfHolderProfile? holderProfile, MultiToolViewModel multiTool, double middleOffset)
	{
		string text = holderProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		LowerToolGroupViewModel lowerGroup = _machineTools.CreateLowerToolGroup();
		double num = CadGeoLoader.GetContours(multiTool.Geometry).Max((Polygon2D x) => x.Vertices.Max((Vector2d y) => y.Y));
		MachineToolsViewModel machineTools = _machineTools;
		string name = text;
		double maxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(holderProfile.Profile["BelastungMax"]);
		double workingHeight = _importHelper.SpecialConvertToDouble(holderProfile.Profile["Arbeitshoehe"]);
		double offsetInX = (holderProfile.Profile.ContainsKey("ZudrueckVersatz") ? _importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckVersatz"]) : 0.0) - middleOffset;
		double? widthHemmingFace = (holderProfile.Profile.ContainsKey("ZudrueckBreite") ? new double?(_importHelper.SpecialConvertToDouble(holderProfile.Profile["ZudrueckBreite"])) : null);
		double? springHeight = (holderProfile.Profile.ContainsKey("ZudrueckHoehe") ? new double?(0.0 - num) : null);
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, lowerGroup, VWidthTypes.Undefined, name, maxAllowableToolForcePerLengthUnit, offsetInX, workingHeight, null, null, null, null, null, widthHemmingFace, springHeight);
		lowerToolViewModel.MountTypeID = Convert.ToInt32(holderProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		lowerToolViewModel.OffsetFrontMm = _importHelper.SpecialConvertToDouble(holderProfile.Profile["SymmetrieX_Negativ"]) - middleOffset;
		lowerToolViewModel.OffsetBackMm = _importHelper.SpecialConvertToDouble(holderProfile.Profile["SymmetrieX_Positiv"]) - middleOffset;
		lowerToolViewModel.IsFoldTool = true;
		return lowerToolViewModel;
	}

	private UpperToolPieceViewModel ConvertUpperAdapterPiece(Dictionary<string, string>? dieSegment, MultiToolViewModel multiTool)
	{
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(dieSegment["ID"].Replace("'", "").Trim()), _importHelper.SpecialConvertToDouble(dieSegment["WZG_LAENGE"]), Convert.ToInt32(dieSegment["WZG_ANZAHL"].Replace("'", "").Trim()));
		upperToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(dieSegment["WZG_MAXBELASTUNG"]);
		return upperToolPieceViewModel;
	}

	private UpperAdapterViewModel ConvertUpperAdapter(TrumpfAdapterProfile? adapterProfile, MultiToolViewModel multiTool)
	{
		string text = adapterProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		UpperAdapterViewModel result = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, _importHelper.SpecialConvertToDouble(adapterProfile.Profile["Arbeitshoehe"]))
		{
			Name = text,
			MountTypeID = Convert.ToInt32(adapterProfile.Profile["AufnahmeID"].Replace("'", "").Trim()),
			SocketId = (adapterProfile.Profile.ContainsKey("WzgAufnahmeID") ? Convert.ToInt32(adapterProfile.Profile["WzgAufnahmeID"].Replace("'", "").Trim()) : 0),
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(adapterProfile.Profile["BelastungMax"]),
			Implemented = true
		};
		if (adapterProfile.Attachements.Count > 0)
		{
			string text2 = adapterProfile.Attachements.Last()["data"];
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = text2.Trim();
				multiTool.Geometry = GetTrumpfGeometry(text2);
			}
		}
		return result;
	}

	private LowerAdapterViewModel ConvertLowerAdapter(TrumpfAdapterProfile? adapterProfile, MultiToolViewModel multiTool)
	{
		string text = adapterProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		LowerAdapterViewModel result = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, _importHelper.SpecialConvertToDouble(adapterProfile.Profile["Arbeitshoehe"]))
		{
			Name = text,
			MountTypeID = Convert.ToInt32(adapterProfile.Profile["AufnahmeID"].Replace("'", "").Trim()),
			SocketId = Convert.ToInt32(adapterProfile.Profile["WzgAufnahmeID"].Replace("'", "").Trim()),
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(adapterProfile.Profile["BelastungMax"]),
			Implemented = true
		};
		if (adapterProfile.Attachements.Count > 0)
		{
			string text2 = adapterProfile.Attachements.Last()["data"];
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = text2.Trim();
				multiTool.Geometry = GetTrumpfGeometry(text2);
			}
		}
		return result;
	}

	private LowerToolExtensionPieceViewModel ConvertHemPiece(Dictionary<string, string>? hemSegment, MultiToolViewModel multiTool)
	{
		int amount = Convert.ToInt32(hemSegment["WZG_ANZAHL"].Replace("'", "").Trim());
		LowerToolExtensionPieceViewModel lowerToolExtensionPieceViewModel = _machineTools.CreateLowerToolExtensionPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(hemSegment["ID"].Replace("'", "").Trim()), _importHelper.SpecialConvertToDouble(hemSegment["WZG_LAENGE"]), amount);
		lowerToolExtensionPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(hemSegment["WZG_MAXBELASTUNG"]);
		return lowerToolExtensionPieceViewModel;
	}

	private LowerToolExtensionViewModel ConvertHem(TrumpfToolProfile hemProfile, MultiToolViewModel multiTool)
	{
		string text = hemProfile.Attachements.Last()["data"];
		AABB<Vector2d> aABB = null;
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Trim();
			WzgLoader.GetElements(GetTrumpfGeometry(text), out var elements);
			aABB = new AABB<Vector2d>(WzgLoader.GetContours(elements).First().Vertices);
		}
		string text2 = hemProfile.Profile["Typ"];
		text2 = text2.Replace("'", "").Trim();
		LowerToolGroupViewModel lowerToolGroupViewModel = _machineTools.CreateLowerToolGroup();
		LowerToolExtensionViewModel lowerToolExtensionViewModel = new LowerToolExtensionViewModel(_unitConverter, _toolStorage, multiTool, lowerToolGroupViewModel, hemWorkingHeight: _importHelper.SpecialConvertToDouble(hemProfile.Profile["Arbeitshoehe"]), offsetInXForHemming: 0.0 - _importHelper.SpecialConvertToDouble(hemProfile.Profile["ZudrueckVersatz"]))
		{
			Name = text2,
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(hemProfile.Profile["BelastungMax"]),
			MountTypeID = Convert.ToInt32(hemProfile.Profile["ZudrueckID"]),
			OffsetFront = (hemProfile.Profile.ContainsKey("SymmetrieX_Negativ") ? _importHelper.SpecialConvertToDouble(hemProfile.Profile["SymmetrieX_Negativ"]) : (aABB?.Min.X ?? 0.0)),
			OffsetBack = (hemProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(hemProfile.Profile["SymmetrieX_Positiv"]) : (aABB?.Max.X ?? 0.0)),
			DepthForHemmingMm = _importHelper.SpecialConvertToDouble(hemProfile.Profile["ZudrueckTiefe"]),
			IsFoldTool = true,
			Implemented = true
		};
		if (text != null)
		{
			text = text.Trim();
			lowerToolExtensionViewModel.MultiTool.Geometry = GetTrumpfGeometry(text);
		}
		return lowerToolExtensionViewModel;
	}

	private UpperToolPieceViewModel ConvertPunchPiece(Dictionary<string, string>? punchSegment, MultiToolViewModel multiTool, TrumpfToolProfile? punchProfile, UpperToolViewModel upperTool)
	{
		string geoName = punchSegment["WZG_ZEICHNUNG"].Replace("'", "").Trim();
		int amount = Convert.ToInt32(punchSegment["WZG_ANZAHL"].Replace("'", "").Trim());
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(geoName), _importHelper.SpecialConvertToDouble(punchSegment["WZG_LAENGE"]), amount);
		upperToolPieceViewModel.HasHeelLeft = punchSegment["WZG_TYP_ID"] == "17";
		upperToolPieceViewModel.HasHeelRight = punchSegment["WZG_TYP_ID"] == "18";
		upperToolPieceViewModel.IsAngleMeasurementTool = punchSegment["WZG_TYP_ID"] == "19" || punchSegment["WZG_TYP_ID"] == "21" || punchSegment["WZG_TYP_ID"] == "24";
		upperToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(punchSegment["WZG_MAXBELASTUNG"]);
		if (!string.IsNullOrEmpty(geoName))
		{
			string text = string.Empty;
			Dictionary<string, string> dictionary = punchProfile.Attachements.First((Dictionary<string, string> a) => a["name"] == geoName);
			if (dictionary.ContainsKey("data"))
			{
				text = dictionary["data"];
			}
			ICadGeo heelFront = null;
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Trim();
				heelFront = GetTrumpfGeometry(text);
			}
			try
			{
				if (WzgLoader.GetHeelTool(upperTool.MultiTool.Geometry, heelFront, out var resultModel, advancedExtrude: true))
				{
					upperToolPieceViewModel.Geometry3D = resultModel;
				}
			}
			catch
			{
				geoName = "";
			}
		}
		return upperToolPieceViewModel;
	}

	private UpperToolViewModel ConvertPunch(TrumpfToolProfile? punchProfile, MultiToolViewModel multiTool, UpperToolGroupViewModel group)
	{
		ICadGeo cadGeo = null;
		string text = punchProfile.Attachements.Last()["data"];
		AABB<Vector2d> aABB = null;
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Trim();
			cadGeo = GetTrumpfGeometry(text);
			WzgLoader.GetElements(cadGeo, out var elements);
			aABB = new AABB<Vector2d>(WzgLoader.GetContours(elements).First().Vertices);
		}
		string text2 = punchProfile.Profile["Typ"];
		text2 = text2.Replace("'", "").Trim();
		UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiTool, group, punchProfile.Profile.ContainsKey("Radius") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["Radius"]) : 0.0, Math.PI / 180.0 * (punchProfile.Profile.ContainsKey("Winkel") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["Winkel"]) : 0.0), _importHelper.SpecialConvertToDouble(punchProfile.Profile["Arbeitshoehe"]), text2, _importHelper.SpecialConvertToDouble(punchProfile.Profile["BelastungMax"]), 0.0, 0.0);
		upperToolViewModel.MountTypeID = Convert.ToInt32(punchProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		upperToolViewModel.OffsetFront = (punchProfile.Profile.ContainsKey("SymmetrieX_Negativ") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["SymmetrieX_Negativ"]) : (aABB?.Min.X ?? 0.0));
		upperToolViewModel.OffsetBack = (punchProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["SymmetrieX_Positiv"]) : (aABB?.Max.X ?? 0.0));
		upperToolViewModel.MultiTool.Geometry = cadGeo;
		return upperToolViewModel;
	}

	private UpperToolViewModel ConvertPunchFold(TrumpfToolProfile? punchProfile, MultiToolViewModel multiTool, UpperToolGroupViewModel group)
	{
		ICadGeo cadGeo = null;
		string text = punchProfile.Attachements.Last()["data"];
		AABB<Vector2d> aABB = null;
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Trim();
			cadGeo = GetTrumpfGeometry(text);
			WzgLoader.GetElements(cadGeo, out var elements);
			aABB = new AABB<Vector2d>(WzgLoader.GetContours(elements).First().Vertices);
		}
		string text2 = punchProfile.Profile["Typ"];
		text2 = text2.Replace("'", "").Trim();
		MachineToolsViewModel machineTools = _machineTools;
		double? radius = (punchProfile.Profile.ContainsKey("Radius") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["Radius"]) : 0.0);
		double? widthHemmingFace = (punchProfile.Profile.ContainsKey("ZudrueckBreite") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["ZudrueckBreite"]) : 0.0);
		double? hemOffsetX = (punchProfile.Profile.ContainsKey("ZudrueckVersatz") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["ZudrueckVersatz"]) : 0.0);
		UpperToolViewModel obj = machineTools.CreateUpperToolProfile(name: text2, multiTool: multiTool, upperGroup: group, radius: radius, angleRad: 0.0, workingHeight: (punchProfile.Profile.ContainsKey("Arbeitshoehe") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["Arbeitshoehe"]) : 0.0) - (punchProfile.Profile.ContainsKey("ZudrueckHoehe") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["ZudrueckHoehe"]) : 0.0), maxAllowableToolForcePerLengthUnit: _importHelper.SpecialConvertToDouble(punchProfile.Profile["BelastungMax"]), hemOffsetX: hemOffsetX, widthHemmingFace: widthHemmingFace);
		obj.MountTypeID = Convert.ToInt32(punchProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		obj.OffsetFront = (punchProfile.Profile.ContainsKey("SymmetrieX_Negativ") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["SymmetrieX_Negativ"]) : (aABB?.Min.X ?? 0.0));
		obj.OffsetBack = (punchProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(punchProfile.Profile["SymmetrieX_Positiv"]) : (aABB?.Max.X ?? 0.0));
		obj.IsFoldTool = true;
		obj.Angle = null;
		obj.MultiTool.Geometry = cadGeo;
		return obj;
	}

	private LowerToolPieceViewModel ConvertDiePiece(Dictionary<string, string>? dieSegment, MultiToolViewModel multiTool)
	{
		LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(dieSegment["ID"].Replace("'", "").Trim()), _importHelper.SpecialConvertToDouble(dieSegment["WZG_LAENGE"]), Convert.ToInt32(dieSegment["WZG_ANZAHL"].Replace("'", "").Trim()));
		lowerToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(dieSegment["WZG_MAXBELASTUNG"]);
		return lowerToolPieceViewModel;
	}

	private LowerToolViewModel ConvertDie(TrumpfToolProfile dieProfile, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		string text = dieProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		if (dieProfile.Attachements.Count <= 0)
		{
			MessageBox.Show("Die Tool: " + text + " error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return null;
		}
		string text2 = string.Empty;
		Dictionary<string, string> dictionary = dieProfile.Attachements?.Last();
		if (dictionary != null && dictionary.ContainsKey("data"))
		{
			text2 = dictionary["data"];
		}
		ICadGeo cadGeo = null;
		AABB<Vector2d> aABB = null;
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = text2.Trim();
			cadGeo = GetTrumpfGeometry(text2);
			WzgLoader.GetElements(cadGeo, out var elements);
			aABB = new AABB<Vector2d>(WzgLoader.GetContours(elements).First().Vertices);
		}
		MachineToolsViewModel machineTools = _machineTools;
		string name = text;
		double maxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(dieProfile.Profile["BelastungMax"]);
		double workingHeight = _importHelper.SpecialConvertToDouble(dieProfile.Profile["Arbeitshoehe"]);
		double? vWidth = (dieProfile.Profile.ContainsKey("Gesenkweite") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Gesenkweite"]) : 0.0);
		double? vAngle = Math.PI / 180.0 * (dieProfile.Profile.ContainsKey("Winkel") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Winkel"]) : 0.0);
		double? cornerRadius = (dieProfile.Profile.ContainsKey("Radius") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Radius"]) : 0.0);
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.BTrumpf, name, maxAllowableToolForcePerLengthUnit, 0.0, workingHeight, vWidth, vAngle, null, cornerRadius);
		lowerToolViewModel.MountTypeID = Convert.ToInt32(dieProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		lowerToolViewModel.OffsetFront = (dieProfile.Profile.ContainsKey("SymmetrieX_Negativ") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["SymmetrieX_Negativ"]) : (aABB?.Min.X ?? 0.0));
		lowerToolViewModel.OffsetBack = (dieProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["SymmetrieX_Positiv"]) : (aABB?.Max.X ?? 0.0));
		lowerToolViewModel.MultiTool.Geometry = cadGeo;
		return lowerToolViewModel;
	}

	private LowerToolViewModel ConvertDieFold(TrumpfToolProfile dieProfile, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		string text = dieProfile.Profile["Typ"];
		text = text.Replace("'", "").Trim();
		if (dieProfile.Attachements.Count <= 0)
		{
			MessageBox.Show("Die Tool: " + text + " error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return null;
		}
		string text2 = string.Empty;
		Dictionary<string, string> dictionary = dieProfile.Attachements?.Last();
		if (dictionary != null && dictionary.ContainsKey("data"))
		{
			text2 = dictionary["data"];
		}
		ICadGeo cadGeo = null;
		AABB<Vector2d> aABB = null;
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = text2.Trim();
			cadGeo = GetTrumpfGeometry(text2);
			WzgLoader.GetElements(cadGeo, out var elements);
			aABB = new AABB<Vector2d>(WzgLoader.GetContours(elements).First().Vertices);
		}
		MachineToolsViewModel machineTools = _machineTools;
		string name = text;
		double maxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(dieProfile.Profile["BelastungMax"]);
		double workingHeight = (dieProfile.Profile.ContainsKey("FALZHOEHE") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["FALZHOEHE"]) : _importHelper.SpecialConvertToDouble(dieProfile.Profile["Arbeitshoehe"]));
		double? foldGap = (dieProfile.Profile.ContainsKey("Falzspalte") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Falzspalte"]) : 0.0);
		LowerToolViewModel obj = machineTools.CreateLowerToolProfile(partFoldOffset: (dieProfile.Profile.ContainsKey("FalzEinlegepositionX") && _importHelper.SpecialConvertToDouble(dieProfile.Profile["FalzEinlegepositionX"]) != 0.0) ? new double?(_importHelper.SpecialConvertToDouble(dieProfile.Profile["FalzEinlegepositionX"])) : null, multiTool: multiTool, lowerGroup: group, vWidthType: VWidthTypes.BTrumpf, name: name, maxAllowableToolForcePerLengthUnit: maxAllowableToolForcePerLengthUnit, offsetInX: (dieProfile.Profile.ContainsKey("FalzEinlegepositionX") && _importHelper.SpecialConvertToDouble(dieProfile.Profile["FalzEinlegepositionX"]) != 0.0) ? 0.0 : (0.0 - (dieProfile.Profile.ContainsKey("Falzversatz") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["Falzversatz"]) : 0.0) + (dieProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["SymmetrieX_Positiv"]) : 0.0)), workingHeight: workingHeight, vWidth: null, vAngle: null, vDepth: null, cornerRadius: null, foldGap: foldGap);
		obj.MountTypeID = Convert.ToInt32(dieProfile.Profile["AufnahmeID"].Replace("'", "").Trim());
		obj.OffsetFront = (dieProfile.Profile.ContainsKey("SymmetrieX_Negativ") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["SymmetrieX_Negativ"]) : (aABB?.Min.X ?? 0.0));
		obj.OffsetBack = (dieProfile.Profile.ContainsKey("SymmetrieX_Positiv") ? _importHelper.SpecialConvertToDouble(dieProfile.Profile["SymmetrieX_Positiv"]) : (aABB?.Max.X ?? 0.0));
		obj.IsFoldTool = true;
		obj.MultiTool.Geometry = cadGeo;
		return obj;
	}

	private LowerToolGroupViewModel ConvertDieGroup(Dictionary<string, string> group)
	{
		return _machineTools.CreateLowerToolGroup(_importHelper.SpecialConvertToDouble(group["WZG_MA_GESENKWEITE"]), _importHelper.SpecialConvertToDouble(group["WZG_MA_WINKEL"]) * Math.PI / 180.0, null, group["WZG_MA_GRUPPE_ID"].Replace("'", ""));
	}

	private static ICadGeo GetTrumpfGeometry(string wzgContent)
	{
		string text = ConvertHex(wzgContent);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		ICadGeo cadGeo = WzgLoader.ReadWzg(text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None).ToList());
		HealCadGeo_.Heal(cadGeo);
		return cadGeo;
	}

	private static string ConvertHex(string hexString)
	{
		try
		{
			string text = string.Empty;
			for (int i = 0; i < hexString.Length; i += 2)
			{
				_ = string.Empty;
				char c = Convert.ToChar(Convert.ToUInt32(hexString.Substring(i, 2), 16));
				text += c;
			}
			return text;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		return string.Empty;
	}
}

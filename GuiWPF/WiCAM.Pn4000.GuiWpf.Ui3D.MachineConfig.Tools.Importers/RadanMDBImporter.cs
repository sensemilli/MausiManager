using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.RadanConfig.Punch;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Unfold.BendTable.MdbImport;
using MdbImporter;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.Encodings;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class RadanMDBImporter : IToolImporter, IAdapterImporter
{
	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IModelFactory _modelFactory;

	private readonly ImportHelper _importHelper;

	private readonly IConfigProvider _configProvider;

	private MachineToolsViewModel _machineTools;

	public RadanMDBImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
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

	public string GetFilterString()
	{
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.RadanMdb") as string;
	}

	public void ImportPunches(string filePath)
	{
		RadanPunch punches = GetPunches(filePath);
		if (punches.PunchProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (RadanToolProfile punchProfile in punches.PunchProfiles)
		{
			string text = (punchProfile.Profile.ContainsKey("Typ") ? punchProfile.Profile["Typ"] : punchProfile.Profile["name"]);
			text = text.Replace("'", "").Trim();
			UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup(_importHelper.SpecialConvertToDouble(punchProfile.Profile["radius"]));
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			UpperToolViewModel upperToolViewModel = ConvertPunch(punchProfile, matchingUpperGroup, multiTool);
			if (upperToolViewModel == null)
			{
				continue;
			}
			foreach (Dictionary<string, string> segment in punchProfile.Segments)
			{
				_machineTools.UpperPieces.Add(ConvertPunchPiece(punchProfile, upperToolViewModel, segment, multiTool));
			}
		}
	}

	public void ImportLowerAdapters(string filePath)
	{
		RadanAdapter adapters = GetAdapters(filePath);
		if (adapters.AdapterProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (RadanAdapterProfile adapterProfile in adapters.AdapterProfiles)
		{
			string text = adapterProfile.Profile["name"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			_machineTools.LowerAdapters.Add(ConvertLowerAdapter(adapterProfile, multiTool));
			foreach (Dictionary<string, string> segment in adapterProfile.Segments)
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiTool));
			}
		}
	}

	public void ImportUpperAdapters(string filePath)
	{
		RadanAdapter adapters = GetAdapters(filePath);
		if (adapters.AdapterProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (RadanAdapterProfile adapterProfile in adapters.AdapterProfiles)
		{
			string text = adapterProfile.Profile["name"];
			text = text.Replace("'", "").Trim();
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			_machineTools.UpperAdapters.Add(ConvertUpperAdapter(adapterProfile, multiTool));
			foreach (Dictionary<string, string> segment in adapterProfile.Segments)
			{
				_machineTools.UpperPieces.Add(ConvertUpperAdapterPiece(segment, multiTool));
			}
		}
	}

	public void ImportDies(string filePath)
	{
		RadanDie dies = GetDies(filePath);
		if (dies.DieProfiles.Count <= 0)
		{
			MessageBox.Show("Error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		foreach (RadanToolProfile dieProfile in dies.DieProfiles)
		{
			double vWidth = _importHelper.SpecialConvertToDouble(dieProfile.Profile["uv_width"]);
			double vAngleDeg = _importHelper.SpecialConvertToDouble(dieProfile.Profile["v_angle"]);
			string text = (dieProfile.Profile.ContainsKey("Typ") ? dieProfile.Profile["Typ"] : dieProfile.Profile["name"]);
			text = text.Replace("'", "").Trim();
			LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(vWidth, vAngleDeg);
			MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(text));
			if (ConvertDie(dieProfile, matchingLowerGroup, multiTool) == null)
			{
				continue;
			}
			foreach (Dictionary<string, string> segment in dieProfile.Segments)
			{
				_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiTool));
			}
		}
	}

	private UpperAdapterViewModel ConvertUpperAdapter(RadanAdapterProfile? adapterProfile, MultiToolViewModel multiTool)
	{
		string text = adapterProfile.Profile["name"];
		text = text.Replace("'", "").Trim();
		UpperAdapterViewModel obj = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiTool, _importHelper.SpecialConvertToDouble(adapterProfile.Profile["working_height"]))
		{
			Name = text,
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(adapterProfile.Profile["max_allowable_tool_force_per_length_unit"]),
			Implemented = Convert.ToBoolean(adapterProfile.Profile["implemented"])
		};
		string filePath = adapterProfile.Attachements.Last()["data"];
		obj.MultiTool.Geometry = _importHelper.Get2DGeometry(filePath);
		return obj;
	}

	private UpperToolPieceViewModel ConvertUpperAdapterPiece(Dictionary<string, string>? dieSegment, MultiToolViewModel multiTool)
	{
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(dieSegment["Id"].Replace("'", "").Trim()), _importHelper.SpecialConvertToDouble(dieSegment["length"]), Convert.ToInt32(dieSegment["amount"].Replace("'", "").Trim()));
		upperToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(dieSegment["max_allowable_tool_force"]);
		return upperToolPieceViewModel;
	}

	private LowerAdapterViewModel ConvertLowerAdapter(RadanAdapterProfile? adapterProfile, MultiToolViewModel multiTool)
	{
		string text = adapterProfile.Profile["name"];
		text = text.Replace("'", "").Trim();
		LowerAdapterViewModel obj = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiTool, _importHelper.SpecialConvertToDouble(adapterProfile.Profile["working_height"]))
		{
			Name = text,
			MaxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(adapterProfile.Profile["max_allowable_tool_force_per_length_unit"]),
			Implemented = Convert.ToBoolean(adapterProfile.Profile["implemented"])
		};
		string filePath = adapterProfile.Attachements.Last()["data"];
		obj.MultiTool.Geometry = _importHelper.Get2DGeometry(filePath);
		return obj;
	}

	private LowerToolPieceViewModel ConvertDiePiece(Dictionary<string, string>? dieSegment, MultiToolViewModel multiTool)
	{
		LowerToolPieceViewModel lowerToolPieceViewModel = _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(dieSegment["Id"].Replace("'", "").Trim()), _importHelper.SpecialConvertToDouble(dieSegment["length"]), Convert.ToInt32(dieSegment["amount"].Replace("'", "").Trim()));
		lowerToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(dieSegment["max_allowable_tool_force"]);
		return lowerToolPieceViewModel;
	}

	private LowerToolViewModel ConvertDie(RadanToolProfile dieProfile, LowerToolGroupViewModel group, MultiToolViewModel multiTool)
	{
		string text = (dieProfile.Profile.ContainsKey("Typ") ? dieProfile.Profile["Typ"] : dieProfile.Profile["name"]);
		text = text.Replace("'", "").Trim();
		MachineToolsViewModel machineTools = _machineTools;
		string name = text;
		double maxAllowableToolForcePerLengthUnit = _importHelper.SpecialConvertToDouble(dieProfile.Profile["max_allowable_tool_force_per_length_unit"]);
		double workingHeight = _importHelper.SpecialConvertToDouble(dieProfile.Profile["working_height"]);
		double? vWidth = _importHelper.SpecialConvertToDouble(dieProfile.Profile["uv_width"]);
		double? vAngle = _importHelper.SpecialConvertToDouble(dieProfile.Profile["v_angle"]);
		double? cornerRadius = _importHelper.SpecialConvertToDouble(dieProfile.Profile["corner_radius"]);
		double offsetInX = _importHelper.SpecialConvertToDouble(dieProfile.Profile["offset_in_x"]);
		double? foldGap = _importHelper.SpecialConvertToDouble(dieProfile.Profile["offset_in_x_for_hemming"]);
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.Undefined, name, maxAllowableToolForcePerLengthUnit, offsetInX, workingHeight, vWidth, vAngle, null, cornerRadius, foldGap);
		lowerToolViewModel.Implemented = bool.Parse(dieProfile.Profile["implemented"]);
		lowerToolViewModel.StoneFactor = _importHelper.SpecialConvertToDouble(dieProfile.Profile["stone_factor"]);
		string filePath = dieProfile.Attachements.Last()["data"];
		lowerToolViewModel.MultiTool.Geometry = _importHelper.Get2DGeometry(filePath);
		return lowerToolViewModel;
	}

	private UpperToolPieceViewModel ConvertPunchPiece(RadanToolProfile radanToolProfile, UpperToolViewModel upperTool, Dictionary<string, string>? punchSegment, MultiToolViewModel multiTool)
	{
		string geoName = punchSegment["geometry_file"].Replace("'", "").Trim();
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(geoName), _importHelper.SpecialConvertToDouble(punchSegment["length"]), Convert.ToInt32(punchSegment["amount"].Replace("'", "").Trim()));
		upperToolPieceViewModel.HasHeelLeft = bool.Parse(punchSegment["has_heel_left"]);
		upperToolPieceViewModel.HasHeelRight = bool.Parse(punchSegment["has_heel_right"]);
		upperToolPieceViewModel.IsAngleMeasurementTool = bool.Parse(punchSegment["is_angle_measurement_tool"]);
		upperToolPieceViewModel.MaxAllowableToolForce = _importHelper.SpecialConvertToDouble(punchSegment["max_allowable_tool_force"]);
		if (!string.IsNullOrEmpty(geoName))
		{
			string text = radanToolProfile.Attachements.First((Dictionary<string, string> a) => a["name"] == geoName)["data"];
			ICadGeo heelFront = _importHelper.Get2DGeometry(text);
			if (!string.IsNullOrEmpty(text))
			{
				_importHelper.Get2DGeometry(text);
				try
				{
					if (WzgLoader.GetHeelTool(upperTool.MultiTool.Geometry, heelFront, out var resultModel, advancedExtrude: true))
					{
						upperToolPieceViewModel.Geometry3D = resultModel;
					}
				}
				catch
				{
				}
			}
		}
		return upperToolPieceViewModel;
	}

	private UpperToolViewModel ConvertPunch(RadanToolProfile punchProfile, UpperToolGroupViewModel group, MultiToolViewModel multiTool)
	{
		string text = (punchProfile.Profile.ContainsKey("Typ") ? punchProfile.Profile["Typ"] : punchProfile.Profile["name"]);
		text = text.Replace("'", "").Trim();
		if (punchProfile.Attachements.Count <= 0)
		{
			MessageBox.Show("Tool: " + text + " error in format.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return null;
		}
		UpperToolViewModel obj = _machineTools.CreateUpperToolProfile(multiTool, group, _importHelper.SpecialConvertToDouble(punchProfile.Profile["radius"]), _importHelper.SpecialConvertToDouble(punchProfile.Profile["angle"]), _importHelper.SpecialConvertToDouble(punchProfile.Profile["working_height"]), widthHemmingFace: _importHelper.SpecialConvertToDouble(punchProfile.Profile["width_hemming_face"]), hemOffsetX: _importHelper.SpecialConvertToDouble(punchProfile.Profile["x_offset_hemming_face"]), name: text, maxAllowableToolForcePerLengthUnit: _importHelper.SpecialConvertToDouble(punchProfile.Profile["max_allowable_tool_force_per_length_unit"]));
		obj.Implemented = Convert.ToBoolean(punchProfile.Profile["implemented"]);
		string filePath = punchProfile.Attachements.Last()["data"];
		obj.MultiTool.Geometry = _importHelper.Get2DGeometry(filePath);
		return obj;
	}

	private RadanAdapter GetAdapters(string filePath)
	{
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "adapterprofiles", typeof(adapterprofiles));
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(filePath, "adapterparts", typeof(adapterparts));
		string directoryName = Path.GetDirectoryName(filePath);
		List<RadanAdapterProfile> list = new List<RadanAdapterProfile>();
		foreach (object profileEntry in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(profileEntry);
			List<Dictionary<string, string>> list2 = source.Where((object e) => ((adapterparts)e).adapterprofile_id == ((adapterprofiles)profileEntry).Id).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item in list2)
			{
				if (!string.IsNullOrEmpty(item["geometry_file"]) && File.Exists(directoryName + "\\" + item["geometry_file"]))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string> { 
					{
						"name",
						item["geometry_file"]
					} };
					string s = File.ReadAllText(directoryName + "\\" + item["geometry_file"]);
					s = BitConverter.ToString(CurrentEncoding.SystemEncoding.GetBytes(s)).Replace("-", "");
					dictionary.Add("data", s);
					list3.Add(dictionary);
				}
			}
			if (File.Exists(directoryName + "\\" + ((adapterprofiles)profileEntry).geometry_file))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
				{
					"name",
					((adapterprofiles)profileEntry).name
				} };
				File.ReadAllText(directoryName + "\\" + ((adapterprofiles)profileEntry).geometry_file);
				string value = directoryName + "\\" + ((adapterprofiles)profileEntry).geometry_file;
				dictionary2.Add("data", value);
				list3.Add(dictionary2);
			}
			list.Add(new RadanAdapterProfile(properties, list2, list3));
		}
		return new RadanAdapter(list);
	}

	private RadanDie GetDies(string filePath)
	{
		IEnumerable<object> enumerable = UniversalMdbImporter.ImportTable(filePath, "dieprofiles", typeof(dieprofiles));
		IEnumerable<object> source = UniversalMdbImporter.ImportTable(filePath, "dieparts", typeof(dieparts));
		string directoryName = Path.GetDirectoryName(filePath);
		List<RadanToolProfile> list = new List<RadanToolProfile>();
		foreach (object profileEntry in enumerable)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(profileEntry);
			List<Dictionary<string, string>> list2 = source.Where((object e) => ((dieparts)e).dieprofile_id == ((dieprofiles)profileEntry).Id).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item in list2)
			{
				if (!string.IsNullOrEmpty(item["geometry_file"]) && File.Exists(directoryName + "\\" + item["geometry_file"]))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string> { 
					{
						"name",
						item["geometry_file"]
					} };
					string value = directoryName + "\\" + item["geometry_file"];
					dictionary.Add("data", value);
					list3.Add(dictionary);
				}
			}
			if (File.Exists(directoryName + "\\" + ((dieprofiles)profileEntry).geometry_file))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
				{
					"name",
					((dieprofiles)profileEntry).name
				} };
				string value2 = directoryName + "\\" + ((dieprofiles)profileEntry).geometry_file;
				dictionary2.Add("data", value2);
				list3.Add(dictionary2);
			}
			list.Add(new RadanToolProfile(properties, list2, list3));
		}
		return new RadanDie(list);
	}

	private RadanPunch GetPunches(string filePath)
	{
		List<object> list = UniversalMdbImporter.ImportTable(filePath, "punchprofiles", typeof(punchprofiles)).ToList();
		List<object> source = UniversalMdbImporter.ImportTable(filePath, "punchparts", typeof(punchparts)).ToList();
		string directoryName = Path.GetDirectoryName(filePath);
		List<RadanToolProfile> list2 = new List<RadanToolProfile>();
		foreach (object profileEntry in list)
		{
			Dictionary<string, string> properties = _importHelper.GetProperties(profileEntry);
			List<Dictionary<string, string>> list3 = source.Where((object e) => (int)e.GetType().GetProperty("punchprofile_id")?.GetValue(e) == (int)profileEntry.GetType().GetProperty("Id")?.GetValue(profileEntry)).Select(_importHelper.GetProperties).ToList();
			List<Dictionary<string, string>> list4 = new List<Dictionary<string, string>>();
			foreach (Dictionary<string, string> item in list3)
			{
				if (!string.IsNullOrEmpty(item["geometry_file"]) && File.Exists(directoryName + "\\" + item["geometry_file"]))
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string> { 
					{
						"name",
						item["geometry_file"]
					} };
					string value = directoryName + "\\" + item["geometry_file"];
					dictionary.Add("data", value);
					list4.Add(dictionary);
				}
			}
			if (File.Exists(directoryName + "\\" + (string)profileEntry.GetType().GetProperty("geometry_file")?.GetValue(profileEntry)))
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string> { 
				{
					"name",
					(string)profileEntry.GetType().GetProperty("Typ")?.GetValue(profileEntry)
				} };
				string value2 = directoryName + "\\" + (string)profileEntry.GetType().GetProperty("geometry_file")?.GetValue(profileEntry);
				dictionary2.Add("data", value2);
				list4.Add(dictionary2);
			}
			list2.Add(new RadanToolProfile(properties, list3, list4));
		}
		return new RadanPunch(list2);
	}
}

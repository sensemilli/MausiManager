using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Die;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.DieWingBend;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Punch;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class DelemXMLImporter : IToolImporter, IAdapterImporter
{
	private readonly IUnitConverter _unitConverter;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IModelFactory _modelFactory;

	private readonly ImportHelper _importHelper;

	private readonly IConfigProvider _configProvider;

	private MachineToolsViewModel _machineTools;

	public DelemXMLImporter(IUnitConverter unitConverter, IGlobalToolStorage toolStorage, IModelFactory modelFactory, ImportHelper importHelper, IConfigProvider configProvider)
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
		return Application.Current.FindResource("l_popup.PopupMachineConfig.l_filter.DelemXml") as string;
	}

	public void ImportDies(string filePath)
	{
		stylesheetDie stylesheetDie = Xml.DeserializeFromXml<stylesheetDie>(filePath);
		if (stylesheetDie.data.ToolDefinition.ToolDefinitionPart.RadiusToolFeature == null)
		{
			stylesheetDieWingBend dies = Xml.DeserializeFromXml<stylesheetDieWingBend>(filePath);
			ImportDies(dies);
		}
		else
		{
			ImportDies(stylesheetDie);
		}
	}

	public void ImportPunches(string filePath)
	{
		BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Punch.ToolDefinition toolDefinition = Xml.DeserializeFromXml<stylesheetPunch>(filePath).data.ToolDefinition;
		if (toolDefinition == null || toolDefinition.ToolType.value != "Punch")
		{
			MessageBox.Show("That is not a Punch tool. Conversion is not possible");
			return;
		}
		UpperToolGroupViewModel matchingUpperGroup = _machineTools.GetMatchingUpperGroup((toolDefinition.ToolDefinitionPart.RadiusToolFeature?.ToolRadius?.value).GetValueOrDefault());
		MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(toolDefinition.ToolName.value));
		UpperToolViewModel upperToolViewModel = ConvertPunch(toolDefinition, multiTool, matchingUpperGroup);
		ICadGeo heelGeo = null;
		if (toolDefinition.HeelDimensions != null)
		{
			heelGeo = ConvertDelemToCadGeo(toolDefinition.HeelDimensions.value, 0.0);
		}
		for (int i = 0; i < toolDefinition.Segments.Length; i++)
		{
			DelemBase.Segment segment = toolDefinition.Segments[i];
			_machineTools.UpperPieces.Add(ConvertPunchPiece(segment, heelGeo, upperToolViewModel.MultiTool.Geometry, toolDefinition, multiTool));
		}
	}

	public void ImportLowerAdapters(string filePath)
	{
		BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Adapter.ToolDefinition toolDefinition = Xml.DeserializeFromXml<stylesheetAdapter>(filePath).data.ToolDefinition;
		bool flag = toolDefinition.ToolTemplate.value.Contains("Punch");
		if (toolDefinition == null || toolDefinition.ToolResistance == null || flag)
		{
			MessageBox.Show("That is not a standard Lower Adapter tool. Convertion is not possible");
			return;
		}
		MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(toolDefinition.ToolName.value));
		_machineTools.LowerAdapters.Add(ConvertLowerAdapter(toolDefinition, multiToolViewModel));
		DelemBase.Segment[] segments = toolDefinition.Segments;
		foreach (DelemBase.Segment segment in segments)
		{
			_machineTools.LowerPieces.Add(ConvertLowerAdapterPiece(segment, multiToolViewModel));
		}
	}

	public void ImportUpperAdapters(string filePath)
	{
		BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Adapter.ToolDefinition toolDefinition = Xml.DeserializeFromXml<stylesheetAdapter>(filePath).data.ToolDefinition;
		bool flag = toolDefinition.ToolTemplate.value.Contains("Punch");
		if (toolDefinition == null || toolDefinition.ToolResistance == null || !flag)
		{
			MessageBox.Show("That is not a standard Upper Adapter tool. Convertion is not possible");
			return;
		}
		MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(toolDefinition.ToolName.value));
		_machineTools.UpperAdapters.Add(ConvertUpperAdapter(toolDefinition, multiToolViewModel));
		DelemBase.Segment[] segments = toolDefinition.Segments;
		foreach (DelemBase.Segment segment in segments)
		{
			_machineTools.UpperPieces.Add(ConvertUpperAdapterPiece(segment, multiToolViewModel));
		}
	}

	private UpperToolPieceViewModel ConvertUpperAdapterPiece(DelemBase.Segment? segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentLength.value.ToString()), Convert.ToDouble(segment.SegmentLength.value, CultureInfo.InvariantCulture), segment.NrOfSegments.value);
	}

	private LowerToolPieceViewModel ConvertLowerAdapterPiece(DelemBase.Segment? segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentLength.value.ToString()), Convert.ToDouble(segment.SegmentLength.value, CultureInfo.InvariantCulture), segment.NrOfSegments.value);
	}

	private UpperAdapterViewModel ConvertUpperAdapter(BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Adapter.ToolDefinition profile, MultiToolViewModel multitool)
	{
		double maxAllowableToolForcePerLengthUnit = ConvertToolResistanceInkNpm(profile.ToolResistance.value, profile.ToolResistance.unit);
		UpperAdapterViewModel result = new UpperAdapterViewModel(_unitConverter, _toolStorage, multitool, profile.ToolHeight.value)
		{
			Name = profile.ToolName.value,
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit,
			Implemented = true
		};
		multitool.Geometry = ConvertDelemToCadGeo(profile.ToolCrossSection.value, profile.ToolHeight.value);
		return result;
	}

	private LowerAdapterViewModel ConvertLowerAdapter(BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Adapter.ToolDefinition profile, MultiToolViewModel multitool)
	{
		double maxAllowableToolForcePerLengthUnit = ConvertToolResistanceInkNpm(profile.ToolResistance.value, profile.ToolResistance.unit);
		LowerAdapterViewModel result = new LowerAdapterViewModel(_unitConverter, _toolStorage, multitool, profile.ToolHeight.value)
		{
			Name = profile.ToolName.value,
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit,
			Implemented = true
		};
		multitool.Geometry = ConvertDelemToCadGeo(profile.ToolCrossSection.value, profile.ToolHeight.value);
		return result;
	}

	private void ImportDies(stylesheetDie? dies)
	{
		BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Die.ToolDefinition toolDefinition = dies.data.ToolDefinition;
		if (toolDefinition == null || toolDefinition.ToolDefinitionPart.RadiusToolFeature == null || toolDefinition.ToolType.value != "Die")
		{
			MessageBox.Show("That is not a right Dies tool. Conversion is not possible");
			return;
		}
		LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup(toolDefinition.ToolDefinitionPart.DieAirBendToolFeatureGroup.VOpening.value, toolDefinition.ToolDefinitionPart.DieAirBendToolFeatureGroup.ToolAngle.value);
		MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(toolDefinition.ToolName.value));
		ConvertDie(toolDefinition, multiTool, matchingLowerGroup);
		DelemBase.Segment[] segments = toolDefinition.Segments;
		foreach (DelemBase.Segment segment in segments)
		{
			_machineTools.LowerPieces.Add(ConvertDiePiece(segment, multiTool));
		}
	}

	private void ImportDies(stylesheetDieWingBend dies)
	{
		BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.DieWingBend.ToolDefinition toolDefinition = dies.data.ToolDefinition;
		if (toolDefinition == null || toolDefinition.ToolType.value != "Die")
		{
			MessageBox.Show("That is not a right Dies Wing Bend tool. Conversion is not possible");
			return;
		}
		double vAngleDeg = ((double?)(int?)toolDefinition.DieWingBendToolFeatureGroup?.ToolAngle.value) ?? 0.0;
		LowerToolGroupViewModel matchingLowerGroup = _machineTools.GetMatchingLowerGroup("WingBend", 0.0, vAngleDeg);
		MultiToolViewModel multiTool = _machineTools.CreateMultiTool(_importHelper.MakeValidFileName(toolDefinition.ToolName.value));
		ConvertWingDie(toolDefinition, multiTool, matchingLowerGroup);
		ToolDefinitionSegment[] segments = toolDefinition.Segments;
		foreach (ToolDefinitionSegment segment in segments)
		{
			_machineTools.LowerPieces.Add(ConvertWingDiePiece(segment, multiTool));
		}
	}

	private LowerToolPieceViewModel ConvertWingDiePiece(ToolDefinitionSegment? segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentLength.value.ToString()), Convert.ToDouble(segment.SegmentLength.value, CultureInfo.InvariantCulture), segment.NrOfSegments.value);
	}

	private LowerToolViewModel ConvertWingDie(BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.DieWingBend.ToolDefinition profile, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		ToolDefinitionDieWingBendToolFeatureGroup dieWingBendToolFeatureGroup = profile.DieWingBendToolFeatureGroup;
		double maxAllowableToolForcePerLengthUnit = ((dieWingBendToolFeatureGroup != null) ? ConvertToolResistanceInkNpm((int)dieWingBendToolFeatureGroup.ToolResistance.value, dieWingBendToolFeatureGroup.ToolResistance.unit) : 0.0);
		ToolDefinitionToolDefinitionPartToolCrossSection toolCrossSection = profile.ToolDefinitionPart[0].ToolCrossSection;
		new DelemBase.ToolCrossSection
		{
			unit = toolCrossSection.unit,
			value = toolCrossSection.value
		};
		MachineToolsViewModel machineTools = _machineTools;
		string value = profile.ToolName.value;
		double workingHeight = (int)profile.ToolHeight.value;
		double? vAngle = ((double?)(int?)dieWingBendToolFeatureGroup?.ToolAngle.value) ?? 0.0;
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.ALvdDelem, value, maxAllowableToolForcePerLengthUnit, 0.0, workingHeight, null, vAngle);
		lowerToolViewModel.MultiTool.Geometry = ConvertDelemToCadGeo(toolCrossSection.value, (int)profile.ToolHeight.value);
		return lowerToolViewModel;
	}

	private LowerToolPieceViewModel ConvertDiePiece(DelemBase.Segment? segment, MultiToolViewModel multiTool)
	{
		return _machineTools.CreateLowerToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentLength.value.ToString()), Convert.ToDouble(segment.SegmentLength.value, CultureInfo.InvariantCulture), segment.NrOfSegments.value);
	}

	private LowerToolViewModel ConvertDie(BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Die.ToolDefinition profile, MultiToolViewModel multiTool, LowerToolGroupViewModel group)
	{
		PartDieAirBendToolFeatureGroup dieAirBendToolFeatureGroup = profile.ToolDefinitionPart.DieAirBendToolFeatureGroup;
		double maxAllowableToolForcePerLengthUnit = ((dieAirBendToolFeatureGroup != null) ? ConvertToolResistanceInkNpm(dieAirBendToolFeatureGroup.ToolResistance.value, dieAirBendToolFeatureGroup.ToolResistance.unit) : 0.0);
		MachineToolsViewModel machineTools = _machineTools;
		string value = profile.ToolName.value;
		double value2 = profile.ToolHeight.value;
		double? vWidth = dieAirBendToolFeatureGroup?.VOpening.value ?? 0.0;
		double? vAngle = dieAirBendToolFeatureGroup?.ToolAngle.value ?? 0.0;
		double? cornerRadius = (profile.ToolDefinitionPart.RadiusToolFeature?.FirstOrDefault()?.ToolRadius.value).GetValueOrDefault();
		LowerToolViewModel lowerToolViewModel = machineTools.CreateLowerToolProfile(multiTool, group, VWidthTypes.ALvdDelem, value, maxAllowableToolForcePerLengthUnit, 0.0, value2, vWidth, vAngle, null, cornerRadius);
		lowerToolViewModel.MultiTool.Geometry = ConvertDelemToCadGeo(profile.ToolCrossSection.value, profile.ToolHeight.value);
		return lowerToolViewModel;
	}

	private UpperToolPieceViewModel ConvertPunchPiece(DelemBase.Segment? segment, ICadGeo? heelGeo, ICadGeo? profileGeo, BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Punch.ToolDefinition? profile, MultiToolViewModel multiTool)
	{
		UpperToolPieceViewModel upperToolPieceViewModel = _machineTools.CreateUpperToolPiece(_machineTools.SelectedToolList, multiTool, _importHelper.MakeValidFileName(segment.SegmentLength.value.ToString()), Convert.ToDouble(segment.SegmentLength.value, CultureInfo.InvariantCulture), segment.NrOfSegments.value);
		upperToolPieceViewModel.HasHeelLeft = segment.HasLeftHeel.value;
		upperToolPieceViewModel.HasHeelRight = segment.HasRightHeel.value;
		bool flag = Convert.ToInt32(segment.HasLeftHeel.value) == 1;
		bool flag2 = Convert.ToInt32(segment.HasRightHeel.value) == 1;
		if ((flag || flag2) && heelGeo != null)
		{
			WzgLoader.GetElements(profileGeo, out var elements);
			WzgLoader.GetElements(heelGeo, out var elements2);
			WzgLoader.SortElements(elements);
			WzgLoader.SortElements(elements2);
			List<Polygon2D> contours = WzgLoader.GetContours(elements);
			List<List<Vector2d>> list = new List<List<Vector2d>>();
			List<List<Vector2d>> list2 = new List<List<Vector2d>>();
			foreach (List<Vector3d> item in elements2)
			{
				List<Vector2d> list3 = new List<Vector2d>();
				List<Vector2d> list4 = new List<Vector2d>();
				for (int i = 0; i < item.Count; i++)
				{
					if (i == 0 || item[i] != item[i - 1])
					{
						list3.Add(new Vector2d(item[i].X + (double)(int)segment.SegmentLength.value, item[i].Y));
						list4.Add(new Vector2d(0.0 - item[i].X, item[i].Y));
					}
				}
				list2.Add(list3);
				list4.Reverse();
				list.Add(list4);
			}
			Polygon2D polygon2D = new Polygon2D();
			polygon2D.Vertices.Add(new Vector2d(0.0, 0.0));
			polygon2D.Vertices.Add(new Vector2d((int)segment.SegmentLength.value, 0.0));
			polygon2D.Vertices.Add(new Vector2d((int)segment.SegmentLength.value, profile.ToolHeight.value + 1.0));
			polygon2D.Vertices.Add(new Vector2d(0.0, profile.ToolHeight.value + 1.0));
			polygon2D.Vertices.Add(new Vector2d(0.0, 0.0));
			List<List<Vector2d>> list5 = new List<List<Vector2d>>();
			list5.Add(polygon2D.Vertices);
			if (flag2)
			{
				foreach (List<Vector2d> item2 in list2)
				{
					list5.Add(item2);
				}
			}
			if (flag)
			{
				foreach (List<Vector2d> item3 in list)
				{
					list5.Add(item3);
				}
			}
			List<List<Vector2d>> list6 = BooleanFunctions2D.PolygonUnion(list5);
			list6[0].Add(list6[0][0]);
			List<List<Vector3d>> list7 = new List<List<Vector3d>>();
			list7.Add(new List<Vector3d>());
			foreach (Vector2d item4 in list6[0])
			{
				list7[0].Add(new Vector3d(item4.X, item4.Y, 0.0));
			}
			WzgLoader.SortElements(list7);
			List<Polygon2D> contours2 = WzgLoader.GetContours(list7);
			WzgLoader.GetHeelTool(contours, contours2, out var resultModel, advancedExtrude: true);
			upperToolPieceViewModel.Geometry3D = resultModel;
		}
		return upperToolPieceViewModel;
	}

	private UpperToolViewModel ConvertPunch(BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DelemConfig.Punch.ToolDefinition profile, MultiToolViewModel multiTool, UpperToolGroupViewModel group)
	{
		AirBendToolFeatureGroup punchAirBendToolFeatureGroup = profile.ToolDefinitionPart.PunchAirBendToolFeatureGroup;
		double maxAllowableToolForcePerLengthUnit = ((punchAirBendToolFeatureGroup != null) ? ConvertToolResistanceInkNpm(punchAirBendToolFeatureGroup.ToolResistance.value, punchAirBendToolFeatureGroup.ToolResistance.unit) : 0.0);
		UpperToolViewModel upperToolViewModel = _machineTools.CreateUpperToolProfile(multiTool, group, (profile.ToolDefinitionPart?.RadiusToolFeature?.ToolRadius?.value).GetValueOrDefault(), (profile.ToolDefinitionPart?.PunchAirBendToolFeatureGroup?.ToolAngle?.value).GetValueOrDefault(), profile.ToolHeight.value, profile.ToolName.value, maxAllowableToolForcePerLengthUnit, 0.0, 0.0);
		upperToolViewModel.MultiTool.Geometry = ConvertDelemToCadGeo(profile.ToolCrossSection.value, profile.ToolHeight.value);
		return upperToolViewModel;
	}

	private static double ConvertToolResistanceInkNpm(double value, string unit)
	{
		if (unit == "KiloNewtonPerMm")
		{
			return value * 1000.0;
		}
		throw new Exception("unknown unit in ToolResistance: " + unit);
	}

	private ICadGeo ConvertDelemToCadGeo(string geometryElements, double workingHeight)
	{
		ICadGeo cadGeo = new CadGeoInfoBase();
		List<string> list = geometryElements.Split(' ').ToList();
		try
		{
			int num = 1;
			while (num < list.Count)
			{
				double num2 = _importHelper.SpecialConvertToDouble(list[num++]);
				double num3 = _importHelper.SpecialConvertToDouble(list[num++]);
				double num4 = _importHelper.SpecialConvertToDouble(list[num++]);
				double num5 = _importHelper.SpecialConvertToDouble(list[num++]);
				if (Convert.ToBoolean(list[num++]))
				{
					bool num6 = Convert.ToBoolean(list[num++]);
					double x = _importHelper.SpecialConvertToDouble(list[num++]);
					double y = _importHelper.SpecialConvertToDouble(list[num++]);
					Vector2d vector2d = new Vector2d(x, y);
					Vector2d vector2d2 = new Vector2d(num2, num3);
					Vector2d vector2d3 = new Vector2d(num4, num5);
					int direction = ((!num6) ? 1 : (-1));
					double length = (vector2d2 - vector2d).Length;
					double num7 = Math.Atan2(vector2d2.Y - vector2d.Y, vector2d2.X - vector2d.X);
					double num8 = Math.Atan2(vector2d3.Y - vector2d.Y, vector2d3.X - vector2d.X);
					GeoElementInfo gi = new GeoArcInfo
					{
						PnColor = 2,
						GroupElementNumber = 0,
						ElementNumber = cadGeo.GeoElements.Count,
						ContourType = 1,
						GeoType = GeoElementType.Ellips,
						Direction = direction,
						BeginAngle = num7 * 180.0 / Math.PI,
						EndAngle = num8 * 180.0 / Math.PI,
						X0 = vector2d.X,
						Y0 = vector2d.Y + workingHeight,
						Radius = length,
						Diameter = 2.0 * length
					};
					cadGeo.AddElement(gi);
				}
				else
				{
					GeoElementInfo gi2 = new GeoLineInfo
					{
						PnColor = 2,
						GroupElementNumber = 0,
						ElementNumber = cadGeo.GeoElements.Count,
						ContourType = 1,
						GeoType = GeoElementType.Line,
						X0 = num2,
						Y0 = num3 + workingHeight,
						X1 = num4,
						Y1 = num5 + workingHeight
					};
					cadGeo.AddElement(gi2);
				}
			}
			return cadGeo;
		}
		catch (Exception)
		{
			throw new Exception("corrupt geometry");
		}
	}
}

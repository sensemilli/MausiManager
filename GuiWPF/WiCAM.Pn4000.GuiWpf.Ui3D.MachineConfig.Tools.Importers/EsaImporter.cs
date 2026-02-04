using System;
using System.Collections.Generic;
using System.IO;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class EsaImporter : IAdapterImporter, IToolImporter
{
	private enum Property
	{
		Chamfer,
		TensileStrength,
		Radius,
		Angle,
		TotalHeight,
		WorkingHeight,
		Di,
		La,
		Lc,
		Lci,
		Li,
		Li1,
		Ls,
		Lt,
		Hc,
		H1,
		Hce,
		Hi,
		Hi1
	}

	private MachineToolsViewModel _machineTools;
    private ITranslator _translator;
    private IUnitConverter _unitConverter;
    private IGlobalToolStorage _toolStorage;

    public EsaImporter(ITranslator _translator, IUnitConverter _unitConverter, IGlobalToolStorage _toolStorage)
	{
		this._translator = _translator ?? throw new ArgumentNullException(nameof(_translator));
		this._unitConverter = _unitConverter ?? throw new ArgumentNullException(nameof(_unitConverter));
		this._toolStorage = _toolStorage ?? throw new ArgumentNullException(nameof(_toolStorage));
    }

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	public string GetFilterString()
	{
		return _translator.Translate("l_popup.PopupMachineConfig.l_filter.Esa");
	}

	public void ImportPunches(string filePath)
	{
		throw new NotImplementedException();
	}

	public void ImportDies(string filePath)
	{
		throw new NotImplementedException();
	}

	public void ImportLowerAdapters(string filePath)
	{
		throw new NotImplementedException();
	}

	public void ImportUpperAdapters(string filePath)
	{
		throw new NotImplementedException();
	}

	private MultiToolViewModel ImportTool(string filePath, bool isUpper, out Dictionary<Property, string> properties)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
		ICadGeo geometry = ReadFile(filePath, isUpper, out properties);
		MultiToolViewModel multiToolViewModel = _machineTools.CreateMultiTool(null);
		multiToolViewModel.Name = fileNameWithoutExtension;
		multiToolViewModel.SetGeometryData(geometry, fileNameWithoutExtension);
		return multiToolViewModel;
	}

	private ICadGeo ReadFile(string filePath, bool isUpper, out Dictionary<Property, string> properties)
	{
		properties = new Dictionary<Property, string>();
		return new CadGeoInfoBase();
	}

	private ICadGeo CreatePunchType1(Dictionary<Property, string> properties)
	{
		double num = double.Parse(properties[Property.H1]);
		double num2 = double.Parse(properties[Property.Lt]);
		double num3 = double.Parse(properties[Property.La]);
		double num4 = double.Parse(properties[Property.Li]);
		double num5 = double.Parse(properties[Property.Hi]);
		double num6 = double.Parse(properties[Property.Lc]);
		double num7 = double.Parse(properties[Property.Hc]);
		double num8 = double.Parse(properties[Property.TotalHeight]);
		double num9 = double.Parse(properties[Property.Radius]);
		double num10 = double.DegreesToRadians(double.Parse(properties[Property.Angle]));
		CadGeoInfoBase result = new CadGeoInfoBase();
		double num11 = (Math.PI - num10) * 0.5;
		double num12 = num3 * Math.Tan(num11);
		_ = Vector2d.Zero;
		new Vector2d(num2 - num3, num12);
		new Vector2d(num2 - num3, num12 + num7);
		new Vector2d(num2 - num3, num8 - num5);
		new Vector2d(num4 - num3, num8 - num5);
		new Vector2d(num4 - num3, num8);
		new Vector2d(0.0 - num3, num8);
		new Vector2d(0.0 - num3, num8 - num);
		new Vector2d(0.0, num9);
		new Vector2d(num9 * Math.Cos(num11), num9 * Math.Sin(num11));
		new Vector2d((0.0 - num9) * Math.Cos(num11), num9 * Math.Sin(num11));
		new Vector2d(num2 - num3 - num6 + num7, 1234567.0);
		return result;
	}

	public static Vector2d FindCenter(Vector2d p1, Vector2d p2, Vector2d p3)
	{
		Vector2d vector2d = p2 - p1;
		Vector2d vector2d2 = p3 - p1;
		Vector2d vector2d3 = p3 - p2;
		double num = 2.0 * (vector2d.X * vector2d3.Y - vector2d.Y * vector2d3.X);
		double num2 = vector2d * (p1 + p2);
		double num3 = vector2d2 * (p1 + p3);
		Vector2d result = default(Vector2d);
		result.X = (vector2d2.Y * num2 - vector2d.Y * num3) / num;
		result.Y = (vector2d.X * num3 - vector2d2.X * num2) / num;
		return result;
	}

	private ICadGeo CreatePunchType2(Dictionary<Property, string> properties)
	{
		return new CadGeoInfoBase();
	}

	private ICadGeo CreatePunchType3(Dictionary<Property, string> properties)
	{
		double.Parse(properties[Property.Angle]);
		double.Parse(properties[Property.H1]);
		double.Parse(properties[Property.Lt]);
		double.Parse(properties[Property.La]);
		double.Parse(properties[Property.Li]);
		double.Parse(properties[Property.Hi]);
		double.Parse(properties[Property.Lc]);
		double.Parse(properties[Property.Hc]);
		double.Parse(properties[Property.Lci]);
		return new CadGeoInfoBase();
	}

	private void CreateArc(CadGeoInfoBase geometry, Vector2d center, Vector2d p0, Vector2d p1)
	{
		Vector2d vector2d = p0 - center;
		Vector2d vector2d2 = p1 - center;
		double length = vector2d.Length;
		double beginAngle = Math.Atan2(vector2d.Y, vector2d.X);
		double endAngle = Math.Atan2(vector2d2.Y, vector2d2.X);
		geometry.AddElement(new GeoArcInfo
		{
			PnColor = 2,
			ElementNumber = geometry.GeoElements.Count,
			GeoType = GeoElementType.Ellips,
			ContourType = 1,
			Direction = 0,
			BeginAngle = beginAngle,
			EndAngle = endAngle,
			X0 = center.X,
			Y0 = center.Y,
			Radius = length,
			Diameter = length * 2.0
		});
	}

	private void CreateLine(CadGeoInfoBase geometry, Vector2d p0, Vector2d p1)
	{
		geometry.AddElement(new GeoLineInfo
		{
			PnColor = 2,
			ElementNumber = geometry.GeoElements.Count,
			ContourType = 1,
			GeoType = GeoElementType.Line,
			X0 = p0.X,
			Y0 = p0.Y,
			X1 = p1.X,
			Y1 = p1.Y
		});
	}
}

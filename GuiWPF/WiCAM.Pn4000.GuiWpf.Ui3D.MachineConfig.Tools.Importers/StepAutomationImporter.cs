using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.Utility;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;

public class StepAutomationImporter(ITranslator _translator, IUnitConverter _unitConverter, IGlobalToolStorage _toolStorage) : IAdapterImporter, IToolImporter, IHolderImporter
{
	private enum Property
	{
		Radius,
		Height,
		ActualHeight,
		RotatedHeight,
		VWidth,
		Angle,
		Type,
		AttachType,
		AttachTypeSup,
		AttachTypeInf,
		GripperType,
		HornLength,
		MaxHHorn,
		MinHHorn,
		Resistance,
		ResistanceHorn,
		TipWidth,
		Safety1,
		Safety2,
		DeltaY,
		Length,
		XOffset,
		ROffset,
		MobileLength,
		FingerNumb,
		NormalHeight
	}

	private MachineToolsViewModel _machineTools;

	public void Init(MachineToolsViewModel machineTools)
	{
		_machineTools = machineTools;
	}

	public string GetFilterString()
	{
		return _translator.Translate("l_popup.PopupMachineConfig.l_filter.StepAutomation");
	}

	public void ImportPunches(string filePath)
	{
		Dictionary<Property, string> properties;
		MultiToolViewModel multiToolViewModel = ImportTool(filePath, isUpper: true, out properties);
		double radius = double.Parse(properties[Property.Radius]);
		double value = double.DegreesToRadians(double.Parse(properties[Property.Angle]));
		double workingHeight = double.Parse(properties[Property.Height]);
		double maxAllowableToolForcePerLengthUnit = MetricTonneForcePerCentiMeterToKiloNewtonPerMeter(double.Parse(properties[Property.Resistance]));
		UpperToolGroupViewModel upperToolGroupViewModel = _machineTools.UpperGroups.FirstOrDefault((UpperToolGroupViewModel x) => x.Radius == radius);
		if (upperToolGroupViewModel == null)
		{
			upperToolGroupViewModel = _machineTools.CreateUpperToolGroup($"R{radius}", radius);
		}
		_machineTools.CreateUpperToolProfile(multiToolViewModel, upperToolGroupViewModel, radius, value, workingHeight, multiToolViewModel.Name, maxAllowableToolForcePerLengthUnit, 0.0, 0.0).MountTypeID = int.Parse(properties[Property.AttachType]);
	}

	public void ImportDies(string filePath)
	{
		Dictionary<Property, string> properties;
		MultiToolViewModel multiToolViewModel = ImportTool(filePath, isUpper: false, out properties);
		double radius = double.Parse(properties[Property.Radius]);
		double angle = double.DegreesToRadians(double.Parse(properties[Property.Angle]));
		double vWidth = double.Parse(properties[Property.VWidth]);
		double workingHeight = double.Parse(properties[Property.Height]);
		double maxAllowableToolForcePerLengthUnit = MetricTonneForcePerCentiMeterToKiloNewtonPerMeter(double.Parse(properties[Property.Resistance]));
		LowerToolGroupViewModel lowerToolGroupViewModel = _machineTools.LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.Radius == radius && x.VWidth == vWidth && Math.Abs(x.VAngle - angle) < 1E-06);
		if (lowerToolGroupViewModel == null)
		{
			lowerToolGroupViewModel = _machineTools.CreateLowerToolGroup(vWidth, angle, radius, $"W{vWidth} A{angle}");
		}
		MachineToolsViewModel machineTools = _machineTools;
		LowerToolGroupViewModel lowerGroup = lowerToolGroupViewModel;
		string name = multiToolViewModel.Name;
		double? cornerRadius = radius;
		double? vAngle = angle;
		double? vWidth2 = vWidth;
		machineTools.CreateLowerToolProfile(multiToolViewModel, lowerGroup, VWidthTypes.ALvdDelem, name, maxAllowableToolForcePerLengthUnit, 0.0, workingHeight, vWidth2, vAngle, null, cornerRadius).MountTypeID = int.Parse(properties[Property.AttachType]);
	}

	public void ImportLowerHolders(string filePath)
	{
		ImportLowerAdapters(filePath);
	}

	public void ImportUpperHolders(string filePath)
	{
		ImportUpperAdapters(filePath);
	}

	public void ImportLowerAdapters(string filePath)
	{
		Dictionary<Property, string> properties;
		MultiToolViewModel multiToolViewModel = ImportTool(filePath, isUpper: false, out properties);
		double maxAllowableToolForcePerLengthUnit = MetricTonneForcePerCentiMeterToKiloNewtonPerMeter(double.Parse(properties[Property.Resistance]));
		LowerAdapterViewModel item = new LowerAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel)
		{
			Name = multiToolViewModel.Name,
			WorkingHeight = double.Parse(properties[Property.Height]),
			MountTypeID = int.Parse(properties[Property.AttachTypeInf]),
			SocketId = int.Parse(properties[Property.AttachTypeSup]),
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit
		};
		_machineTools.LowerAdapters.Add(item);
	}

	public void ImportUpperAdapters(string filePath)
	{
		Dictionary<Property, string> properties;
		MultiToolViewModel multiToolViewModel = ImportTool(filePath, isUpper: true, out properties);
		double maxAllowableToolForcePerLengthUnit = MetricTonneForcePerCentiMeterToKiloNewtonPerMeter(double.Parse(properties[Property.Resistance]));
		UpperAdapterViewModel item = new UpperAdapterViewModel(_unitConverter, _toolStorage, multiToolViewModel)
		{
			Name = multiToolViewModel.Name,
			WorkingHeight = double.Parse(properties[Property.Height]),
			MountTypeID = int.Parse(properties[Property.AttachTypeInf]),
			SocketId = int.Parse(properties[Property.AttachTypeSup]),
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit
		};
		_machineTools.UpperAdapters.Add(item);
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
		CadGeoInfoBase cadGeoInfoBase = new CadGeoInfoBase();
		Regex regex = new Regex("-?(?:\\d*\\.*\\d+)", RegexOptions.Compiled);
		bool flag = false;
		Vector2d? vector2d = null;
		Vector2d? vector2d2 = null;
		Vector2d zero = Vector2d.Zero;
		foreach (string item in File.ReadLines(filePath))
		{
			Property[] values = System.Enum.GetValues<Property>();
			foreach (Property property in values)
			{
				if (item.StartsWith(property switch
				{
					Property.Radius => "Rad =", 
					Property.Height => "Hei =", 
					Property.ActualHeight => "RealHei =", 
					Property.RotatedHeight => "HeR =", 
					Property.VWidth => "Lar =", 
					Property.Angle => "Ang =", 
					Property.HornLength => "Len =", 
					Property.MaxHHorn => "Max =", 
					Property.MinHHorn => "Min =", 
					Property.Resistance => "Res =", 
					Property.ResistanceHorn => "ResHorn =", 
					Property.Type => "Type =", 
					Property.AttachType => "AttachType =", 
					Property.AttachTypeSup => "AttachTypeSup =", 
					Property.AttachTypeInf => "AttachTypeInf =", 
					Property.GripperType => "GripperType =", 
					Property.TipWidth => "TipWidth =", 
					Property.Safety1 => "Sic =", 
					Property.Safety2 => "SiR =", 
					Property.DeltaY => "DeltaY =", 
					Property.Length => "Lun =", 
					Property.XOffset => "Ofx =", 
					Property.ROffset => "Ofr =", 
					Property.MobileLength => "LunVar", 
					Property.FingerNumb => "NumApp", 
					Property.NormalHeight => "Alt1 =", 
					_ => throw new ArgumentOutOfRangeException("property", property, null), 
				}) && properties.TryAdd(property, regex.Match(item).Value))
				{
					break;
				}
			}
			if (item.StartsWith("[Ele ="))
			{
				flag = true;
			}
			else if (item.StartsWith("[EndEle]"))
			{
				flag = false;
			}
			else if (!item.StartsWith("[EleB =") && !item.StartsWith("[EndEleB]"))
			{
				if (!isUpper && item.StartsWith("[VeSp1 ="))
				{
					MatchCollection matchCollection = regex.Matches(item);
					zero.X = double.Parse(matchCollection[1].Value, NumberStyles.Any);
					zero.Y = double.Parse(matchCollection[2].Value, NumberStyles.Any);
				}
				else if (isUpper && item.StartsWith("[VeSp2 ="))
				{
					MatchCollection matchCollection2 = regex.Matches(item);
					zero.X = double.Parse(matchCollection2[1].Value, NumberStyles.Any);
					zero.Y = double.Parse(matchCollection2[2].Value, NumberStyles.Any);
				}
				else if ((!isUpper || !item.StartsWith("[VeSp3 =")) && isUpper)
				{
					item.StartsWith("[VeSp4 =");
				}
			}
			if (!flag)
			{
				continue;
			}
			if (item.StartsWith("V"))
			{
				MatchCollection matchCollection3 = regex.Matches(item);
				Vector2d vector2d3 = default(Vector2d);
				vector2d3.X = double.Parse(matchCollection3[1].Value, NumberStyles.Any);
				vector2d3.Y = double.Parse(matchCollection3[2].Value, NumberStyles.Any);
				Vector2d vector2d4 = vector2d3;
				if (vector2d2.HasValue)
				{
					cadGeoInfoBase.AddElement(new GeoLineInfo
					{
						PnColor = 2,
						GroupElementNumber = 0,
						ElementNumber = cadGeoInfoBase.GeoElements.Count,
						ContourType = 1,
						GeoType = GeoElementType.Line,
						X0 = vector2d2.Value.X,
						Y0 = vector2d2.Value.Y,
						X1 = vector2d4.X,
						Y1 = vector2d4.Y
					});
				}
				vector2d3 = vector2d.GetValueOrDefault();
				if (!vector2d.HasValue)
				{
					vector2d3 = vector2d4;
					vector2d = vector2d3;
				}
				vector2d2 = vector2d4;
			}
			else
			{
				if (!item.StartsWith("C"))
				{
					continue;
				}
				MatchCollection matchCollection4 = regex.Matches(item);
				Vector2d vector2d3 = default(Vector2d);
				vector2d3.X = double.Parse(matchCollection4[1].Value, NumberStyles.Any);
				vector2d3.Y = double.Parse(matchCollection4[2].Value, NumberStyles.Any);
				Vector2d vector2d5 = vector2d3;
				double num = double.DegreesToRadians(double.Parse(matchCollection4[3].Value, NumberStyles.Any));
				if (vector2d2.HasValue)
				{
					Vector2d vector2d6 = vector2d2.Value - vector2d5;
					double length = vector2d6.Length;
					double num2 = Math.Atan2(vector2d6.Y, vector2d6.X);
					double num3 = num2 + num;
					Vector2d vector2d7 = new Vector2d(Math.Cos(num3), Math.Sin(num3));
					vector2d2 = vector2d5 + vector2d7 * length;
					if (Math.Sign(num) < 0)
					{
						double num4 = num3;
						num3 = num2;
						num2 = num4;
					}
					cadGeoInfoBase.AddElement(new GeoArcInfo
					{
						PnColor = 2,
						GroupElementNumber = 0,
						ElementNumber = cadGeoInfoBase.GeoElements.Count,
						ContourType = 1,
						GeoType = GeoElementType.Ellips,
						Direction = 1,
						BeginAngle = double.RadiansToDegrees(num2),
						EndAngle = double.RadiansToDegrees(num3),
						X0 = vector2d5.X,
						Y0 = vector2d5.Y,
						Radius = length,
						Diameter = length * 2.0
					});
				}
			}
		}
		if (vector2d.HasValue && vector2d2.HasValue)
		{
			cadGeoInfoBase.AddElement(new GeoLineInfo
			{
				PnColor = 2,
				GroupElementNumber = 0,
				ElementNumber = cadGeoInfoBase.GeoElements.Count,
				ContourType = 1,
				GeoType = GeoElementType.Line,
				X0 = vector2d2.Value.X,
				Y0 = vector2d2.Value.Y,
				X1 = vector2d.Value.X,
				Y1 = vector2d.Value.Y
			});
		}
		cadGeoInfoBase.MoveAll(0.0 - zero.X, 0.0 - zero.Y);
		return cadGeoInfoBase;
	}

	private static double MetricTonneForcePerCentiMeterToKiloNewtonPerMeter(double value)
	{
		return value * 9.80665 * 100.0;
	}
}

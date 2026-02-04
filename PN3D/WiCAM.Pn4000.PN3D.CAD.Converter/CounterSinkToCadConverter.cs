using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Services.ConfigProviders.Contracts;
using Vector3d = WiCAM.Pn4000.BendModel.Base.Vector3d;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class CounterSinkToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 25;

	public static bool AddCadGeoElements(CounterSink counterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, ICad2DDatabase db2D, IConfigProvider configProvider, IPnPathService pathService)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		bool exportCounterSinkAngle = ConvertConfig.GetAnalyzeConfig(configProvider, pathService).ExportCounterSinkAngle;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = counterSink.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(v))
		{
		case OrientationTypes.Top:
			flag = true;
			break;
		case OrientationTypes.Bottom:
			flag = false;
			break;
		default:
			return false;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = counterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		if (exportCounterSinkAngle)
		{
			db2D.AddText(CounterSinkToCadConverter.GetCadTxt(counterSink.Side1Radius, counterSink.Side0Radius, v2, flag ? "+" : "-", counterSink.Angle));
		}
		else
		{
			db2D.AddText(CounterSinkToCadConverter.GetCadTxt(counterSink.Side1Radius, counterSink.Side0Radius, v2, flag ? "+" : "-"));
		}
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v2, 0.0, 360.0, counterSink.Side1Radius, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v2, 0.0, 360.0, counterSink.Side0Radius, flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double innerRadius, double outerRadius, 
    Vector3d middlePoint, string addOn)
	{
		const int toolType = 25;
		
		var text = string.Format(CultureInfo.InvariantCulture,
			"_TT_{0}_D{1}_D{2}_{3}",
			toolType,
			Math.Round(innerRadius * 2.0, 4),
			Math.Round(outerRadius * 2.0, 4),
			addOn);

		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = toolType,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	private static CadTxtText GetCadTxt(double innerRadius, double outerRadius, 
    Vector3d middlePoint, string addOn, double angle)
	{
		const int toolType = 25;
		
		var text = string.Format(CultureInfo.InvariantCulture,
			"_TT_{0}_D{1}_D{2}_{3}_{4}",
			toolType,
			Math.Round(innerRadius * 2.0, 4),
			Math.Round(outerRadius * 2.0, 4),
			addOn,
			Math.Round(angle, 2));

		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = toolType,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	public static CounterSinkXml GetXmlElement(CounterSink counterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = counterSink.Orientation;
		double[] radii = counterSink.RadiusDepths.Select((CounterSink.Level x) => x.Radius).ToArray();
		double[] depths = counterSink.RadiusDepths.Select((CounterSink.Level x) => x.Depth).ToArray();
		worldMatrix.TransformNormalInPlace(ref v);
		topDirectionVisibleFace.IsParallel(v, out var direction);
		bool flag = direction == 1;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = counterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		return new CounterSinkXml
		{
			ID = counterSink.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			TopRadius = counterSink.Side0Radius,
			BottomRadius = counterSink.Side1Radius,
			Radii = radii,
			Depths = depths,
			Direction = (flag ? 1 : (-1))
		};
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class TwoSideCountersinkToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 25;

	public static bool AddCadGeoElements(TwoSidedCounterSink twoSidedCounterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, ICad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		bool valueOrDefault = configProvider.InjectOrCreate<AnalyzeConfig>()?.ExportCounterSinkAngle == true;
		double innerRadius = twoSidedCounterSink.InnerRadius;
		bool flag;
		double num;
		double outerTopAngle;
		double num2;
		double outerBottomAngle;
		switch (MacroToCadConverterBase.GetOrientation(twoSidedCounterSink.Orientation, worldMatrix))
		{
		case OrientationTypes.Top:
			flag = true;
			num = twoSidedCounterSink.Side0Radius;
			outerTopAngle = twoSidedCounterSink.Side0Angle;
			num2 = twoSidedCounterSink.Side1Radius;
			outerBottomAngle = twoSidedCounterSink.Side1Angle;
			break;
		case OrientationTypes.Bottom:
			flag = false;
			num = twoSidedCounterSink.Side1Radius;
			outerTopAngle = twoSidedCounterSink.Side1Angle;
			num2 = twoSidedCounterSink.Side0Radius;
			outerBottomAngle = twoSidedCounterSink.Side0Angle;
			break;
		default:
			return false;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = twoSidedCounterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		if (valueOrDefault)
		{
			db2D.AddText(TwoSideCountersinkToCadConverter.GetCadTxt(twoSidedCounterSink.InnerRadius, num, num2, v, flag ? "+" : "-", outerTopAngle, outerBottomAngle));
		}
		else
		{
			db2D.AddText(TwoSideCountersinkToCadConverter.GetCadTxt(twoSidedCounterSink.InnerRadius, num, num2, v, flag ? "+" : "-"));
		}
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, innerRadius, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, num, macro3DConfig.MacroPosColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, num2, macro3DConfig.MacroNegColorPn)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double innerDiameter, double outerTopDiameter, double outerBottomDiameter, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn, double outerTopAngle, double outerBottomAngle)
	{
        CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        var text = string.Format(invariantCulture,
            "_TT_{0}_D{1}_D{2}_D{3}_{4}_{5}_{6}",
            25,
            Math.Round(innerDiameter * 2.0, 4),
            Math.Round(outerTopDiameter * 2.0, 4),
            Math.Round(outerBottomDiameter * 2.0, 4),
            addOn,
            Math.Round(outerTopAngle, 2),
            Math.Round(outerBottomAngle, 2));

        return new CadTxtText
        {
            Text = text,
            Angle = 0.0,
            Color = 25,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

    private static CadTxtText GetCadTxt(double innerDiameter, double outerTopDiameter, double outerBottomDiameter, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
    {
        CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        var text = string.Format(invariantCulture,
            "_TT_{0}_D{1}_D{2}_D{3}_{4}",
            25,
            Math.Round(innerDiameter * 2.0, 4),
            Math.Round(outerTopDiameter * 2.0, 4),
            Math.Round(outerBottomDiameter * 2.0, 4),
            addOn);

        return new CadTxtText
        {
            Text = text,
            Angle = 0.0,
            Color = 25,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

    public static TwoSidedCounterSinkXml GetXmlElement(TwoSidedCounterSink twoSidedCounterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		double innerRadius = twoSidedCounterSink.InnerRadius;
		bool flag = 1.0 - topDirectionVisibleFace.Z > 0.0;
		double topRadius;
		double bottomRadius;
		double[] radii;
		double[] depths;
		if (flag)
		{
			topRadius = twoSidedCounterSink.Side0Radius;
			bottomRadius = twoSidedCounterSink.Side1Radius;
			radii = twoSidedCounterSink.RadiusDepths.Select((CounterSink.Level x) => x.Radius).ToArray();
			depths = twoSidedCounterSink.RadiusDepths.Select((CounterSink.Level x) => x.Depth).ToArray();
		}
		else
		{
			topRadius = twoSidedCounterSink.Side1Radius;
			bottomRadius = twoSidedCounterSink.Side0Radius;
			List<double> list = twoSidedCounterSink.RadiusDepths.Select((CounterSink.Level x) => x.Depth).Skip(1).Reverse()
				.ToList();
			list.Insert(0, 0.0);
			depths = list.ToArray();
			radii = twoSidedCounterSink.RadiusDepths.Select((CounterSink.Level x) => x.Radius).Reverse().ToArray();
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = twoSidedCounterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return new TwoSidedCounterSinkXml
		{
			ID = twoSidedCounterSink.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			},
			TopRadius = topRadius,
			MiddleRadius = innerRadius,
			BottomRadius = bottomRadius,
			Radii = radii,
			Depths = depths,
			Direction = (flag ? 1 : (-1))
		};
	}
}

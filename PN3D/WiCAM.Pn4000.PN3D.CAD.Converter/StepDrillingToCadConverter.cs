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

public class StepDrillingToCadConverter : MacroToCadConverterBase
{
	public static bool AddCadGeoElements(StepDrilling stepDrilling, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, ICad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		if (stepDrilling.RadiusDepths.Count != 4 && stepDrilling.RadiusDepths.Count != 6)
		{
			return false;
		}
		if (stepDrilling.Steps == 2)
		{
			double num = Math.Min(stepDrilling.Side0Radius, stepDrilling.Side1Radius);
			double num2 = Math.Max(stepDrilling.Side0Radius, stepDrilling.Side1Radius);
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v = stepDrilling.Orientation;
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
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = stepDrilling.AnchorPoint;
			worldMatrix.TransformInPlace(ref v2);
			db2D.AddText(StepDrillingToCadConverter.GetCadTxt(num, num2, stepDrilling.RadiusDepths.Sum((StepDrilling.Level x) => x.Depth), stepDrilling.Side1Depth, v2, flag ? "+" : "-"));
			db2D.AddInnerLine(new HashSet<CadGeoElement>
			{
				MacroToCadConverterBase.GetCadGeoCircleByDiameter(v2, 0.0, 360.0, num, macro3DConfig.BaseColorPn),
				MacroToCadConverterBase.GetCadGeoCircleByDiameter(v2, 0.0, 360.0, num2, flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn)
			});
			return true;
		}
		double innerRadius = stepDrilling.InnerRadius;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = stepDrilling.Orientation;
		worldMatrix.TransformNormalInPlace(ref v3);
		bool flag2;
		double num3;
		double outerTopDepth;
		double num4;
		double outerBottomDepth;
		switch (MacroToCadConverterBase.GetOrientation(v3))
		{
		case OrientationTypes.Top:
			flag2 = true;
			num3 = stepDrilling.Side0Radius;
			outerTopDepth = stepDrilling.Side0Depth;
			num4 = stepDrilling.Side1Radius;
			outerBottomDepth = stepDrilling.Side1Depth;
			break;
		case OrientationTypes.Bottom:
			flag2 = false;
			num3 = stepDrilling.Side1Radius;
			outerTopDepth = stepDrilling.Side1Depth;
			num4 = stepDrilling.Side0Radius;
			outerBottomDepth = stepDrilling.Side0Depth;
			break;
		default:
			return false;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v4 = stepDrilling.AnchorPoint;
		worldMatrix.TransformInPlace(ref v4);
		double totalDepth = stepDrilling.RadiusDepths.Sum((StepDrilling.Level x) => x.Depth);
		_ = stepDrilling.Side1Depth;
		db2D.AddText(StepDrillingToCadConverter.GetCadTxtDoubleStep(innerRadius, num3, num4, totalDepth, outerTopDepth, outerBottomDepth, v4, flag2 ? "+" : "-"));
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v4, 0.0, 360.0, innerRadius, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v4, 0.0, 360.0, num3, macro3DConfig.MacroPosColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v4, 0.0, 360.0, num4, macro3DConfig.MacroNegColorPn)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double innerRadius, double outerRadius, double totalDepth, double depthOfMinRadius, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
        const int toolType = 26;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}_D{1}_D{2}_{3}_{4}_{5}",
            toolType,
            Math.Round(innerRadius * 2.0, 4),
            Math.Round(outerRadius * 2.0, 4),
            Math.Round(totalDepth, 4),
            Math.Round(depthOfMinRadius, 4),
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

	private static CadTxtText GetCadTxtDoubleStep(double innerRadius, double outerTopDiameter, double outerBottomDiameter, double totalDepth, double outerTopDepth, double outerBottomDepth, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
        const int toolType = 27;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}_D{1}_D{2}_D{3}_{4}_{5}_{6}_{7}",
            toolType,
            Math.Round(innerRadius * 2.0, 4),
            Math.Round(outerTopDiameter * 2.0, 4),
            Math.Round(outerBottomDiameter * 2.0, 4),
            Math.Round(totalDepth, 4),
            Math.Round(outerTopDepth, 4),
            Math.Round(outerBottomDepth, 4),
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

	public static StepDrillingXml GetXmlElement(StepDrilling stepDrilling, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = stepDrilling.Orientation;
		double[] radii = stepDrilling.RadiusDepths.Select((StepDrilling.Level x) => x.Radius).ToArray();
		double[] depths = stepDrilling.RadiusDepths.Select((StepDrilling.Level x) => x.Depth).ToArray();
		double side1Radius = stepDrilling.Side1Radius;
		double side0Radius = stepDrilling.Side0Radius;
		worldMatrix.TransformNormalInPlace(ref v);
		topDirectionVisibleFace.IsParallel(v, out var direction);
		bool flag = direction == 1;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = stepDrilling.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		return new StepDrillingXml
		{
			ID = stepDrilling.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			TopRadius = side0Radius,
			BottomRadius = side1Radius,
			Radii = radii,
			Depths = depths,
			Direction = (flag ? 1 : (-1))
		};
	}
}

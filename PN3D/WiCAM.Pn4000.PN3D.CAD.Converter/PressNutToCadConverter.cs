using System;
using System.Collections.Generic;
using System.Globalization;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class PressNutToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 50;

	public static bool AddCadGeoElements(PressNut pressNut, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(pressNut.Orientation, worldMatrix))
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = pressNut.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		double num = Math.Min(pressNut.Side0Radius, pressNut.Side1Radius);
		double num2 = Math.Max(pressNut.Side0Radius, pressNut.Side1Radius);
		db2D.AddText(PressNutToCadConverter.GetCadTxt(num, num2, v, flag ? "+" : "-"));
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, num, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, num2, flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double smallDiameter, double largeDiameter, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
        const int toolType = 50;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}_D{1}_D{2}_{3}",
            toolType,
            Math.Round(smallDiameter * 2.0, 4),
            Math.Round(largeDiameter * 2.0, 4),
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

	public static PressNutXml GetXmlElement(PressNut pressNut, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		double bottomRadius;
		double topRadius;
		if (pressNut.Side0Radius < pressNut.Side1Radius)
		{
			bottomRadius = pressNut.Side0Radius;
			topRadius = pressNut.Side1Radius;
		}
		else
		{
			bottomRadius = pressNut.Side1Radius;
			topRadius = pressNut.Side0Radius;
		}
		bool flag = 1.0 - topDirectionVisibleFace.Z > 0.0;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = pressNut.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return new PressNutXml
		{
			ID = pressNut.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			},
			TopRadius = topRadius,
			BottomRadius = bottomRadius,
			Direction = (flag ? 1 : (-1))
		};
	}
}

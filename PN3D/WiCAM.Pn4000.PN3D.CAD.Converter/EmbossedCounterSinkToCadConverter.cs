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

public class EmbossedCounterSinkToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 24;

	public static bool AddCadGeoElements(EmbossedCounterSink embossedCounterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		double innerRadius = embossedCounterSink.InnerRadius;
		double middleRadius = embossedCounterSink.MiddleRadius;
		double outerRadius = embossedCounterSink.OuterRadius;
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(embossedCounterSink.Orientation, worldMatrix))
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embossedCounterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		db2D.AddText(EmbossedCounterSinkToCadConverter.GetCadTxt(innerRadius, middleRadius, outerRadius, v, flag ? "+" : "-"));
		int color = (flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn);
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, innerRadius, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, middleRadius, color),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, outerRadius, color)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double innerRadius, double middleRadius, double outerRadius, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
        const int toolType = 24;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}_D{1}_D{2}_D{3}_{4}",
            toolType,
            Math.Round(innerRadius * 2.0, 4),
            Math.Round(middleRadius * 2.0, 4),
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

	public static EmbossedCounterSinkXml GetXmlElement(EmbossedCounterSink embossedCounterSink, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		double innerRadius = embossedCounterSink.InnerRadius;
		double innerRadius2 = embossedCounterSink.InnerRadius;
		double outerRadius = embossedCounterSink.OuterRadius;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embossedCounterSink.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		bool flag = Math.Abs(topDirectionVisibleFace.Z - v.Z) > 0.0;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = embossedCounterSink.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		return new EmbossedCounterSinkXml
		{
			ID = embossedCounterSink.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			TopRadius = outerRadius,
			MiddleRadius = innerRadius2,
			BottomRadius = innerRadius,
			Direction = (flag ? 1 : (-1))
		};
	}
}

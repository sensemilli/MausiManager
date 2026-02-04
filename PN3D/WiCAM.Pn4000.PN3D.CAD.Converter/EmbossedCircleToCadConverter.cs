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

public class EmbossedCircleToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 13;

	public static bool AddCadGeoElements(EmbossedCircle embossedCircle, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		int color;
		string depthSign;
		switch (MacroToCadConverterBase.GetOrientation(embossedCircle.Orientation, worldMatrix))
		{
		case OrientationTypes.Top:
			color = macro3DConfig.MacroPosColorPn;
			depthSign = "+";
			break;
		case OrientationTypes.Bottom:
			color = macro3DConfig.MacroNegColorPn;
			depthSign = "-";
			break;
		default:
			return false;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint = worldMatrix.Transform(embossedCircle.AnchorPoint);
		db2D.AddText(EmbossedCircleToCadConverter.GetCadTxt(middlePoint, macro3DConfig.MacroEmbossedToolSuffix, embossedCircle.Depth, depthSign, embossedCircle.Radius));
		db2D.AddInnerLine(new HashSet<CadGeoElement> { MacroToCadConverterBase.GetCadGeoCircleByDiameter(middlePoint, 0.0, 360.0, embossedCircle.Radius, color) });
		return true;
	}

	private static CadTxtText GetCadTxt(global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string toolSuffix, double depth, string depthSign, double radius)
	{
        const int toolType = 13;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}{1}_R{2}_T{3}{4}",
            toolType,
            toolSuffix,
            Math.Round(radius, 4),
            depthSign,
            Math.Round(depth, 4));

        return new CadTxtText
        {
            Text = text,
            Angle = 0.0,
            Color = toolType,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

	public static EmbossedCircleXml GetXmlElement(EmbossedCircle embossedCircle, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embossedCircle.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		int direction = Math.Sign(v.Z);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = embossedCircle.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		return new EmbossedCircleXml
		{
			Depth = embossedCircle.Depth,
			Radius = embossedCircle.Radius,
			ID = embossedCircle.ID,
			Direction = direction,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			}
		};
	}
}

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

public class EmbossedRectangleRoundedToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 13;

	public static bool AddCadGeoElements(EmbossedRectangleRounded embossed, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		global::WiCAM.Pn4000.BendModel.Base.Vector3d orientation = embossed.Orientation;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d widthDir = embossed.WidthDir;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d anchorPoint = embossed.AnchorPoint;
		orientation = worldMatrix.TransformNormal(orientation);
		widthDir = worldMatrix.TransformNormal(widthDir).Normalized;
		anchorPoint = worldMatrix.Transform(anchorPoint);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d normalized = orientation.Cross(widthDir).Normalized;
		int color;
		string depthSign;
		switch (MacroToCadConverterBase.GetOrientation(orientation))
		{
		case OrientationTypes.Top:
			color = macro3DConfig.MacroPosColorPn;
			depthSign = "+";
			break;
		case OrientationTypes.Bottom:
			color = macro3DConfig.MacroNegColorPn;
			depthSign = "-";
			normalized *= -1.0;
			break;
		default:
			return false;
		}
		double num;
		if (Math.Abs(widthDir.X) < 1E-06)
		{
			num = ((!(widthDir.Y > 0.0)) ? 270.0 : 90.0);
		}
		else
		{
			num = Math.Atan(widthDir.Y / widthDir.X) * 180.0 / Math.PI;
			if (widthDir.X < 0.0)
			{
				num += 180.0;
			}
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d = widthDir * (0.5 * embossed.Width - embossed.Radius);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d2 = widthDir * embossed.Radius;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d3 = normalized * (0.5 * embossed.Height - embossed.Radius);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d4 = normalized * embossed.Radius;
		db2D.AddText(EmbossedRectangleRoundedToCadConverter.GetCadTxt(anchorPoint, macro3DConfig.MacroEmbossedToolSuffix, embossed.Depth, depthSign, embossed.Width, embossed.Height, embossed.Radius, num));
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement> { MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint + vector3d + vector3d3, 90.0 + num, 0.0 + num, embossed.Radius, color) };
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint + vector3d + vector3d3 + vector3d4, anchorPoint - vector3d + vector3d3 + vector3d4, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		hashSet.Add(MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint - vector3d + vector3d3, 180.0 + num, 90.0 + num, embossed.Radius, color));
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint - vector3d - vector3d2 + vector3d3, anchorPoint - vector3d - vector3d2 - vector3d3, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		hashSet.Add(MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint - vector3d - vector3d3, 270.0 + num, 180.0 + num, embossed.Radius, color));
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint - vector3d - vector3d3 - vector3d4, anchorPoint + vector3d - vector3d3 - vector3d4, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		hashSet.Add(MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint + vector3d - vector3d3, 360.0 + num, 270.0 + num, embossed.Radius, color));
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint + vector3d + vector3d2 - vector3d3, anchorPoint + vector3d + vector3d2 + vector3d3, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		db2D.AddInnerLine(hashSet);
		return true;
	}

	private static CadTxtText GetCadTxt(global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string toolSuffix, double depth, string depthSign, double width, double height, double radius, double angle)
	{
        const int toolType = 13;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}{1}_L{2}_H{3}_R{4}_T{5}{6}",
            toolType,
            toolSuffix,
            Math.Round(width, 4),
            Math.Round(height, 4),
            Math.Round(radius, 4),
            depthSign,
            Math.Round(depth, 4));

        return new CadTxtText
        {
            Text = text,
            Angle = angle,
            Color = toolType,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

	public static EmbossedRectangleRoundedXml GetXmlElement(EmbossedRectangleRounded embossed, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embossed.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		int direction = Math.Sign(v.Z);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = embossed.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = embossed.WidthDir;
		worldMatrix.TransformNormalInPlace(ref v3);
		return new EmbossedRectangleRoundedXml
		{
			Depth = embossed.Depth,
			Radius = embossed.Radius,
			Width = embossed.Width,
			Height = embossed.Height,
			ID = embossed.ID,
			Direction = direction,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			WidthDir = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v3.X,
				Y = v3.Y,
				Z = v3.Z
			}
		};
	}
}

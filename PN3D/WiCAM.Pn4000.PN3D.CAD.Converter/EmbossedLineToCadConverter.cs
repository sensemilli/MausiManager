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

public class EmbossedLineToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 13;

	public static bool AddCadGeoElements(EmbossedLine embossed, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d = widthDir * 0.5 * (embossed.Width - embossed.Diameter);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d2 = normalized * embossed.Diameter / 2.0;
		db2D.AddText(EmbossedLineToCadConverter.GetCadTxt(anchorPoint, macro3DConfig.MacroEmbossedToolSuffix, embossed.Depth, depthSign, embossed.Width, embossed.Diameter, num));
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement> { MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint + vector3d, 90.0 + num, 270.0 + num, embossed.Diameter / 2.0, color) };
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint + vector3d + vector3d2, anchorPoint - vector3d + vector3d2, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		hashSet.Add(MacroToCadConverterBase.GetCadGeoCircleByDiameter(anchorPoint - vector3d, 270.0 + num, 90.0 + num, embossed.Diameter / 2.0, color));
		hashSet.Add(MacroToCadConverterBase.GetLine(anchorPoint - vector3d - vector3d2, anchorPoint + vector3d - vector3d2, global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color));
		db2D.AddInnerLine(hashSet);
		return true;
	}

	private static CadTxtText GetCadTxt(global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string toolSuffix, double depth, string depthSign, double width, double diameter, double angle)
	{
        const int toolType = 13;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}{1}_L{2}_D{3}_T{4}{5}",
            toolType,
            toolSuffix,
            Math.Round(width, 4),
            Math.Round(diameter, 4),
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

	public static EmbossedLineXml GetXmlElement(EmbossedLine embossedLine, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embossedLine.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		int direction = Math.Sign(v.Z);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = embossedLine.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = embossedLine.WidthDir;
		worldMatrix.TransformNormalInPlace(ref v3);
		return new EmbossedLineXml
		{
			Depth = embossedLine.Depth,
			Diameter = embossedLine.Diameter,
			Width = embossedLine.Width,
			ID = embossedLine.ID,
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

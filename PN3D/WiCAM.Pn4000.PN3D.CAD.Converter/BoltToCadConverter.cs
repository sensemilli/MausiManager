using System;
using System.Collections.Generic;
using System.Globalization;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class BoltToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 51;

	public static bool AddCadGeoElements(Bolt bolt, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		Face baseFace = bolt.BaseFace;
		double radius = bolt.Radius;
		double height = bolt.Height;
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(baseFace, worldMatrix))
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = bolt.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		db2D.AddText(BoltToCadConverter.GetCadTxt(radius, height, v, flag ? "+" : "-", flag));
		db2D.AddInnerLine(new HashSet<CadGeoElement> { MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, radius, flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn) });
		return true;
	}

	private static CadTxtText GetCadTxt(double innerRadius, double height, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn, bool posDirection)
	{
        const int toolType = 51;

        var text = string.Format(CultureInfo.InvariantCulture,
            "_TT_{0}_D{1}_H{2}_{3}",
            toolType,
            Math.Round(innerRadius * 2.0, 4),
            Math.Round(height, 4),
            addOn);

        return new CadTxtText
        {
            Text = text,
            Angle = posDirection ? 0 : 180,
            Color = toolType,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

	public static BoltXml GetXmlElement(Bolt bolt, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d topDirectionVisibleFace)
	{
		double radius = bolt.Radius;
		bool flag = 1.0 - topDirectionVisibleFace.Z > 0.0;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = bolt.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return new BoltXml
		{
			ID = bolt.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			},
			Radius = radius,
			Direction = (flag ? 1 : (-1)),
			Height = bolt.Height
		};
	}
}

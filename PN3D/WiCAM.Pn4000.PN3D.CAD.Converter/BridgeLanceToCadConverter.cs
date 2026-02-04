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

public class BridgeLanceToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 13;

	public static bool AddCadGeoElements(BridgeLance bridge, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		double size = bridge.Size0;
		double size2 = bridge.Size1;
		double size3 = bridge.Size2;
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(bridge.Orientation, worldMatrix))
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = bridge.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		int color = (flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = bridge.OrientationX;
		worldMatrix.TransformNormalInPlace(ref v2);
		double num = new global::WiCAM.Pn4000.BendModel.Base.Vector3d(1.0, 0.0, 0.0).SignedAngle(v2, new global::WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 1.0)) * (180.0 / Math.PI);
		if (flag)
		{
			num += 180.0;
		}
		if (num >= 360.0)
		{
			num -= 360.0;
		}
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		foreach (Face face in bridge.Faces)
		{
			foreach (FaceHalfEdge item in face.BoundaryEdgesCcw)
			{
				foreach (CadGeoElement item2 in MacroToCadConverterBase.GetCadGeoElement(item, worldMatrix, color))
				{
					hashSet.Add(item2);
				}
			}
		}
		db2D.AddInnerLine(Cad2DDatabase.SimplifyGeometry(hashSet));
		db2D.AddText(BridgeLanceToCadConverter.GetCadTxt(v, num, size, size2, size3, flag ? "+" : "-"));
		return true;
	}

	private static CadTxtText GetCadTxt(global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, double textAngle, double length, double width, double height, string addOn)
	{
        var text = string.Format(CultureInfo.InvariantCulture,
     "_TT_{0}_L{1}_H{2}_{3}",
     13,
     Math.Round(length, 4),
     Math.Round(width, 4),
     addOn);

        return new CadTxtText
        {
            Text = text,
            Angle = textAngle,
            Color = 13,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

	public static BridgeXml GetXmlElement(BridgeLance bridge, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		double size = bridge.Size0;
		double size2 = bridge.Size1;
		_ = bridge.Size2;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = bridge.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		bool flag = Math.Round(v.Z, 5) > 0.0;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = bridge.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		double angle = 0.0;
		return new BridgeXml
		{
			ID = bridge.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			Length = size,
			Width = size2,
			Angle = angle,
			Direction = (flag ? 1 : (-1))
		};
	}
}

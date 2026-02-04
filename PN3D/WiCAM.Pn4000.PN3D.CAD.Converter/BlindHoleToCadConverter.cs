using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class BlindHoleToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 80;

	public static bool AddCadGeoElements(BlindHole blindHole, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider, Face topFace)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		string addOn = "";
		int num;
		int color;
		switch (MacroToCadConverterBase.GetOrientation(blindHole.Orientation, worldMatrix))
		{
		case OrientationTypes.Top:
			num = 1;
			addOn = "+";
			color = macro3DConfig.MacroPosColorPn;
			break;
		case OrientationTypes.Bottom:
			num = -1;
			addOn = "-";
			color = macro3DConfig.MacroNegColorPn;
			break;
		case OrientationTypes.Side:
			num = 0;
			color = macro3DConfig.BlindHoleSideColorPn;
			break;
		default:
			return false;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = blindHole.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		if (num == 0)
		{
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = blindHole.AnchorPoint + blindHole.Orientation * blindHole.Depth / 2.0;
			worldMatrix.TransformInPlace(ref v2);
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v3;
			global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d;
			if (topFace.FlatFacePlane != null)
			{
				v3 = topFace.FlatFacePlane.Origin;
				vector3d = topFace.FlatFacePlane.Normal;
			}
			else
			{
				v3 = topFace.Mesh.First().V0.Pos;
				vector3d = topFace.Mesh.First().CalculatedTriangleNormal;
			}
			db2D.AddText(BlindHoleToCadConverter.GetCadTxtSide(vector3d.Dot(v3) - vector3d.Dot(blindHole.AnchorPoint), blindHole.Radius, v2));
			db2D.AddInnerLine(new HashSet<CadGeoElement> { MacroToCadConverterBase.GetLine(v, v + 2.0 * (v2 - v), global::WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity, color) });
		}
		else
		{
			db2D.AddText(BlindHoleToCadConverter.GetCadTxt(blindHole.Depth, v, addOn));
			db2D.AddInnerLine(new HashSet<CadGeoElement> { MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, blindHole.Radius, color) });
		}
		return true;
	}

	private static CadTxtText GetCadTxt(double depth, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "_TT_{0}_{1}_{2}", 80, Math.Round(depth, 4), addOn);
		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = 80,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	private static CadTxtText GetCadTxtSide(double distTop, double radius, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "_HB_{0}_{1}", Math.Round(distTop, 4), Math.Round(radius * 2.0, 4));
		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = 61,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	public static BlindHoleXml GetXmlElement(BlindHole blindHole, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = blindHole.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		int direction = 0;
		if (Math.Round(v.Z, 5) > 0.001)
		{
			direction = 1;
		}
		else if (Math.Round(v.Z, 5) < -0.001)
		{
			direction = -1;
		}
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = blindHole.AnchorPoint;
		worldMatrix.TransformInPlace(ref v2);
		return new BlindHoleXml
		{
			Depth = blindHole.Depth,
			Radius = blindHole.Radius,
			ID = blindHole.ID,
			Direction = direction,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v2.X,
				Y = v2.Y,
				Z = v2.Z
			},
			Normal = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			}
		};
	}
}

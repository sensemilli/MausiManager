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

public class EmbossedFreeformToCadConverter : MacroToCadConverterBase
{
	private const int ToolType = 31;

	public static bool AddCadGeoElements(EmbossedFreeform embossed, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		if ((uint)MacroToCadConverterBase.GetOrientation(embossed.Orientation, worldMatrix) > 1u)
		{
			return false;
		}
		int embossedPosColorPn = configProvider.InjectOrCreate<Macro3DConfig>().EmbossedPosColorPn;
		string depthSign = "-";
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> item in embossed.Contour)
		{
			if (item.Count > 0)
			{
				foreach (CadGeoElement item2 in MacroToCadConverterBase.GetContour(item.Select(delegate(global::WiCAM.Pn4000.BendModel.Base.Vector3d x)
				{
					global::WiCAM.Pn4000.BendModel.Base.Vector3d v = x;
					worldMatrix.TransformInPlace(ref v);
					return new Vector2d(v.X, v.Y);
				}).ToList(), embossedPosColorPn, (item.First() - item.Last()).LengthSquared > 1E-06))
				{
					hashSet.Add(item2);
				}
			}
			if (item.Count > 1)
			{
				db2D.AddText(EmbossedFreeformToCadConverter.GetCadTxt(angle: EmbossedFreeformToCadConverter.GetAngle(worldMatrix, item[0], item[1], out var middlePoint), middlePoint: middlePoint, depthSign: depthSign));
			}
			if (item.Count > 2)
			{
				db2D.AddText(EmbossedFreeformToCadConverter.GetCadTxt(angle: EmbossedFreeformToCadConverter.GetAngle(worldMatrix, item[item.Count - 2], item.Last(), out var middlePoint2), middlePoint: middlePoint2, depthSign: depthSign));
			}
		}
		db2D.AddInnerLine(hashSet);
		return true;
	}

	private static double GetAngle(global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, global::WiCAM.Pn4000.BendModel.Base.Vector3d posA, global::WiCAM.Pn4000.BendModel.Base.Vector3d posB, out global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = posA;
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = posB;
		worldMatrix.TransformInPlace(ref v);
		worldMatrix.TransformInPlace(ref v2);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d vector3d = v2 - v;
		double num;
		if (Math.Abs(vector3d.X) < 1E-06)
		{
			num = ((!(vector3d.Y > 0.0)) ? 270.0 : 90.0);
		}
		else
		{
			num = Math.Atan(vector3d.Y / vector3d.X) * 180.0 / Math.PI;
			if (vector3d.X < 0.0)
			{
				num += 180.0;
			}
			if (num < 0.0)
			{
				num += 360.0;
			}
		}
		middlePoint = (v + v2) * 0.5;
		return num;
	}

	private static CadTxtText GetCadTxt(global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string depthSign, double angle)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "_TT_{0}", 31);
		return new CadTxtText
		{
			Text = text,
			Angle = angle,
			Color = 31,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	public static EmbossedFreeformXml GetXmlElement(EmbossedFreeform embossed, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = embossed.Orientation;
		worldMatrix.TransformNormalInPlace(ref v2);
		int direction = Math.Sign(v2.Z);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = embossed.AnchorPoint;
		worldMatrix.TransformInPlace(ref v3);
		return new EmbossedFreeformXml
		{
			Depth = embossed.Depth,
			ID = embossed.ID,
			Direction = direction,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v3.X,
				Y = v3.Y,
				Z = v3.Z
			},
			Contour = embossed.Contour.Select((List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> x) => x.Select((global::WiCAM.Pn4000.BendModel.Base.Vector3d v) => new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			}).ToList()).ToList()
		};
	}
}

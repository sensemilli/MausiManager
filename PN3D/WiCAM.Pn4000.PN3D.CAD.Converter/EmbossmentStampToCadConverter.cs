using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class EmbossmentStampToCadConverter : MacroToCadConverterBase
{
	public static bool AddCadGeoElements(EmbossmentStamp embossmentStamp, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(embossmentStamp.Orientation, worldMatrix))
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
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		int color = (flag ? macro3DConfig.EmbStampPosColorPn : macro3DConfig.EmbStampNegColorPn);
		if (embossmentStamp.IsSpecialVisible || embossmentStamp.IsSpecialDirectionGrinding)
		{
			color = macro3DConfig.EmbStampSpecialColorPn;
		}
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> closedOutline in embossmentStamp.ClosedOutlines)
		{
			foreach (CadGeoElement item in MacroToCadConverterBase.GetContour(closedOutline.Select(delegate(global::WiCAM.Pn4000.BendModel.Base.Vector3d v)
			{
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = v;
				worldMatrix.TransformInPlace(ref v2);
				return new Vector2d(v2.X, v2.Y);
			}).ToList(), color, isOpenContour: false))
			{
				hashSet.Add(item);
			}
		}
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> openOutline in embossmentStamp.OpenOutlines)
		{
			foreach (CadGeoElement item2 in MacroToCadConverterBase.GetContour(openOutline.Select(delegate(global::WiCAM.Pn4000.BendModel.Base.Vector3d v)
			{
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = v;
				worldMatrix.TransformInPlace(ref v3);
				return new Vector2d(v3.X, v3.Y);
			}).ToList(), color, isOpenContour: true))
			{
				hashSet.Add(item2);
			}
		}
		db2D.AddInnerLine(Cad2DDatabase.SimplifyGeometry(hashSet));
		return true;
	}

	public static EmbossmentStampXml GetXmlElement(EmbossmentStamp embStamp, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = embStamp.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		bool flag = Math.Round(v.Z, 5) > 0.0;
		return new EmbossmentStampXml
		{
			ID = embStamp.ID,
			Direction = (flag ? 1 : (-1)),
			IsSpecialVisible = embStamp.IsSpecialVisible,
			IsSpecialDirectionGrinding = embStamp.IsSpecialDirectionGrinding,
			DirectionGrinding = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = embStamp.DirectionGrinding.X,
				Y = embStamp.DirectionGrinding.Y,
				Z = embStamp.DirectionGrinding.Z
			}
		};
	}
}

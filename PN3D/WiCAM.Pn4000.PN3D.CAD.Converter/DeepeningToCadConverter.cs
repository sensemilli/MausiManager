using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class DeepeningToCadConverter : MacroToCadConverterBase
{
	private const string ToolType = "Deepening";

	public static bool AddCadGeoElements(Deepening deepening, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider, Dictionary<List<Vector2d>, List<Pair<CircleSegment2D, List<Vector2d>>>> contourCircleMap)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		AnalyzeConfig analyzeConfig = configProvider.InjectOrCreate<AnalyzeConfig>();
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(deepening.Orientation, worldMatrix))
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
		int color;
		if (analyzeConfig.DeepeningListDepth == null || analyzeConfig.DeepeningListDepth.Count == 0 || analyzeConfig.DeepeningListDepth.All((double d) => d <= deepening.Depth) || analyzeConfig.ShowDeepeningLevels == false)
		{
			color = (flag ? macro3DConfig.DeepeningPosColorPn : macro3DConfig.DeepeningNegColorPn);
		}
		else
		{
			int index = analyzeConfig.DeepeningListDepth.FindIndex((double d) => d > deepening.Depth);
			color = (flag ? analyzeConfig.DeepeningListColorsPos[index] : analyzeConfig.DeepeningListColorsNeg[index]);
		}
		if (deepening.IsSpecialVisible || deepening.IsSpecialDirectionGrinding)
		{
			color = macro3DConfig.EmbStampSpecialColorPn;
		}
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> closedOutline in deepening.ClosedOutlines)
		{
			foreach (CadGeoElement item in MacroToCadConverterBase.GetContour(closedOutline.Select(delegate(global::WiCAM.Pn4000.BendModel.Base.Vector3d v)
			{
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v2 = v;
				worldMatrix.TransformInPlace(ref v2);
				return new Vector2d(v2.X, v2.Y);
			}).ToList(), color, isOpenContour: false, contourCircleMap))
			{
				hashSet.Add(item);
			}
		}
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> openOutline in deepening.OpenOutlines)
		{
			foreach (CadGeoElement item2 in MacroToCadConverterBase.GetContour(openOutline.Select(delegate(global::WiCAM.Pn4000.BendModel.Base.Vector3d v)
			{
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = v;
				worldMatrix.TransformInPlace(ref v3);
				return new Vector2d(v3.X, v3.Y);
			}).ToList(), color, isOpenContour: true, contourCircleMap))
			{
				hashSet.Add(item2);
			}
		}
		db2D.AddInnerLine(Cad2DDatabase.SimplifyGeometry(hashSet));
		return true;
	}

	public static DeepeningXml GetXmlElement(Deepening deepening, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = deepening.Orientation;
		worldMatrix.TransformNormalInPlace(ref v);
		bool flag = Math.Round(v.Z, 5) > 0.0;
		return new DeepeningXml
		{
			ID = deepening.ID,
			Direction = (flag ? 1 : (-1)),
			IsSpecialVisible = deepening.IsSpecialVisible,
			IsSpecialDirectionGrinding = deepening.IsSpecialDirectionGrinding,
			DirectionGrinding = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = deepening.DirectionGrinding.X,
				Y = deepening.DirectionGrinding.Y,
				Z = deepening.DirectionGrinding.Z
			}
		};
	}
}

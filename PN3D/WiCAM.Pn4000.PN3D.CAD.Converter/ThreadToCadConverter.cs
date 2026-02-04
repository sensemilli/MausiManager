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

public class ThreadToCadConverter : MacroToCadConverterBase
{
	private const int ToolTypeInt = 5;

	private const string ToolType = "05";

	public static bool AddCadGeoElements(Macro thread, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		double num = 0.0;
		double num2 = 0.0;
		bool flag;
		switch (MacroToCadConverterBase.GetOrientation(thread.Orientation, worldMatrix))
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
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = thread.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		db2D.AddText(ThreadToCadConverter.GetCadTxt(num, num2, v));
		db2D.AddInnerLine(new HashSet<CadGeoElement>
		{
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 360.0, num, macro3DConfig.BaseColorPn),
			MacroToCadConverterBase.GetCadGeoCircleByDiameter(v, 0.0, 90.0, num2, flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn)
		});
		return true;
	}

	private static CadTxtText GetCadTxt(double innerDiameter, double threadDiameter, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "_TT_{0}_D{1}_D{2}", "05", Math.Round(innerDiameter * 2.0, 4), Math.Round(threadDiameter * 2.0, 4));
		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = 5,
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	public static ThreadXml GetXmlElement(Thread thread, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = thread.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return new ThreadXml
		{
			ID = thread.ID,
			Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
			{
				X = v.X,
				Y = v.Y,
				Z = v.Z
			},
			Radius = thread.Radius
		};
	}
}

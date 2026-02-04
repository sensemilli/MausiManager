using System;
using System.Collections.Generic;
using System.Globalization;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure.ManufacturingData;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class ManufacturingMacroToCadConverter : MacroToCadConverterBase
{
	public static bool AddCadGeoElements(ManufacturingMacro mm, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase cad2DDatabase, IConfigProvider configProvider)
	{
		if (mm.ManufacturingData is SManufacturingDataHoleBase sManufacturingDataHoleBase)
		{
			if ((uint)MacroToCadConverterBase.GetOrientation(sManufacturingDataHoleBase.PrimaryDirection, worldMatrix) > 1u)
			{
				return false;
			}
			Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
			if (mm.ManufacturingData is SManufacturingDataSimpleHole { Position: var v } sManufacturingDataSimpleHole)
			{
				worldMatrix.TransformInPlace(ref v);
				if (sManufacturingDataSimpleHole.Thread != null)
				{
					cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadThreadTxt("05", sManufacturingDataSimpleHole.Thread.Diameter * 0.5, v));
					cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
					{
						new CadGeoCircle
						{
							Color = macro3DConfig.MacroPosColorPn,
							Type = 2,
							Center = new Vector2d(Math.Round(v.X, 5), Math.Round(v.Y, 5)),
							Radius = sManufacturingDataSimpleHole.Thread.Diameter * 0.5,
							StartAngle = 0.0,
							EndAngle = 360.0,
							Direction = -1
						}
					});
				}
			}
			else if (mm.ManufacturingData is SManufacturingDataCounterSinkHole { PrimaryDirection: var v2 } sManufacturingDataCounterSinkHole)
			{
				worldMatrix.TransformNormalInPlace(ref v2);
				bool flag = v2.Z < 0.0;
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = sManufacturingDataCounterSinkHole.Position;
				worldMatrix.TransformInPlace(ref v3);
				cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadTxt(25, sManufacturingDataCounterSinkHole.PrimaryDiameter * 0.5, sManufacturingDataCounterSinkHole.CounterSinkDiameter * 0.5, v3, flag ? "+" : "-"));
				cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
				{
					new CadGeoCircle
					{
						Color = (flag ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn),
						Type = 2,
						Center = new Vector2d(Math.Round(v3.X, 5), Math.Round(v3.Y, 5)),
						Radius = sManufacturingDataCounterSinkHole.CounterSinkDiameter * 0.5,
						StartAngle = 0.0,
						EndAngle = 360.0,
						Direction = -1
					}
				});
				if (sManufacturingDataCounterSinkHole.Thread != null)
				{
					cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadThreadTxt("05", sManufacturingDataCounterSinkHole.Thread.Diameter * 0.5, v3));
					cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
					{
						new CadGeoCircle
						{
							Color = macro3DConfig.MacroPosColorPn,
							Type = 2,
							Center = new Vector2d(Math.Round(v3.X, 5), Math.Round(v3.Y, 5)),
							Radius = sManufacturingDataCounterSinkHole.Thread.Diameter * 0.5,
							StartAngle = 0.0,
							EndAngle = 360.0,
							Direction = -1
						}
					});
				}
			}
			else if (mm.ManufacturingData is SManufacturingDataCounterBoreHole { PrimaryDirection: var v4 } sManufacturingDataCounterBoreHole)
			{
				worldMatrix.TransformNormalInPlace(ref v4);
				bool flag2 = v4.Z < 0.0;
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v5 = sManufacturingDataCounterBoreHole.Position;
				worldMatrix.TransformInPlace(ref v5);
				cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadTxt(25, sManufacturingDataCounterBoreHole.PrimaryDiameter * 0.5, sManufacturingDataCounterBoreHole.CounterBoreDiameter * 0.5, v5, flag2 ? "+" : "-"));
				cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
				{
					new CadGeoCircle
					{
						Color = (flag2 ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn),
						Type = 2,
						Center = new Vector2d(Math.Round(v5.X, 5), Math.Round(v5.Y, 5)),
						Radius = sManufacturingDataCounterBoreHole.CounterBoreDiameter * 0.5,
						StartAngle = 0.0,
						EndAngle = 360.0,
						Direction = -1
					}
				});
				if (sManufacturingDataCounterBoreHole.Thread != null)
				{
					cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadThreadTxt("05", sManufacturingDataCounterBoreHole.Thread.Diameter * 0.5, v5));
					cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
					{
						new CadGeoCircle
						{
							Color = macro3DConfig.MacroPosColorPn,
							Type = 2,
							Center = new Vector2d(Math.Round(v5.X, 5), Math.Round(v5.Y, 5)),
							Radius = sManufacturingDataCounterBoreHole.Thread.Diameter * 0.5,
							StartAngle = 0.0,
							EndAngle = 360.0,
							Direction = -1
						}
					});
				}
			}
			else if (mm.ManufacturingData is SManufacturingDataCounterDrillHole { PrimaryDirection: var v6 } sManufacturingDataCounterDrillHole)
			{
				worldMatrix.TransformNormalInPlace(ref v6);
				bool flag3 = v6.Z > 0.0;
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v7 = sManufacturingDataCounterDrillHole.Position;
				worldMatrix.TransformInPlace(ref v7);
				cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadTxt(25, sManufacturingDataCounterDrillHole.PrimaryDiameter * 0.5, sManufacturingDataCounterDrillHole.CounterDrillDiameter * 0.5, v7, flag3 ? "+" : "-"));
				cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
				{
					new CadGeoCircle
					{
						Color = (flag3 ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn),
						Type = 2,
						Center = new Vector2d(Math.Round(v7.X, 5), Math.Round(v7.Y, 5)),
						Radius = sManufacturingDataCounterDrillHole.CounterDrillDiameter * 0.5,
						StartAngle = 0.0,
						EndAngle = 360.0,
						Direction = -1
					}
				});
				if (sManufacturingDataCounterDrillHole.Thread != null)
				{
					cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadThreadTxt("05", sManufacturingDataCounterDrillHole.Thread.Diameter * 0.5, v7));
					cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
					{
						new CadGeoCircle
						{
							Color = macro3DConfig.MacroPosColorPn,
							Type = 2,
							Center = new Vector2d(Math.Round(v7.X, 5), Math.Round(v7.Y, 5)),
							Radius = sManufacturingDataCounterDrillHole.Thread.Diameter * 0.5,
							StartAngle = 0.0,
							EndAngle = 360.0,
							Direction = -1
						}
					});
				}
			}
			else
			{
				if (!(mm.ManufacturingData is SManufacturingDataTaperHole { PrimaryDirection: var v8 } sManufacturingDataTaperHole))
				{
					return false;
				}
				worldMatrix.TransformNormalInPlace(ref v8);
				bool flag4 = v8.Z > 0.0;
				global::WiCAM.Pn4000.BendModel.Base.Vector3d v9 = sManufacturingDataTaperHole.Position;
				worldMatrix.TransformInPlace(ref v9);
				cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadTxt(25, sManufacturingDataTaperHole.PrimaryDiameter * 0.5, (sManufacturingDataTaperHole.PrimaryDiameter - Math.Tan(sManufacturingDataTaperHole.TaperAngleDegrees / 180.0 * Math.PI) * sManufacturingDataTaperHole.Depth) * 0.5, v9, flag4 ? "+" : "-"));
				cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
				{
					new CadGeoCircle
					{
						Color = (flag4 ? macro3DConfig.MacroPosColorPn : macro3DConfig.MacroNegColorPn),
						Type = 2,
						Center = new Vector2d(Math.Round(v9.X, 5), Math.Round(v9.Y, 5)),
						Radius = (sManufacturingDataTaperHole.PrimaryDiameter - Math.Tan(sManufacturingDataTaperHole.TaperAngleDegrees / 180.0 * Math.PI) * sManufacturingDataTaperHole.Depth) * 0.5,
						StartAngle = 0.0,
						EndAngle = 360.0,
						Direction = -1
					}
				});
				if (sManufacturingDataTaperHole.Thread != null)
				{
					cad2DDatabase.AddText(ManufacturingMacroToCadConverter.GetCadThreadTxt("05", sManufacturingDataTaperHole.Thread.Diameter * 0.5, v9));
					cad2DDatabase.AddInnerLine(new HashSet<CadGeoElement>
					{
						new CadGeoCircle
						{
							Color = macro3DConfig.MacroPosColorPn,
							Type = 2,
							Center = new Vector2d(Math.Round(v9.X, 5), Math.Round(v9.Y, 5)),
							Radius = sManufacturingDataTaperHole.Thread.Diameter * 0.5,
							StartAngle = 0.0,
							EndAngle = 360.0,
							Direction = -1
						}
					});
				}
			}
			return true;
		}
		return false;
	}

	private static CadTxtText GetCadTxt(int toolType, double innerRadius, double outerRadius, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint, string addOn)
	{
        var text = string.Format(CultureInfo.InvariantCulture,
      "_TT_{0}_D{1}_D{2}_{3}",
      toolType,
      Math.Round(innerRadius * 2.0, 4),
      Math.Round(outerRadius * 2.0, 4),
      addOn);

        return new CadTxtText
        {
            Text = text,
            Angle = 0.0,
            Color = toolType,
            Height = 1.0,
            Position = new Vector2d(middlePoint.X, middlePoint.Y)
        };
    }

	private static CadTxtText GetCadThreadTxt(string toolType, double radius, global::WiCAM.Pn4000.BendModel.Base.Vector3d middlePoint)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "_TT_{0}_D{1}", toolType, Math.Round(radius * 2.0, 4));
		return new CadTxtText
		{
			Text = text,
			Angle = 0.0,
			Color = Convert.ToInt32(toolType),
			Height = 1.0,
			Position = new Vector2d(middlePoint.X, middlePoint.Y)
		};
	}

	public static void AddToSpecialTools(ManufacturingMacro mm, SpecialTools tools, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		if (mm.ManufacturingData is SManufacturingDataSimpleHole { Position: var v } sManufacturingDataSimpleHole)
		{
			worldMatrix.TransformInPlace(ref v);
			tools.Threads.Add(new ThreadXml
			{
				Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
				{
					X = v.X,
					Y = v.Y,
					Z = v.Z
				},
				Radius = sManufacturingDataSimpleHole.PrimaryDiameter * 0.5,
				ID = mm.ID
			});
		}
		else if (mm.ManufacturingData is SManufacturingDataCounterSinkHole { PrimaryDirection: var v2 } sManufacturingDataCounterSinkHole)
		{
			worldMatrix.TransformNormalInPlace(ref v2);
			bool flag = v2.Z < 0.0;
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v3 = sManufacturingDataCounterSinkHole.Position;
			worldMatrix.TransformInPlace(ref v3);
			tools.CounterSinks.Add(new CounterSinkXml
			{
				Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
				{
					X = v3.X,
					Y = v3.Y,
					Z = v3.Z
				},
				TopRadius = sManufacturingDataCounterSinkHole.CounterSinkDiameter * 0.5,
				BottomRadius = sManufacturingDataCounterSinkHole.PrimaryDiameter * 0.5,
				Direction = (flag ? 1 : 0),
				ID = mm.ID
			});
		}
		else if (mm.ManufacturingData is SManufacturingDataCounterBoreHole { PrimaryDirection: var v4 } sManufacturingDataCounterBoreHole)
		{
			worldMatrix.TransformNormalInPlace(ref v4);
			bool flag2 = v4.Z < 0.0;
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v5 = sManufacturingDataCounterBoreHole.Position;
			worldMatrix.TransformInPlace(ref v5);
			tools.CounterSinks.Add(new CounterSinkXml
			{
				Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
				{
					X = v5.X,
					Y = v5.Y,
					Z = v5.Z
				},
				TopRadius = sManufacturingDataCounterBoreHole.CounterBoreDiameter * 0.5,
				BottomRadius = sManufacturingDataCounterBoreHole.PrimaryDiameter * 0.5,
				Direction = (flag2 ? 1 : 0),
				ID = mm.ID
			});
		}
		else if (mm.ManufacturingData is SManufacturingDataCounterDrillHole { PrimaryDirection: var v6 } sManufacturingDataCounterDrillHole)
		{
			worldMatrix.TransformNormalInPlace(ref v6);
			bool flag3 = v6.Z > 0.0;
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v7 = sManufacturingDataCounterDrillHole.Position;
			worldMatrix.TransformInPlace(ref v7);
			tools.CounterSinks.Add(new CounterSinkXml
			{
				Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
				{
					X = v7.X,
					Y = v7.Y,
					Z = v7.Z
				},
				TopRadius = sManufacturingDataCounterDrillHole.CounterDrillDiameter * 0.5,
				BottomRadius = sManufacturingDataCounterDrillHole.PrimaryDiameter * 0.5,
				Direction = (flag3 ? 1 : 0),
				ID = mm.ID
			});
		}
		else if (mm.ManufacturingData is SManufacturingDataTaperHole { PrimaryDirection: var v8 } sManufacturingDataTaperHole)
		{
			worldMatrix.TransformNormalInPlace(ref v8);
			bool flag4 = v8.Z > 0.0;
			global::WiCAM.Pn4000.BendModel.Base.Vector3d v9 = sManufacturingDataTaperHole.Position;
			worldMatrix.TransformInPlace(ref v9);
			tools.CounterSinks.Add(new CounterSinkXml
			{
				Center = new global::WiCAM.Pn4000.PartsReader.DataClasses.Vector3d
				{
					X = v9.X,
					Y = v9.Y,
					Z = v9.Z
				},
				TopRadius = (sManufacturingDataTaperHole.PrimaryDiameter - Math.Tan(sManufacturingDataTaperHole.TaperAngleDegrees / 180.0 * Math.PI) * sManufacturingDataTaperHole.Depth) * 0.5,
				BottomRadius = sManufacturingDataTaperHole.PrimaryDiameter * 0.5,
				Direction = (flag4 ? 1 : 0),
				ID = mm.ID
			});
		}
	}
}

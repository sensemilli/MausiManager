using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.PN3D.Converter;

public class ConvertConfig : IAnalyzeConfigProvider
{
	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	public ConvertConfig(IConfigProvider configProvider, IPnPathService pathService)
	{
		this._configProvider = configProvider;
		this._pathService = pathService;
	}

	public global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig GetAnalyzeConfig()
	{
		return ConvertConfig.GetAnalyzeConfig(this._configProvider, this._pathService);
	}

	public global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig ConvertAnalyzeConfig(global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig config)
	{
		return ConvertConfig.ConvertAnalyze(config);
	}

	public static global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig GetAnalyzeConfig(IConfigProvider configProvider, IPnPathService pathService)
	{
		List<Polygon2D> list = CadGeoLoader.LoadCadGeo2DContours(pathService.GetFileInGFiles("TOPVIEW"));
		List<Polygon2D> list2 = CadGeoLoader.LoadCadGeo2DContours(pathService.GetFileInGFiles("ROLLVIEW"));
		if (configProvider == null)
		{
			return null;
		}
		global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig analyzeConfig = ConvertConfig.ConvertAnalyze(configProvider.InjectOrCreate<global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig>());
		analyzeConfig.ReconstructBendsConfig = ConvertConfig.Convert(configProvider.InjectOrCreate<global::WiCAM.Pn4000.Config.DataStructures.ReconstructIrregularBendsConfig>());
		analyzeConfig.EmbStampVisibleFaceLengths = null;
		analyzeConfig.EmbStampVisibleFaceAngles = null;
		if (list != null && list.Count == 1 && MacroAnalyzer.GetEmbossmentStampSpecialContour(list.First().Vertices, out var lengthsNorm, out var angles, out var _))
		{
			analyzeConfig.EmbStampVisibleFaceLengths = lengthsNorm;
			analyzeConfig.EmbStampVisibleFaceAngles = angles;
		}
		analyzeConfig.EmbStampDirGrindingLengths = null;
		analyzeConfig.EmbStampDirGrindingAngles = null;
		analyzeConfig.EmbStampGrindingDirX = 0.0;
		analyzeConfig.EmbStampGrindingDirY = 0.0;
		analyzeConfig.EmbStampGrindingX0 = 0;
		analyzeConfig.EmbStampGrindingX1 = 0;
		if (list2 != null && list2.Count == 1 && MacroAnalyzer.GetEmbossmentStampSpecialContour(list2.First().Vertices, out var lengthsNorm2, out var angles2, out var contour2dSimplify2))
		{
			int num = GetIndDistance(0);
			int num2 = GetIndDistance(num);
			Vector2d vector2d = contour2dSimplify2[num] - contour2dSimplify2[num2];
			double num3 = Math.Atan(vector2d.Y / vector2d.X);
			analyzeConfig.EmbStampDirGrindingLengths = lengthsNorm2;
			analyzeConfig.EmbStampDirGrindingAngles = angles2;
			analyzeConfig.EmbStampGrindingDirX = Math.Cos(num3);
			analyzeConfig.EmbStampGrindingDirY = 0.0 - Math.Sin(num3);
			analyzeConfig.EmbStampGrindingX0 = num2;
			analyzeConfig.EmbStampGrindingX1 = num;
		}
		return analyzeConfig;
		int GetIndDistance(int indexRef)
		{
			double num4 = 0.0;
			int result = 0;
			for (int i = 0; i < contour2dSimplify2.Count; i++)
			{
				double lengthSquared = (contour2dSimplify2[i] - contour2dSimplify2[indexRef]).LengthSquared;
				if (lengthSquared > num4)
				{
					num4 = lengthSquared;
					result = i;
				}
			}
			return result;
		}
	}

	private static global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig ConvertAnalyze(global::WiCAM.Pn4000.Config.DataStructures.AnalyzeConfig config)
	{
		return new global::WiCAM.Pn4000.BendModel.Config.AnalyzeConfig
		{
			SmallPartTubeLength = (config.SmallPartTubeLength ?? 10.0),
			SmallPartCubeLength = (config.SmallPartCubeLength ?? 250.0),
			BorderTolerableOrthogonalOffset = (config.BorderTolerableOrthogonalOffset ?? 0.0174533),
			DeepeningMinDepth = (config.DeepeningMinDepth ?? (-0.05)),
			DeepeningMaxDepth = (config.DeepeningMaxDepth ?? (-20.0)),
			CounterSinkMaxRadius = (config.CounterSinkMaxRadius ?? 20.0),
			TwoSidedCounterSinkMaxRadius = (config.TwoSidedCounterSinkMaxRadius ?? 20.0),
			ExportCounterSinkAngle = (config.ExportCounterSinkAngle == true),
			DetectLances = (config.DetectLances ?? true),
			BlindHoleMinDepth = (config.BlindHoleMinDepth ?? 0.1),
			BlindHoleMaxRadius = (config.BlindHoleMaxRadius ?? 15.0),
			ShowChamferLine = (config.ShowChamferLine == true),
			SpecialVisibleFaceColor = config.SpecialVisibleFaceColor.ToBendColor(),
			SearchSpecialVisibleFaceColor = config.SearchSpecialVisibleFaceColor,
			EmbStampToleranceLength = (config.EmbStampToleranceLength ?? 1E-05),
			EmbStampToleranceAngle = (config.EmbStampToleranceAngle ?? 0.01),
			ShowDeepeningLevels = (config.ShowDeepeningLevels == true),
			DeepeningListColor3D = (config.DeepeningListColor3D?.Select((CfgColor x) => x.ToBendColor()).ToList() ?? new List<Color>()),
			DeepeningListColorsPos = (config.DeepeningListColorsPos ?? new List<int>()),
			DeepeningListColorsNeg = (config.DeepeningListColorsNeg ?? new List<int>()),
			DeepeningListDepth = (config.DeepeningListDepth ?? new List<double>())
		};
	}

	private static global::WiCAM.Pn4000.BendModel.Config.ReconstructIrregularBendsConfig Convert(global::WiCAM.Pn4000.Config.DataStructures.ReconstructIrregularBendsConfig config)
	{
		return new global::WiCAM.Pn4000.BendModel.Config.ReconstructIrregularBendsConfig
		{
			ReconstructAfterImportMode = ((ReconstructionMode?)config.ReconstructAfterImport).GetValueOrDefault(),
			RepairAlreadyDetectedBendingZones = (config.ReconstructAllBends == true)
		};
	}
}

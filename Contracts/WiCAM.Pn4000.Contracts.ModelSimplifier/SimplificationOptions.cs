using System;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.Contracts.ModelSimplifier;

[Serializable]
public class SimplificationOptions
{
	public static double DefaultMaxHoleArea = 5000000000.0;

	public double MaxHoleArea { get; set; } = SimplificationOptions.DefaultMaxHoleArea;

	public bool KeepBorder { get; set; } = true;

	public bool KeepOriginalFlanges { get; set; }

	public bool RemoveHoles { get; set; }

	public bool RemoveBendingZones { get; set; }

	public bool DecimateFlanges { get; set; } = true;

	public bool DecimateBendingZones { get; set; } = true;

	public int DecimationTargetVertexCount { get; set; } = 3;

	public double DecimationMaxHausdorff { get; set; } = 8.0;

	public double DecimationMaxDegNormalDeviation { get; set; } = 20.0;

	public double DecimationMaxAspectRatio { get; set; } = 7.0;

	public bool DecimationDiscourageGrowing { get; set; }

	public IMessageDisplay? Logger { get; set; }
}

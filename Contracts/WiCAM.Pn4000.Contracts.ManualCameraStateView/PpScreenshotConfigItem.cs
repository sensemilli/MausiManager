using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.Screen;

namespace WiCAM.Pn4000.Contracts.ManualCameraStateView;

public class PpScreenshotConfigItem : IPpScreenshotConfigItem, IEquatable<IPpScreenshotConfigItem>
{
	public Dictionary<MachineParts, double> Opacities { get; set; } = new Dictionary<MachineParts, double>();

	public ProjectionType ProjectionType { get; set; }

	public Matrix4d ViewRotation { get; set; }

	public bool AdjustManually { get; set; }

	public bool Equals(IPpScreenshotConfigItem? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (this.Opacities.SequenceEqual(other.Opacities) && this.ProjectionType == other.ProjectionType && this.ViewRotation == other.ViewRotation)
		{
			return this.AdjustManually == other.AdjustManually;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != base.GetType())
		{
			return false;
		}
		return this.Equals((IPpScreenshotConfigItem)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Opacities, (int)this.ProjectionType, this.ViewRotation, this.AdjustManually);
	}
}

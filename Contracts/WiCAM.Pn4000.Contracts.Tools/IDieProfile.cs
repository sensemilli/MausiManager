namespace WiCAM.Pn4000.Contracts.Tools;

public interface IDieProfile : ILowerToolProfile, IToolProfile
{
	double? VWidth { get; set; }

	double? VAngleDeg { get; set; }

	double? VAngleRad { get; set; }

	double? VDepth { get; set; }

	double? VRadius { get; set; }

	double? VGroundWidth { get; set; }

	double? CornerRadius { get; set; }

	double? StoneFactor { get; set; }

	VWidthTypes? VWidthType { get; set; }

	double OffsetInX { get; set; }

	double? FoldDepth { get; set; }

	double? FoldGap { get; set; }

	double? LegLengthMin { get; set; }

	double? LegLengthMinOrDefaut => this.LegLengthMin ?? (this.VWidth * 0.75);

	double? PartFoldOffset { get; set; }

	double? WidthHemmingFace { get; set; }

	bool IsRollBend
	{
		get
		{
			return this.ProfileType.HasFlag(ToolProfileTypes.Roll);
		}
		set
		{
			if (value)
			{
				this.ProfileType |= ToolProfileTypes.Roll;
			}
			else
			{
				this.ProfileType &= ~ToolProfileTypes.Roll;
			}
		}
	}

	double? TractrixRadius { get; set; }

	double? TransitionAngle { get; set; }
}

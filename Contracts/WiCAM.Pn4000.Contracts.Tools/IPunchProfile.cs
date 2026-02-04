namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPunchProfile : IUpperToolProfile, IToolProfile
{
	double? AngleDeg { get; set; }

	double? AngleRad { get; set; }

	double? Radius { get; set; }

	double? HemOffsetX { get; set; }

	double? WidthHemmingFace { get; set; }

	bool IsRadiusTool
	{
		get
		{
			return this.ProfileType.HasFlag(ToolProfileTypes.Radius);
		}
		set
		{
			if (value)
			{
				this.ProfileType |= ToolProfileTypes.Radius;
			}
			else
			{
				this.ProfileType &= ~ToolProfileTypes.Radius;
			}
		}
	}
}

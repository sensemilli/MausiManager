using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.BendDataBase;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolProfile
{
	IMultiToolProfile MultiToolProfile { get; set; }

	int ID { get; }

	string Name { get; set; }

	IProfileGroup Group { get; set; }

	int GroupID { get; }

	string GroupName { get; }

	int Priority { get; set; }

	double WorkingHeight { get; set; }

	double MaxToolLoad { get; set; }

	bool Implemented { get; }

	bool FlippedByDefault { get; set; }

	AllowedFlippedStates FlippingAllowed { get; set; }

	IEnumerable<IToolPieceProfile> PieceProfiles => this.MultiToolProfile.PieceProfiles;

	ToolProfileTypes ProfileType { get; set; }

	bool IsFoldTool
	{
		get
		{
			if (!(this is IDieFoldExtentionProfile) && (!(this is IDieProfile dieProfile) || !dieProfile.ProfileType.HasFlag(ToolProfileTypes.Fold)))
			{
				if (this is IPunchProfile punchProfile)
				{
					return punchProfile.ProfileType.HasFlag(ToolProfileTypes.Fold);
				}
				return false;
			}
			return true;
		}
	}

	bool IsAdapter => this.ProfileType.HasFlag(ToolProfileTypes.Adapter);

	[Obsolete("Use MultiToolProfile.MountTypeId")]
	int MountTypeId => this.MultiToolProfile.MountTypeId;

	int? MountTypeChildId { get; set; }

	AdapterDirections AdapterDirection { get; set; }

	AdapterXPositions AdapterXPosition { get; set; }

	double? SpringHeight { get; set; }
}

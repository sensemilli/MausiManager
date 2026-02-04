using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolSection
{
	List<IToolPiece> Pieces { get; }

	IMultiToolProfile MultiToolProfile { get; set; }

	[Obsolete("use MultiToolProfile")]
	IToolProfile ToolProfile { get; }

	bool IsUpperSection { get; }

	double OffsetLocalX
	{
		get
		{
			return this.OffsetLocal.X;
		}
		set
		{
			Vector3d offsetLocal = this.OffsetLocal;
			offsetLocal.X = value;
			this.OffsetLocal = offsetLocal;
		}
	}

	Vector3d OffsetLocal { get; set; }

	Vector3d OffsetWorld { get; set; }

	IToolCluster Cluster { get; set; }

	IToolSetups ToolSetups { get; }

	double Length { get; }

	[Obsolete]
	bool IsAdapterSection
	{
		get
		{
			IToolProfile toolProfile = this.ToolProfile;
			if (toolProfile == null)
			{
				return false;
			}
			return toolProfile.ProfileType.HasFlag(ToolProfileTypes.Adapter);
		}
	}

	bool HasOnlyAdapter => this.MultiToolProfile.ToolProfiles.All((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.Adapter));

	bool HasAnyAdapter => this.MultiToolProfile.ToolProfiles.Any((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.Adapter));

	bool HasUpDownAdapter => this.MultiToolProfile.ToolProfiles.Any((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.Adapter) && x.AdapterDirection == AdapterDirections.TopDown);

	bool HasFrontAdapter => this.MultiToolProfile.ToolProfiles.Any((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.Adapter) && x.AdapterDirection == AdapterDirections.Front);

	bool HasBackAdapter => this.MultiToolProfile.ToolProfiles.Any((IToolProfile x) => x.ProfileType.HasFlag(ToolProfileTypes.Adapter) && x.AdapterDirection == AdapterDirections.Back);

	double? ZMin => this.OffsetWorld.Z + this.MultiToolProfile.OffsetBottom - this.MultiToolProfile.AnchorPoint;

	double? ZMax => this.OffsetWorld.Z + this.MultiToolProfile.OffsetTop - this.MultiToolProfile.AnchorPoint;

	double? XMin => this.Pieces.FirstOrDefault()?.OffsetWorld.X;

	double? XMax
	{
		get
		{
			IToolPiece toolPiece = this.Pieces.LastOrDefault();
			if (toolPiece != null)
			{
				return toolPiece.OffsetWorld.X + toolPiece.PieceProfile.Length;
			}
			return null;
		}
	}

	bool HasTools => this.Pieces.Count > 0;

	bool Flipped { get; set; }

	double? ReservedSpaceLeft { get; set; }

	double? ReservedSpaceRight { get; set; }

	Matrix4d TransformWorld(bool? flipped, Vector3d? offsetWorld, double length);
}

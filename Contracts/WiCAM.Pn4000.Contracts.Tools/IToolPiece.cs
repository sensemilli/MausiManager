using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolPiece
{
	IToolPieceProfile PieceProfile { get; }

	bool Flipped { get; set; }

	double Length => this.PieceProfile.Length;

	Vector3d OffsetLocal { get; set; }

	Vector3d OffsetWorld { get; set; }

	Matrix4d Transform { get; }

	IToolSection ToolSection { get; }

	void SetSection(IToolSection section, bool removeFromOldSection = true);
}

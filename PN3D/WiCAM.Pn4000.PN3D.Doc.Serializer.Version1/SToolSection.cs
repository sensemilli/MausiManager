using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolSection
{
	public List<SToolPiece> Pieces { get; set; }

	public List<SAcbPunchPiece> AcbPieces { get; set; }

	public int MultiToolProfileId { get; set; }

	public bool IsUpperSection { get; set; }

	public SVector3d OffsetLocal { get; set; }

	public double Length { get; set; }

	public bool Flipped { get; set; }

	public double? ReservedSpaceLeft { get; set; }

	public double? ReservedSpaceRight { get; set; }
}

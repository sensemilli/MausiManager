namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolPiece
{
	public int Order { get; set; }

	public int PieceProfileId { get; set; }

	public bool Flipped { get; set; }

	public SVector3d OffsetLocal { get; set; }
}

using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendState
{
	public int FaceGroupId { get; set; }

	public double Angle { get; set; }

	public double KFactor { get; set; }

	public double FinalRadius { get; set; }

	public SBendState()
	{
	}

	public SBendState(IBendState bend)
	{
		this.FaceGroupId = bend.FaceGroupId;
		this.Angle = bend.Angle;
		this.KFactor = bend.KFactor;
		this.FinalRadius = bend.FinalRadius;
	}
}

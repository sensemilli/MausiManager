namespace WiCAM.Pn4000.PN3D.Assembly;

public class AssemblyCommonBendInfo
{
	public int ID { get; set; }

	public string FaceGroupIds { get; set; }

	public int BendsCount { get; set; }

	public double LengthWithGaps { get; set; }

	public double LengthWithoutGaps { get; set; }

	public double Angle { get; set; }

	public double Radius { get; set; }

	public AssemblyCommonBendInfo()
	{
	}

	public AssemblyCommonBendInfo(int id, string faceGroupIds, int count, double lengthWithGaps, double lengthWithoutGaps, double angle, double radius)
	{
		this.ID = id;
		this.FaceGroupIds = faceGroupIds;
		this.BendsCount = count;
		this.LengthWithGaps = lengthWithGaps;
		this.LengthWithoutGaps = lengthWithoutGaps;
		this.Angle = angle;
		this.Radius = radius;
	}
}

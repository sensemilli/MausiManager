namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SPreferredProfile
{
	public int Id { get; set; }

	public string PressFile { get; set; }

	public string MaterialGroupName { get; set; }

	public int MaterialGroupID { get; set; }

	public string MaterialName { get; set; }

	public double Thickness { get; set; }

	public string BendProcess { get; set; }

	public int PunchGroupId { get; set; }

	public string PunchGroupName { get; set; }

	public int DieGroupId { get; set; }

	public string DieGroupName { get; set; }

	public double VWidth { get; set; }

	public double VAngle { get; set; }

	public double CornerRadius { get; set; }

	public double PunchRadius { get; set; }

	public double MinRadius { get; set; }

	public double MaxRadius { get; set; }

	public double MinAngle { get; set; }

	public double MaxAngle { get; set; }

	public bool UseForStepBends { get; set; }
}

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendParametersBase
{
	public int EntryFaceGroupId { get; set; }

	public double? ManualRadius { get; set; }

	public double? ToolRadius { get; set; }

	public int AngleSign { get; set; }

	public double? ManualBendDeduction { get; set; }
}

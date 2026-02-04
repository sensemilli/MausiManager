using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolStationSection
{
	public double Start { get; set; }

	public List<int> Division { get; set; }

	public double Length { get; set; }

	public double End { get; set; }
}

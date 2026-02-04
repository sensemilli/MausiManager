using System.Collections.Generic;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SFingerStopCombination
{
	public ulong? CombinationType { get; set; }

	public List<string>? FaceNames { get; set; }

	public bool BoundingBoxPosition { get; set; }

	public bool NoValidPositionFound { get; set; }
}

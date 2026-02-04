using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class FlangeLengths
{
	public Vector2d Origin { get; set; }

	public Vector2d Direction { get; set; }

	public List<AtomicBendLength> AtomicBendLengths { get; set; } = new List<AtomicBendLength>();

	public Matrix4d BendingZoneWorldMatrix { get; set; }
}

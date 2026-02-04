using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class ProjectionIntervals
{
	public List<IRange>? UnspecifiedProjectionInterval { get; set; }

	public List<IRange>? HoleProjectionInterval { get; set; }

	public List<IRange>? OutlineProjectionInterval { get; set; }
}

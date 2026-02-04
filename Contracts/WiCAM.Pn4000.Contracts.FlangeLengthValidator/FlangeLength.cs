using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.BendTools.Macros;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class FlangeLength
{
	public double MinLength { get; set; }

	public double MaxLength { get; set; }

	public Polygon2D RegionOfInterest { get; set; }

	public Dictionary<Macro, MacroValidationResult> MacroBendAxisDistances { get; set; } = new Dictionary<Macro, MacroValidationResult>();

	public ProjectionIntervals ProjectionIntervals { get; set; } = new ProjectionIntervals();
}

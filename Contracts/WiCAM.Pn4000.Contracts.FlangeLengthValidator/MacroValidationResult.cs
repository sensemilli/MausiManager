using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class MacroValidationResult
{
	public double DistanceToBendingAxis { get; set; }

	public Vector2d ClosestPointToBendingAxis { get; set; }

	public bool? IsOnOutline { get; set; }
}

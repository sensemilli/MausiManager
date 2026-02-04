using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class BendFlangeLengthResult
{
	public ICombinedBendDescriptor CombinedBendDescriptor { get; set; }

	public IBendDescriptor BendDescriptor { get; set; }

	public FlangeLengths FlangeLengths { get; set; }
}

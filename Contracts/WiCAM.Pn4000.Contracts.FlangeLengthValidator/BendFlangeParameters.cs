using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class BendFlangeParameters
{
	public ICombinedBendDescriptor CombinedBendDescriptor { get; set; }

	public IBendDescriptor BendDescriptor { get; set; }

	public FlangeLengthParameters FlangeLengthParameters { get; set; }
}

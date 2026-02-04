using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;

namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public interface IFlangeLengthValidator
{
	void Validate(IPnBndDoc doc, ICalculationArg option);

	List<BendFlangeLengthResult> Validate(IPnBndDoc doc, FlangeLengthParameters parameters);

	List<BendFlangeLengthResult> Validate(IPnBndDoc doc, List<BendFlangeParameters> parametersMap);
}

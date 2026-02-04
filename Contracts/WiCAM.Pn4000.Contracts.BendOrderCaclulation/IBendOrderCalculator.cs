using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;

namespace WiCAM.Pn4000.Contracts.BendOrderCaclulation;

public interface IBendOrderCalculator
{
	void ChangeBendOrder(IPnBndDoc doc, ICalculationArg calcArg);
}

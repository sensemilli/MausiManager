using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolCalcModelProvider
{
	Model CreateModelForSimulation(bool simplify, IPnBndDoc doc);
}

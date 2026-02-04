using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.ModelSimplifier;

public interface IModelSimplifier
{
	Model Simplify(Model original, SimplificationOptions options = null);
}

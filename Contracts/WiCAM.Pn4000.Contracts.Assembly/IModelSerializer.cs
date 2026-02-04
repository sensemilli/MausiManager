using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Assembly;

public interface IModelSerializer
{
	Model Deserialize(string path);
}

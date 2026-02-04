using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IModelCache
{
	Model GetOrCreateModel3d(string filename);

	void Clear();

	void Clear(string filename);
}

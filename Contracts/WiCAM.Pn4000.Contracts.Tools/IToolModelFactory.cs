using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IToolModelFactory
{
	Model CreateToolModel(string path, IToolPieceProfile profile);

	Model CreateDiskModel(string path, ISensorDiskProfile profile);
}

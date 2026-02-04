using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Doc3d;

public interface IArrowFactory
{
	Model CreateDoubleArrow();

	Model CreateAxisCrossSimple();
}

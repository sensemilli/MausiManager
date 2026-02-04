using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabViewer3D;

public interface IViewer3dViewModel : ITab
{
	bool IsActive { get; }

	void HoverModel(Model model);

	void DeHoverModel(Model model);

	void Dispose();
}

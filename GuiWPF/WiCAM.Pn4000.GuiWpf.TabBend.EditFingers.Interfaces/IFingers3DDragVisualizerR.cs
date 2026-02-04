using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;

internal interface IFingers3DDragVisualizerR
{
	bool IsDragging { get; }

	void Start(Model model, Model referenceSystemModel, double minR, double maxR);

	double Stop();

	void Drag(Vector2f pos);
}

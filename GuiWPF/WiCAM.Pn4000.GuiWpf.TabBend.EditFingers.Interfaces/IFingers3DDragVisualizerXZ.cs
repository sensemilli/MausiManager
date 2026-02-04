using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;

internal interface IFingers3DDragVisualizerXZ
{
	bool IsDragging { get; }

	void Start(Model model, Model referenceSystemModel, double minX, double maxX, double minY, double maxY);

	(double distanceOnPrimaryDir, double distanceOnSecondaryDir) Stop();

	void Drag(Vector2f pos);
}

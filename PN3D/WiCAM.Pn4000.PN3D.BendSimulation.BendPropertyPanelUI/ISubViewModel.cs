using System;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

public interface ISubViewModel
{
	event Action<ISubViewModel, Triangle, Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	event Action RequestRepaint;

	void SetActive(bool active);

	bool Close();

	void KeyUp(object sender, IPnInputEventArgs e);

	void MouseSelectTriangle(object sender, ITriangleEventArgs e);

	void MouseMove(object sender, MouseEventArgs e);

	void ColorModelParts(IPaintTool paintTool);
}

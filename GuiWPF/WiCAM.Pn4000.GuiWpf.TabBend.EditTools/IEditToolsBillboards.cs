using System;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public interface IEditToolsBillboards
{
	event Action? RequestRepaint;

	void MouseMove(object sender, MouseEventArgs e);

	void KeyUp(object sender, IPnInputEventArgs e);

	void MouseSelectTriangle(object sender, ITriangleEventArgs e);

	void ColorModelParts(IPaintTool painter);

	void SetActive(bool active);

	void ShowBillboards(Vector3d position);

	void HideBillboards();
}

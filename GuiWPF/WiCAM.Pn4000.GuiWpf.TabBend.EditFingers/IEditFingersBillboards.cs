using System;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

public interface IEditFingersBillboards
{
	event Action? RequestRepaint;

	event Action<IPnInputEventArgs> CmdMoveR;

	event Action<IPnInputEventArgs> CmdMoveXZ;

	event Action<IPnInputEventArgs> CmdSnapUp;

	event Action<IPnInputEventArgs> CmdSnapLeft;

	event Action<IPnInputEventArgs> CmdSnapRight;

	void SetActive(bool active);

	void ShowBillboards();

	void HideBillboards();

	void SetBillboardLocation(Vector3d position, PartRole finger);
}

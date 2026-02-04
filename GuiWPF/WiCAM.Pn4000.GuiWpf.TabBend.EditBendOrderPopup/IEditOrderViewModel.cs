using System;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal interface IEditOrderViewModel
{
	void Init(Action refresh);

	bool Merge(int order);

	bool Close();
}

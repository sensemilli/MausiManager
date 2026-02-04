using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

public interface IBendViewModel : ITab
{
	bool IsActive { get; }

	void Dispose();

	void EditTools3D();

	void ToolCalcEditProfile();
}

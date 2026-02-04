namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public interface IDeleteTab
{
	bool IsDeleteButtonEnabled { get; }

	void DeleteButtonClick();
}

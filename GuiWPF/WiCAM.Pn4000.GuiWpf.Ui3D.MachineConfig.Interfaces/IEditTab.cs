namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;

public interface IEditTab
{
	bool IsEditButtonEnabled { get; }

	void EditButtonClick();
}
